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
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Properties;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// Renders a particle system by manipulating positions of a batch of vertexes.
    /// </summary>
    /// <seealso cref="GpuParticleSystem"/>
    public class ParticleShader : SurfaceShader
    {
        #region Variables
        private EffectHandle _particleTextureHandle;

        /// <summary>
        /// A 1D-texture containing the particle color spectrum
        /// </summary>
        internal ITextureProvider ParticleTexture;
        #endregion

        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel => new(2, 0);

        private float _spawnRadius, _systemHeight;

        /// <summary>
        /// The largest distance from the emitter at which particle shall be spawned
        /// </summary>
        [DefaultValue(0f), Description("The largest distance from the emitter at which particle shall be spawned")]
        public float SpawnRadius { get => _spawnRadius; set => value.To(ref _spawnRadius, () => SetShaderParameter("SpawnRadius", value)); }

        /// <summary>
        /// The largest distance from the emitter particles can travel before dying
        /// </summary>
        [DefaultValue(0f), Description("The largest distance from the emitter particles can travel before dying")]
        public float SystemHeight { get => _systemHeight; set => value.To(ref _systemHeight, () => SetShaderParameter("SystemHeight", value)); }

        private float _particleSpeed, _particleSpread, _particleSize, _particleShape;

        /// <summary>
        /// The speed with which the particles move
        /// </summary>
        [DefaultValue(0f), Description("The speed with which the particles move")]
        public float ParticleSpeed { get => _particleSpeed; set => value.To(ref _particleSpeed, () => SetShaderParameter("ParticleSpeed", value)); }

        /// <summary>
        /// How to spread the particles
        /// </summary>
        [DefaultValue(0f), Description("How to spread the particles")]
        public float ParticleSpread { get => _particleSpread; set => value.To(ref _particleSpread, () => SetShaderParameter("ParticleSpread", value)); }

        /// <summary>
        /// The size of the particles
        /// </summary>
        [DefaultValue(0f), Description("The size of the particles")]
        public float ParticleSize { get => _particleSize; set => value.To(ref _particleSize, () => SetShaderParameter("ParticleSize", value)); }

        /// <summary>
        /// The shape of the particles
        /// </summary>
        [DefaultValue(0f), Description("The shape of the particles")]
        public float ParticleShape { get => _particleShape; set => value.To(ref _particleShape, () => SetShaderParameter("ParticleShape", value)); }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the water shader with refraction and reflection
        /// </summary>
        /// <param name="particleTexture">The normal texture to apply to the water surface for ripples</param>
        public ParticleShader(ITextureProvider particleTexture)
        {
            #region Sanity checks
            if (particleTexture == null) throw new ArgumentNullException(nameof(particleTexture));
            #endregion

            ParticleTexture = particleTexture;
            ParticleTexture.HoldReference();
        }
        #endregion

        //--------------------//

        #region Apply
        /// <summary>
        /// Applies the shader to the content in the render delegate.
        /// </summary>
        /// <param name="render">The render delegate (is called once for every shader pass).</param>
        /// <param name="material">The material to be used by this shader; <c>null</c> for device texture.</param>
        /// <param name="camera">The camera for transformation information.</param>
        /// <param name="lights">An array of all lights this shader should consider; should be <c>null</c>.</param>
        public override void Apply(Action render, XMaterial material, Camera camera, params LightSource[] lights)
        {
            #region Sanity checks
            if (render == null) throw new ArgumentNullException(nameof(render));
            if (camera == null) throw new ArgumentNullException(nameof(camera));
            if (lights == null) throw new ArgumentNullException(nameof(lights));
            #endregion

            // Always reset the texture since it might change its memory address at a device reset
            Effect.SetTexture(_particleTextureHandle, ParticleTexture?.Texture);

            base.Apply(render, material, camera, lights);
        }
        #endregion

        //--------------------//

        #region Engine
        /// <inheritdoc/>
        protected override void OnEngineSet()
        {
            if (MinShaderModel > Engine.Capabilities.MaxShaderModel)
                throw new NotSupportedException(Resources.NotSupportedShader);

            LoadShaderFile("ParticleSystem.fxo");

            _particleTextureHandle = Effect.GetParameter(null, "ParticleTexture");
            base.OnEngineSet();
        }
        #endregion

        #region Dispose
        /// <inheritdoc/>
        protected override void OnDispose()
        {
            try
            {
                if (ParticleTexture != null)
                {
                    ParticleTexture.ReleaseReference();
                    ParticleTexture = null;
                }
            }
            finally
            {
                base.OnDispose();
            }
        }
        #endregion
    }
}
