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
using OmegaEngine.Assets;
using OmegaEngine.Properties;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// A post-screen shader that creates "scratched film" effect.
    /// </summary>
    public class PostScratchedFilmShader : PostShader
    {
        #region Variables
        private XTexture _noiseTexture;
        #endregion

        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel => new(2, 0);

        private float _speed = 0.03f, _speed2 = 0.02f, _scratchIntensity = 0.65f, _scratchWidth = 0.0075f;

        /// <summary>
        /// The horizontal speed of the scratches
        /// </summary>
        [DefaultValue(0.03f), Description("The horizontal speed of the scratches")]
        public float Speed { get { return _speed; } set { value.To(ref _speed, () => SetShaderParameter("Speed", value)); } }

        /// <summary>
        /// The vertical speed of the scratches
        /// </summary>
        [DefaultValue(0.02f), Description("The vertical speed of the scratches")]
        public float Speed2 { get { return _speed2; } set { value.To(ref _speed2, () => SetShaderParameter("Speed2", value)); } }

        /// <summary>
        /// The number of scratches
        /// </summary>
        [DefaultValue(0.65f), Description("The number of scratches")]
        public float ScratchIntensity { get { return _scratchIntensity; } set { value.To(ref _scratchIntensity, () => SetShaderParameter("ScratchIntensity", value)); } }

        /// <summary>
        /// The size of the scratches
        /// </summary>
        [DefaultValue(0.0075f), Description("The size of the scratches")]
        public float ScratchWidth { get { return _scratchWidth; } set { value.To(ref _scratchWidth, () => SetShaderParameter("IS", value)); } }
        #endregion

        #region Engine
        /// <inheritdoc/>
        protected override void OnEngineSet()
        {
            if (MinShaderModel > Engine.Capabilities.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            LoadShaderFile("Post_ScratchedFilm.fxo");

            // Load noise texture
            _noiseTexture = XTexture.Get(Engine, "Shaders/Noise128.dds");
            _noiseTexture.HoldReference();
            Effect.SetTexture("Noise2DTex", _noiseTexture);

            base.OnEngineSet();
        }
        #endregion

        #region Dispose
        /// <inheritdoc/>
        protected override void OnDispose()
        {
            try
            {
                _noiseTexture.ReleaseReference();
            }
            finally
            {
                base.OnDispose();
            }
        }
        #endregion
    }
}
