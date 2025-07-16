/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using NanoByte.Common;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// A post-screen shader for applying TV-like settings like brightness, contrast, hue, etc.
    /// </summary>
    public class PostColorCorrectionShader : PostShader
    {
        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel => new(2, 0);

        private float _brightness = 1, _contrast = 1, _saturation = 1, _hue;

        /// <summary>
        /// How bright the picture should be - values between 0 (black) and 5 (5x normal)
        /// </summary>
        [DefaultValue(1f), Description("How bright the picture should be - values between 0 (black) and 5 (5x normal)")]
        public float Brightness
        {
            get { return _brightness; }
            set
            {
                value = value.Clamp(0, 5);
                value.To(ref _brightness, () => SetShaderParameter("Brightness", value));
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
                value = value.Clamp(-5, 5);
                value.To(ref _contrast, () => SetShaderParameter("Contrast", value));
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
                value = value.Clamp(-5, 5);
                value.To(ref _saturation, () => SetShaderParameter("Saturation", value));
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
                value = value.Clamp(0, 360);
                value.To(ref _hue, () => SetShaderParameter("Hue", value));
            }
        }
        #endregion

        #region Engine
        protected override void OnEngineSet()
        {
            if (MinShaderModel > Engine.Capabilities.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            LoadShaderFile("Post_ColorCorrection.fxo");

            base.OnEngineSet();
        }
        #endregion
    }
}
