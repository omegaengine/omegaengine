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
        private readonly EffectHandle _particleTextureHandle;

        /// <summary>
        /// A 1D-texture containing the particle color spectrum
        /// </summary>
        internal ITextureProvider ParticleTexture;
        #endregion

        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel { get { return new Version(2, 0); } }

        private float _spawnRadius, _systemHeight;
        private readonly EffectHandle _spawnRadiusHandle, _systemHeightHandle;

        /// <summary>
        /// The largest distance from the emitter at which particle shall be spawned
        /// </summary>
        [DefaultValue(0f), Description("The largest distance from the emitter at which particle shall be spawned")]
        public float SpawnRadius
        {
            get { return _spawnRadius; }
            set
            {
                if (Disposed) return;
                value.To(ref _spawnRadius, () => Effect.SetValue(_spawnRadiusHandle, value));
            }
        }

        /// <summary>
        /// The largest distance from the emitter particles can travel before dying
        /// </summary>
        [DefaultValue(0f), Description("The largest distance from the emitter particles can travel before dying")]
        public float SystemHeight
        {
            get { return _systemHeight; }
            set
            {
                if (Disposed) return;
                value.To(ref _systemHeight, () => Effect.SetValue(_systemHeightHandle, value));
            }
        }

        private float _particleSpeed, _particleSpread, _particleSize, _particleShape;
        private readonly EffectHandle _particleSpeedHandle, _particleSpreadHandle, _particleSizeHandle, _particleShapeHandle;

        /// <summary>
        /// The speed with which the particles move
        /// </summary>
        [DefaultValue(0f), Description("The speed with which the particles move")]
        public float ParticleSpeed
        {
            get { return _particleSpeed; }
            set
            {
                if (Disposed) return;
                value.To(ref _particleSpeed, () => Effect.SetValue(_particleSpeedHandle, value));
            }
        }

        /// <summary>
        /// How to spread the particles
        /// </summary>
        [DefaultValue(0f), Description("How to spread the particles")]
        public float ParticleSpread
        {
            get { return _particleSpread; }
            set
            {
                if (Disposed) return;
                value.To(ref _particleSpread, () => Effect.SetValue(_particleSpreadHandle, value));
            }
        }

        /// <summary>
        /// The size of the particles
        /// </summary>
        [DefaultValue(0f), Description("The size of the particles")]
        public float ParticleSize
        {
            get { return _particleSize; }
            set
            {
                if (Disposed) return;
                value.To(ref _particleSize, () => Effect.SetValue(_particleSizeHandle, value));
            }
        }

        /// <summary>
        /// The shape of the particles
        /// </summary>
        [DefaultValue(0f), Description("The shape of the particles")]
        public float ParticleShape
        {
            get { return _particleShape; }
            set
            {
                if (Disposed) return;
                value.To(ref _particleShape, () => Effect.SetValue(_particleShapeHandle, value));
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the water shader with refraction and reflection
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the shader into</param>
        /// <param name="particleTexture">The normal texture to apply to the water surface for ripples</param>
        public ParticleShader(Engine engine, ITextureProvider particleTexture) : base(engine, "ParticleSystem.fxo")
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (particleTexture == null) throw new ArgumentNullException("particleTexture");
            if (MinShaderModel > engine.Capabilities.MaxShaderModel)
                throw new NotSupportedException(Resources.NotSupportedShader);
            #endregion

            ParticleTexture = particleTexture;
            ParticleTexture.HoldReference();

            // Get handles to shader parameters for quick access
            _particleTextureHandle = Effect.GetParameter(null, "ParticleTexture");
            _spawnRadiusHandle = Effect.GetParameter(null, "SpawnRadius");
            _systemHeightHandle = Effect.GetParameter(null, "SystemHeight");
            _particleSpeedHandle = Effect.GetParameter(null, "ParticleSpeed");
            _particleSpreadHandle = Effect.GetParameter(null, "ParticleSpread");
            _particleSizeHandle = Effect.GetParameter(null, "ParticleSize");
            _particleShapeHandle = Effect.GetParameter(null, "ParticleShape");
        }
        #endregion

        //--------------------//

        #region Apply
        /// <summary>
        /// Applies the shader to the content in the render delegate.
        /// </summary>
        /// <param name="render">The render delegate (is called once for every shader pass).</param>
        /// <param name="material">The material to be used by this shader; <see langword="null"/> for device texture.</param>
        /// <param name="camera">The camera for transformation information.</param>
        /// <param name="lights">An array of all lights this shader should consider; should be <see langword="null"/>.</param>
        public override void Apply(Action render, XMaterial material, Camera camera, params LightSource[] lights)
        {
            #region Sanity checks
            if (render == null) throw new ArgumentNullException("render");
            if (camera == null) throw new ArgumentNullException("camera");
            if (lights == null) throw new ArgumentNullException("lights");
            #endregion

            // Always reset the texture since it might change its memory address at a device reset
            Effect.SetTexture(_particleTextureHandle, (ParticleTexture == null) ? null : ParticleTexture.Texture);

            base.Apply(render, material, camera, lights);
        }
        #endregion

        //--------------------//

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
