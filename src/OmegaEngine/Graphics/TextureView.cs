/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using OmegaEngine.Graphics.Cameras;

namespace OmegaEngine.Graphics;

/// <summary>
/// A special kind of <see cref="View"/> that directs its output into a texture <see cref="RenderTarget"/> instead of printing straight to the screen.
/// </summary>
/// <param name="scene">The <see cref="Scene"/> to render</param>
/// <param name="camera">The <see cref="Camera"/> to look at the <see cref="Scene"/> with</param>
/// <param name="size">The size of screen area this view should fill (leave empty for fullscreen)</param>
/// <remarks>These <see cref="View"/>s usually provide helper data (e.g. reflections) for the main <see cref="View"/>s.
/// They are then referenced in <see cref="View.ChildViews"/>.</remarks>
public abstract class TextureView(Scene scene, Camera camera, Size size) : View(scene, camera, new(new(), size))
{
    /// <summary>
    /// Not applicable to <see cref="TextureView"/>
    /// </summary>
    [Browsable(false)]
    public sealed override int FullAlpha { get => 0; set { } }

    /// <summary>
    /// Does this <see cref="View"/> render to a texture <see cref="RenderTarget"/>? <c>true</c> since this is a <see cref="TextureView"/>.
    /// </summary>
    protected override bool TextureRenderTarget => true;

    /// <summary>
    /// The texture this view renders to. May change, do not store externally!
    /// </summary>
    public RenderTarget GetRenderTarget()
    {
        PrepareRenderTarget();
        return RenderTarget;
    }

    /// <inheritdoc/>
    protected override void ApplyPostShaders(bool sceneOnBackBuffer)
    {
        // Note: Doesn't call base methods

        Debug.Assert(!sceneOnBackBuffer, "Scene should never be on back-buffer with external render target");

        foreach (var shader in PostShaders)
        {
            // Update the RenderTarget texture to incorporate the PostShader effect
            if (shader.Enabled) ShaderToRenderTarget(shader);
        }
    }

    /// <inheritdoc/>
    protected override void OnEngineSet()
    {
        base.OnEngineSet();
        PrepareRenderTarget();
    }
}
