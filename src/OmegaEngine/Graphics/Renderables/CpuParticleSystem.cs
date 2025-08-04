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
using NanoByte.Common;
using OmegaEngine.Assets;
using OmegaEngine.Collections;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.VertexDecl;
using OmegaEngine.Storage;
using OmegaEngine.Values;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.Renderables;

/// <summary>
/// A particle system whose particles are tracked by the CPU.
/// </summary>
public class CpuParticleSystem : PositionableRenderable
{
    #region Variables
    private readonly Pool<CpuParticle>
        _firstLifeParticles = new(),
        _secondLifeParticles = new();

    /// <summary>A free-list of <see cref="CpuParticle"/>s to be reused</summary>
    private readonly Stack<CpuParticle> _deadParticles = new();

    /// <summary>The last <see cref="Camera"/> <see cref="Render"/> was called with</summary>
    private Camera _lastCamera;

    private VertexBuffer _vb;
    private XMaterial _material1, _material2;
    #endregion

    #region Properties
    /// <summary>
    /// The configuration of this particle system.
    /// </summary>
    [Browsable(false)]
    public CpuParticlePreset Preset { get; set; }

    /// <summary>
    /// The base velocity of all particles spawned by this particle system
    /// </summary>
    [Description("The base velocity of all particles spawned by this particle system"), Category("Behavior")]
    public Vector3 Velocity { get; set; }

    /// <summary>
    /// The position with the <see cref="PositionableRenderable.PreTransform"/> applied
    /// </summary>
    private DoubleVector3 PreTransformedPosition => Position + Vector3.TransformCoordinate(new(), PreTransform * Matrix.RotationQuaternion(Rotation));
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new particle system.
    /// </summary>
    /// <param name="preset">The initial configuration of the particle system.</param>
    public CpuParticleSystem(CpuParticlePreset preset)
    {
        Pickable = false;
        RenderIn = ViewType.NormalOnly;
        Preset = preset ?? throw new ArgumentNullException(nameof(preset));

        // Set the highest alpha blending value here to get correct sorting behaviour
        Alpha = preset.Particle1Alpha > preset.Particle2Alpha ? preset.Particle1Alpha : preset.Particle2Alpha;

        // ReSharper disable CompareOfFloatsByEqualityOperator
        // Calculate bounding sphere
        // If any of the four values is set to infinite...
        if (preset.LowerParameters1.LifeTime == CpuParticleParameters.InfiniteFlag || preset.UpperParameters1.LifeTime == CpuParticleParameters.InfiniteFlag ||
            preset.LowerParameters2.LifeTime == CpuParticleParameters.InfiniteFlag || preset.UpperParameters2.LifeTime == CpuParticleParameters.InfiniteFlag)
        { // ... use rather wild approximation
            float maxDistance = preset.RandomAcceleration * preset.EmitterRepelRange * preset.EmitterRepelSpeed / 10;

            BoundingSphere = new BoundingSphere(new(), maxDistance / 2);
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
        if (_firstUpdate)
        { // Fast forward the elapsed time for the first render
            _firstUpdate = false;
            return Preset.WarmupTime;
        }

        float maxStep = Preset.WarmupTime.Clamp(0, 0.2f);
        return (Math.Abs(Engine.LastFrameGameTime) * Preset.Speed).Clamp(0, maxStep);
    }

    private float GetLodFactor()
    {
        float lodFactor;
        if (VisibilityDistance > 0 && _lastCamera != null)
        {
            float distanceToFade = VisibilityDistance - (float)(PreTransformedPosition - _lastCamera.Position).Length();
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
        if (_firstLifeParticles.Count + _secondLifeParticles.Count >= Preset.MaxParticles * lodFactor) return;

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
                particle.Color = RandomUtils.GetRandomColor(Preset.LowerParameters2.Color, Preset.UpperParameters2.Color);
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
        _newParticlesBuffer += elapsedTime * Preset.SpawnRate * lodFactor;
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
        ApplyGlobalForces(particle, elapsedTime);
        ApplyEmitterForces(particle, elapsedTime);
        particle.Update(elapsedTime);
    }

    private void ApplyGlobalForces(CpuParticle particle, float elapsedTime)
    {
        particle.Velocity += (Preset.Gravity + Velocity) * elapsedTime;

        if (Preset.RandomAcceleration > 0)
        {
            float randomFactor = Preset.RandomAcceleration / 1.732f /* approx. sqrt(3) */;
            particle.Velocity += RandomUtils.GetRandomVector3(
                new(-randomFactor, -randomFactor, -randomFactor),
                new(randomFactor, randomFactor, randomFactor));
        }
    }

    private void ApplyEmitterForces(CpuParticle particle, float elapsedTime)
    {
        Vector3 particlePosition = particle.Position.ApplyOffset(PreTransformedPosition);
        float particleDistance = particlePosition.Length();
        Vector3 particleDirection = Vector3.Normalize(particlePosition);

        if (particleDistance < Preset.EmitterRepelRange)
        {
            float repelFactor = 1 - (particleDistance / Preset.EmitterRepelRange);
            particle.Velocity += particleDirection * repelFactor * Preset.EmitterRepelSpeed * elapsedTime;
        }

        if (particleDistance > Preset.EmitterSuctionRange)
        {
            float suctionFactor = (particleDistance / Preset.EmitterSuctionRange) - 1;
            particle.Velocity -= particleDirection * suctionFactor * Preset.EmitterSuctionSpeed * elapsedTime;
        }
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
            PreTransformedPosition + GetSpawnPosition(),
            GetFirstLifeParameters(lodFactor),
            GetSecondLifeParameters(lodFactor));
    }

    private Vector3 GetSpawnPosition()
    {
        float randomSpawnRadius = RandomUtils.GetRandomFloat(0, Preset.SpawnRadius);
        var rotationMatrix = Matrix.RotationYawPitchRoll(
            RandomUtils.GetRandomFloat(0, 2 * (float)Math.PI), 0,
            RandomUtils.GetRandomFloat(0, 2 * (float)Math.PI));

        return Vector3.TransformCoordinate(new(randomSpawnRadius, 0, 0), rotationMatrix);
    }

    private CpuParticleParametersStruct GetFirstLifeParameters(float lodFactor)
    {
        // ReSharper disable CompareOfFloatsByEqualityOperator
        return new()
        {
            LifeTime = Preset.InfiniteLifetime1
                ? CpuParticleParameters.InfiniteFlag
                : RandomUtils.GetRandomFloat(Preset.LowerParameters1.LifeTime, Preset.UpperParameters1.LifeTime),
            Size = (RandomUtils.GetRandomFloat(Preset.LowerParameters1.Size, Preset.UpperParameters1.Size) * lodFactor),
            DeltaSize = RandomUtils.GetRandomFloat(Preset.LowerParameters1.DeltaSize, Preset.UpperParameters1.DeltaSize),
            Friction = RandomUtils.GetRandomFloat(Preset.LowerParameters1.Friction, Preset.UpperParameters1.Friction),
            Color = RandomUtils.GetRandomColor(Preset.LowerParameters1.Color, Preset.UpperParameters1.Color),
            DeltaColor = RandomUtils.GetRandomFloat(Preset.LowerParameters1.DeltaColor, Preset.UpperParameters1.DeltaColor)
        };
        // ReSharper restore CompareOfFloatsByEqualityOperator
    }

    private CpuParticleParametersStruct GetSecondLifeParameters(float lodFactor)
    {
        return new()
        {
            LifeTime = Preset.InfiniteLifetime2
                ? CpuParticleParameters.InfiniteFlag
                : RandomUtils.GetRandomFloat(Preset.LowerParameters2.LifeTime, Preset.UpperParameters2.LifeTime),
            Size = (RandomUtils.GetRandomFloat(Preset.LowerParameters2.Size, Preset.UpperParameters2.Size) * lodFactor),
            DeltaSize = RandomUtils.GetRandomFloat(Preset.LowerParameters2.DeltaSize, Preset.UpperParameters2.DeltaSize),
            Friction = RandomUtils.GetRandomFloat(Preset.LowerParameters2.Friction, Preset.UpperParameters2.Friction),
            Color = RandomUtils.GetRandomColor(Preset.LowerParameters2.Color, Preset.UpperParameters2.Color),
            DeltaColor = RandomUtils.GetRandomFloat(Preset.LowerParameters2.DeltaColor, Preset.UpperParameters2.DeltaColor)
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
            particle.Velocity = default;
            particle.Parameters1 = parameters1;
            particle.Parameters2 = parameters2;
            particle.Color = parameters1.Color;
            particle.SecondLife = false;
            _firstLifeParticles.Add(particle);
        }
        else _firstLifeParticles.Add(new(position, parameters1, parameters2));
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
        if (!string.IsNullOrEmpty(Preset.Particle1Texture))
        {
            // Get texture path
            string texture = Path.Combine("Particles", Preset.Particle1Texture);

            // Check if texture path is valid
            if (ContentManager.FileExists("Textures", texture))
            {
                _material1 = new(XTexture.Get(Engine, texture));
                _material1.HoldReference();
            }
        }
        #endregion

        #region Second-life material
        _material2 = XMaterial.DefaultMaterial;
        if (!string.IsNullOrEmpty(Preset.Particle2Texture))
        {
            // Get texture path
            string texture = Path.Combine("Particles", Preset.Particle2Texture);

            // Check if texture path is valid
            if (ContentManager.FileExists("Textures", texture))
            {
                _material2 = new(XTexture.Get(Engine, texture));
                _material2.HoldReference();
            }
        }
        #endregion

        Preset.TexturesDirty = false;
    }
    #endregion

    #region Render
    /// <inheritdoc/>
    internal override void Render(Camera camera, GetLights? getLights = null)
    {
        base.Render(camera, getLights);

        _lastCamera = camera;

        // Reload textures when they change
        if (Preset.TexturesDirty) UpdateSpriteTextures();

        // Never light a particle system
        SurfaceEffect = SurfaceEffect.Plain;

        if (camera.ClipPlane != default)
        {
            // Rendering without shaders, so the clip plane is in world space
            Engine.State.UserClipPlane = camera.EffectiveClipPlane;
        }

        RenderParticles(camera);

        // Restore defaults
        Engine.State.UserClipPlane = default;
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
                Engine.State.AlphaBlend = Preset.Particle1Alpha;
                Engine.State.SetTexture(_material1.DiffuseMaps[0]);
                _firstLifeParticles.ForEach(particle => particle.Render(Engine, camera));
            }
        }

        if (_material2.DiffuseMaps[0] != null && _secondLifeParticles.Count > 0)
        {
            using (new ProfilerEvent("Render second-life particles"))
            {
                Engine.State.AlphaBlend = Preset.Particle2Alpha;
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
        PositionTextured[] vertexes =
        [
            new(new Vector3(-0.5f, 0.5f, 0), 0, 0),
            new(new Vector3(0.5f, 0.5f, 0), 1, 0),
            new(new Vector3(-0.5f, -0.5f, 0), 0, 1),
            new(new Vector3(0.5f, -0.5f, 0), 1, 1)
        ];
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

            _vb?.Dispose();
        }
        finally
        {
            base.OnDispose();
        }
    }
    #endregion
}
