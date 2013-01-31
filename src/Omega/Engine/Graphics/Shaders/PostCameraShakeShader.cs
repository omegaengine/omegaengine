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
using SlimDX;
using SlimDX.Direct3D9;
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
        #region Variables
        private float _speed = 20.0f, _shakiness = 0.25f, _sharpness = 2.2f;
        private Vector2 _timeDelta = new Vector2(1, 0.2f);

        private readonly EffectHandle _speedHandle, _shakinessHandle, _sharpnessHandle, _timeDeltaHandle;
        #endregion

        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel { get { return new Version(2, 0); } }

        /// <summary>
        /// How fast to shake the camera - values between -1 and 100
        /// </summary>
        [DefaultValue(20.0f), Description("How fast to shake the camera - values between -1 and 100")]
        public float Speed
        {
            get { return _speed; }
            set
            {
                if (Disposed) return;
                if (value < -1 || value > 100) throw new ArgumentOutOfRangeException("value");
                UpdateHelper.Do(ref _speed, value, () => Effect.SetValue(_speedHandle, value));
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
                if (Disposed) return;
                if (value < 0 || value > 1) throw new ArgumentOutOfRangeException("value");
                UpdateHelper.Do(ref _shakiness, value, () => Effect.SetValue(_shakinessHandle, value));
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
                if (Disposed) return;
                if (value < 0 || value > 10) throw new ArgumentOutOfRangeException("value");
                UpdateHelper.Do(ref _sharpness, value, () => Effect.SetValue(_sharpnessHandle, value));
            }
        }

        public Vector2 TimeDelta
        {
            get { return _timeDelta; }
            set
            {
                if (Disposed) return;
                UpdateHelper.Do(ref _timeDelta, value, () => Effect.SetValue(_timeDeltaHandle, new[] {value.X, value.Y}));
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the shader
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the shader into</param>
        /// <exception cref="NotSupportedException">Thrown if the graphics card does not support this shader</exception>
        public PostCameraShakeShader(Engine engine) : base(engine, "Post_CameraShake.fxo")
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (MinShaderModel > engine.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            #endregion

            // Get handles to shader parameters for quick access
            _speedHandle = Effect.GetParameter(null, "Speed");
            _shakinessHandle = Effect.GetParameter(null, "Shake");
            _sharpnessHandle = Effect.GetParameter(null, "Sharpness");
            _timeDeltaHandle = Effect.GetParameter(null, "TimeDelta");
        }
        #endregion
    }
}
