/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using Common;
using Common.Utils;
using SlimDX;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Shaders;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine
{

    #region Enumerations
    /// <seealso cref="Engine.ZBufferMode"/>
    internal enum ZBufferMode
    {
        /// <summary>Test against the ZBuffer before drawing a pixel and write to it afterwards</summary>
        Normal,

        /// <summary>Test against the ZBuffer before drawing a pixel but don't write to it</summary>
        ReadOnly,

        /// <summary>Do not use the ZBuffer</summary>
        Off
    }
    #endregion

    // This file contains methods and properties for manipulating the RenderState (mainly used be Entity.Render(...) methods)
    partial class Engine
    {
        #region Drawing mode
        private FillMode _fillMode = FillMode.Solid;

        /// <summary>
        /// Controls how vertexes are filled (normal, wireframe, dotted)
        /// </summary>
        internal FillMode FillMode { get { return _fillMode; } set { UpdateHelper.Do(ref _fillMode, value, () => Device.SetRenderState(RenderState.FillMode, (int)value)); } }

        private Cull _cullMode = Cull.Counterclockwise;

        /// <summary>
        /// The current culling mode used for rendering
        /// </summary>
        internal Cull CullMode { get { return _cullMode; } set { UpdateHelper.Do(ref _cullMode, value, () => Device.SetRenderState(RenderState.CullMode, (int)value)); } }

        private ZBufferMode _zBufferMode = ZBufferMode.Normal;

        /// <summary>
        /// Controls how the ZBuffer works
        /// </summary>
        internal ZBufferMode ZBufferMode
        {
            get { return _zBufferMode; }
            set
            {
                UpdateHelper.Do(ref _zBufferMode, value, delegate
                {
                    switch (value)
                    {
                        case ZBufferMode.Normal:
                            Device.SetRenderState(RenderState.ZEnable, true);
                            Device.SetRenderState(RenderState.ZWriteEnable, true);
                            break;
                        case ZBufferMode.ReadOnly:
                            Device.SetRenderState(RenderState.ZEnable, true);
                            Device.SetRenderState(RenderState.ZWriteEnable, false);
                            break;
                        case ZBufferMode.Off:
                            Device.SetRenderState(RenderState.ZEnable, false);
                            break;
                    }
                });
            }
        }
        #endregion

        #region Lighting
        private bool _ffpLighting = true;

        /// <summary>
        /// Controls whether fixed-function pipeline lighting is used at the moment instead of <see cref="SurfaceShader"/>s
        /// </summary>
        internal bool FfpLighting { get { return _ffpLighting; } set { UpdateHelper.Do(ref _ffpLighting, value, () => Device.SetRenderState(RenderState.Lighting, value)); } }
        #endregion

        #region Fading
        /// <summary>
        /// The level of scene fading from 0 (fully visible) to 255 (invisible) - use <see cref="FadeIn"/>, <see cref="DimDown"/> and <see cref="DimUp"/> to manipulate
        /// </summary>
        public int FadeLevel { get; private set; }

        /// <summary>
        /// Is <see cref="ExtraRender"/> faded along with the scene?
        /// </summary>
        /// <seealso cref="FadeIn"/>
        /// <seealso cref="DimDown"/>
        /// <seealso cref="DimUp"/>
        public bool FadeExtra { get; private set; }
        #endregion

        #region Fog
        private bool _fog;

        /// <summary>
        /// Shall a linear fog effect be applied?
        /// </summary>
        internal bool Fog
        {
            get { return _fog; }
            set
            {
                _fog = value;

                Device.SetRenderState(RenderState.FogEnable, value);
                if (value)
                {
                    Device.SetRenderState(RenderState.FogTableMode, (int)FogMode.Linear);

                    // Reset projection matrix, otherwise fog settings will be ignored
                    Device.SetTransform(TransformState.Projection, _projectionTransform);
                }
            }
        }

        private Color _fogColor;

        /// <summary>
        /// The color of the fog
        /// </summary>
        internal Color FogColor { get { return _fogColor; } set { UpdateHelper.Do(ref _fogColor, value, () => Device.SetRenderState(RenderState.FogColor, value.ToArgb())); } }

        private float _fogStart, _fogEnd;

        /// <summary>
        /// The distance at which the linear fog shall start
        /// </summary>
        internal float FogStart { get { return _fogStart; } set { UpdateHelper.Do(ref _fogStart, value, () => Device.SetRenderState(RenderState.FogStart, value)); } }

        /// <summary>
        /// The distance at which the linear fog shall have obscured everything
        /// </summary>
        internal float FogEnd { get { return _fogEnd; } set { UpdateHelper.Do(ref _fogEnd, value, () => Device.SetRenderState(RenderState.FogEnd, value)); } }
        #endregion

        #region Alpha blending

        #region Constants
        /// <summary>
        /// Value for <see cref="AlphaBlend"/> to use no alpha blending
        /// </summary>
        internal const int Opaque = 0;

        /// <summary>
        /// Value for <see cref="AlphaBlend"/> to make something completley transparent (i.e., invisible)
        /// </summary>
        internal const int Invisible = 255;

        /// <summary>
        /// Value for <see cref="AlphaBlend"/> to use alpha channel for transparency (augmented with alpha testing)
        /// </summary>
        internal const int AlphaChannel = 256;

        /// <summary>
        /// Value for <see cref="AlphaBlend"/> to use alpha channel for binary alpha testing
        /// </summary>
        internal const int BinaryAlphaChannel = -256;

        /// <summary>
        /// Value for <see cref="AlphaBlend"/> to apply additive blending (use color map)
        /// </summary>
        internal const int AdditivBlending = 257;
        #endregion

        private int _alphaBlend;

        /// <summary>
        /// The level of transparency from 0 (solid) to 255 (invisible),
        /// <see cref="AlphaChannel"/>, <see cref="BinaryAlphaChannel"/> or <see cref="AdditivBlending"/>
        /// </summary>
        internal int AlphaBlend
        {
            get { return _alphaBlend; }
            set
            {
                UpdateHelper.Do(ref _alphaBlend, value, delegate
                {
                    switch (value)
                    {
                        case Opaque:
                            using (new ProfilerEvent("Alpha blending off"))
                            {
                                Device.SetRenderState(RenderState.AlphaBlendEnable, false);
                                Device.SetRenderState(RenderState.AlphaTestEnable, false);
                            }
                            break;

                        case AlphaChannel:
                            using (new ProfilerEvent("Alpha blending to alpha channel"))
                            {
                                // Blend using alpha channel
                                Device.SetRenderState(RenderState.AlphaBlendEnable, true);
                                Device.SetRenderState(RenderState.SourceBlend, (int)Blend.SourceAlpha);
                                Device.SetRenderState(RenderState.DestinationBlend, (int)Blend.InverseSourceAlpha);

                                // Cut out invisible parts
                                Device.SetRenderState(RenderState.AlphaTestEnable, true);
                                Device.SetRenderState(RenderState.AlphaFunc, (int)Compare.Greater);
                                Device.SetRenderState(RenderState.AlphaRef, 0);
                            }
                            break;

                        case BinaryAlphaChannel:
                            using (new ProfilerEvent("Alpha blending to binary alpha channel"))
                            {
                                Device.SetRenderState(RenderState.AlphaBlendEnable, false);

                                // Cut off in the middle
                                Device.SetRenderState(RenderState.AlphaTestEnable, true);
                                Device.SetRenderState(RenderState.AlphaFunc, (int)Compare.Greater);
                                Device.SetRenderState(RenderState.AlphaRef, 127);
                            }
                            break;

                        case AdditivBlending:
                            using (new ProfilerEvent("Alpha blending to additive blending"))
                            {
                                // Blend using color
                                Device.SetRenderState(RenderState.AlphaBlendEnable, true);
                                Device.SetRenderState(RenderState.SourceBlend, (int)Blend.One);
                                Device.SetRenderState(RenderState.DestinationBlend, (int)Blend.One);

                                Device.SetRenderState(RenderState.AlphaTestEnable, false);
                            }
                            break;

                        default:
                            value = MathUtils.Clamp(value, Opaque, Invisible);
                            using (new ProfilerEvent(() => "Alpha blending to constant: " + value))
                            {
                                // Blend using constant factor
                                Device.SetRenderState(RenderState.AlphaBlendEnable, true);
                                Device.SetRenderState(RenderState.DestinationBlend, (int)Blend.BlendFactor);
                                Device.SetRenderState(RenderState.SourceBlend, (int)Blend.InverseBlendFactor);
                                Device.SetRenderState(RenderState.BlendFactor,
                                    Color.FromArgb(255, value, value, value).ToArgb());

                                Device.SetRenderState(RenderState.AlphaTestEnable, false);
                            }
                            break;
                    }
                });
            }
        }
        #endregion

        //--------------------//

        #region Transforms
        private Matrix _worldTransform, _viewTransform, _projectionTransform;

        /// <summary>
        /// The currently active world transformation matrix
        /// </summary>
        internal Matrix WorldTransform
        {
            get { return _worldTransform; }
            set
            {
                _worldTransform = value;
                Device.SetTransform(TransformState.World, value);
            }
        }

        /// <summary>
        /// The currently active view transformation matrix
        /// </summary>
        internal Matrix ViewTransform { get { return _viewTransform; } set { UpdateHelper.Do(ref _viewTransform, value, () => Device.SetTransform(TransformState.View, value)); } }

        /// <summary>
        /// The currently active projection transformation matrix
        /// </summary>
        internal Matrix ProjectionTransform { get { return _projectionTransform; } set { UpdateHelper.Do(ref _projectionTransform, value, () => Device.SetTransform(TransformState.Projection, value)); } }
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
        internal Plane UserClipPlane
        {
            get { return _userClipPlane; }
            set
            {
                UpdateHelper.Do(ref _userClipPlane, value, delegate
                {
                    if (value == default(Plane))
                    { // No plane set
                        Device.SetRenderState(RenderState.ClipPlaneEnable, 0);
                    }
                    else
                    { // Apply new plane
                        Device.SetClipPlane(0, value);
                        Device.SetRenderState(RenderState.ClipPlaneEnable, 1);
                    }
                });
            }
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
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (buffer.Description.FVF == VertexFormat.None) throw new ArgumentException(Resources.VBMustBeFVF, "buffer");
            #endregion

            Device.SetStreamSource(0, buffer, 0, D3DX.GetFVFVertexSize(buffer.Description.FVF));
            Device.VertexFormat = buffer.Description.FVF;
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
            Device.SetTexture(0, (texture == null) ? null : texture.Texture);
        }
        #endregion

        #region Texture filtering
        /// <summary>
        /// Apply texture filtering modes to <see cref="Device"/>.
        /// </summary>
        private void SetupTextureFiltering()
        {
            var filter = (int)Device.Capabilities.TextureFilterCaps;
            Device.SetSamplerState(0, SamplerState.MaxAnisotropy, Device.Capabilities.MaxAnisotropy);

            // Always use linear filtering for mip-maps if possible
            if (MathUtils.CheckFlag(filter, (int)FilterCaps.MipLinear))
                Device.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.Linear);

            if (Anisotropic)
            { // Use anisotropic filtering for magnifying and minifying if it was enabled
                Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Anisotropic);
                Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Anisotropic);
            }
            else if (MathUtils.CheckFlag(filter, (int)(FilterCaps.MinLinear | FilterCaps.MagLinear)))
            { // Otherwise use linear filtering if possible
                Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);
                Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);
            }
        }
        #endregion
    }
}
