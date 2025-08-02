/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.IO;
using SlimDX;
using SlimDX.Direct3D9;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Shaders;
using OmegaEngine.Storage;

namespace OmegaEngine.Graphics.Renderables
{
    /// <summary>
    /// A particle system whose particles are tracked by the GPU).
    /// </summary>
    /// <seealso cref="ParticleShader"/>
    public class GpuParticleSystem : PositionableRenderable
    {
        #region Variables
        private Mesh _particleMesh;
        private ParticleShader _particleShader;
        #endregion

        #region Properties
        /// <summary>
        /// The configuration of this particle system.
        /// </summary>
        [Browsable(false)]
        public GpuParticlePreset Preset { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new particle system.
        /// </summary>
        /// <param name="preset">The initial configuration of the particle system.</param>
        public GpuParticleSystem(GpuParticlePreset preset)
        {
            Pickable = false;
            RenderIn = ViewType.NormalOnly;
            SurfaceEffect = SurfaceEffect.Shader;
            Preset = preset ?? throw new ArgumentNullException(nameof(preset));
            Alpha = EngineState.AdditivBlending; // Set alpha blending value here to get correct sorting behaviour

            // Calculate bounding sphere
            float maxDistance = preset.SpawnRadius > preset.SystemHeight ? preset.SpawnRadius : preset.SystemHeight;
            BoundingSphere = new BoundingSphere(default(Vector3), maxDistance);
        }
        #endregion

        //--------------------//

        #region Render
        /// <inheritdoc/>
        internal override void Render(Camera camera, GetLights getLights = null)
        {
            base.Render(camera, getLights);
            Engine.State.WorldTransform = WorldTransform;

            UpdateShader();

            // Disable ZBuffer writing so particles can flow into each other
            Engine.State.ZBufferMode = ZBufferMode.ReadOnly;

            RenderHelper(() => _particleMesh.DrawSubset(0), XMaterial.DefaultMaterial, camera);

            // Restore defaults
            Engine.State.ZBufferMode = ZBufferMode.Normal;
        }

        private void UpdateShader()
        {
            // Always apply the shader
            SurfaceEffect = SurfaceEffect.Shader;
            SurfaceShader = _particleShader;

            _particleShader.SpawnRadius = Preset.SpawnRadius;
            _particleShader.SystemHeight = Preset.SystemHeight;
            _particleShader.ParticleSpeed = Preset.Movement.Length();
            _particleShader.ParticleSpread = Preset.ParticleSpread;
            _particleShader.ParticleSize = Preset.ParticleSize;
            _particleShader.ParticleShape = Preset.ParticleShape;

            if (Preset.TextureDirty)
            {
                // Release the previous texture
                _particleShader.ParticleTexture?.ReleaseReference();

                string id = Path.Combine("Shaders", Preset.ParticleTexture);
                _particleShader.ParticleTexture =
                    // Check the new texture is available/exists
                    (!string.IsNullOrEmpty(Preset.ParticleTexture) && ContentManager.FileExists("Textures", Path.Combine("Shaders", Preset.ParticleTexture)))
                        ? XTexture.Get(Engine, id)
                        : null;

                Preset.TextureDirty = false;
            }
        }
        #endregion

        //--------------------//

        #region Engine
        protected override void OnEngineSet()
        {
            base.OnEngineSet();

            // Load particle mesh
            using (var stream = ContentManager.GetFileStream("Meshes", "Engine/Particles.x"))
                _particleMesh = Mesh.FromStream(Engine.Device, stream, MeshFlags.Managed);

            // Load particle shader
            string id = Path.Combine("Shaders", Preset.ParticleTexture);
            _particleShader = new(XTexture.Get(Engine, id));
        }
        #endregion

        #region Dispose
        /// <inheritdoc/>
        protected override void OnDispose()
        {
            try
            {
                _particleShader?.Dispose();
                _particleMesh?.Dispose();
            }
            finally
            {
                base.OnDispose();
            }
        }
        #endregion
    }
}
