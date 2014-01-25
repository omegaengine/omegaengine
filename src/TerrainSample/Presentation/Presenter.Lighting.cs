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
using System.Drawing;
using Common.Utils;
using Common.Values;
using OmegaEngine.Graphics;
using SlimDX;

namespace TerrainSample.Presentation
{
    partial class Presenter
    {
        private readonly DirectionalLight
            _lightSun = new DirectionalLight {Name = "Sun", Enabled = false},
            _lightMoon = new DirectionalLight {Name = "Moon", Enabled = false};

        /// <summary>
        /// The ratio between the strength of diffuse and specular lighting
        /// </summary>
        private const float DiffuseToSpecularRatio = 0.5f;

        /// <summary>
        /// Updates <see cref="_lightSun"/> and <see cref="_lightMoon"/> based on the light phase in <see cref="Universe"/>.
        /// </summary>
        protected void UpdateLighting()
        {
            float sunPhase = Universe.LightPhase;
            float moonPhase = (Universe.LightPhase + 3) % 4;

            // Calculate lighting directions
            _lightSun.Direction = GetDirection(0.5f * Math.PI * (1 - sunPhase), Universe.SunInclination.DegreeToRadian());
            _lightMoon.Direction = GetDirection(0.5f * Math.PI * (1 - moonPhase), Universe.MoonInclination.DegreeToRadian());

            UpdateLightingColorSun();
            UpdateLightingColorMoon();

            if (_colorCorrectionShader != null) UpdateColorCorrection();
            
            View.Fog = Universe.Fog;
            View.BackgroundColor = Universe.FogColor;
            View.Camera.FarClip = Universe.Fog ? Universe.FogDistance : 1e+6f;

            if (_bleachShader != null) _bleachShader.Enabled = Universe.Bleach;
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
        /// Updates the color of <see cref="_lightSun"/>.
        /// </summary>
        private void UpdateLightingColorSun()
        {
            float elevationFactor = (-_lightSun.Direction.Y * 2).Clamp();

            _lightSun.Diffuse = ColorUtils.Interpolate(elevationFactor, Color.Black, Universe.SunColor);
            _lightSun.Specular = Color4.Scale(_lightSun.Diffuse, DiffuseToSpecularRatio).ToColor();
            _lightSun.Ambient = Universe.AmbientColor;
            _lightSun.Enabled = true; // Always on (for ambient light)
        }

        /// <summary>
        /// Updates the color of <see cref="_lightMoon"/>.
        /// </summary>
        private void UpdateLightingColorMoon()
        {
            float elevationFactor = (-_lightMoon.Direction.Y * 2).Clamp();

            _lightMoon.Diffuse = ColorUtils.Interpolate(elevationFactor, Color.Black, Universe.MoonColor);
            _lightMoon.Specular = Color4.Scale(_lightMoon.Diffuse, DiffuseToSpecularRatio).ToColor();
            _lightMoon.Ambient = Color.Black;
            _lightMoon.Enabled = (elevationFactor > 0); // Only on when above horizon
        }

        /// <summary>
        /// Updates the <see cref="_colorCorrectionShader"/> <see cref="ColorCorrection"/> values.
        /// </summary>
        private void UpdateColorCorrection()
        {
            // Interpolate between the color correction settings for the different sun phases
            var correction = ColorCorrection.SinusInterpolate(Universe.LightPhase,
                Universe.ColorCorrectionMidnight, Universe.ColorCorrectionDawn, Universe.ColorCorrectionNoon, Universe.ColorCorrectionDusk, Universe.ColorCorrectionMidnight);

            // If the color correction values aren't all at default, activate the shader and transfer the values
            _colorCorrectionShader.Enabled = (correction != ColorCorrection.Default);
            _colorCorrectionShader.Brightness = correction.Brightness;
            _colorCorrectionShader.Contrast = correction.Contrast;
            _colorCorrectionShader.Saturation = correction.Saturation;
            _colorCorrectionShader.Hue = correction.Hue;
        }
    }
}
