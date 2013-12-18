/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using Common.Collections;
using Common.Utils;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics
{
    /// <summary>
    /// Provides an in-memory area to render to instead of directly painting on the screen.
    /// </summary>
    /// <remarks>Lost devices are automatically handled.</remarks>
    /// <seealso cref="TextureView.GetRenderTarget"/>
    public sealed class RenderTarget : EngineElement, ITextureProvider, IPoolable<RenderTarget>
    {
        #region Variables
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
        /// A reference to the next element in the <see cref="Pool{T}"/> chain.
        /// </summary>
        RenderTarget IPoolable<RenderTarget>.NextElement { get; set; }

        /// <summary>
        /// The size for this render target (is empty for fullscreen)
        /// </summary>
        internal Size Size { get; private set; }

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
            return (renderTarget == null) ? null : renderTarget.Texture;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new render target texture wrapper
        /// </summary>
        /// <param name="size">The size of the texture - leave empty for fullscreen</param>
        public RenderTarget(Size size)
        {
            Size = size;
        }
        #endregion

        #region Device Lost
        private void OnLostDevice()
        {
            if (!Engine.Disposed)
            {
                Texture.Dispose();
                Surface.Dispose();
                if (_rtsHelper != null) _rtsHelper.OnLostDevice();
            }
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            if (!Engine.Disposed)
            {
                // Use the engine's default viewport if selected
                Viewport = (Size == Size.Empty) ? Engine.RenderViewport : new Viewport {Width = Size.Width, Height = Size.Height, MaxZ = 1};

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
                if (_rtsHelper != null) _rtsHelper.OnResetDevice();

                // Create the target texture and surface
                Texture = new Texture(Engine.Device, Viewport.Width, Viewport.Height, 1,
                    Usage.RenderTarget, Engine.PresentParams.BackBufferFormat, Pool.Default);
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
                _rtsHelper = new RenderToSurface(Engine.Device, _rtsHelperSize.Width, _rtsHelperSize.Height,
                    Engine.PresentParams.BackBufferFormat, Engine.PresentParams.AutoDepthStencilFormat);
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

        #region Engine
        /// <inheritdoc/>
        protected override void OnEngineSet()
        {
            base.OnEngineSet();

            // Hook device events
            Engine.DeviceLost += OnLostDevice;
            Engine.DeviceReset += Initialize;

            Initialize();
        }
        #endregion

        #region Dispose
        /// <inheritdoc/>
        protected override void OnDispose()
        {
            // Unhook device events
            Engine.DeviceLost -= OnLostDevice;
            Engine.DeviceReset -= Initialize;

            if (_rtsHelper != null) _rtsHelper.Dispose();
            if (Surface != null) Surface.Dispose();
            Texture.Dispose();
        }
        #endregion
    }
}
