/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using Common.Utils;
using SlimDX;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// A post-screen shader that simulates a "shaking camera" effect.
    /// </summary>
    /// <remarks>
    /// The effect is actually fake, since the perspective cannot be extended beyound the original borders of the scene.
    /// Close-up slow-motition observation would reveal the borders to be stretched or squashed.
    /// </remarks>
    public class PostCameraShakeShader : PostShader
    {
        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel { get { return new Version(2, 0); } }

        private float _speed = 20.0f, _shakiness = 0.25f, _sharpness = 2.2f;

        /// <summary>
        /// How fast to shake the camera - values between -1 and 100
        /// </summary>
        [DefaultValue(20.0f), Description("How fast to shake the camera - values between -1 and 100")]
        public float Speed
        {
            get { return _speed; }
            set
            {
                value = value.Clamp(-1, 100);
                value.To(ref _speed, () => SetShaderParameter("Speed", value));
            }
        }

        /// <summary>
        /// How erratically to shake the camera - values between 0 and 1
        /// </summary>
        [DefaultValue(0.25f), Description("How erratically to shake the camera - values between 0 and 1")]
        public float Shakiness
        {
            get { return _shakiness; }
            set
            {
                value = value.Clamp();
                value.To(ref _shakiness, () => SetShaderParameter("Shakiness", value));
            }
        }

        /// <summary>
        /// How close the the origin to keep the shaking view - values between 0 and 10
        /// </summary>
        [DefaultValue(2.2f), Description("How close the the origin to keep the shaking view - values between 0 and 10")]
        public float Sharpness
        {
            get { return _sharpness; }
            set
            {
                value = value.Clamp(0, 10);
                value.To(ref _sharpness, () => SetShaderParameter("Sharpness", value));
            }
        }

        private Vector2 _timeDelta = new Vector2(1, 0.2f);

        public Vector2 TimeDelta { get { return _timeDelta; } set { value.To(ref _timeDelta, () => SetShaderParameter("TimeDelta", value)); } }
        #endregion

        #region Engine
        /// <inheritdoc/>
        protected override void OnEngineSet()
        {
            if (MinShaderModel > Engine.Capabilities.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            LoadShaderFile("Post_CameraShake.fxo");

            base.OnEngineSet();
        }
        #endregion
    }
}
