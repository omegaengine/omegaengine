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
using NanoByte.Common;
using NanoByte.Common.Storage;
using OmegaEngine.Foundation.Storage;
using SlimDX;

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
    public CpuParticleParameters LowerParameters1 { get; set; } = new() {LifeTime = 2, Size = 10}; // Default value not part of serialization contract

    /// <summary>
    /// The upper values of the range of parameters used to spawn new particles
    /// </summary>
    [Browsable(false)]
    public CpuParticleParameters UpperParameters1 { get; set; } = new() {LifeTime = 2, Size = 10}; // Default value not part of serialization contract

    /// <summary>
    /// <c>true</c> <see cref="CpuParticleParameters.LifeTime"/> is set to <see cref="CpuParticleParameters.InfiniteFlag"/> for <see cref="LowerParameters1"/> or <see cref="UpperParameters1"/>.
    /// </summary>
    [Browsable(false)]
    public bool InfiniteLifetime1 => LowerParameters1.LifeTime == CpuParticleParameters.InfiniteFlag || UpperParameters1.LifeTime == CpuParticleParameters.InfiniteFlag;

    /// <summary>
    /// The lower values of the range of parameters used to start particles' "second life"
    /// </summary>
    [Browsable(false)]
    public CpuParticleParameters LowerParameters2 { get; set; } = new();

    /// <summary>
    /// The upper values of the range of parameters used to start particles' "second life"
    /// </summary>
    [Browsable(false)]
    public CpuParticleParameters UpperParameters2 { get; set; } = new();

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
    public float SpawnRadius { get; set; }

    /// <summary>
    /// The maximum number particles in existence at any one point in time
    /// </summary>
    [DefaultValue(512), Category("Spawn"), Description("The maximum number particles in existence at any one point in time")]
    public int MaxParticles { get; set; } = 512;

    /// <summary>
    /// How far the emitter's repelling force can reach
    /// </summary>
    [DefaultValue(0f), Category("Acceleration"), Description("How far the emitter's repelling force can reach")]
    public float EmitterRepelRange { get; set; }

    /// <summary>
    /// How fast particles will be pushed away from the emitter's centre
    /// </summary>
    [DefaultValue(0f), Category("Acceleration"), Description("How fast particles will be pushed away from the emitter's centre")]
    public float EmitterRepelSpeed { get; set; }

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
    public Vector3 Gravity { get; set; }

    /// <summary>
    /// Randomly accelerate particles with up to the specified speed
    /// </summary>
    [DefaultValue(0f), Category("Acceleration"), Description("Randomly accelerate particles with up to the specified speed")]
    public float RandomAcceleration { get; set; }

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
    /// <see cref="EngineState.AlphaChannel"/>, <see cref="EngineState.BinaryAlphaChannel"/> or <see cref="EngineState.AdditivBlending"/>
    /// </summary>
    [Category("Render"), Description("The level of transparency from 0 (solid) to 255 (invisible), 256 for alpha channel, -256 for binary alpha channel, 257 for additive blending")]
    public int Particle1Alpha { get; set; } = EngineState.AdditivBlending; // Default value not part of serialization contract

    private string? _particleTexture2;

    /// <summary>
    /// The ID of the texture to place on the particles during their "second life"
    /// </summary>
    [DefaultValue(""), Category("Render"), Description("The ID of the texture to place on the particles during their \"second life\"")]
    public string Particle2Texture { get => _particleTexture2; set => value.To(ref _particleTexture2, ref TexturesDirty); }

    /// <summary>
    /// The level of transparency from 0 (solid) to 255 (invisible) for particles' "second life",
    /// <see cref="EngineState.AlphaChannel"/>, <see cref="EngineState.BinaryAlphaChannel"/> or <see cref="EngineState.AdditivBlending"/>
    /// </summary>
    [DefaultValue(0), Category("Render"), Description("The level of transparency from 0 (solid) to 255 (invisible) for particles' \"second life\", 256 for alpha channel, -256 for binary alpha channel, 257 for additive blending")]
    public int Particle2Alpha { get; set; }

    /// <summary>
    /// Calculates a bounding sphere that encompasses all particles that could be created by a particle system with this configuration.
    /// </summary>
    public BoundingSphere CalculateBoundingSphere()
    {
        // If any of the four values is set to infinite...
        if (LowerParameters1.LifeTime == CpuParticleParameters.InfiniteFlag || UpperParameters1.LifeTime == CpuParticleParameters.InfiniteFlag ||
            LowerParameters2.LifeTime == CpuParticleParameters.InfiniteFlag || UpperParameters2.LifeTime == CpuParticleParameters.InfiniteFlag)
        { // ... use rather wild approximation
            float maxDistance = RandomAcceleration * EmitterRepelRange * EmitterRepelSpeed / 10;

            return new(center: new(), radius: maxDistance / 2);
        }
        else
        { // ... otherwise sum up the maximums
            float maxLifeTime = UpperParameters1.LifeTime + UpperParameters2.LifeTime;
            float minFriction = LowerParameters1.Friction + LowerParameters2.Friction;
            float netAcceleration = Math.Max(Gravity.Length() - minFriction, 0);
            float maxDistance = SpawnRadius + netAcceleration * maxLifeTime * maxLifeTime / 2f;

            return new(
                // Move half the way in gravity direction, handle first half of repelling force
                center: Vector3.Normalize(Gravity) * (maxDistance / 2f - EmitterRepelRange),
                // Encapsulate the entire gravity area, handle second half of repelling force
                radius: maxDistance / 2 + EmitterRepelRange);
        }
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
