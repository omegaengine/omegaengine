/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Drawing;
using NanoByte.Common;
using OmegaEngine.Properties;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// A post-screen shader that creates an "old paper" look.
    /// </summary>
    public class PostSepiaShader : PostShader
    {
        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel { get { return new Version(2, 0); } }

        private Color _paperTone = Color.FromArgb(255, 229, 127), _stainTone = Color.FromArgb(51, 12, 0);

        /// <summary>
        /// The color to give the image after turning it into grayscale
        /// </summary>
        [Description("The color to give the image after turning it into grayscale")]
        public Color PaperTone { get { return _paperTone; } set { value.To(ref _paperTone, () => SetShaderParameter("LightColor", value)); } }

        /// <summary>
        /// The color of the image stains
        /// </summary>
        [Description("The color of the image stains")]
        public Color StainTone { get { return _stainTone; } set { value.To(ref _stainTone, () => SetShaderParameter("DarkColor", value)); } }

        private float _desaturation = 0.5f, _toning = 1.0f;

        /// <summary>
        /// How strongly to turn the image it into grayscale - values between 0 and 1
        /// </summary>
        [DefaultValue(0.5f), Description("How strongly to turn the image it into grayscale - values between 0 and 1")]
        public float Desaturation
        {
            get { return _desaturation; }
            set
            {
                value = value.Clamp();
                value.To(ref _desaturation, () => SetShaderParameter("Desat", value));
            }
        }

        /// <summary>
        /// How strongly to apply the <see cref="PaperTone"/> color - values between 0 and 1
        /// </summary>
        [DefaultValue(1f), Description("How strongly to apply the PaperTone color - values between 0 and 1")]
        public float Toning
        {
            get { return _toning; }
            set
            {
                value = value.Clamp();
                value.To(ref _toning, () => SetShaderParameter("Toned", value));
            }
        }
        #endregion

        #region Engine
        /// <inheritdoc/>
        protected override void OnEngineSet()
        {
            if (MinShaderModel > Engine.Capabilities.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            LoadShaderFile("Post_Sepia.fxo");

            base.OnEngineSet();
        }
        #endregion
    }
}
