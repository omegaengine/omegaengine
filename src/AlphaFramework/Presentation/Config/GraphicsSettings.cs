/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.ComponentModel;
using OmegaEngine;
using OmegaEngine.Foundation.Light;
using OmegaEngine.Graphics.Shaders;

namespace AlphaFramework.Presentation.Config;

/// <summary>
/// Stores graphics settings (effect details, etc.). Changes here don't require the engine to be reset.
/// </summary>
/// <seealso cref="SettingsBase.Graphics"/>
public sealed class GraphicsSettings
{
    /// <summary>
    /// Occurs when a setting in this group is changed.
    /// </summary>
    [Description("Occurs when a setting in this group is changed.")]
    public event Action Changed = () => {};

    /// <summary>
    /// Use anisotropic texture filtering
    /// </summary>
    [DefaultValue(true), Description("Use anisotropic texture filtering")]
    public bool Anisotropic { get; set => value.To(ref field, Changed); } = true;

    /// <summary>
    /// Apply normal mapping effects to models when available
    /// </summary>
    [DefaultValue(true), Description("Apply normal mapping effects to models when available")]
    public bool NormalMapping { get; set => value.To(ref field, Changed); } = true;

    /// <summary>
    /// Apply post-screen effects to the scene
    /// </summary>
    [DefaultValue(true), Description("Apply post-screen effects to the scene")]
    public bool PostScreenEffects { get; set => value.To(ref field, Changed); } = true;

    /// <summary>
    /// Enable or disable shadowing casting (does not affect terrain self-shadowing)
    /// </summary>
    [DefaultValue(true), Description("Enable or disable shadowing casting (does not affect terrain self-shadowing)")]
    public bool Shadows { get; set => value.To(ref field, Changed); } = true;

    /// <summary>
    /// Sample textures twice with different texture coordinates to create an illusion of more details
    /// </summary>
    [DefaultValue(true), Description("Sample textures twice with different texture coordinates to create an illusion of more details")]
    public bool DetailMapping { get; set => value.To(ref field, Changed); } = true;

    /// <summary>
    /// The size of a terrain rendering block
    /// </summary>
    [DefaultValue(32), Description("The size of a terrain rendering block")]
    public int TerrainBlockSize { get; set => value.To(ref field, Changed); } = 32;

    /// <summary>
    /// What kind of effects to display on water (e.g. reflections)
    /// </summary>
    [DefaultValue(WaterEffectsType.ReflectAll), Description("What kind of effects to display on water (e.g. reflections)")]
    public WaterEffectsType WaterEffects { get; set => value.To(ref field, Changed); } = WaterEffectsType.ReflectAll;

    /// <summary>
    /// The camera field of view in degrees
    /// </summary>
    [DefaultValue(60f), Description("The camera field of view in degrees")]
    public float FieldOfView { get; set => value.To(ref field, Changed); } = 60;

    /// <summary>
    /// The gamma of the display the image is shown on
    /// </summary>
    [DefaultValue(PostColorCorrectionShader.DefaultGamma), Description("The gamma of the display the image is shown on")]
    public float Gamma { get; set => value.To(ref field, Changed); } = PostColorCorrectionShader.DefaultGamma;

    /// <summary>
    /// Applies the settings to the engine.
    /// </summary>
    public void ApplyTo(Engine engine)
    {
        engine.Anisotropic = Anisotropic;
        engine.Effects.NormalMapping = NormalMapping;
        engine.Effects.PostScreenEffects = PostScreenEffects;
        engine.Effects.Shadows = Shadows;
        engine.Effects.DetailMapping = DetailMapping;
        engine.Effects.WaterEffects = WaterEffects;

        // Read back in case any of the values were invalid or unsupported
        Anisotropic = engine.Anisotropic;
        NormalMapping = engine.Effects.NormalMapping;
        PostScreenEffects = engine.Effects.PostScreenEffects;
        Shadows = engine.Effects.Shadows;
        DetailMapping = engine.Effects.DetailMapping;
        WaterEffects = engine.Effects.WaterEffects;

        // Note: FieldOfView and Gamma need to be applied elsewhere
    }
}
