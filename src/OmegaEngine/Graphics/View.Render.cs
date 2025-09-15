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
using System.Linq;
using SlimDX;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;

namespace OmegaEngine.Graphics;

// This file contains code for the actual rendering
partial class View
{
    #region Events
    /// <summary>
    /// Occurs immediately before the <see cref="Scene"/> begins rendering.
    /// <see cref="ChildViews"/> will have been rendered already.
    /// </summary>
    [Description("Occurs immediately before the Scene begins rendering. Child views will have been rendered already.")]
    public event Action<Camera>? PreRender;
    #endregion

    //--------------------//

    #region Render scene
    /// <summary>
    /// Renders the <see cref="Scene"/> and any <see cref="FloatingModel"/>s to the back buffer.
    /// </summary>
    private void RenderSceneToBackBuffer()
    {
        Engine.Device.BeginScene();
        RenderScene();
        Engine.Device.EndScene();
    }

    /// <summary>
    /// Renders the <see cref="Scene"/> and any <see cref="FloatingModel"/>s to <see cref="RenderTarget"/>.
    /// </summary>
    private void RenderSceneToTexture()
    {
        // No alpha-channel in BackgroundColor, using FullAlpha instead
        _backgroundColor = Color.FromArgb(255, _backgroundColor);

        // Render the scene to a texture
        PrepareRenderTarget();
        RenderTarget.RenderTo(RenderScene);

        // Immediately output simple alpha-blended views
        if (!TextureRenderTarget && _effectivePostShaders.Count == 0)
        {
            Engine.State.SetTexture(RenderTarget);
            OutputRenderTarget(FullAlpha);
        }
    }

    /// <summary>
    /// Renders the <see cref="Scene"/> and any <see cref="FloatingModel"/>s.
    /// </summary>
    private void RenderScene()
    {
        using (new ProfilerEvent("Render scene"))
        {
            RenderBackground();

            // Set up normal transfromations for bodies
            Engine.State.ViewTransform = Camera.View;
            Engine.State.ProjectionTransform = Camera.Projection;

            #region Activate lights
            if (Lighting && Scene.Lights.Count > 0)
            {
                using (new ProfilerEvent("Setup lights"))
                    Scene.ActivateLights(this);
            }
            #endregion

            #region Render bodies
            // Opaque bodies
            if (_sortedOpaqueBodies.Count > 0)
            {
                using (new ProfilerEvent("Render solid bodies"))
                    _sortedOpaqueBodies.ForEach(RenderBody);
            }

            // Terrain
            if (_sortedTerrains.Count > 0)
            {
                using (new ProfilerEvent("Render terrains"))
                    _sortedTerrains.ForEach(RenderBody);
            }

            // Water
            if (_sortedWaters.Count > 0)
            {
                using (new ProfilerEvent("Render waters"))
                { // Render back-to-front
                    for (int i = _sortedWaters.Count - 1; i >= 0; i--)
                        RenderBody(_sortedWaters[i]);
                }
            }

            // Transparent bodies
            if (_sortedTransparentBodies.Count > 0)
            {
                using (new ProfilerEvent("Render transparent bodies"))
                { // Render back-to-front
                    for (int i = _sortedTransparentBodies.Count - 1; i >= 0; i--)
                        RenderBody(_sortedTransparentBodies[i]);
                }
            }
            #endregion

            #region Deactivate lights
            if (Lighting && Scene.Lights.Count > 0)
            {
                using (new ProfilerEvent("Deactivate lights"))
                    Scene.DeactivateLights();
            }
            #endregion
        }

        #region Floating models
        if (FloatingModels.Count > 0)
        {
            using (new ProfilerEvent("Render floating models"))
            {
                // Clear ZBuffer and turn off view transformation so the models float on top of everything else
                Engine.Device.Clear(ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                Engine.State.ViewTransform = Matrix.Identity;

                foreach (FloatingModel model in FloatingModels.Where(model => model.Visible))
                {
                    // ReSharper disable once AccessToForEachVariableInClosure
                    using (new ProfilerEvent(() => $"Render {model}"))
                        model.Render(Camera);
                }
            }
        }
        #endregion
    }
    #endregion

    #region Render background
    /// <summary>
    /// Renders the back of the <see cref="Scene"/>
    /// </summary>
    protected virtual void RenderBackground()
    {
        #region Clearing
        if (_backgroundColor.A == 255 && _backgroundColor != Color.Empty)
        {
            using (new ProfilerEvent("Clear ZBuffer and BackBuffer"))
                Engine.Device.Clear(ClearFlags.ZBuffer | ClearFlags.Target, Color.FromArgb(0, BackgroundColor), 1.0f, 0);
        }
        else
        {
            using (new ProfilerEvent("Clear ZBuffer"))
                Engine.Device.Clear(ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            if (_backgroundColor.A != 255)
            {
                using (new ProfilerEvent("Taint background"))
                {
                    Engine.State.ZBufferMode = ZBufferMode.Off;

                    // Draw alpha blended quad for transparent background color
                    Engine.State.AlphaBlend = 255 - BackgroundColor.A;
                    Engine.DrawQuadColored(BackgroundColor);

                    Engine.State.ZBufferMode = ZBufferMode.Normal;
                }
            }
        }
        #endregion

        #region Skybox
        // Don't render Skybox in shadow/glow maps
        if (Scene.Skybox is { Visible: true } && !Fog)
        {
            // Render first, before all other entities using simplified transformations
            using (new ProfilerEvent(() => $"Render {Scene.Skybox}"))
            {
                // Render Skybox with no ZBuffer and no positioning information
                Engine.State.ZBufferMode = ZBufferMode.Off;
                Engine.State.ViewTransform = Camera.SimpleView;
                Engine.State.ProjectionTransform = Camera.SimpleProjection;
                Scene.Skybox.Render(Camera);
                Engine.State.ZBufferMode = ZBufferMode.Normal;
            }
        }
        #endregion
    }
    #endregion

    #region Render body
    /// <summary>
    /// Renders a <see cref="PositionableRenderable"/> from the <see cref="Scene"/>
    /// </summary>
    /// <param name="body">The <see cref="PositionableRenderable"/> to render</param>
    protected virtual void RenderBody(PositionableRenderable body)
    {
        #region Sanity checks
        if (body == null) throw new ArgumentNullException(nameof(body));
        #endregion

        // Apply the camera offset to make sure positioning is right
        ApplyCameraBase(body);

        using (new ProfilerEvent(() => $"Render {body}"))
            body.Render(Camera, Scene.GetEffectiveLights);
    }
    #endregion

    //--------------------//

    /// <summary>
    /// Renders the view
    /// </summary>
    internal virtual void Render()
    {
        #region Sanity checks
        if (IsDisposed) throw new ObjectDisposedException(ToString());
        #endregion

        // Culling-debug information
        RenderedInLastFrame = true;
        Engine.QueueReset(this);

        #region Camera
        // If a cinematic camera has stopped moving, replace it with the final target
        if (_cinematicCamera != null && _targetCamera != null && !_cinematicCamera.Moving)
        {
            Camera = _targetCamera;
            _cinematicCamera = null;
            _targetCamera = null;
        }

        // Update transformation matrices
        Camera.Size = new(_viewport.Width, _viewport.Height);
        #endregion

        using (new ProfilerEvent(() => $"Sort bodies for {this}"))
            SortBodies();

        #region Child views
        if (_childViews.Count > 0)
        {
            using (new ProfilerEvent(() => $"Child views of {this}"))
                HandleChildViews();
        }
        #endregion

        using (new ProfilerEvent(() => $"Render {this}"))
        {
            PreRender?.Invoke(Camera);

            Engine.Device.Viewport = _viewport;
            Engine.State.CullMode = InvertCull ? Cull.Clockwise : Cull.Counterclockwise;

            // Setup fog
            if (Fog)
            {
                Engine.State.Fog = true;
                Engine.State.FogColor = _backgroundColor;
                Engine.State.FogStart = Camera.NearClip;
                Engine.State.FogEnd = Camera.FarClip;
            }
            else Engine.State.Fog = false;

            // Activate render-to-texture for external render targets and alpha blending
            bool sceneOnBackBuffer = !TextureRenderTarget && FullAlpha == 0;

            // Render scene
            if (sceneOnBackBuffer) RenderSceneToBackBuffer();
            else RenderSceneToTexture();

            // Post-screen shaders
            if (Engine.Effects.PostScreenEffects && PostShaders.Count > 0)
            {
                using (new ProfilerEvent("Applying post-screen shaders"))
                    ApplyPostShaders(sceneOnBackBuffer);
            }
        }
    }
}
