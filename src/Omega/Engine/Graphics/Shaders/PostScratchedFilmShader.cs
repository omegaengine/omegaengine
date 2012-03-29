/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using Common;
using SlimDX.Direct3D9;
using OmegaEngine.Assets;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// A post-screen shader that creates "scratched film" effect.
    /// </summary>
    public class PostScratchedFilmShader : PostShader
    {
        #region Variables
        private float _speed = 0.03f, _speed2 = 0.02f, _scratchIntensity = 0.65f, _scratchWidth = 0.0075f;

        private readonly EffectHandle _speedHandle, _speed2Handle, _scratchIntensityHandle, _scratchWidthHandle;

        private readonly XTexture _noiseTexture;
        #endregion

        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel { get { return new Version(2, 0); } }

        /// <summary>
        /// The horizontal speed of the scratches
        /// </summary>
        [DefaultValue(0.03f), Description("The horizontal speed of the scratches")]
        public float Speed
        {
            get { return _speed; }
            set
            {
                if (Disposed) return;
                UpdateHelper.Do(ref _speed, value, () => Effect.SetValue(_speedHandle, value));
            }
        }

        /// <summary>
        /// The vertical speed of the scratches
        /// </summary>
        [DefaultValue(0.02f), Description("The vertical speed of the scratches")]
        public float Speed2
        {
            get { return _speed2; }
            set
            {
                if (Disposed) return;
                UpdateHelper.Do(ref _speed2, value, () => Effect.SetValue(_speed2Handle, value));
            }
        }

        /// <summary>
        /// The number of scratches
        /// </summary>
        [DefaultValue(0.65f), Description("The number of scratches")]
        public float ScratchIntensity
        {
            get { return _scratchIntensity; }
            set
            {
                if (Disposed) return;
                UpdateHelper.Do(ref _scratchIntensity, value, () => Effect.SetValue(_scratchIntensityHandle, value));
            }
        }

        /// <summary>
        /// The size of the scratches
        /// </summary>
        [DefaultValue(0.0075f), Description("The size of the scratches")]
        public float ScratchWidth
        {
            get { return _scratchWidth; }
            set
            {
                if (Disposed) return;
                UpdateHelper.Do(ref _scratchWidth, value, () => Effect.SetValue(_scratchWidthHandle, value));
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the shader
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the shader into</param>
        /// <exception cref="NotSupportedException">Thrown if the graphics card does not support this shader</exception>
        public PostScratchedFilmShader(Engine engine) : base(engine, "Post_ScratchedFilm.fxo")
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (MinShaderModel > engine.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            #endregion

            // Get handles to shader parameters for quick access
            _speedHandle = Effect.GetParameter(null, "Speed");
            _speed2Handle = Effect.GetParameter(null, "Speed2");
            _scratchIntensityHandle = Effect.GetParameter(null, "ScratchIntensity");
            _scratchWidthHandle = Effect.GetParameter(null, "IS");

            // Load noise texture
            _noiseTexture = XTexture.Get(engine, "Shaders/Noise128.dds", false);
            _noiseTexture.HoldReference();
            Effect.SetTexture("Noise2DTex", _noiseTexture);
        }
        #endregion

        //--------------------//

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (Disposed || Engine == null || Engine.Disposed) return; // Don't try to dispose more than once

            try
            {
                if (disposing)
                { // This block will only be executed on manual disposal, not by Garbage Collection
                    _noiseTexture.ReleaseReference();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion
    }
}
