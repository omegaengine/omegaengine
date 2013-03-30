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
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// A post-screen shader that blurs the view using a Gaussian blur filter.
    /// </summary>
    public class PostBlurShader : PostShader
    {
        #region Variables
        private float _blurStrength = 1;

        private readonly EffectHandle _blurStrengthHandle;
        #endregion

        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel { get { return new Version(2, 0); } }

        /// <summary>
        /// How strongly to blur the image - values between 0 and 10
        /// </summary>
        [DefaultValue(1f), Description("How strongly to blur the image - values between 0 and 10")]
        public float BlurStrength
        {
            get { return _blurStrength; }
            set
            {
                if (Disposed) return;
                if (value < 0 || value > 10) throw new ArgumentOutOfRangeException("value");
                value.To(ref _blurStrength, () => Effect.SetValue(_blurStrengthHandle, value));
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the shader
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the shader into</param>
        /// <exception cref="NotSupportedException">Thrown if the graphics card does not support this shader</exception>
        public PostBlurShader(Engine engine) : base(engine, "Post_Blur.fxo")
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (MinShaderModel > engine.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            #endregion

            // Get handles to shader parameters for quick access
            _blurStrengthHandle = Effect.GetParameter(null, "BlurStrength");
        }
        #endregion
    }
}
