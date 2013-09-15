/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.IO;
using Common.Storage;
using SlimDX;
using SlimDX.Direct3D9;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Shaders;

namespace OmegaEngine.Graphics.Renderables
{
    /// <summary>
    /// A particle system whose particles are tracked by the GPU).
    /// </summary>
    /// <seealso cref="ParticleShader"/>
    public class GpuParticleSystem : PositionableRenderable
    {
        #region Variables
        private readonly Mesh _particleMesh;
        private readonly ParticleShader _particleShader;
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
        /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
        /// <param name="preset">The initial configuration of the particle system.</param>
        public GpuParticleSystem(Engine engine, GpuParticlePreset preset) : base(engine)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (preset == null) throw new ArgumentNullException("preset");
            #endregion

            Pickable = false;
            RenderIn = ViewType.NormalOnly;
            SurfaceEffect = SurfaceEffect.Shader;
            Preset = preset;
            Alpha = EngineState.AdditivBlending; // Set alpha blending value here to get correct sorting behaviour

            // Load particle mesh
            using (var stream = ContentManager.GetFileStream("Meshes", "Engine/Particles.x"))
                _particleMesh = Mesh.FromStream(engine.Device, stream, MeshFlags.Managed);

            // Load particle shader
            string id = Path.Combine("Shaders", preset.ParticleTexture);
            _particleShader = new ParticleShader(engine, XTexture.Get(engine, id));

            // Calculate bounding sphere
            float maxDistance = preset.SpawnRadius > preset.SystemHeight ? preset.SpawnRadius : preset.SystemHeight;
            BoundingSphere = new BoundingSphere(default(Vector3), maxDistance);
        }
        #endregion

        #region Static access
        /// <summary>
        /// Creates a new particle system based upon a preset file.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to add the particle system into.</param>
        /// <param name="id">The ID of the preset file to load.</param>
        /// <returns>The model that was created.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the specified file could not be found.</exception>
        /// <exception cref="IOException">Thrown if there was an error reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurred while deserializing the XML data.</exception>
        public static GpuParticleSystem FromPreset(Engine engine, string id)
        {
            return new GpuParticleSystem(engine, GpuParticlePreset.FromContent(id));
        }
        #endregion

        //--------------------//

        #region Render
        /// <inheritdoc />
        internal override void Render(Camera camera, GetLights lights)
        {
            base.Render(camera, lights);

            // Set world transform in the engine
            Engine.State.WorldTransform = WorldTransform;

            #region Update shader
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
                if (_particleShader.ParticleTexture != null)
                    _particleShader.ParticleTexture.ReleaseReference();

                string id = Path.Combine("Shaders", Preset.ParticleTexture);
                _particleShader.ParticleTexture =
                    // Check the new texture is available/exists
                    (!string.IsNullOrEmpty(Preset.ParticleTexture) && ContentManager.FileExists("Textures", Path.Combine("Shaders", Preset.ParticleTexture), true))
                        ? XTexture.Get(Engine, id)
                        : null;

                Preset.TextureDirty = false;
            }
            #endregion

            // Disable ZBuffer writing so particles can flow into each other
            Engine.State.ZBufferMode = ZBufferMode.ReadOnly;

            // Render the particle mesh
            RenderHelper(() => _particleMesh.DrawSubset(0), XMaterial.DefaultMaterial, camera, null);

            // Restore defaults
            Engine.State.ZBufferMode = ZBufferMode.Normal;
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
                    if (_particleShader != null) _particleShader.Dispose();
                    if (_particleMesh != null) _particleMesh.Dispose();
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
