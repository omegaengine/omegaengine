/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using NanoByte.Common;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Graphics.Shaders;

namespace OmegaEngine.Graphics
{
    // This file contains internal helper methods
    partial class View
    {
        #region Visibility check
        /// <summary>
        /// Checks if a <see cref="PositionableRenderable"/> is supposed to be rendered in this type of <see cref="View"/>
        /// </summary>
        /// <param name="body">The <see cref="PositionableRenderable"/> to check</param>
        /// <returns><c>true</c> if the <see cref="PositionableRenderable"/> is supposed to be rendered</returns>
        protected virtual bool IsToRender(PositionableRenderable body)
        {
            #region Sanity checks
            if (body == null) throw new ArgumentNullException("body");
            #endregion

            switch (body.RenderIn)
            {
                case ViewType.All:
                case ViewType.NormalOnly:
                    return true;
                case ViewType.SupportOnly:
                case ViewType.GlowOnly:
                    return false;
                default:
                    throw new ArgumentException("Invalid ViewType!", "body");
            }
        }
        #endregion

        #region Camera helpers
        /// <summary>
        /// Applies the <see cref="Cameras.Camera.PositionBase"/> as the <see cref="IPositionableOffset.Offset"/>.
        /// </summary>
        /// <param name="offsetable">The positionable object to be shifted</param>
        internal void ApplyCameraBase(IPositionableOffset offsetable)
        {
            offsetable.Offset = Camera.PositionBase;
        }

        /// <summary>
        /// Updates <see cref="_viewport"/> for usage in <see cref="Render"/>
        /// </summary>
        private void UpdateViewport()
        {
            if (_area.Width == 0 || _area.Height == 0)
            { // Use the engine's default viewport (fill the whole screen)
                _viewport = Engine.RenderViewport;
            }
            else
            { // Use a custom viewport
                // Convert the area rectangle to a viewport
                _viewport = new Viewport {X = _area.X, Y = _area.Y, Width = _area.Width, Height = _area.Height, MaxZ = 1};

                // Trim the viewport size if it sticks over the edge
                if (_viewport.X + _viewport.Width > Engine.RenderSize.Width)
                    _viewport.Width = Engine.RenderSize.Width - _viewport.X;
                if (_viewport.Y + _viewport.Height > Engine.RenderSize.Height)
                    _viewport.Height = Engine.RenderSize.Height - _viewport.Y;
            }
        }
        #endregion

        //--------------------//

        #region Apply post-screen shaders
        /// <summary>
        /// Applies <see cref="PostShaders"/> to the output
        /// </summary>
        /// <param name="sceneOnBackBuffer">
        /// Is the scene currently on the backbuffer?<br/>
        /// If this is <c>false</c>, it is in <see cref="RenderTarget"/>.
        /// </param>
        protected virtual void ApplyPostShaders(bool sceneOnBackBuffer)
        {
            // Note: We need a separate list, because we have to be able to determine which is the last effective post-screen shader
            _effectivePostShaders.Clear();
            _effectivePostShaders.AddRange(_postShaders.Where(shader => shader.Enabled));

            for (int i = 0; i < _effectivePostShaders.Count; i++)
            {
                var shader = _effectivePostShaders[i];
                if (i == _effectivePostShaders.Count - 1) ApplyPostShaderLast(shader, ref sceneOnBackBuffer);
                else ApplyPostShaderIntermediate(shader, ref sceneOnBackBuffer);
            }
        }

        /// <summary>
        /// Applies an intermediate shader from <see cref="PostShaders"/> to the output.
        /// </summary>
        /// <param name="shader">The shader to apply.</param>
        /// <param name="sceneOnBackBuffer">
        /// Is the scene currently on the backbuffer?<br/>
        /// If this is <c>false</c>, it is in <see cref="RenderTarget"/>.
        /// </param>
        private void ApplyPostShaderIntermediate(PostShader shader, ref bool sceneOnBackBuffer)
        {
            if (sceneOnBackBuffer && shader.OverlayRendering)
            {
                // Apply the shader directly on the back-buffer
                ShaderToBackBuffer(shader, 0);
            }
            else
            {
                // Make sure the scene is inside a texture
                if (sceneOnBackBuffer)
                {
                    CopyBackBufferToTexture();
                    sceneOnBackBuffer = false;
                }

                // Update the RenderTarget texture to incorporate the PostShader effect
                ShaderToRenderTarget(shader);
            }
        }

        /// <summary>
        /// Applies the last shader from <see cref="PostShaders"/> to the output.
        /// </summary>
        /// <param name="shader">The shader to apply.</param>
        /// <param name="sceneOnBackBuffer">
        /// Is the scene currently on the backbuffer?<br/>
        /// If this is <c>false</c>, it is in <see cref="RenderTarget"/>.
        /// </param>
        private void ApplyPostShaderLast(PostShader shader, ref bool sceneOnBackBuffer)
        {
            if (FullAlpha == 0)
            { // Opaque
                // Scene must be on the back-buffer before using a shader with overlay rendering
                if (shader.OverlayRendering && !sceneOnBackBuffer) OutputRenderTarget(0);

                // Make sure the scene ends up in a texture (out of the back-buffer)
                if (!shader.OverlayRendering && sceneOnBackBuffer)
                {
                    CopyBackBufferToTexture();
                    sceneOnBackBuffer = false;
                }

                // Apply the shader directly on the back-buffer
                ShaderToBackBuffer(shader, 0);
            }
            else
            { // Transparent
                Debug.Assert(!sceneOnBackBuffer, "Scene should never be on back-buffer with alpha-blended output");

                if (shader.OverlayRendering)
                {
                    // Update the RenderTarget texture to incorporate the PostShader effect and then output it
                    ShaderToRenderTarget(shader);
                    OutputRenderTarget(FullAlpha);
                }
                else
                {
                    // Apply the shader while outputing to a transparent quad
                    ShaderToBackBuffer(shader, FullAlpha);
                }
            }
        }
        #endregion

        #region Render-to-texture helpers
        /// <summary>
        /// Prepares a <see cref="OmegaEngine.Graphics.RenderTarget"/> texture if there is none yet - call as often as you want
        /// </summary>
        protected void PrepareRenderTarget()
        {
            if (RenderTarget != null) return;

            using (new ProfilerEvent("Prepare render target texture"))
                RenderTarget = new RenderTarget(Engine, _area.Size);
        }

        /// <summary>
        /// Swaps the <see cref="OmegaEngine.Graphics.RenderTarget"/> texture with a secondary one - used for multiple <see cref="PostShaders"/> entries
        /// </summary>
        private void SwapRenderTarget()
        {
            // Make sure all fields are ready for use
            PrepareRenderTarget();
            if (_secondaryRenderTarget == null)
            {
                using (new ProfilerEvent("Prepare secondary render target texture"))
                    _secondaryRenderTarget = new RenderTarget(Engine, _area.Size);
            }

            // Swap the render targets
            UpdateUtils.Swap(ref RenderTarget, ref _secondaryRenderTarget);
        }

        /// <summary>
        /// Copies the contents of the back-buffer to <see cref="RenderTarget"/>
        /// </summary>
        private void CopyBackBufferToTexture()
        {
            PrepareRenderTarget();

            using (new ProfilerEvent("Copy BackBuffer to texture"))
            {
                Engine.Device.StretchRectangle(
                    Engine.BackBuffer,
                    new Rectangle(_viewport.X, _viewport.Y, _viewport.Width, _viewport.Height),
                    RenderTarget.Surface, new Rectangle(0, 0, _viewport.Width, _viewport.Height),
                    TextureFilter.None);
            }
        }

        /// <summary>
        /// Applies a <see cref="PostShader"/> to the <see cref="OmegaEngine.Graphics.RenderTarget"/> and outputs the result to the back-buffer
        /// </summary>
        /// <param name="shader">The <see cref="PostShader"/> to apply</param>
        /// <param name="alpha">The level of transparency from 0 (solid) to 255 (invisible)</param>
        private void ShaderToBackBuffer(PostShader shader, int alpha)
        {
            Engine.State.AlphaBlend = alpha;
            using (new ProfilerEvent(() => "Apply " + shader))
            {
                shader.Apply(delegate
                {
                    Engine.Device.BeginScene();
                    Engine.DrawQuadShader();
                    Engine.Device.EndScene();
                }, _area.Size, shader.OverlayRendering ? null : RenderTarget);
            }
        }

        /// <summary>
        /// Updates the <see cref="OmegaEngine.Graphics.RenderTarget"/> using a <see cref="PostShader"/>
        /// </summary>
        /// <param name="shader">The <see cref="PostShader"/> to apply</param>
        protected void ShaderToRenderTarget(PostShader shader)
        {
            #region Sanity checks
            if (shader == null) throw new ArgumentNullException("shader");
            #endregion

            // Make sure input and output texture aren't the same
            var sceneMap = RenderTarget;
            if (!shader.OverlayRendering) SwapRenderTarget();

            // Apply the shader and move data from one texture to the other
            using (new ProfilerEvent(() => "Apply " + shader))
                shader.Apply(() => RenderTarget.RenderTo(Engine.DrawQuadShader), sceneMap.Size, shader.OverlayRendering ? null : sceneMap);
        }

        /// <summary>
        /// Outputs <see cref="OmegaEngine.Graphics.RenderTarget"/> to the back-buffer using a quad
        /// </summary>
        /// <param name="alpha">The level of transparency from 0 (solid) to 255 (invisible)</param>
        private void OutputRenderTarget(int alpha)
        {
            Engine.Device.Viewport = _viewport;
            Engine.Device.BeginScene();
            Engine.State.AlphaBlend = alpha;
            Engine.State.SetTexture(RenderTarget);
            Engine.DrawQuadTextured();
            Engine.Device.EndScene();
        }
        #endregion
    }
}
