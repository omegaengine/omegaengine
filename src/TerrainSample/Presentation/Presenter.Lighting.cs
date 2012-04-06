/*
 * Copyright 2006-2012 Bastian Eicher
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
using System.Drawing;
using Common.Utils;
using Common.Values;
using OmegaEngine.Graphics;
using SlimDX;

namespace Presentation
{
    partial class Presenter
    {
        #region Lighting
        private readonly DirectionalLight
            _light1 = new DirectionalLight {Name = "Sun", Enabled = false},
            _light2 = new DirectionalLight {Name = "Moon", Enabled = false};

        /// <summary>
        /// The ratio between the strength of diffuse and specular lighting
        /// </summary>
        private const float DiffuseToSpecularRatio = 0.5f;

        /// <summary>
        /// Updates <see cref="_light1"/> and <see cref="_light2"/> based on the light phase in <see cref="Universe"/>.
        /// </summary>
        protected void UpdateLighting()
        {
            #region Sanity checks
            if (Disposed) throw new ObjectDisposedException(ToString());
            #endregion

            float sunPhase = Universe.LightPhase;
            float moonPhase = ((Universe.LightPhase + 2.25f) % 4);

            // Calculate lighting directions
            _light1.Direction = GetDirection(0.5f * Math.PI * (1 - sunPhase), MathUtils.DegreeToRadian(Universe.SunInclination));
            _light2.Direction = GetDirection(0.5f * Math.PI * (1 - moonPhase), MathUtils.DegreeToRadian(Universe.MoonInclination));

            UpdateLightingColorSun(sunPhase);
            UpdateLightingColorMoon(moonPhase);

            if (_colorCorrectionShader != null) UpdateColorCorrection();
        }

        /// <summary>
        /// Creates a <see cref="Vector3"/> with a <see cref="Vector3.Length"/> of 1 pointing in a specific direction.
        /// </summary>
        /// <param name="vertAngle">The vertical angle in radians.</param>
        /// <param name="inclination">The inclination angle in radians.</param>
        /// <returns>The newly created <see cref="Vector3"/>.</returns>
        private static Vector3 GetDirection(double vertAngle, double inclination)
        {
            return new Vector3(
                (float)(Math.Cos(vertAngle) * Math.Cos(inclination)),
                (float)(Math.Sin(vertAngle) * Math.Cos(inclination)),
                (float)Math.Sin(inclination));
        }

        /// <summary>
        /// Updates the color of <see cref="_light1"/>.
        /// </summary>
        private void UpdateLightingColorSun(float sunPhase)
        {
            _light1.Enabled = true; // Always on for ambient light

            if (sunPhase < 0.9 || sunPhase >= 3.1)
            { // Night
                _light1.Diffuse = Color.Black;
            }
            else if (sunPhase >= 0.9 && sunPhase < 1.4) // Start fading in just before the horizon
            { // Dawn
                _light1.Diffuse = ColorUtils.Interpolate((sunPhase - 0.9f) * 2, Color.Black, Universe.SunColor);
            }
            else if (sunPhase >= 1.4 && sunPhase < 2.6)
            { // Noon
                _light1.Diffuse = Universe.SunColor;
            }
            else if (sunPhase >= 2.6 && sunPhase < 3.1) // Fade out until just after the horizon
            { // Twilight
                _light1.Diffuse = ColorUtils.Interpolate((sunPhase - 2.6f) * 2, Universe.SunColor, Color.Black);
            }
            else throw new InvalidOperationException("Invalid sun phase!");

            _light1.Specular = ColorUtils.Multiply(_light1.Diffuse, DiffuseToSpecularRatio);
            _light1.Ambient = Universe.AmbientColor;
        }

        /// <summary>
        /// Updates the color of <see cref="_light2"/>.
        /// </summary>
        private void UpdateLightingColorMoon(float moonPhase)
        {
            if (moonPhase < 0.9 || moonPhase >= 3.1)
            { // No moon
                _light2.Enabled = false;
            }
            else if (moonPhase >= 0.9 && moonPhase < 1.4) // Start fading in just before the horizon
            { // Rising moon
                _light2.Enabled = true;
                _light2.Diffuse = ColorUtils.Interpolate((moonPhase - 0.9f) * 2, Color.Black, Universe.MoonColor);
            }
            else if (moonPhase >= 1.4 && moonPhase < 2.6)
            { // Full moon
                _light2.Enabled = true;
                _light2.Diffuse = Universe.MoonColor;
            }
            else if (moonPhase >= 2.6 && moonPhase < 3.1) // Fade out until just after the horizon
            { // Setting moon
                _light2.Enabled = true;
                _light2.Diffuse = ColorUtils.Interpolate((moonPhase - 2.6f) * 2, Universe.MoonColor, Color.Black);
            }
            else throw new InvalidOperationException("Invalid moon phase!");

            _light2.Specular = ColorUtils.Multiply(_light2.Diffuse, DiffuseToSpecularRatio);
            _light2.Ambient = Color.Black;
        }

        /// <summary>
        /// Updates the <see cref="_colorCorrectionShader"/> <see cref="ColorCorrection"/> values.
        /// </summary>
        private void UpdateColorCorrection()
        {
            // Interpolate between the color correction settings for the different sun phases
            var correction = ColorCorrection.SinusInterpolate(Universe.LightPhase,
                Universe.ColorCorrectionPhase0, Universe.ColorCorrectionPhase1, Universe.ColorCorrectionPhase2, Universe.ColorCorrectionPhase3, Universe.ColorCorrectionPhase0);

            // If the color correction values aren't all at default, activate the shader and transfer the values
            _colorCorrectionShader.Enabled = (correction != ColorCorrection.Default);
            _colorCorrectionShader.Brightness = correction.Brightness;
            _colorCorrectionShader.Contrast = correction.Contrast;
            _colorCorrectionShader.Saturation = correction.Saturation;
            _colorCorrectionShader.Hue = correction.Hue;
        }
        #endregion

        #region Fog
        /// <summary>
        /// Updates fog based on the values in <see cref="Universe"/>
        /// </summary>
        private void UpdateFog()
        {
            View.Fog = Universe.Fog;
            View.BackgroundColor = Universe.FogColor;
            View.Camera.FarClip = Universe.Fog ? Universe.FogDistance : 1e+6f;
        }
        #endregion

        #region Bleach
        /// <summary>
        /// Updates <see cref="_bleachShader"/> based on the values in <see cref="Universe"/>
        /// </summary>
        private void UpdateBleach()
        {
            if (_bleachShader == null) return;

            _bleachShader.Enabled = Universe.Bleach;
        }
        #endregion
    }
}
