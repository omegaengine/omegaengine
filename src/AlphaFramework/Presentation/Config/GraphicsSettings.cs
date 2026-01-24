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
using NanoByte.Common;
using OmegaEngine;
using OmegaEngine.Foundation.Light;

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

    private bool _anisotropic = true;

    /// <summary>
    /// Use anisotropic texture filtering
    /// </summary>
    [DefaultValue(true), Description("Use anisotropic texture filtering")]
    public bool Anisotropic { get => _anisotropic; set => value.To(ref _anisotropic, Changed); }

    private bool _normalMapping = true;

    /// <summary>
    /// Apply normal mapping effects to models when available
    /// </summary>
    [DefaultValue(true), Description("Apply normal mapping effects to models when available")]
    public bool NormalMapping { get => _normalMapping; set => value.To(ref _normalMapping, Changed); }

    private bool _postScreenEffects = true;

    /// <summary>
    /// Apply post-screen effects to the scene
    /// </summary>
    [DefaultValue(true), Description("Apply post-screen effects to the scene")]
    public bool PostScreenEffects { get => _postScreenEffects; set => value.To(ref _postScreenEffects, Changed); }

    private bool _shadows = true;

    /// <summary>
    /// Enable or disable shadowing casting (does not affect terrain self-shadowing)
    /// </summary>
    [DefaultValue(true), Description("Enable or disable shadowing casting (does not affect terrain self-shadowing)")]
    public bool Shadows { get => _shadows; set => value.To(ref _shadows, Changed); }

    private bool _doubleSampling = true;

    /// <summary>
    /// Sample textures twice with different texture coordinates for better image quality
    /// </summary>
    [DefaultValue(true), Description("Sample textures twice with different texture coordinates for better image quality")]
    public bool DoubleSampling { get => _doubleSampling; set => value.To(ref _doubleSampling, Changed); }

    private int _terrainBlockSize = 32;

    /// <summary>
    /// The size of a terrain rendering block
    /// </summary>
    [DefaultValue(32), Description("The size of a terrain rendering block")]
    public int TerrainBlockSize { get => _terrainBlockSize; set => value.To(ref _terrainBlockSize, Changed); }

    private WaterEffectsType _waterEffects = WaterEffectsType.ReflectAll;

    /// <summary>
    /// What kind of effects to display on water (e.g. reflections)
    /// </summary>
    [DefaultValue(WaterEffectsType.ReflectAll), Description("What kind of effects to display on water (e.g. reflections)")]
    public WaterEffectsType WaterEffects { get => _waterEffects; set => value.To(ref _waterEffects, Changed); }

    private bool _fading = true;

    /// <summary>
    /// Fade in game scenes from black
    /// </summary>
    [DefaultValue(true), Description("Fade in game scenes from black")]
    public bool Fading { get => _fading; set => value.To(ref _fading, Changed); }

    private float _fieldOfView = 60;

    /// <summary>
    /// The camera field of view in degrees
    /// </summary>
    [DefaultValue(60f), Description("The camera field of view in degrees")]
    public float FieldOfView { get => _fieldOfView; set => value.To(ref _fieldOfView, Changed); }

    private float _gamma = 1.0f;

    /// <summary>
    /// The gamma correction value (1 for no correction; 2.2 for sRGB)
    /// </summary>
    [Description("The gamma correction value (1 for no correction; 2.2 for sRGB)")]
    public float Gamma { get => _gamma; set => value.To(ref _gamma, Changed); }

    /// <summary>
    /// Applies the settings to the engine.
    /// </summary>
    public void ApplyTo(Engine engine)
    {
        engine.Anisotropic = Anisotropic;
        engine.Effects.NormalMapping = NormalMapping;
        engine.Effects.PostScreenEffects = PostScreenEffects;
        engine.Effects.Shadows = Shadows;
        engine.Effects.DoubleSampling = DoubleSampling;
        engine.Effects.WaterEffects = WaterEffects;

        // Read back in case any of the values were invalid or unsupported
        Anisotropic = engine.Anisotropic;
        NormalMapping = engine.Effects.NormalMapping;
        PostScreenEffects = engine.Effects.PostScreenEffects;
        Shadows = engine.Effects.Shadows;
        DoubleSampling = engine.Effects.DoubleSampling;
        WaterEffects = engine.Effects.WaterEffects;

        // Note: FieldOfView and Gamma need to be applied elsewhere
    }
}
