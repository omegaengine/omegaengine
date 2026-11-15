/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Drawing;

namespace OmegaEngine.Graphics.Shaders;

/// <summary>
/// A post-screen shader that creates an "old paper" look.
/// </summary>
public class PostSepiaShader : PostShader
{
    #region Properties
    /// <summary>
    /// The color to give the image after turning it into grayscale
    /// </summary>
    [Description("The color to give the image after turning it into grayscale")]
    public Color PaperTone { get; set => value.To(ref field, () => SetShaderParameter("LightColor", value)); } = Color.FromArgb(255, 229, 127);

    /// <summary>
    /// The color of the image stains
    /// </summary>
    [Description("The color of the image stains")]
    public Color StainTone { get; set => value.To(ref field, () => SetShaderParameter("DarkColor", value)); } = Color.FromArgb(51, 12, 0);

    /// <summary>
    /// How strongly to turn the image it into grayscale - values between 0 and 1
    /// </summary>
    [DefaultValue(0.5f), Description("How strongly to turn the image it into grayscale - values between 0 and 1")]
    public float Desaturation
    {
        get;
        set
        {
            value = value.Clamp();
            value.To(ref field, () => SetShaderParameter("Desat", value));
        }
    } = 0.5f;

    /// <summary>
    /// How strongly to apply the <see cref="PaperTone"/> color - values between 0 and 1
    /// </summary>
    [DefaultValue(1f), Description("How strongly to apply the PaperTone color - values between 0 and 1")]
    public float Toning
    {
        get;
        set
        {
            value = value.Clamp();
            value.To(ref field, () => SetShaderParameter("Toned", value));
        }
    } = 1.0f;
    #endregion

    #region Engine
    /// <inheritdoc/>
    protected override void OnEngineSet()
    {
        LoadShaderFile("Post_Sepia.fxo");

        base.OnEngineSet();
    }
    #endregion
}
