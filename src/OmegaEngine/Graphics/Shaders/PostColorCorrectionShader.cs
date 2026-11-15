/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using OmegaEngine.Foundation.Light;

namespace OmegaEngine.Graphics.Shaders;

/// <summary>
/// A post-screen shader for applying TV-like settings like brightness, contrast, hue, gamma, etc.
/// </summary>
public class PostColorCorrectionShader : PostShader
{
    #region Properties
    /// <summary>
    /// How bright the picture should be - values between 0 (black) and 5 (5x normal)
    /// </summary>
    [DefaultValue(1f), Description("How bright the picture should be - values between 0 (black) and 5 (5x normal)")]
    public float Brightness
    {
        get;
        set => value.Clamp(0, 5).To(ref field, () => SetShaderParameter("Brightness", value));
    } = 1;

    /// <summary>
    /// The contrast level of the picture - values between -5 and 5
    /// </summary>
    [DefaultValue(1f), Description("The contrast level of the picture - values between -5 and 5")]
    public float Contrast
    {
        get;
        set => value.Clamp(-5, 5).To(ref field, () => SetShaderParameter("Contrast", value));
    } = 1;

    /// <summary>
    /// The color saturation level of the picture - values between -5 and 5
    /// </summary>
    [DefaultValue(1f), Description("The color saturation level of the picture - values between -5 and 5")]
    public float Saturation
    {
        get;
        set => value.Clamp(-5, 5).To(ref field, () => SetShaderParameter("Saturation", value));
    } = 1;

    /// <summary>
    /// The color hue rotation of the picture - values between 0 and 360
    /// </summary>
    [DefaultValue(0f), Description("The color hue rotation of the picture - values between 0 and 360")]
    public float Hue
    {
        get;
        set => value.Clamp(0, 360).To(ref field, () => SetShaderParameter("Hue", value));
    }

    /// <summary>
    /// Sets multiple color correction parameters in one goe.
    /// </summary>
    public void SetParameters(ColorCorrection correction)
    {
        Brightness = correction.Brightness;
        Contrast = correction.Contrast;
        Saturation = correction.Saturation;
        Hue = correction.Hue;
    }

    /// <summary>
    /// The gamma of the display the image is shown on
    /// </summary>
    /// <remarks>
    /// The engine already gamma-encodes its output for a standard sRGB display (gamma 2.2).
    /// Other values re-target the image for a different gamma: higher values brighten the mid-tones, lower values darken them.
    /// </remarks>
    [DefaultValue(DefaultGamma), Description("The gamma of the display the image is shown on")]
    public float Gamma
    {
        get;
        set => value.To(ref field, () => SetShaderParameter("Gamma", value));
    } = DefaultGamma;

    /// <summary>
    /// The value for <see cref="Gamma"/> that leaves the image unchanged.
    /// </summary>
    public const float DefaultGamma = 2.2f;
    #endregion

    #region Engine
    protected override void OnEngineSet()
    {
        LoadShaderFile("Post_ColorCorrection.fxo");

        base.OnEngineSet();
    }
    #endregion
}
