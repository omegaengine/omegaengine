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
    /// A post-screen shader that bleaches out the colors.
    /// </summary>
    public class PostBleachShader : PostShader
    {
        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel { get { return new Version(2, 0); } }

        private float _opacity = 1.0f;

        /// <summary>
        /// How strong the bleaching effect should be - values between 0 and 1
        /// </summary>
        [DefaultValue(1f), Description("How strong the bleaching effect should be - values between 0 and 1")]
        public float Opacity
        {
            get { return _opacity; }
            set
            {
                value = value.Clamp();
                value.To(ref _opacity, () => SetShaderParameter("Opacity", value));
            }
        }
        #endregion

        #region Engine
        /// <inheritdoc/>
        protected override void OnEngineSet()
        {
            if (MinShaderModel > Engine.Capabilities.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            LoadShaderFile("Post_Bleach.fxo");

            base.OnEngineSet();
        }
        #endregion
    }
}
