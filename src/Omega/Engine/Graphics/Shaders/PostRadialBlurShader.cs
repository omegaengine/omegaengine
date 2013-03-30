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
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// A post-screen shader that blurs the view radially.
    /// </summary>
    public class PostRadialBlurShader : PostShader
    {
        #region Variables
        private float _blurStart = 5.0f, _blurWidth = -0.05f;
        private Vector2 _blurCenter = new Vector2(0.5f, 0.5f);

        private readonly EffectHandle _blurStartHandle, _blurWidthHandle, _blurCenterHandle;
        #endregion

        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel { get { return new Version(2, 0); } }

        /// <summary>
        /// The minimum range at which to start blur sampling - values between 0 and 10
        /// </summary>
        [DefaultValue(5.0f), Description("The minimum range at which to start blur sampling - values between 0 and 10")]
        public float BlurStart
        {
            get { return _blurStart; }
            set
            {
                if (Disposed) return;
                if (value < 0 || value > 10) throw new ArgumentOutOfRangeException("value");
                value.To(ref _blurStart, () => Effect.SetValue(_blurStartHandle, value));
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
                if (Disposed) return;
                if (value < -1 || value > 1) throw new ArgumentOutOfRangeException("value");
                value.To(ref _blurWidth, () => Effect.SetValue(_blurWidthHandle, value));
            }
        }

        /// <summary>
        /// The center/origin of the radial blur effect
        /// </summary>
        [DefaultValue(typeof(Vector2), "0; 0"), Description("The center/origin of the radial blur effect")]
        public Vector2 BlurCenter
        {
            get { return _blurCenter; }
            set
            {
                if (Disposed) return;
                value.To(ref _blurCenter, () => Effect.SetValue(_blurCenterHandle, new[] {value.X, value.Y}));
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the shader
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the shader into</param>
        /// <exception cref="NotSupportedException">Thrown if the graphics card does not support this shader</exception>
        public PostRadialBlurShader(Engine engine) : base(engine, "Post_RadialBlur.fxo")
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (MinShaderModel > engine.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            #endregion

            // Get handles to shader parameters for quick access
            _blurStartHandle = Effect.GetParameter(null, "BlurStart");
            _blurWidthHandle = Effect.GetParameter(null, "BlurWidth");
            _blurCenterHandle = Effect.GetParameter(null, "BlurCenter");

            // In FX Composer the center is at (0.5|0.5) but here it is at (0|0)
            BlurCenter = new Vector2();
        }
        #endregion
    }
}
