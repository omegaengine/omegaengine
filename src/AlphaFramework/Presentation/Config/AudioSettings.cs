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

namespace AlphaFramework.Presentation.Config;

/// <summary>
/// Stores audio settings.
/// </summary>
/// <seealso cref="SettingsBase.Audio"/>
public sealed class AudioSettings
{
    /// <summary>
    /// Occurs when a setting in this group is changed.
    /// </summary>
    [Description("Occurs when a setting in this group is changed.")]
    public event Action Changed = () => {};

    private float _musicVolume = 1f;

    /// <summary>
    /// The volume of background music (0 = silent, 1 = normal)
    /// </summary>
    [DefaultValue(1f), Description("The volume of background music (0 = silent, 1 = normal)")]
    public float MusicVolume { get => _musicVolume; set => value.To(ref _musicVolume, Changed); }

    private float _soundVolume = 1f;

    /// <summary>
    /// The volume of sound effects (0 = silent, 1 = normal)
    /// </summary>
    [DefaultValue(1f), Description("The volume of sound effects (0 = silent, 1 = normal)")]
    public float SoundVolume { get => _soundVolume; set => value.To(ref _soundVolume, Changed); }

    /// <summary>
    /// Applies the settings to the engine.
    /// </summary>
    public void ApplyTo(Engine engine)
    {
        engine.Audio.MusicVolume = MusicVolume;
        engine.Audio.SoundVolume = SoundVolume;
    }
}
