/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Drawing;
using JetBrains.Annotations;
using OmegaEngine.Foundation.Collections;
using Vortice.Direct3D9;

namespace OmegaEngine.Graphics;

/// <summary>
/// Provides an in-memory area to render to instead of directly painting on the screen.
/// </summary>
/// <remarks>Lost devices are automatically handled.</remarks>
/// <seealso cref="TextureView.GetRenderTarget"/>
public sealed class RenderTarget : ITextureProvider, IDisposable, IPoolable<RenderTarget>
{
    #region Variables
    private readonly Engine _engine;
    private IDirect3DSurface9? _depthStencil;
    private Size _depthStencilSize;
    #endregion

    #region Properties
    /// <summary>
    /// The surface to render onto.
    /// </summary>
    internal IDirect3DSurface9 Surface { get; private set; }

    /// <summary>
    /// The texture containing the rendered content.
    /// </summary>
    public IDirect3DTexture9 Texture { get; private set; }

    /// <summary>
    /// Was this object already disposed?
    /// </summary>
    [Browsable(false)]
    public bool Disposed { get; private set; }

    /// <summary>
    /// A reference to the next element in the <see cref="Pool{T}"/> chain.
    /// </summary>
    RenderTarget? IPoolable<RenderTarget>.NextElement { get; set; }

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
    /// Convert a <see cref="RenderTarget"/> into its contained <see cref="IDirect3DTexture9"/>.
    /// </summary>
    public static implicit operator IDirect3DTexture9?(RenderTarget? renderTarget) => renderTarget?.Texture;
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new render target texture wrapper
    /// </summary>
    /// <param name="engine">The <see cref="OmegaEngine.Engine"/> to store the texture in</param>
    /// <param name="size">The size of the texture - leave empty for fullscreen</param>
    public RenderTarget(Engine engine, Size size)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        Size = size;

        // Hook device events
        engine.DeviceLost += OnLostDevice;
        engine.DeviceReset += Initialize;

        try
        {
            Initialize();
        }
        catch
        {
            engine.DeviceLost -= OnLostDevice;
            engine.DeviceReset -= Initialize;
            throw;
        }
    }
    #endregion

    #region Device Lost
    private void OnLostDevice()
    {
        if (!_engine.IsDisposed)
        {
            Texture.Dispose();
            Surface.Dispose();
            _depthStencil?.Dispose();
            _depthStencil = null;
        }
    }
    #endregion

    #region Initialize
    private void Initialize()
    {
        if (!_engine.IsDisposed)
        {
            // Use the engine's default viewport if selected
            Viewport = (Size == Size.Empty) ? _engine.RenderViewport : new() {Width = Size.Width, Height = Size.Height, MaxZ = 1};

            // Dispose the depth-stencil surface if it already exists but the size has changed
            new Size(Viewport.Width, Viewport.Height).To(ref _depthStencilSize, delegate
            {
                _depthStencil?.Dispose();
                _depthStencil = null;
            });

            // Create the target texture and surface
            Texture = _engine.Device.CreateTexture((uint)Viewport.Width, (uint)Viewport.Height, 1,
                Usage.RenderTarget, _engine.PresentParams.BackBufferFormat, Pool.Default);
            try
            {
                Surface = Texture.GetSurfaceLevel(0);
            }
            catch
            {
                Texture.Dispose();
                Texture = null!;
                throw;
            }
        }
    }
    #endregion

    //--------------------//

    #region Render
    /// <summary>
    /// Renders the content of a delegate to this texture
    /// </summary>
    /// <param name="render">The content to be rendered</param>
    internal void RenderTo([InstantHandle] Action render)
    {
        #region Sanity checks
        if (Disposed) throw new ObjectDisposedException(ToString());
        if (render == null) throw new ArgumentNullException(nameof(render));
        #endregion

        // Don't initialise this earlier, would cause trouble with resetting the device
        var device = _engine.Device;
        _depthStencil ??= device.CreateDepthStencilSurface((uint)_depthStencilSize.Width, (uint)_depthStencilSize.Height,
            _engine.PresentParams.AutoDepthStencilFormat, MultisampleType.None, multisampleQuality: 0, discard: false);

        using (new ProfilerEvent("Rendering to texture"))
        {
            // Redirect rendering to the texture and a matching depth-stencil surface, then restore everything afterwards
            using var previousTarget = device.GetRenderTarget(0);
            using var previousDepthStencil = device.DepthStencilSurface;
            var previousViewport = device.Viewport;

            device.SetRenderTarget(0, Surface);
            device.DepthStencilSurface = _depthStencil;
            device.Viewport = Viewport;
            device.BeginScene();
            try
            {
                render();
            }
            finally
            {
                device.EndScene();
                device.SetRenderTarget(0, previousTarget);
                device.DepthStencilSurface = previousDepthStencil;
                device.Viewport = previousViewport;
            }
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
    /// Disposes the <see cref="IDirect3DTexture9"/> this object wraps
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
    private void Dispose(bool disposing)
    {
        if (disposing)
        { // This block will only be executed on manual disposal, not by Garbage Collection
            // Unhook device events
            _engine.DeviceLost -= OnLostDevice;
            _engine.DeviceReset -= Initialize;

            if (_engine.IsDisposed) return;
            _depthStencil?.Dispose();
            Surface?.Dispose();
            Texture?.Dispose();
        }
        else
        { // This block will only be executed on Garbage Collection, not by manual disposal
            Log.Error($"Forgot to call Dispose on {this}");
#if DEBUG
            throw new InvalidOperationException($"Forgot to call Dispose on {this}");
#endif
        }
    }
    #endregion
}
