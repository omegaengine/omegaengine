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
using System.Drawing;
using System.Drawing.Design;
using System.Xml.Serialization;
using LuaInterface;
using NanoByte.Common;
using OmegaEngine.Foundation.Design;
using OmegaEngine.Foundation.Light;

namespace FrameOfReference.World;

partial class Universe
{
    #region Events
    /// <summary>
    /// Occurs when <see cref="LightPhase"/>, <see cref="AmbientColor"/>, <see cref="SunColor"/>,
    /// <see cref="ColorCorrectionDawn"/>, <see cref="ColorCorrectionNoon"/> or <see cref="ColorCorrectionDusk"/>, <see cref="ColorCorrectionMidnight"/> was changed.
    /// </summary>
    [Description("Occurs when LightPhase, AmbientColor, SunColor or ColorCorrectionPhase* was changed.")]
    public event Action? LightingChanged;

    private void OnLightingChanged() => LightingChanged?.Invoke();
    #endregion

    #region Light phase
    private float _lightPhase;

    /// <summary>
    /// A value between 0 and 4 representing the current sun and moon positions. (0 = dawn, 1 = noon, 2 = dusk, 3 = midnight)
    /// </summary>
    [FloatRange(0f, 4f), Category("Lighting"), Description("A value between 0 and 4 representing the current sun and moon positions. (0 = dawn, 1 = noon, 2 = dusk, 3 = midnight)")]
    [Editor(typeof(SliderEditor), typeof(UITypeEditor))]
    public float LightPhase { get => _lightPhase; set => (value.Modulo(4)).To(ref _lightPhase, OnLightingChanged); }

    /// <summary>
    /// The speed with which the <see cref="LightPhase"/> is incremented.
    /// </summary>
    [DefaultValue(1 / 40f), Category("Lighting"), Description("The speed with which the light phase is incremented.")]
    public float LightPhaseSpeedFactor { get; set; } = 1 / 40f;
    #endregion

    #region Light sources
    private Color _ambientColor = Color.FromArgb(40, 40, 40);

    /// <summary>
    /// The color of the ambient light (background light that is always visible and has no direction).
    /// </summary>
    /// <remarks>Is not serialized/stored, <see cref="AmbientColorValue"/> is used for that.</remarks>
    [XmlIgnore, Category("Lighting"), Description("The color of the ambient light (background light that is always visible and has no direction).")]
    public Color AmbientColor { get => _ambientColor; set => value.DropAlpha().To(ref _ambientColor, OnLightingChanged); }

    /// <summary>Used for XML serialization.</summary>
    /// <seealso cref="AmbientColor"/>
    [XmlElement("AmbientColor"), LuaHide, Browsable(false)]
    public XColor AmbientColorValue { get => AmbientColor; set => AmbientColor = value.DropAlpha(); }

    private Color _sunColor = Color.FromArgb(180, 180, 180);

    /// <summary>
    /// The color of the diffuse light (normal directional light) of the sun.
    /// </summary>
    /// <remarks>Is not serialized/stored, <see cref="SunColorValue"/> is used for that.</remarks>
    [XmlIgnore, Category("Lighting"), Description("The color of the diffuse light (normal directional light) of the sun.")]
    public Color SunColor { get => _sunColor; set => value.DropAlpha().To(ref _sunColor, OnLightingChanged); }

    /// <summary>Used for XML serialization.</summary>
    /// <seealso cref="SunColor"/>
    [XmlElement("SunColor"), LuaHide, Browsable(false)]
    public XColor SunColorValue { get => SunColor; set => SunColor = value.DropAlpha(); }

    private float _sunInclination = 70;

    /// <summary>
    /// The angle of inclination of the sun's path away from the horizon towards south in degrees.
    /// </summary>
    [DefaultValue(70f), Category("Lighting"), Description("The angle of inclination of the sun's path away from the horizon towards south in degrees.")]
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
    public float SunInclination
    {
        get => _sunInclination;
        set =>
            value.To(ref _sunInclination, () =>
            {
                OnLightingChanged();
                if (Terrain != null) Terrain.OcclusionIntervalMapOutdated = true;
            });
    }

    private Color _moonColor = Color.FromArgb(110, 110, 160);

    /// <summary>
    /// The color of the diffuse light (normal directional light) of the second moon.
    /// </summary>
    /// <remarks>Is not serialized/stored, <see cref="MoonColorValue"/> is used for that.</remarks>
    [XmlIgnore, Category("Lighting"), Description("The color of the diffuse light (normal directional light) of the second moon.")]
    public Color MoonColor { get => _moonColor; set => value.DropAlpha().To(ref _moonColor, OnLightingChanged); }

    /// <summary>Used for XML serialization.</summary>
    /// <seealso cref="MoonColor"/>
    [XmlElement("MoonColor"), LuaHide, Browsable(false)]
    public XColor MoonColorValue { get => MoonColor; set => MoonColor = value.DropAlpha(); }

    private float _moonInclination = 70;

    /// <summary>
    /// The angle of inclination of the second moon's path away from the horizon towards south in degrees.
    /// </summary>
    [Category("Lighting"), Description("The angle of inclination of the second moon's path away from the horizon towards south in degrees.")]
    [DefaultValue(70f), Editor(typeof(AngleEditor), typeof(UITypeEditor))]
    public float MoonInclination
    {
        get => _moonInclination;
        set =>
            value.To(ref _moonInclination, () =>
            {
                OnLightingChanged();
                if (Terrain != null) Terrain.OcclusionIntervalMapOutdated = true;
            });
    }
    #endregion

    #region Color correction
    private ColorCorrection _colorCorrectionDawn = new(brightness: 1.4f, saturation: 0.7f);

    /// <summary>
    /// Color correction values to apply at dawn.
    /// </summary>
    [Category("Lighting"), Description("Color correction values to apply in light phase 1 or 5 (dawn).")]
    public ColorCorrection ColorCorrectionDawn { get => _colorCorrectionDawn; set => value.To(ref _colorCorrectionDawn, OnLightingChanged); }

    private ColorCorrection _colorCorrectionNoon = ColorCorrection.Default;

    /// <summary>
    /// Color correction values to apply at noon.
    /// </summary>
    [Category("Lighting"), Description("Color correction values to apply in light phase 2 or 6 (noon).")]
    public ColorCorrection ColorCorrectionNoon { get => _colorCorrectionNoon; set => value.To(ref _colorCorrectionNoon, OnLightingChanged); }

    private ColorCorrection _colorCorrectionDusk = new(brightness: 1.4f, contrast: 1.4f, saturation: 0.7f);

    /// <summary>
    /// Color correction values to apply at dusk.
    /// </summary>
    [Category("Lighting"), Description("Color correction values to apply in light phase  3 or 7 (twilight).")]
    public ColorCorrection ColorCorrectionDusk { get => _colorCorrectionDusk; set => value.To(ref _colorCorrectionDusk, OnLightingChanged); }

    private ColorCorrection _colorCorrectionMidnight = new(brightness: 2, saturation: 0.5f);

    /// <summary>
    /// Color correction values to apply at midnight.
    /// </summary>
    [Category("Lighting"), Description("Color correction values to apply in light phase 0 or 4 (night).")]
    public ColorCorrection ColorCorrectionMidnight { get => _colorCorrectionMidnight; set => value.To(ref _colorCorrectionMidnight, OnLightingChanged); }
    #endregion

    #region Effects
    private bool _fog;

    /// <summary>
    /// Is the fog active?
    /// </summary>
    [DefaultValue(false), Category("Effects"), Description("Is the fog active?")]
    public bool Fog { get => _fog; set => value.To(ref _fog, OnLightingChanged); }

    private float _fogDistance = 5000;

    /// <summary>
    /// The maximum distance one can look through the fog.
    /// </summary>
    [DefaultValue(5000f), Category("Effects"), Description("The maximum distance one can look through the fog.")]
    public float FogDistance { get => _fogDistance; set => value.To(ref _fogDistance, OnLightingChanged); }

    private Color _fogColor = Color.Gray;

    /// <summary>
    /// The color of the fog.
    /// </summary>
    /// <remarks>Is not serialized/stored, <see cref="FogColorValue"/> is used for that.</remarks>
    [XmlIgnore, DefaultValue(typeof(Color), "Gray"), Category("Effects"), Description("The color of the fog.")]
    public Color FogColor { get => _fogColor; set => value.To(ref _fogColor, OnLightingChanged); }

    /// <summary>Used for XML serialization.</summary>
    /// <seealso cref="FogColor"/>
    [XmlElement("FogColor"), LuaHide, Browsable(false)]
    public XColor FogColorValue { get => FogColor; set => FogColor = Color.FromArgb(value.R, value.G, value.B); }

    private bool _bleach;

    /// <summary>
    /// Is the fog active?
    /// </summary>
    [DefaultValue(false), Category("Effects"), Description("Is the bleach effect active?")]
    public bool Bleach { get => _bleach; set => value.To(ref _bleach, OnLightingChanged); }
    #endregion
}
