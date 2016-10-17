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
using OmegaEngine.Properties;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// A post-screen shader that blurs the view using a Gaussian blur filter.
    /// </summary>
    public class PostBlurShader : PostShader
    {
        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel => new Version(2, 0);

        private float _blurStrength = 1;

        /// <summary>
        /// How strongly to blur the image - values between 0 and 10
        /// </summary>
        [DefaultValue(1f), Description("How strongly to blur the image - values between 0 and 10")]
        public float BlurStrength
        {
            get { return _blurStrength; }
            set
            {
                value = value.Clamp(0, 10);
                value.To(ref _blurStrength, () => SetShaderParameter("BlurStrength", value));
            }
        }
        #endregion

        #region Engine
        /// <inheritdoc/>
        protected override void OnEngineSet()
        {
            if (MinShaderModel > Engine.Capabilities.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            LoadShaderFile("Post_Blur.fxo");

            base.OnEngineSet();
        }
        #endregion
    }
}
