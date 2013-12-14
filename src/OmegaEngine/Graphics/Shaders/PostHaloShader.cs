/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Drawing;
using Common.Utils;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// A post-screen shader that adds shining halos around objects in the scene.
    /// </summary>
    public class PostHaloShader : PostShader
    {
        #region Variables
        private Color _glowColor = Color.White;
        private float _glowness = 1.6f;

        private readonly EffectHandle _glowColorHandle, _glownessHandle;
        #endregion

        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel { get { return new Version(2, 0); } }

        /// <summary>
        /// The color of the halo
        /// </summary>
        [Description("The color of the halo")]
        public Color GlowColor
        {
            get { return _glowColor; }
            set
            {
                if (Disposed) return;
                value.To(ref _glowColor, () => Effect.SetValue(_glowColorHandle, value.ToVector4()));
            }
        }

        /// <summary>
        /// How strong the halo shall glow - values between 0 and 3
        /// </summary>
        [DefaultValue(1.6f), Description("How strong the halo shall glow - values between 0 and 3")]
        public float Glowness
        {
            get { return _glowness; }
            set
            {
                if (Disposed) return;
                if (value < 0 || value > 3) throw new ArgumentOutOfRangeException("value");
                value.To(ref _glowness, () => Effect.SetValue(_glownessHandle, value));
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the shader
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the shader into</param>
        /// <exception cref="NotSupportedException">Thrown if the graphics card does not support this shader</exception>
        public PostHaloShader(Engine engine) : base(engine, "Post_Halo.fxo")
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (MinShaderModel > engine.Capabilities.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            #endregion

            // Get handles to shader parameters for quick access
            _glowColorHandle = Effect.GetParameter(null, "GlowCol");
            _glownessHandle = Effect.GetParameter(null, "Glowness");
        }
        #endregion
    }
}
