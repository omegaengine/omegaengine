/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using NanoByte.Common;
using NanoByte.Common.Collections;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics
{
    /// <summary>
    /// Provides an in-memory area to render to instead of directly painting on the screen.
    /// </summary>
    /// <remarks>Lost devices are automatically handled.</remarks>
    /// <seealso cref="TextureView.GetRenderTarget"/>
    public sealed class RenderTarget : ITextureProvider, IDisposable, IPoolable<RenderTarget>
    {
        #region Variables
        private readonly Engine _engine;
        private RenderToSurface _rtsHelper;
        private Size _rtsHelperSize;
        #endregion

        #region Properties
        /// <summary>
        /// The surface to render onto.
        /// </summary>
        internal Surface Surface { get; private set; }

        /// <summary>
        /// The texture containing the rendered content.
        /// </summary>
        public Texture Texture { get; private set; }

        /// <summary>
        /// Was this object already disposed?
        /// </summary>
        [Browsable(false)]
        public bool Disposed { get; private set; }

        /// <summary>
        /// A reference to the next element in the <see cref="Pool{T}"/> chain.
        /// </summary>
        RenderTarget IPoolable<RenderTarget>.NextElement { get; set; }

        /// <summary>
        /// The size for this render target (is empty for fullscreen)
        /// </summary>
        internal Size Size { get; }

        /// <summary>
        /// The currently effective <see cref="Viewport"/> for this render target
        /// </summary>
        internal Viewport Viewport { get; private set; }
        #endregion

        #region Conversion
        /// <summary>
        /// Convert a <see cref="RenderTarget"/> into its contained <see cref="SlimDX.Direct3D9.Texture"/>.
        /// </summary>
        public static implicit operator Texture(RenderTarget renderTarget)
        {
            return renderTarget?.Texture;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new render target texture wrapper
        /// </summary>
        /// <param name="engine">The <see cref="OmegaEngine.Engine"/> to store the texture in</param>
        /// <param name="size">The size of the texture - leave empty for fullscreen</param>
        public RenderTarget(Engine engine, Size size)
        {
            if (engine == null) throw new ArgumentNullException("engine");

            _engine = engine;
            Size = size;

            // Hook device events
            engine.DeviceLost += OnLostDevice;
            engine.DeviceReset += Initialize;

            Initialize();
        }
        #endregion

        #region Device Lost
        private void OnLostDevice()
        {
            if (!_engine.IsDisposed)
            {
                Texture.Dispose();
                Surface.Dispose();
                _rtsHelper?.OnLostDevice();
            }
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            if (!_engine.IsDisposed)
            {
                // Use the engine's default viewport if selected
                Viewport = (Size == Size.Empty) ? _engine.RenderViewport : new Viewport {Width = Size.Width, Height = Size.Height, MaxZ = 1};

                // Dispose the _rtsHelper if it already exists but the size has changed
                new Size(Viewport.Width, Viewport.Height).To(ref _rtsHelperSize, delegate
                {
                    if (_rtsHelper != null)
                    {
                        _rtsHelper.Dispose();
                        _rtsHelper = null;
                    }
                });

                // Reset the _rtsHelper if it already exists and is still valid
                _rtsHelper?.OnResetDevice();

                // Create the target texture and surface
                Texture = new Texture(_engine.Device, Viewport.Width, Viewport.Height, 1,
                    Usage.RenderTarget, _engine.PresentParams.BackBufferFormat, Pool.Default);
                Surface = Texture.GetSurfaceLevel(0);
            }
        }
        #endregion

        //--------------------//

        #region Render
        /// <summary>
        /// Renders the content of a delegate to this texture
        /// </summary>
        /// <param name="render">The content to be rendered</param>
        internal void RenderTo(Action render)
        {
            #region Sanity checks
            if (Disposed) throw new ObjectDisposedException(ToString());
            if (render == null) throw new ArgumentNullException("render");
            #endregion

            // Don't initialise this earlier, would cause trouble with resetting the device
            if (_rtsHelper == null)
            {
                _rtsHelper = new RenderToSurface(_engine.Device, _rtsHelperSize.Width, _rtsHelperSize.Height,
                    _engine.PresentParams.BackBufferFormat, _engine.PresentParams.AutoDepthStencilFormat);
            }

            using (new ProfilerEvent("Rendering to texture"))
            {
                _rtsHelper.BeginScene(Surface, Viewport);
                render();
                _rtsHelper.EndScene(Filter.None);
            }
        }
        #endregion

        //--------------------//

        #region Reference control
        /// <summary>
        /// Is ignored.
        /// </summary>
        public void HoldReference()
        {}

        /// <summary>
        /// Is ignored.
        /// </summary>
        public void ReleaseReference()
        {}
        #endregion

        //--------------------//

        #region Dispose
        /// <summary>
        /// Disposes the <see cref="Texture"/> this object wraps
        /// </summary>
        public void Dispose()
        {
            if (Disposed) return;
            Dispose(true);
            GC.SuppressFinalize(this);
            Disposed = true;
        }

        /// <inheritdoc/>
        ~RenderTarget()
        {
            Dispose(false);
        }

        /// <summary>
        /// To be called by <see cref="IDisposable.Dispose"/> and the object destructor.
        /// </summary>
        /// <param name="disposing"><c>true</c> if called manually and not by the garbage collector.</param>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Only for debugging, not present in Release code")]
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_rtsHelper", Justification = "_rtsHelper is queued for disposal at the end of the frame")]
        private void Dispose(bool disposing)
        {
            if (disposing)
            { // This block will only be executed on manual disposal, not by Garbage Collection
                // Unhook device events
                _engine.DeviceLost -= OnLostDevice;
                _engine.DeviceReset -= Initialize;

                if (_engine == null || _engine.IsDisposed) return;
                _rtsHelper?.Dispose();
                Surface?.Dispose();
                Texture?.Dispose();
            }
            else
            { // This block will only be executed on Garbage Collection, not by manual disposal
                Log.Error("Forgot to call Dispose on " + this);
#if DEBUG
                throw new InvalidOperationException("Forgot to call Dispose on " + this);
#endif
            }
        }
        #endregion
    }
}
