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
using Common;
using Common.Utils;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// A post-screen shader that creates an "old paper" look.
    /// </summary>
    public class PostSepiaShader : PostShader
    {
        #region Variables
        private Color _paperTone = Color.FromArgb(255, 229, 127), _stainTone = Color.FromArgb(51, 12, 0);
        private float _desaturation = 0.5f, _toning = 1.0f;

        private readonly EffectHandle _paperToneHandle, _stainToneHandle, _desatHandle, _tonedHandle;
        #endregion

        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel { get { return new Version(2, 0); } }

        /// <summary>
        /// The color to give the image after turning it into grayscale
        /// </summary>
        [Description("The color to give the image after turning it into grayscale")]
        public Color PaperTone
        {
            get { return _paperTone; }
            set
            {
                if (Disposed) return;
                value.To(ref _paperTone, () => Effect.SetValue(_paperToneHandle, value.ToVector4()));
            }
        }

        /// <summary>
        /// The color of the image stains
        /// </summary>
        [Description("The color of the image stains")]
        public Color StainTone
        {
            get { return _stainTone; }
            set
            {
                if (Disposed) return;
                value.To(ref _stainTone, () => Effect.SetValue(_stainToneHandle, value.ToVector4()));
            }
        }

        /// <summary>
        /// How strongly to turn the image it into grayscale - values between 0 and 1
        /// </summary>
        [DefaultValue(0.5f), Description("How strongly to turn the image it into grayscale - values between 0 and 1")]
        public float Desaturation
        {
            get { return _desaturation; }
            set
            {
                if (Disposed) return;
                if (value < 0 || value > 1) throw new ArgumentOutOfRangeException("value");
                value.To(ref _desaturation, () => Effect.SetValue(_desatHandle, value));
            }
        }

        /// <summary>
        /// How strongly to apply the <see cref="PaperTone"/> color - values between 0 and 1
        /// </summary>
        [DefaultValue(1f), Description("How strongly to apply the PaperTone color - values between 0 and 1")]
        public float Toning
        {
            get { return _toning; }
            set
            {
                if (Disposed) return;
                if (value < 0 || value > 1) throw new ArgumentOutOfRangeException("value");
                value.To(ref _toning, () => Effect.SetValue(_tonedHandle, value));
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the shader
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the shader into</param>
        /// <exception cref="NotSupportedException">Thrown if the graphics card does not support this shader</exception>
        public PostSepiaShader(Engine engine) : base(engine, "Post_Sepia.fxo")
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (MinShaderModel > engine.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            #endregion

            // Get handles to shader parameters for quick access
            _paperToneHandle = Effect.GetParameter(null, "LightColor");
            _stainToneHandle = Effect.GetParameter(null, "DarkColor");
            _desatHandle = Effect.GetParameter(null, "Desat");
            _tonedHandle = Effect.GetParameter(null, "Toned");
        }
        #endregion
    }
}
