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
using NanoByte.Common;

namespace OmegaEngine.Graphics.Shaders;

/// <summary>
///A post-screen shader that adds a bloom-like glow-effect around objects in the scene.
/// </summary>
public class PostGlowShader : PostBlurShader
{
    #region Variables
    private readonly TextureView _glowView;
    #endregion

    #region Properties
    /// <summary>
    /// The minimum shader model version required to use this shader
    /// </summary>
    public new static Version MinShaderModel => new(2, 0);

    /// <summary>
    /// Does this post-screen shader use overlay rendering instead of a scene map?
    /// </summary>
    [Description("Does this post-screen shader use overlay rendering instead of a scene map?")]
    public sealed override bool OverlayRendering => true;

    private float _glowStrength = 1;

    /// <summary>
    /// A factor by which the blurred glow color is multiplied - values between 0 and 100
    /// </summary>
    [DefaultValue(1f), Description("A factor by which the blurred glow color is multiplied - values between 0 and 100")]
    public float GlowStrength
    {
        get => _glowStrength;
        set
        {
            value = value.Clamp(0, 100);
            value.To(ref _glowStrength, () => SetShaderParameter("GlowStrength", value));
        }
    }
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new instance of the shader
    /// </summary>
    /// <param name="glowView">A render target storing the glow map of the current view</param>
    /// <exception cref="NotSupportedException">The graphics card does not support this shader.</exception>
    public PostGlowShader(TextureView glowView)
    {
        _glowView = glowView ?? throw new ArgumentNullException(nameof(glowView));
    }
    #endregion

    #region Passes
    /// <summary>
    /// Runs the actual shader passes
    /// </summary>
    /// <param name="render">The render delegate (is called once for every shader pass)</param>
    /// <param name="sceneSize">The size of the scene on the screen - leave empty for fullscreen</param>
    /// <param name="sceneMap">Should be <c>null</c> because a glow map is used instead</param>
    protected override void RunPasses(Action render, Size sceneSize, RenderTarget sceneMap)
    {
        // Pass the glow map instead of the scene map to the blurring filter
        base.RunPasses(render, sceneSize, _glowView.GetRenderTarget());
    }
    #endregion

    //--------------------//

    #region Engine
    /// <inheritdoc/>
    protected override void OnEngineSet()
    {
        base.OnEngineSet();

        // Output is layered on top of existing scene via additive alpha-blending
        Effect.Technique = "Glow";
    }
    #endregion
}
