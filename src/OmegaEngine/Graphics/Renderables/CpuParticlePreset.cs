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
using System.Xml.Serialization;
using NanoByte.Common.Storage;
using OmegaEngine.Foundation.Storage;
using System.Numerics;
using OmegaEngine.Foundation.Geometry;
using Vortice.Mathematics;

namespace OmegaEngine.Graphics.Renderables;

/// <summary>
/// A set of information about a particle system as a whole
/// </summary>
[XmlRoot("CpuParticlePreset")] // Suppress XMLSchema declarations (no inheritance used for properties)
public record CpuParticlePreset
{
    /// <summary>
    /// Flag indicating that <see cref="Particle1Texture"/> and/or <see cref="Particle2Texture"/> have changed and need to be reloaded
    /// </summary>
    internal bool TexturesDirty;

    /// <summary>
    /// The lower values of the range of parameters used to spawn new particles
    /// </summary>
    [Browsable(false)]
    public CpuParticleParameters LowerParameters1 { get; init; } = new() {LifeTime = 2, Size = 10}; // Default value not part of serialization contract

    /// <summary>
    /// The upper values of the range of parameters used to spawn new particles
    /// </summary>
    [Browsable(false)]
    public CpuParticleParameters UpperParameters1 { get; init; } = new() {LifeTime = 2, Size = 10}; // Default value not part of serialization contract

    /// <summary>
    /// <c>true</c> <see cref="CpuParticleParameters.LifeTime"/> is set to <see cref="CpuParticleParameters.InfiniteFlag"/> for <see cref="LowerParameters1"/> or <see cref="UpperParameters1"/>.
    /// </summary>
    [Browsable(false)]
    public bool InfiniteLifetime1 => LowerParameters1.LifeTime == CpuParticleParameters.InfiniteFlag || UpperParameters1.LifeTime == CpuParticleParameters.InfiniteFlag;

    /// <summary>
    /// The lower values of the range of parameters used to start particles' "second life"
    /// </summary>
    [Browsable(false)]
    public CpuParticleParameters LowerParameters2 { get; init; } = new();

    /// <summary>
    /// The upper values of the range of parameters used to start particles' "second life"
    /// </summary>
    [Browsable(false)]
    public CpuParticleParameters UpperParameters2 { get; init; } = new();

    /// <summary>
    /// <c>true</c> <see cref="CpuParticleParameters.LifeTime"/> is set to <see cref="CpuParticleParameters.InfiniteFlag"/> for <see cref="LowerParameters2"/> or <see cref="UpperParameters2"/>.
    /// </summary>
    [Browsable(false)]
    public bool InfiniteLifetime2 => LowerParameters2.LifeTime == CpuParticleParameters.InfiniteFlag || UpperParameters2.LifeTime == CpuParticleParameters.InfiniteFlag;

    /// <summary>
    /// How many new particles shall be spawned per second
    /// </summary>
    [Category("Spawn"), Description("How many new particles shall be spawned per second")]
    public float SpawnRate { get; set; } = 10; // Default value not part of serialization contract

    /// <summary>
    /// The largest distance from the emitter at which particle shall be spawned
    /// </summary>
    [DefaultValue(0f), Category("Spawn"), Description("The largest distance from the emitter at which particle shall be spawned")]
    public float SpawnRadius { get; init; }

    /// <summary>
    /// The maximum number particles in existence at any one point in time
    /// </summary>
    [DefaultValue(512), Category("Spawn"), Description("The maximum number particles in existence at any one point in time")]
    public int MaxParticles { get; set; } = 512;

    /// <summary>
    /// How far the emitter's repelling force can reach
    /// </summary>
    [DefaultValue(0f), Category("Acceleration"), Description("How far the emitter's repelling force can reach")]
    public float EmitterRepelRange { get; init; }

    /// <summary>
    /// How fast particles will be pushed away from the emitter's centre
    /// </summary>
    [DefaultValue(0f), Category("Acceleration"), Description("How fast particles will be pushed away from the emitter's centre")]
    public float EmitterRepelSpeed { get; init; }

    /// <summary>
    /// From where the emitter suction force starts to act
    /// </summary>
    [DefaultValue(32768f), Category("Acceleration"), Description("When the emitter suction force starts")]
    public float EmitterSuctionRange { get; set; } = 32768;

    /// <summary>
    /// How fast particles will be sucked back to the emitter's centre
    /// </summary>
    [DefaultValue(0f), Category("Acceleration"), Description("How fast particles will be sucked back to the emitter's centre")]
    public float EmitterSuctionSpeed { get; set; }

    /// <summary>
    /// A permanent acceleration force applied to all particles
    /// </summary>
    [Category("Acceleration"), Description("A permanent acceleration force applied to all particles")]
    public Vector3 Gravity { get; init; }

    /// <summary>
    /// Randomly accelerate particles with up to the specified speed
    /// </summary>
    [DefaultValue(0f), Category("Acceleration"), Description("Randomly accelerate particles with up to the specified speed")]
    public float RandomAcceleration { get; init; }

    /// <summary>
    /// A factor by which all elapsed times are multiplied
    /// </summary>
    [DefaultValue(1f), Category("General"), Description("A factor by which all elapsed times are multiplied")]
    public float Speed { get; set; } = 1;

    /// <summary>
    /// How many seconds to "fast forward" the particle system before it's render the first time
    /// </summary>
    [DefaultValue(0f), Category("General"), Description("How many seconds to \"fast forward\" the particle system before it's render the first time")]
    public float WarmupTime { get; set; }

    private string _particleTexture1 = "fire.png"; // Default value not part of serialization contract

    /// <summary>
    /// The ID of the texture to place on the particles
    /// </summary>
    [Category("Render"), Description("The ID of the texture to place on the particles")]
    public string Particle1Texture { get => _particleTexture1; set => value.To(ref _particleTexture1, ref TexturesDirty); }

    /// <summary>
    /// The level of transparency from 0 (solid) to 255 (invisible),
    /// <see cref="EngineState.AlphaChannel"/>, <see cref="EngineState.BinaryAlphaChannel"/> or <see cref="EngineState.AdditiveBlending"/>
    /// </summary>
    [Category("Render"), Description("The level of transparency from 0 (solid) to 255 (invisible), 256 for alpha channel, -256 for binary alpha channel, 257 for additive blending")]
    public int Particle1Alpha { get; init; } = EngineState.AdditiveBlending; // Default value not part of serialization contract

    private string _particleTexture2 = "";

    /// <summary>
    /// The ID of the texture to place on the particles during their "second life"
    /// </summary>
    [DefaultValue(""), Category("Render"), Description("The ID of the texture to place on the particles during their \"second life\"")]
    public string Particle2Texture { get => _particleTexture2; set => value.To(ref _particleTexture2, ref TexturesDirty); }

    /// <summary>
    /// The level of transparency from 0 (solid) to 255 (invisible) for particles' "second life",
    /// <see cref="EngineState.AlphaChannel"/>, <see cref="EngineState.BinaryAlphaChannel"/> or <see cref="EngineState.AdditiveBlending"/>
    /// </summary>
    [DefaultValue(0), Category("Render"), Description("The level of transparency from 0 (solid) to 255 (invisible) for particles' \"second life\", 256 for alpha channel, -256 for binary alpha channel, 257 for additive blending")]
    public int Particle2Alpha { get; init; }

    /// <summary>
    /// Calculates a bounding sphere that encompasses all particles that could be created by a particle system with this configuration.
    /// </summary>
    public BoundingSphere CalculateBoundingSphere()
    {
        // If any of the four values is set to infinite...
        if (LowerParameters1.LifeTime == CpuParticleParameters.InfiniteFlag || UpperParameters1.LifeTime == CpuParticleParameters.InfiniteFlag ||
            LowerParameters2.LifeTime == CpuParticleParameters.InfiniteFlag || UpperParameters2.LifeTime == CpuParticleParameters.InfiniteFlag)
        { // ... use a generous approximation: spawn area + repel range + 1s of random acceleration
            float maxDistance = SpawnRadius + EmitterRepelRange + RandomAcceleration;

            return new(center: new(), radius: Math.Max(maxDistance, MaxParticleRadius));
        }
        else
        { // ... otherwise follow the longest-lived, least damped particle through both lives
            float gravity = Gravity.Length();
            var (firstDistance, firstVelocity) = Drift(0, gravity, LowerParameters1.Friction, UpperParameters1.LifeTime);
            var (secondDistance, _) = Drift(firstVelocity, gravity, LowerParameters2.Friction, UpperParameters2.LifeTime);
            float maxDistance = SpawnRadius + firstDistance + secondDistance;

            // Speed a particle can have picked up while being pushed through the repel range, which carries it on in any direction
            float repelVelocity = (float)Math.Sqrt(2 * Math.Max(EmitterRepelSpeed, 0) * Math.Max(EmitterRepelRange, 0));
            var (firstRepel, carriedRepel) = Drift(repelVelocity, 0, LowerParameters1.Friction, UpperParameters1.LifeTime);
            var (secondRepel, _) = Drift(carriedRepel, 0, LowerParameters2.Friction, UpperParameters2.LifeTime);

            return new(
                // Move half the way in gravity direction, handle first half of repelling force
                center: VectorMath.NormalizeOrZero(Gravity) * (maxDistance / 2f - EmitterRepelRange),
                // Encapsulate the entire gravity area, handle second half of repelling force
                radius: maxDistance / 2 + EmitterRepelRange + firstRepel + secondRepel);
        }
    }

    /// <summary>
    /// The radius of a sphere enclosing the largest particle billboard at spawn time, in any orientation.
    /// </summary>
    private float MaxParticleRadius
        => Math.Max(
            Math.Max(LowerParameters1.Size, UpperParameters1.Size),
            Math.Max(LowerParameters2.Size, UpperParameters2.Size)) * (float)Math.Sqrt(0.5); // Half the diagonal of a unit quad

    /// <summary>
    /// Calculates how far a particle travels under constant acceleration while friction slows it down.
    /// </summary>
    /// <param name="velocity">The particle's speed at the start, along the acceleration.</param>
    /// <param name="acceleration">The constant acceleration.</param>
    /// <param name="friction">The share of its velocity the particle loses per second.</param>
    /// <param name="time">How long the particle travels.</param>
    /// <returns>The distance travelled and the particle's speed at the end.</returns>
    private static (float Distance, float Velocity) Drift(float velocity, float acceleration, float friction, float time)
    {
        if (time <= 0) return (0, velocity);
        if (friction <= 1e-6f) return (velocity * time + acceleration * time * time / 2, velocity + acceleration * time);

        float terminal = acceleration / friction;
        float decay = (float)Math.Exp(-friction * time);
        return (terminal * time + (velocity - terminal) * (1 - decay) / friction, terminal + (velocity - terminal) * decay);
    }

    /// <summary>
    /// Loads a preset from an XML file via the <see cref="ContentManager"/>.
    /// </summary>
    /// <param name="id">The ID of the XML file to load</param>
    /// <returns>The loaded preset</returns>
    /// <exception cref="FileNotFoundException">The specified file could not be found.</exception>
    /// <exception cref="IOException">There was an error reading the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
    /// <exception cref="InvalidOperationException">A problem occurred while deserializing the XML data.</exception>
    public static CpuParticlePreset FromContent(string id)
    {
        using var stream = ContentManager.GetFileStream("Graphics/CpuParticleSystem", id);
        return XmlStorage.LoadXml<CpuParticlePreset>(stream);
    }
}
