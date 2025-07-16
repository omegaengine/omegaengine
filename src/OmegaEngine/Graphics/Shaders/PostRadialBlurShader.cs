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
using SlimDX;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// A post-screen shader that blurs the view radially.
    /// </summary>
    public class PostRadialBlurShader : PostShader
    {
        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel => new(2, 0);

        private float _blurStart = 5.0f, _blurWidth = -0.05f;

        /// <summary>
        /// The minimum range at which to start blur sampling - values between 0 and 10
        /// </summary>
        [DefaultValue(5.0f), Description("The minimum range at which to start blur sampling - values between 0 and 10")]
        public float BlurStart
        {
            get { return _blurStart; }
            set
            {
                value = value.Clamp(0, 10);
                value.To(ref _blurStart, () => SetShaderParameter("BlurStart", value));
            }
        }

        /// <summary>
        /// How wide a blur sampling is - higher value = more blurry image - values between -1 and 1
        /// </summary>
        [DefaultValue(-0.05f), Description("The minimum range at which to start blur sampling - values between -1 and 1")]
        public float BlurWidth
        {
            get { return _blurWidth; }
            set
            {
                value = value.Clamp(-1, 1);
                value.To(ref _blurWidth, () => SetShaderParameter("BlurWidth", value));
            }
        }

        private Vector2 _blurCenter = new(0.5f, 0.5f);

        /// <summary>
        /// The center/origin of the radial blur effect
        /// </summary>
        [Description("The center/origin of the radial blur effect")]
        public Vector2 BlurCenter { get { return _blurCenter; } set { value.To(ref _blurCenter, () => SetShaderParameter("BlurCenter", value)); } }
        #endregion

        #region Engine
        /// <inheritdoc/>
        protected override void OnEngineSet()
        {
            if (MinShaderModel > Engine.Capabilities.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            LoadShaderFile("Post_RadialBlur.fxo");

            base.OnEngineSet();
        }
        #endregion
    }
}
