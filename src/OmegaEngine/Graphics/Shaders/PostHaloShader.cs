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
using NanoByte.Common.Utils;
using OmegaEngine.Properties;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// A post-screen shader that adds shining halos around objects in the scene.
    /// </summary>
    public class PostHaloShader : PostShader
    {
        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel { get { return new Version(2, 0); } }

        private Color _glowColor = Color.White;

        /// <summary>
        /// The color of the halo
        /// </summary>
        [Description("The color of the halo")]
        public Color GlowColor { get { return _glowColor; } set { value.To(ref _glowColor, () => SetShaderParameter("GlowCol", value)); } }

        private float _glowness = 1.6f;

        /// <summary>
        /// How strong the halo shall glow - values between 0 and 3
        /// </summary>
        [DefaultValue(1.6f), Description("How strong the halo shall glow - values between 0 and 3")]
        public float Glowness { get { return _glowness; } set { value.To(ref _glowness, () => SetShaderParameter("Glowness", value)); } }
        #endregion

        #region Engine
        /// <inheritdoc/>
        protected override void OnEngineSet()
        {
            if (MinShaderModel > Engine.Capabilities.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            LoadShaderFile("Post_Halo.fxo");

            base.OnEngineSet();
        }
        #endregion
    }
}
