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
        private readonly EffectHandle _opacityHandle;

        /// <summary>
        /// How strong the bleaching effect should be - values between 0 and 1
        /// </summary>
        [DefaultValue(1f), Description("How strong the bleaching effect should be - values between 0 and 1")]
        public float Opacity
        {
            get { return _opacity; }
            set
            {
                if (Disposed) return;
                if (value < 0 || value > 1) throw new ArgumentOutOfRangeException("value");
                value.To(ref _opacity, () => Effect.SetValue(_opacityHandle, value));
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the shader
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the shader into</param>
        /// <exception cref="NotSupportedException">Thrown if the graphics card does not support this shader</exception>
        public PostBleachShader(Engine engine) : base(engine, "Post_Bleach.fxo")
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (MinShaderModel > engine.Capabilities.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            #endregion

            // Get handles to shader parameters for quick access
            _opacityHandle = Effect.GetParameter(null, "Opacity");
        }
        #endregion
    }
}
