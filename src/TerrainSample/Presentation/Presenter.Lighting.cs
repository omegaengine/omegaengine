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
using AlphaFramework.Presentation;
using Common.Utils;
using Common.Values;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Shaders;
using SlimDX;

namespace TerrainSample.Presentation
{
    partial class Presenter
    {
        /// <summary>
        /// Use lighting in this presentation?
        /// </summary>
        protected bool Lighting = true;

        private readonly DirectionalLight
            _lightSun = new DirectionalLight {Name = "Sun", Enabled = false},
            _lightMoon = new DirectionalLight {Name = "Moon", Enabled = false};

        /// <summary>
        /// The ratio between the strength of diffuse and specular lighting
        /// </summary>
        private const float DiffuseToSpecularRatio = 0.5f;

        private PostSepiaShader _sepiaShader;
        private PostColorCorrectionShader _colorCorrectionShader;
        private PostBleachShader _bleachShader;

        /// <summary>
        /// Helper method for setting up the lighting and post-screen effects
        /// </summary>
        private void SetupLighting()
        {
            // Prepare post-screen shaders if the hardware supports them
            if (Engine.Capabilities.MaxShaderModel >= PostBleachShader.MinShaderModel)
            {
                // Auto-setup glow shader
                View.SetupGlow();

                // Pre-load deactivated effects for later use
                View.PostShaders.Add(_bleachShader = new PostBleachShader {Enabled = false});
                View.PostShaders.Add(_colorCorrectionShader = new PostColorCorrectionShader {Enabled = false});
                View.PostShaders.Add(_sepiaShader = new PostSepiaShader {Enabled = false, Desaturation = 0, Toning = 0});
            }

            // Add the lights to the scene
            Scene.Lights.Add(_lightSun);
            Scene.Lights.Add(_lightMoon);
        }

        /// <summary>
        /// Updates <see cref="_lightSun"/> and <see cref="_lightMoon"/> based on the light phase in <see cref="PresenterBase{TUniverse,TCoordinates}.Universe"/>.
        /// </summary>
        protected void UpdateLighting()
        {
            // Calculate lighting directions
            _lightSun.Direction = GetLightDirection(Universe.SunInclination, Universe.LightPhase);
            _lightMoon.Direction = GetLightDirection(Universe.MoonInclination, (Universe.LightPhase + 3) % 4);

            UpdateLightingColorSun();
            UpdateLightingColorMoon();

            if (_colorCorrectionShader != null) UpdateColorCorrection();

            View.Fog = Universe.Fog;
            View.BackgroundColor = Universe.FogColor;
            View.Camera.FarClip = Universe.Fog ? Universe.FogDistance : 1e+6f;

            if (_bleachShader != null) _bleachShader.Enabled = Universe.Bleach;
        }

        private static Vector3 GetLightDirection(float inclination, float phase)
        {
            return MathUtils.UnitVector(inclination.DegreeToRadian(), phase * -Math.PI / 2);
        }

        /// <summary>
        /// Updates the color of <see cref="_lightSun"/>.
        /// </summary>
        private void UpdateLightingColorSun()
        {
            // Fast intensity fall-off near the horizon
            float elevationFactor = (-_lightSun.Direction.Y * 4).Clamp();

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
            // Fast intensity fall-off near the horizon
            float elevationFactor = (-_lightMoon.Direction.Y * 4).Clamp();

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
                Universe.ColorCorrectionDawn, Universe.ColorCorrectionNoon, Universe.ColorCorrectionDusk, Universe.ColorCorrectionMidnight, Universe.ColorCorrectionDawn);

            // If the color correction values aren't all at default, activate the shader and transfer the values
            _colorCorrectionShader.Enabled = (correction != ColorCorrection.Default);
            _colorCorrectionShader.Brightness = correction.Brightness;
            _colorCorrectionShader.Contrast = correction.Contrast;
            _colorCorrectionShader.Saturation = correction.Saturation;
            _colorCorrectionShader.Hue = correction.Hue;
        }
    }
}
