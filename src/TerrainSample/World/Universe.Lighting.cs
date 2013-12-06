/*
 * Copyright 2006-2013 Bastian Eicher
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
using Common.Utils;
using Common.Values;
using Common.Values.Design;
using LuaInterface;

namespace World
{
    partial class Universe<TCoordinates>
    {
        #region Events
        /// <summary>
        /// Occurs when <see cref="LightPhase"/>, <see cref="AmbientColor"/>, <see cref="SunColor"/>,
        /// <see cref="ColorCorrectionPhase0"/>, <see cref="ColorCorrectionPhase1"/>, <see cref="ColorCorrectionPhase2"/> or <see cref="ColorCorrectionPhase3"/> was changed.
        /// </summary>
        [Description("Occurs when LightPhase, AmbientColor, SunColor, ColorCorrectionPhase0, ColorCorrectionPhase1, ColorCorrectionPhase2 or ColorCorrectionPhase3 was changed.")]
        public event Action LightingChanged;

        private void OnLightingChanged()
        {
            if (LightingChanged != null) LightingChanged();
        }

        /// <summary>
        /// Occurs when <see cref="Fog"/>, <see cref="FogDistance"/> or <see cref="FogColor"/> was changed.
        /// </summary>
        [Description("Occurs when Fog was changed.")]
        public event Action FogChanged;

        private void OnFogChanged()
        {
            if (FogChanged != null) FogChanged();
        }

        /// <summary>
        /// Occurs when <see cref="Bleach"/> was changed.
        /// </summary>
        [Description("Occurs when Bleach was changed.")]
        public event Action BleachChanged;

        private void OnBleachChanged()
        {
            if (BleachChanged != null) BleachChanged();
        }
        #endregion

        #region Lighting
        private Color _ambientColor = Color.FromArgb(60, 60, 60);

        /// <summary>
        /// The color of the ambient light (background light that is always visible and has no direction).
        /// </summary>
        /// <remarks>Is not serialized/stored, <see cref="AmbientColorValue"/> is used for that.</remarks>
        [XmlIgnore, Category("Lighting"), Description("The color of the ambient light (background light that is always visible and has no direction).")]
        public Color AmbientColor { get { return _ambientColor; } set { Color.FromArgb(255, value).To(ref _ambientColor, OnLightingChanged); /* Drop alpha-channel */ } }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="AmbientColor"/>
        [XmlElement("AmbientColor"), LuaHide, Browsable(false)]
        public XColor AmbientColorValue { get { return AmbientColor; } set { AmbientColor = Color.FromArgb(value.R, value.G, value.B); } }

        private Color _sunColor = Color.FromArgb(250, 250, 200);

        /// <summary>
        /// The color of the diffuse light (normal directional light) of the sun.
        /// </summary>
        /// <remarks>Is not serialized/stored, <see cref="SunColorValue"/> is used for that.</remarks>
        [XmlIgnore, Category("Lighting"), Description("The color of the diffuse light (normal directional light) of the sun.")]
        public Color SunColor { get { return _sunColor; } set { Color.FromArgb(255, value).To(ref _sunColor, OnLightingChanged); /* Drop alpha-channel */ } }

        private float _sunInclination = 20;

        /// <summary>
        /// The angle of inclination of the sun's path away from the zenith in degrees.
        /// </summary>
        [DefaultValue(20f), Category("Lighting"), Description("The angle of inclination of the sun's path away from the zenith towards south in degrees.")]
        [EditorAttribute(typeof(AngleEditor), typeof(UITypeEditor))]
        public float SunInclination { get { return _sunInclination; } set { value.To(ref _sunInclination, OnLightingChanged); } }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="SunColor"/>
        [XmlElement("SunColor"), LuaHide, Browsable(false)]
        public XColor SunColorValue { get { return SunColor; } set { SunColor = Color.FromArgb(value.R, value.G, value.B); /* Drop alpha-value */ } }

        private Color _moonColor = Color.FromArgb(140, 140, 190);

        /// <summary>
        /// The color of the diffuse light (normal directional light) of the second moon.
        /// </summary>
        /// <remarks>Is not serialized/stored, <see cref="MoonColorValue"/> is used for that.</remarks>
        [XmlIgnore, Category("Lighting"), Description("The color of the diffuse light (normal directional light) of the second moon.")]
        public Color MoonColor { get { return _moonColor; } set { Color.FromArgb(255, value).To(ref _moonColor, OnLightingChanged); /* Drop alpha-channel */ } }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="MoonColor"/>
        [XmlElement("MoonColor"), LuaHide, Browsable(false)]
        public XColor MoonColorValue { get { return MoonColor; } set { MoonColor = Color.FromArgb(value.R, value.G, value.B); } }

        private float _moonInclination = 340;

        /// <summary>
        /// The angle of inclination of the second moon's path away from the zenith in degrees.
        /// </summary>
        [DefaultValue(340f), Category("Lighting"), Description("The angle of inclination of the second moon's path away from the zenith towards south in degrees.")]
        [EditorAttribute(typeof(AngleEditor), typeof(UITypeEditor))]
        public float MoonInclination { get { return _moonInclination; } set { value.To(ref _moonInclination, OnLightingChanged); } }

        private float _lightPhase;

        /// <summary>
        /// A value between 0 and 4 representing the current sun and moon positions.
        /// </summary>
        /// <remarks>
        /// 0 = night<br/>
        /// 1 = dawn<br/>
        /// 2 = noon<br/>
        /// 3 = twilight<br/>
        /// </remarks>
        [FloatRange(0f, 4f), DefaultValue(0f), Category("Lighting"), Description("A value between 0 and 4 representing the current sun and moon positions.")]
        [EditorAttribute(typeof(SliderEditor), typeof(UITypeEditor))]
        public float LightPhase { get { return _lightPhase; } set { (value % 4).To(ref _lightPhase, OnLightingChanged); } }

        /// <summary>
        /// The speed with which the <see cref="LightPhase"/> is incremented.
        /// </summary>
        [DefaultValue(1f), Category("Lighting"), Description("The speed with which the light phase is incremented.")]
        public float LightPhaseSpeedFactor { get; set; }
        #endregion

        #region Color correction
        private ColorCorrection _colorCorrectionPhase0 = ColorCorrection.Default;

        /// <summary>
        /// Color correction values to apply in light phase 0 or 4 (night).
        /// </summary>
        [Category("Color correction"), Description("Color correction values to apply in light phase 0 or 4 (night).")]
        public ColorCorrection ColorCorrectionPhase0 { get { return _colorCorrectionPhase0; } set { value.To(ref _colorCorrectionPhase0, OnLightingChanged); } }

        private ColorCorrection _colorCorrectionPhase1 = ColorCorrection.Default;

        /// <summary>
        /// Color correction values to apply in light phase 1 or 5 (dawn).
        /// </summary>
        [Category("Color correction"), Description("Color correction values to apply in light phase 1 or 5 (dawn).")]
        public ColorCorrection ColorCorrectionPhase1 { get { return _colorCorrectionPhase1; } set { value.To(ref _colorCorrectionPhase1, OnLightingChanged); } }

        private ColorCorrection _colorCorrectionPhase2 = ColorCorrection.Default;

        /// <summary>
        /// Color correction values to apply in light phase 2 or 6 (noon).
        /// </summary>
        [Category("Color correction"), Description("Color correction values to apply in light phase 2 or 6 (noon).")]
        public ColorCorrection ColorCorrectionPhase2 { get { return _colorCorrectionPhase2; } set { value.To(ref _colorCorrectionPhase2, OnLightingChanged); } }

        private ColorCorrection _colorCorrectionPhase3 = ColorCorrection.Default;

        /// <summary>
        /// Color correction values to apply in light phase 3 or 7 (twilight).
        /// </summary>
        [Category("Color correction"), Description("Color correction values to apply in light phase  3 or 7 (twilight).")]
        public ColorCorrection ColorCorrectionPhase3 { get { return _colorCorrectionPhase3; } set { value.To(ref _colorCorrectionPhase3, OnLightingChanged); } }
        #endregion

        #region Effects
        private bool _fog;

        /// <summary>
        /// Is the fog active?
        /// </summary>
        [DefaultValue(false), Category("Effects"), Description("Is the fog active?")]
        public bool Fog { get { return _fog; } set { value.To(ref _fog, OnFogChanged); } }

        private float _fogDistance = 5000;

        /// <summary>
        /// The maximum distance one can look through the fog.
        /// </summary>
        [DefaultValue(5000f), Category("Effects"), Description("The maximum distance one can look through the fog.")]
        public float FogDistance { get { return _fogDistance; } set { value.To(ref _fogDistance, OnFogChanged); } }

        private Color _fogColor = Color.Gray;

        /// <summary>
        /// The color of the fog.
        /// </summary>
        /// <remarks>Is not serialized/stored, <see cref="FogColorValue"/> is used for that.</remarks>
        [XmlIgnore, DefaultValue(typeof(Color), "Gray"), Category("Effects"), Description("The color of the fog.")]
        public Color FogColor { get { return _fogColor; } set { value.To(ref _fogColor, FogChanged); } }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="FogColor"/>
        [XmlElement("FogColor"), LuaHide, Browsable(false)]
        public XColor FogColorValue { get { return FogColor; } set { FogColor = Color.FromArgb(value.R, value.G, value.B); } }

        private bool _bleach;

        /// <summary>
        /// Is the fog active?
        /// </summary>
        [DefaultValue(false), Category("Effects"), Description("Is the bleach effect active?")]
        public bool Bleach { get { return _bleach; } set { value.To(ref _bleach, OnBleachChanged); } }
        #endregion
    }
}
