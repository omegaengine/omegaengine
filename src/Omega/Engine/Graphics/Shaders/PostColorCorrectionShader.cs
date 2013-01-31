/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using Common;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// A post-screen shader for applying TV-like settings like brightness, contrast, hue, etc.
    /// </summary>
    public class PostColorCorrectionShader : PostShader
    {
        #region Variables
        private float _brightness = 1, _contrast = 1, _saturation = 1, _hue;

        private readonly EffectHandle _brightnessHandle, _contrastHandle, _saturationHandle, _hueHandle;
        #endregion

        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel { get { return new Version(2, 0); } }

        /// <summary>
        /// How bright the picture should be - values between 0 (black) and 5 (5x normal)
        /// </summary>
        [DefaultValue(1f), Description("How bright the picture should be - values between 0 (black) and 5 (5x normal)")]
        public float Brightness
        {
            get { return _brightness; }
            set
            {
                if (Disposed) return;
                if (value < 0 || value > 5) throw new ArgumentOutOfRangeException("value");
                UpdateHelper.Do(ref _brightness, value, () => Effect.SetValue(_brightnessHandle, value));
            }
        }

        /// <summary>
        /// The contrast level of the picture - values between -5 and 5
        /// </summary>
        [DefaultValue(1f), Description("The contrast level of the picture - values between -5 and 5")]
        public float Contrast
        {
            get { return _contrast; }
            set
            {
                if (Disposed) return;
                if (value < -5 || value > 5) throw new ArgumentOutOfRangeException("value");
                UpdateHelper.Do(ref _contrast, value, () => Effect.SetValue(_contrastHandle, value));
            }
        }

        /// <summary>
        /// The color saturation level of the picture - values between -5 and 5
        /// </summary>
        [DefaultValue(1f), Description("The color saturation level of the picture - values between -5 and 5")]
        public float Saturation
        {
            get { return _saturation; }
            set
            {
                if (Disposed) return;
                if (value < -5 || value > 5) throw new ArgumentOutOfRangeException("value");
                UpdateHelper.Do(ref _saturation, value, () => Effect.SetValue(_saturationHandle, value));
            }
        }

        /// <summary>
        /// The color hue rotation of the picture - values between 0 and 360
        /// </summary>
        [DefaultValue(0f), Description("The color hue rotation of the picture - values between 0 and 360")]
        public float Hue
        {
            get { return _hue; }
            set
            {
                if (Disposed) return;
                if (value < 0 || value > 360) throw new ArgumentOutOfRangeException("value");
                UpdateHelper.Do(ref _hue, value, () => Effect.SetValue(_hueHandle, value));
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the shader
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the shader into</param>
        /// <exception cref="NotSupportedException">Thrown if the graphics card does not support this shader</exception>
        public PostColorCorrectionShader(Engine engine) : base(engine, "Post_ColorCorrection.fxo")
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (MinShaderModel > engine.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            #endregion

            // Get handles to shader parameters for quick access
            _brightnessHandle = Effect.GetParameter(null, "Brightness");
            _contrastHandle = Effect.GetParameter(null, "Contrast");
            _saturationHandle = Effect.GetParameter(null, "Saturation");
            _hueHandle = Effect.GetParameter(null, "Hue");
        }
        #endregion
    }
}
