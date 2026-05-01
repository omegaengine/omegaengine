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
using OmegaEngine.Foundation.Collections;
using OmegaEngine.Foundation;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.VertexDecl;
using OmegaEngine.Foundation.Storage;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.Renderables;

/// <summary>
/// A particle system whose particles are tracked by the CPU.
/// </summary>
public class CpuParticleSystem : PositionableRenderable
{
    private readonly Pool<CpuParticle>
        _firstLifeParticles = new(),
        _secondLifeParticles = new();

    /// <summary>A free-list of <see cref="CpuParticle"/>s to be reused</summary>
    private readonly Stack<CpuParticle> _deadParticles = new();

    private VertexBuffer? _vb;
    private XMaterial _material1, _material2;

    private CpuParticlePreset _preset = new();

    /// <summary>
    /// The configuration of this particle system.
    /// </summary>
    [Browsable(false)]
    public required CpuParticlePreset Preset
    {
        get => _preset;
        set
        {
            Alpha = Math.Max(value.Particle1Alpha, value.Particle2Alpha);
            BoundingSphere = value.CalculateBoundingSphere();
            _preset = value;
        }
    }

    /// <summary>
    /// Controls whether particles are tracked relative to the particle system instead of world space.
    /// </summary>
    /// <remarks>When <c>true</c>, moving the particle system moves all existing particles along with it.</remarks>
    [Description("Controls whether particles are tracked relative to the particle system instead of world space.")]
    public bool LocalSpace { get; set; }

    /// <summary>
    /// Creates a new particle system.
    /// </summary>
    public CpuParticleSystem()
    {
        Pickable = false;
        RenderIn = ViewType.NormalOnly;
    }

    private bool _firstUpdate = true;

    /// <inheritdoc />
    protected override void OnPreRender()
    {
        base.OnPreRender();

        double elapsedTime = GetElapsedTime();

        using (new ProfilerEvent("Update particles"))
        {
            // Update in fixed intervals to increase accuracy
            const float updateInterval = 0.06f;
            while (elapsedTime > updateInterval)
            {
                UpdateParticles(updateInterval);
                elapsedTime -= updateInterval;
            }
            UpdateParticles((float)elapsedTime);
        }
    }

    private double GetElapsedTime()
    {
        if (_firstUpdate)
        { // Fast forward the elapsed time for the first render
            _firstUpdate = false;
            return Preset.WarmupTime;
        }

        float maxStep = Math.Max(Preset.WarmupTime, 0.2f);
        return (Math.Abs(Engine.LastFrameGameTime) * Preset.Speed).Clamp(0, maxStep);
    }

    private float _newParticlesBuffer;

    /// <summary>
    /// Updates all particles in the system
    /// </summary>
    /// <param name="elapsedTime">How many seconds have passed since the last call of this function</param>
    private void UpdateParticles(float elapsedTime)
    {
        UpdateFirstLifeParticles(elapsedTime);
        UpdateSecondLifeParticles(elapsedTime);

        // Don't add new particles if the maximum amount has been reached
        if (_firstLifeParticles.Count + _secondLifeParticles.Count >= Preset.MaxParticles) return;

        SpawnNewParticles(elapsedTime);
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

    private void SpawnNewParticles(float elapsedTime)
    {
        // Use a float buffer so that decimals don't get lost
        _newParticlesBuffer += elapsedTime * Preset.SpawnRate;
        while (_newParticlesBuffer >= 1)
        {
            _newParticlesBuffer--;
            AddParticle();
        }
    }

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
        particle.Velocity += _effectiveGravity * elapsedTime;

        if (Preset.RandomAcceleration > 0)
            particle.Velocity += RandomUtils.GetRandomPointInsideSphere(Preset.RandomAcceleration * (float)Math.Sqrt(elapsedTime));
    }

    private void ApplyEmitterForces(CpuParticle particle, float elapsedTime)
    {
        var particleOffset = LocalSpace ? new() : Position;
        Vector3 particlePosition = particle.Position.ApplyOffset(particleOffset);
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

    /// <summary>
    /// Adds a new particle with random values
    /// </summary>
    /// <param name="lodFactor">A factor by which sizes are multiplied for level-of-detail purposes</param>
    private void AddParticle(float lodFactor = 1)
    {
        var particleOffset = LocalSpace ? new() : Position;
        AddParticle(
            particleOffset + RandomUtils.GetRandomPointInsideSphere(Preset.SpawnRadius),
            GetFirstLifeParameters(lodFactor),
            GetSecondLifeParameters(lodFactor));
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
                _material1 = XTexture.Get(Engine, texture);
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
                _material2 = XTexture.Get(Engine, texture);
                _material2.HoldReference();
            }
        }
        #endregion

        Preset.TexturesDirty = false;
    }

    private Vector3 _effectiveGravity;

    /// <inheritdoc/>
    protected override void RecalcWorldTransform()
    {
        if (!WorldTransformDirty) return;

        base.RecalcWorldTransform();

        _effectiveGravity = Vector3.TransformNormal(Preset.Gravity, WorldTransformCached);
    }

    /// <inheritdoc/>
    internal override void Render(Camera camera, GetEffectiveLights? getEffectiveLights = null)
    {
        base.Render(camera, getEffectiveLights);

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
        Engine.State.SetVertexBuffer(_vb ?? throw new InvalidOperationException());
        Engine.State.ZBufferMode = ZBufferMode.ReadOnly;
        bool fog = Engine.State.Fog;
        Engine.State.Fog = false;

        var renderOffset = LocalSpace ? Position : new();
        if (!PreTransform.IsIdentity)
            renderOffset += Vector3.TransformCoordinate(new(), PreTransform * Matrix.RotationQuaternion(Rotation));

        if (_material1.DiffuseMaps[0] != null)
        {
            using (new ProfilerEvent("Render first-life particles"))
            {
                Engine.State.AlphaBlend = Preset.Particle1Alpha;
                Engine.State.SetTexture(_material1.DiffuseMaps[0]);
                _firstLifeParticles.ForEach(particle => particle.Render(Engine, camera, renderOffset));
            }
        }

        if (_material2.DiffuseMaps[0] != null && _secondLifeParticles.Count > 0)
        {
            using (new ProfilerEvent("Render second-life particles"))
            {
                Engine.State.AlphaBlend = Preset.Particle2Alpha;
                Engine.State.SetTexture(_material2.DiffuseMaps[0]);
                _secondLifeParticles.ForEach(particle => particle.Render(Engine, camera, renderOffset));
            }
        }

        Engine.State.FfpLighting = false;
        Engine.State.Fog = fog;
    }

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
        _vb = Engine.Device.CreateVertexBuffer(vertexes, PositionTextured.Format);

        // Load textures for the first time
        UpdateSpriteTextures();
    }

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
}
