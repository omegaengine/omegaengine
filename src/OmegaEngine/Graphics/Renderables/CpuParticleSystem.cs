/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Common.Collections;
using Common.Storage;
using Common.Utils;
using Common.Values;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.VertexDecl;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.Renderables
{
    /// <summary>
    /// A particle system whose particles are tracked by the CPU.
    /// </summary>
    public class CpuParticleSystem : PositionableRenderable
    {
        #region Variables
        private readonly Pool<CpuParticle>
            _firstLifeParticles = new Pool<CpuParticle>(),
            _secondLifeParticles = new Pool<CpuParticle>();

        /// <summary>A free-list of <see cref="CpuParticle"/>s to be reused</summary>
        private readonly Stack<CpuParticle> _deadParticles = new Stack<CpuParticle>();

        /// <summary>The last <see cref="Camera"/> <see cref="Render"/> was called with</summary>
        private Camera _lastCamera;

        private VertexBuffer _vb;
        private XMaterial _material1, _material2;

        private CpuParticlePreset _preset;
        private Vector3 _velocity;
        #endregion

        #region Properties
        /// <summary>
        /// The configuration of this particle system.
        /// </summary>
        [Browsable(false)]
        public CpuParticlePreset Preset { get { return _preset; } set { _preset = value; } }

        /// <summary>
        /// The base velocity of all particles spawned by this particle system
        /// </summary>
        [Description("The base velocity of all particles spawned by this particle system"), Category("Behavior")]
        public Vector3 Velocity { get { return _velocity; } set { _velocity = value; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new particle system.
        /// </summary>
        /// <param name="preset">The initial configuration of the particle system.</param>
        public CpuParticleSystem(CpuParticlePreset preset)
        {
            #region Sanity checks
            if (preset == null) throw new ArgumentNullException("preset");
            #endregion

            Pickable = false;
            RenderIn = ViewType.NormalOnly;
            _preset = preset;

            // Set the highest alpha blending value here to get correct sorting behaviour
            Alpha = preset.Particle1Alpha > preset.Particle2Alpha ? preset.Particle1Alpha : preset.Particle2Alpha;

            // ReSharper disable CompareOfFloatsByEqualityOperator
            // Calculate bounding sphere
            // If any of the four values is set to infinite...
            if (preset.LowerParameters1.LifeTime == CpuParticleParameters.InfiniteFlag || preset.UpperParameters1.LifeTime == CpuParticleParameters.InfiniteFlag ||
                preset.LowerParameters2.LifeTime == CpuParticleParameters.InfiniteFlag || preset.UpperParameters2.LifeTime == CpuParticleParameters.InfiniteFlag)
            { // ... use rather wild approximation
                float maxDistance = preset.RandomAcceleration * preset.EmitterRepelRange * preset.EmitterRepelSpeed / 10;

                BoundingSphere = new BoundingSphere(new Vector3(), maxDistance / 2);
            }
                // ReSharper restore CompareOfFloatsByEqualityOperator

            else
            { // ... otherwise sum up the maximums
                float maxLifeTime = preset.UpperParameters1.LifeTime + preset.UpperParameters2.LifeTime;
                float minFriction = preset.LowerParameters1.Friction + preset.LowerParameters2.Friction;
                float maxDistance = preset.SpawnRadius + (preset.Gravity.Length() - minFriction) * maxLifeTime * maxLifeTime / 2f;

                BoundingSphere = new BoundingSphere(
                    // Move half the way in gravity direction, handle first half of replling force
                    Vector3.Normalize(preset.Gravity) * (maxDistance / 2f - preset.EmitterRepelRange),
                    // Encapsulate the entire gravity area, handle secibd half of replling force
                    maxDistance / 2 + preset.EmitterRepelRange);
            }

            // Make the update function execute once per frame
            PreRender += Update;
        }
        #endregion

        //--------------------//

        #region Update system
        private bool _firstUpdate = true;

        /// <summary>
        /// Updates the particle system
        /// </summary>
        private void Update()
        {
            float lodFactor = GetLodFactor();
            double elapsedTime = GetElapsedTime();

            using (new ProfilerEvent("Update particles"))
            {
                // Update in intervals to increase accuracy
                while (elapsedTime > 0.06f)
                {
                    UpdateParticles(0.06f, lodFactor);
                    elapsedTime -= 0.06f;
                }
                UpdateParticles((float)elapsedTime, lodFactor);
            }
        }

        private double GetElapsedTime()
        {
            double elapsedTime;
            if (_firstUpdate)
            {
                // Fast forward the elapsed time for the first render
                elapsedTime = _preset.WarmupTime;
                _firstUpdate = false;
            }
            else
            {
                elapsedTime = Engine.LastFrameGameTime * _preset.Speed;

                // Prevent extremly large updates
                float maxUpdate = _preset.WarmupTime;
                if (maxUpdate < 0.2f) maxUpdate = 0.2f;
                if (elapsedTime > maxUpdate) elapsedTime = maxUpdate;
            }
            return elapsedTime;
        }

        private float GetLodFactor()
        {
            float lodFactor;
            if (VisibilityDistance > 0 && _lastCamera != null)
            {
                float distanceToFade = VisibilityDistance - (float)(Position - _lastCamera.Position).Length();
                lodFactor = (distanceToFade * distanceToFade) / (VisibilityDistance * VisibilityDistance);
            }
            else lodFactor = 1;

            switch (Engine.Effects.ParticleSystemQuality)
            {
                case Quality.Low:
                    lodFactor *= 0.2f;
                    break;
                case Quality.Medium:
                    lodFactor *= 0.6f;
                    break;
                case Quality.High:
                    lodFactor *= 1.0f;
                    break;
            }
            return lodFactor;
        }
        #endregion

        #region Update particles
        private float _newParticlesBuffer;

        /// <summary>
        /// Updates all particles in the system
        /// </summary>
        /// <param name="elapsedTime">How many seconds have passed since the last call of this function</param>
        /// <param name="lodFactor">A factor by which sizes and spawn-rates are multiplied for level-of-detail purposes</param>
        private void UpdateParticles(float elapsedTime, float lodFactor)
        {
            UpdateFirstLifeParticles(elapsedTime);
            UpdateSecondLifeParticles(elapsedTime);

            // Don't add new particles if the maximum amount has been reached
            if (_firstLifeParticles.Count + _secondLifeParticles.Count >= _preset.MaxParticles * lodFactor) return;

            SpawnNewParticles(elapsedTime, lodFactor);
        }

        private void UpdateFirstLifeParticles(float elapsedTime)
        {
            // Similar to a foreach loop, but allows you to remove elements while iterating
            _firstLifeParticles.RemoveWhere(particle =>
            {
                UpdateParticle(particle, elapsedTime);

                // Identify particles that have died
                if (!particle.Alive)
                {
                    _deadParticles.Push(particle);
                    return true; // Remove from the pool
                }

                // Identify particles that have begun their second life
                if (particle.SecondLife)
                {
                    particle.Color = RandomUtils.GetRandomColor(_preset.LowerParameters2.Color, _preset.UpperParameters2.Color);
                    _secondLifeParticles.Add(particle);
                    return true; // Remove from the pool
                }

                return false; // Keep in the pool
            });
        }

        private void UpdateSecondLifeParticles(float elapsedTime)
        {
            // Similar to a foreach loop, but allows you to remove elements while iterating
            _secondLifeParticles.RemoveWhere(particle =>
            {
                UpdateParticle(particle, elapsedTime);

                // Identify particles that have died
                if (!particle.Alive)
                {
                    _deadParticles.Push(particle);
                    return true; // Remove from the pool
                }

                return false; // Keep in the pool
            });
        }

        private void SpawnNewParticles(float elapsedTime, float lodFactor)
        {
            // Use a float buffer so that decimals don't get lost
            _newParticlesBuffer += elapsedTime * _preset.SpawnRate * lodFactor;
            while (_newParticlesBuffer >= 1)
            {
                _newParticlesBuffer--;
                AddParticle(1 / (float)Math.Sqrt(lodFactor - 0.05f));
            }
        }
        #endregion

        #region Update particle
        /// <summary>
        /// Updates a specific particle
        /// </summary>
        /// <param name="particle">The particle to update</param>
        /// <param name="elapsedTime">How many seconds have passed since the update of this particle</param>
        private void UpdateParticle(CpuParticle particle, float elapsedTime)
        {
            // Apply gravity and base velocity
            particle.Velocity += (_preset.Gravity + _velocity) * elapsedTime;

            // Apply random acceleration
            if (_preset.RandomAcceleration > 0)
            {
                float randomFactor = _preset.RandomAcceleration / 1.732f /* approx. sqrt(3) */;
                particle.Velocity += RandomUtils.GetRandomVector3(
                    new Vector3(-randomFactor, -randomFactor, -randomFactor),
                    new Vector3(randomFactor, randomFactor, randomFactor));
            }

            Vector3 particlePosition = particle.Position.ApplyOffset(Position); // Subtract particle system position
            float particleDistance = particlePosition.Length();
            Vector3 particleDirection = Vector3.Normalize(particlePosition);

            // Apply emitter repelling force
            if (particleDistance < _preset.EmitterRepelRange)
            {
                float repelFactor = 1 - (particleDistance / _preset.EmitterRepelRange);
                particle.Velocity += particleDirection * repelFactor * _preset.EmitterRepelSpeed * elapsedTime;
            }

            // Apply emitter suction force
            if (particleDistance > _preset.EmitterSuctionRange)
            {
                float suctionFactor = (particleDistance / _preset.EmitterSuctionRange) - 1;
                particle.Velocity -= particleDirection * suctionFactor * _preset.EmitterSuctionSpeed * elapsedTime;
            }

            particle.Update(elapsedTime);
        }
        #endregion

        #region Add particle
        /// <summary>
        /// Adds a new particle with random values
        /// </summary>
        /// <param name="lodFactor">A factor by which sizes are multiplied for level-of-detail purposes</param>
        private void AddParticle(float lodFactor)
        {
            AddParticle(
                Position + GetSpawnPosition(),
                GetFirstLifeParameters(lodFactor),
                GetSecondLifeParameters(lodFactor));
        }

        private Vector3 GetSpawnPosition()
        {
            float randomSpawnRadius = RandomUtils.GetRandomFloat(0, _preset.SpawnRadius);
            var rotationMatrix = Matrix.RotationYawPitchRoll(
                RandomUtils.GetRandomFloat(0, 2 * (float)Math.PI), 0,
                RandomUtils.GetRandomFloat(0, 2 * (float)Math.PI));

            return Vector3.TransformCoordinate(new Vector3(randomSpawnRadius, 0, 0), rotationMatrix);
        }

        private CpuParticleParametersStruct GetFirstLifeParameters(float lodFactor)
        {
            return new CpuParticleParametersStruct
            {
                LifeTime =
                    // If any of the two values is set to infinite...
                    (_preset.LowerParameters1.LifeTime == CpuParticleParameters.InfiniteFlag || _preset.UpperParameters1.LifeTime == CpuParticleParameters.InfiniteFlag)
                        // ... use no limit, ...    
                        ? CpuParticleParameters.InfiniteFlag
                        // ... otherwise select a random intermediate value
                        : RandomUtils.GetRandomFloat(_preset.LowerParameters1.LifeTime, _preset.UpperParameters1.LifeTime),
                Size = (RandomUtils.GetRandomFloat(_preset.LowerParameters1.Size, _preset.UpperParameters1.Size) * lodFactor),
                DeltaSize = RandomUtils.GetRandomFloat(_preset.LowerParameters1.DeltaSize, _preset.UpperParameters1.DeltaSize),
                Friction = RandomUtils.GetRandomFloat(_preset.LowerParameters1.Friction, _preset.UpperParameters1.Friction),
                Color = RandomUtils.GetRandomColor(_preset.LowerParameters1.Color, _preset.UpperParameters1.Color),
                DeltaColor = RandomUtils.GetRandomFloat(_preset.LowerParameters1.DeltaColor, _preset.UpperParameters1.DeltaColor)
            };
        }

        private CpuParticleParametersStruct GetSecondLifeParameters(float lodFactor)
        {
            return new CpuParticleParametersStruct
            {
                LifeTime =
                    // If any of the two values is set to infinite...
                    (_preset.LowerParameters2.LifeTime == CpuParticleParameters.InfiniteFlag || _preset.UpperParameters2.LifeTime == CpuParticleParameters.InfiniteFlag)
                        // ... use no limit, ...
                        ? CpuParticleParameters.InfiniteFlag
                        // ... otherwise select a random intermediate value
                        : RandomUtils.GetRandomFloat(_preset.LowerParameters2.LifeTime, _preset.UpperParameters2.LifeTime),
                Size = (RandomUtils.GetRandomFloat(_preset.LowerParameters2.Size, _preset.UpperParameters2.Size) * lodFactor),
                DeltaSize = RandomUtils.GetRandomFloat(_preset.LowerParameters2.DeltaSize, _preset.UpperParameters2.DeltaSize),
                Friction = RandomUtils.GetRandomFloat(_preset.LowerParameters2.Friction, _preset.UpperParameters2.Friction),
                Color = RandomUtils.GetRandomColor(_preset.LowerParameters2.Color, _preset.UpperParameters2.Color),
                DeltaColor = RandomUtils.GetRandomFloat(_preset.LowerParameters2.DeltaColor, _preset.UpperParameters2.DeltaColor)
            };
        }

        /// <summary>
        /// Adds a new particle
        /// </summary>
        /// <param name="position">The initial position of the particle</param>
        /// <param name="parameters1">The initial configuration of this particle</param>
        /// <param name="parameters2">The configuration this particle will take in its "second life"</param>
        private void AddParticle(DoubleVector3 position, CpuParticleParametersStruct parameters1, CpuParticleParametersStruct parameters2)
        {
            if (_deadParticles.Count > 0)
            {
                CpuParticle particle = _deadParticles.Pop();
                particle.Alive = true;
                particle.Position = position;
                particle.Velocity = default(Vector3);
                particle.Parameters1 = parameters1;
                particle.Parameters2 = parameters2;
                particle.Color = parameters1.Color;
                particle.SecondLife = false;
                _firstLifeParticles.Add(particle);
            }
            else _firstLifeParticles.Add(new CpuParticle(position, parameters1, parameters2));
        }
        #endregion

        //--------------------//

        #region Texture helper
        /// <summary>
        /// Loads the textures for the <see cref="CpuParticle"/> sprites
        /// </summary>
        private void UpdateSpriteTextures()
        {
            // Release previous textures
            _material1.ReleaseReference();
            _material2.ReleaseReference();

            #region First-life material
            // Start with empty material
            _material1 = XMaterial.DefaultMaterial;
            if (!string.IsNullOrEmpty(_preset.Particle1Texture))
            {
                // Get texture path
                string texture = Path.Combine("Particles", _preset.Particle1Texture);

                // Check if texture path is valid
                if (ContentManager.FileExists("Textures", texture, true))
                {
                    _material1 = new XMaterial(XTexture.Get(Engine, texture));
                    _material1.HoldReference();
                }
            }
            #endregion

            #region Second-life material
            _material2 = XMaterial.DefaultMaterial;
            if (!string.IsNullOrEmpty(_preset.Particle2Texture))
            {
                // Get texture path
                string texture = Path.Combine("Particles", _preset.Particle2Texture);

                // Check if texture path is valid
                if (ContentManager.FileExists("Textures", texture, true))
                {
                    _material2 = new XMaterial(XTexture.Get(Engine, texture));
                    _material2.HoldReference();
                }
            }
            #endregion

            _preset.TexturesDirty = false;
        }
        #endregion

        #region Render
        /// <inheritdoc />
        internal override void Render(Camera camera, GetLights lights)
        {
            base.Render(camera, lights);

            _lastCamera = camera;

            // Reload textures when they change
            if (_preset.TexturesDirty) UpdateSpriteTextures();

            // Never light a particle system
            SurfaceEffect = SurfaceEffect.Plain;

            if (camera.ClipPlane != default(DoublePlane))
            {
                // Rendering without shaders, so the clip plane is in world space
                Engine.State.UserClipPlane = camera.EffectiveClipPlane;
            }

            RenderParticles(camera);

            // Restore defaults
            Engine.State.UserClipPlane = default(Plane);
            Engine.State.ZBufferMode = ZBufferMode.Normal;
        }

        private void RenderParticles(Camera camera)
        {
            // Set shared settings for all particles
            Engine.State.FfpLighting = true;
            Engine.State.SetVertexBuffer(_vb);
            Engine.State.ZBufferMode = ZBufferMode.ReadOnly;
            bool fog = Engine.State.Fog;
            Engine.State.Fog = false;

            if (_material1.DiffuseMaps[0] != null)
            {
                using (new ProfilerEvent("Render first-life particles"))
                {
                    Engine.State.AlphaBlend = _preset.Particle1Alpha;
                    Engine.State.SetTexture(_material1.DiffuseMaps[0]);
                    _firstLifeParticles.ForEach(particle => particle.Render(Engine, camera));
                }
            }

            if (_material2.DiffuseMaps[0] != null && _secondLifeParticles.Count > 0)
            {
                using (new ProfilerEvent("Render second-life particles"))
                {
                    Engine.State.AlphaBlend = _preset.Particle2Alpha;
                    Engine.State.SetTexture(_material2.DiffuseMaps[0]);
                    _secondLifeParticles.ForEach(particle => particle.Render(Engine, camera));
                }
            }

            Engine.State.FfpLighting = false;
            Engine.State.Fog = fog;
        }
        #endregion

        //--------------------//

        #region Engine
        protected override void OnEngineSet()
        {
            base.OnEngineSet();

            // Create single vertex for all particles
            var vertexes = new[]
            {
                new PositionTextured(new Vector3(-0.5f, 0.5f, 0), 0, 0),
                new PositionTextured(new Vector3(0.5f, 0.5f, 0), 1, 0),
                new PositionTextured(new Vector3(-0.5f, -0.5f, 0), 0, 1),
                new PositionTextured(new Vector3(0.5f, -0.5f, 0), 1, 1)
            };
            _vb = BufferHelper.CreateVertexBuffer(Engine.Device, vertexes, PositionTextured.Format);

            // Load textures for the first time
            UpdateSpriteTextures();
        }
        #endregion

        #region Dispose
        /// <inheritdoc/>
        protected override void OnDispose()
        {
            try
            {
                _material1.ReleaseReference();
                _material2.ReleaseReference();

                if (_vb != null) _vb.Dispose();
            }
            finally
            {
                base.OnDispose();
            }
        }
        #endregion
    }
}
