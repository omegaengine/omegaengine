/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using NanoByte.Common;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Shaders;
using SlimDX;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine;

#region Enumerations
/// <seealso cref="EngineState.ZBufferMode"/>
public enum ZBufferMode
{
    /// <summary>Test against the ZBuffer before drawing a pixel and write to it afterwards</summary>
    Normal,

    /// <summary>Test against the ZBuffer before drawing a pixel but don't write to it</summary>
    ReadOnly,

    /// <summary>Do not use the ZBuffer</summary>
    Off
}
#endregion

/// <summary>
/// Represents the current graphics render state of the <see cref="Engine.Device"/>.
/// </summary>
public sealed class EngineState
{
    #region Dependencies
    private readonly Device _device;

    /// <summary>
    /// Creates an engine state object.
    /// </summary>
    /// <param name="device">The Direct3D device to be manipulated internally.</param>
    internal EngineState(Device device)
    {
        _device = device ?? throw new ArgumentNullException(nameof(device));
    }

    /// <summary>
    /// Call when render states got reset to their default values on the device.
    /// </summary>
    internal void Reset()
    {
        _fillMode = FillMode.Solid;
        _cullMode = Cull.Counterclockwise;
        _zBufferMode = ZBufferMode.Normal;
        _ffpLighting = true;
        _fog = false;
        _fogColor = Color.Empty;
        _fogStart = _fogEnd = 0;
        _alphaBlend = 0;
        _worldTransform = _viewTransform = _projectionTransform = new();
    }
    #endregion

    //--------------------//

    #region Drawing mode
    private FillMode _fillMode = FillMode.Solid;

    /// <summary>
    /// Controls how vertexes are filled (normal, wireframe, dotted)
    /// </summary>
    public FillMode FillMode { get => _fillMode; set => value.To(ref _fillMode, () => _device.SetRenderState(RenderState.FillMode, (int)value)); }

    private Cull _cullMode = Cull.Counterclockwise;

    /// <summary>
    /// The current culling mode used for rendering
    /// </summary>
    public Cull CullMode { get => _cullMode; set => value.To(ref _cullMode, () => _device.SetRenderState(RenderState.CullMode, (int)value)); }

    private ZBufferMode _zBufferMode = ZBufferMode.Normal;

    /// <summary>
    /// Controls how the ZBuffer works
    /// </summary>
    public ZBufferMode ZBufferMode
    {
        get => _zBufferMode;
        set =>
            value.To(ref _zBufferMode, delegate
            {
                switch (value)
                {
                    case ZBufferMode.Normal:
                        _device.SetRenderState(RenderState.ZEnable, true);
                        _device.SetRenderState(RenderState.ZWriteEnable, true);
                        break;
                    case ZBufferMode.ReadOnly:
                        _device.SetRenderState(RenderState.ZEnable, true);
                        _device.SetRenderState(RenderState.ZWriteEnable, false);
                        break;
                    case ZBufferMode.Off:
                        _device.SetRenderState(RenderState.ZEnable, false);
                        break;
                }
            });
    }
    #endregion

    #region Lighting
    private bool _ffpLighting = true;

    /// <summary>
    /// Controls whether fixed-function pipeline lighting is used at the moment instead of <see cref="SurfaceShader"/>s
    /// </summary>
    public bool FfpLighting { get => _ffpLighting; set => value.To(ref _ffpLighting, () => _device.SetRenderState(RenderState.Lighting, value)); }
    #endregion

    #region Fog
    private bool _fog;

    /// <summary>
    /// Shall a linear fog effect be applied?
    /// </summary>
    public bool Fog
    {
        get => _fog;
        set
        {
            _fog = value;

            _device.SetRenderState(RenderState.FogEnable, value);
            if (value)
            {
                _device.SetRenderState(RenderState.FogTableMode, (int)FogMode.Linear);

                // Reset projection matrix, otherwise fog settings will be ignored
                _device.SetTransform(TransformState.Projection, _projectionTransform);
            }
        }
    }

    private Color _fogColor;

    /// <summary>
    /// The color of the fog
    /// </summary>
    public Color FogColor { get => _fogColor; set => value.To(ref _fogColor, () => _device.SetRenderState(RenderState.FogColor, value.ToArgb())); }

    private float _fogStart, _fogEnd;

    /// <summary>
    /// The distance at which the linear fog shall start
    /// </summary>
    public float FogStart { get => _fogStart; set => value.To(ref _fogStart, () => _device.SetRenderState(RenderState.FogStart, value)); }

    /// <summary>
    /// The distance at which the linear fog shall have obscured everything
    /// </summary>
    public float FogEnd { get => _fogEnd; set => value.To(ref _fogEnd, () => _device.SetRenderState(RenderState.FogEnd, value)); }
    #endregion

    #region Alpha blending

    #region Constants
    /// <summary>
    /// Value for <see cref="AlphaBlend"/> to use no alpha blending
    /// </summary>
    public const int Opaque = 0;

    /// <summary>
    /// Value for <see cref="AlphaBlend"/> to make something completley transparent (i.e., invisible)
    /// </summary>
    public const int Invisible = 255;

    /// <summary>
    /// Value for <see cref="AlphaBlend"/> to use alpha channel for transparency (augmented with alpha testing)
    /// </summary>
    public const int AlphaChannel = 256;

    /// <summary>
    /// Value for <see cref="AlphaBlend"/> to use alpha channel for binary alpha testing
    /// </summary>
    public const int BinaryAlphaChannel = -256;

    /// <summary>
    /// Value for <see cref="AlphaBlend"/> to apply additive blending (use color map)
    /// </summary>
    public const int AdditivBlending = 257;
    #endregion

    private int _alphaBlend;

    /// <summary>
    /// The level of transparency from 0 (solid) to 255 (invisible),
    /// <see cref="AlphaChannel"/>, <see cref="BinaryAlphaChannel"/> or <see cref="AdditivBlending"/>
    /// </summary>
    public int AlphaBlend
    {
        get => _alphaBlend;
        set =>
            value.To(ref _alphaBlend, delegate
            {
                switch (value)
                {
                    case Opaque:
                        using (new ProfilerEvent("Alpha blending off"))
                        {
                            _device.SetRenderState(RenderState.AlphaBlendEnable, false);
                            _device.SetRenderState(RenderState.AlphaTestEnable, false);
                        }
                        break;

                    case AlphaChannel:
                        using (new ProfilerEvent("Alpha blending to alpha channel"))
                        {
                            // Blend using alpha channel
                            _device.SetRenderState(RenderState.AlphaBlendEnable, true);
                            _device.SetRenderState(RenderState.SourceBlend, (int)Blend.SourceAlpha);
                            _device.SetRenderState(RenderState.DestinationBlend, (int)Blend.InverseSourceAlpha);

                            // Cut out invisible parts
                            _device.SetRenderState(RenderState.AlphaTestEnable, true);
                            _device.SetRenderState(RenderState.AlphaFunc, (int)Compare.Greater);
                            _device.SetRenderState(RenderState.AlphaRef, 0);
                        }
                        break;

                    case BinaryAlphaChannel:
                        using (new ProfilerEvent("Alpha blending to binary alpha channel"))
                        {
                            _device.SetRenderState(RenderState.AlphaBlendEnable, false);

                            // Cut off in the middle
                            _device.SetRenderState(RenderState.AlphaTestEnable, true);
                            _device.SetRenderState(RenderState.AlphaFunc, (int)Compare.Greater);
                            _device.SetRenderState(RenderState.AlphaRef, 127);
                        }
                        break;

                    case AdditivBlending:
                        using (new ProfilerEvent("Alpha blending to additive blending"))
                        {
                            // Blend using color
                            _device.SetRenderState(RenderState.AlphaBlendEnable, true);
                            _device.SetRenderState(RenderState.SourceBlend, (int)Blend.One);
                            _device.SetRenderState(RenderState.DestinationBlend, (int)Blend.One);

                            _device.SetRenderState(RenderState.AlphaTestEnable, false);
                        }
                        break;

                    default:
                        value = value.Clamp(Opaque, Invisible);
                        using (new ProfilerEvent(() => "Alpha blending to constant: " + value))
                        {
                            // Blend using constant factor
                            _device.SetRenderState(RenderState.AlphaBlendEnable, true);
                            _device.SetRenderState(RenderState.DestinationBlend, (int)Blend.BlendFactor);
                            _device.SetRenderState(RenderState.SourceBlend, (int)Blend.InverseBlendFactor);
                            _device.SetRenderState(RenderState.BlendFactor,
                                Color.FromArgb(255, value, value, value).ToArgb());

                            _device.SetRenderState(RenderState.AlphaTestEnable, false);
                        }
                        break;
                }
            });
    }
    #endregion

    //--------------------//

    #region Transforms
    private Matrix _worldTransform, _viewTransform, _projectionTransform;

    /// <summary>
    /// The currently active world transformation matrix
    /// </summary>
    public Matrix WorldTransform
    {
        get => _worldTransform;
        set
        {
            _worldTransform = value;
            _device.SetTransform(TransformState.World, value);
        }
    }

    /// <summary>
    /// The currently active view transformation matrix
    /// </summary>
    public Matrix ViewTransform { get => _viewTransform; set => value.To(ref _viewTransform, () => _device.SetTransform(TransformState.View, value)); }

    /// <summary>
    /// The currently active projection transformation matrix
    /// </summary>
    public Matrix ProjectionTransform { get => _projectionTransform; set => value.To(ref _projectionTransform, () => _device.SetTransform(TransformState.Projection, value)); }
    #endregion

    #region User clip plane
    private Plane _userClipPlane;

    /// <summary>
    /// The currently active user clip plane; default <see cref="Plane"/> for none
    /// </summary>
    /// <remarks>
    /// When rendering without <see cref="SurfaceShader"/>s the clip plane must be in world space.
    /// When rendering with <see cref="SurfaceShader"/>s it must be in camera space.
    /// </remarks>
    public Plane UserClipPlane
    {
        get => _userClipPlane;
        set =>
            value.To(ref _userClipPlane, delegate
            {
                if (value == default)
                { // No plane set
                    _device.SetRenderState(RenderState.ClipPlaneEnable, 0);
                }
                else
                { // Apply new plane
                    _device.SetClipPlane(0, value);
                    _device.SetRenderState(RenderState.ClipPlaneEnable, 1);
                }
            });
    }
    #endregion

    //--------------------//

    #region Vertex buffer
    /// <summary>
    /// Sets a <see cref="VertexBuffer"/> with a fixed-function vertex format as the current active stream source.
    /// </summary>
    /// <param name="buffer">The <see cref="VertexBuffer"/> with a fixed-function vertex format.</param>
    public void SetVertexBuffer(VertexBuffer buffer)
    {
        #region Sanity checks
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (buffer.Description.FVF == VertexFormat.None) throw new ArgumentException(Resources.VBMustBeFVF, nameof(buffer));
        #endregion

        _device.SetStreamSource(0, buffer, 0, D3DX.GetFVFVertexSize(buffer.Description.FVF));
        _device.VertexFormat = buffer.Description.FVF;
    }
    #endregion

    #region Texture
    /// <summary>
    /// Sets the currently active texture for the first texture stage.
    /// </summary>
    /// <param name="texture">The object providing the texture.</param>
    /// <remarks>Corresponds to calling <see cref="SlimDX.Direct3D9.Device.SetTexture"/> with the texture stage parameter set to 0.</remarks>
    public void SetTexture(ITextureProvider texture)
    {
        _device.SetTexture(0, texture?.Texture);
    }
    #endregion
}
