/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using AwesomeAssertions;
using Xunit;

namespace OmegaEngine.Graphics.Renderables;

public class CpuParticleSystemTest : EngineTestBase
{
    [Fact]
    public void UpdatesParticlesWithoutThrowing()
    {
        using var system = new PokableParticleSystem {Preset = new CpuParticlePreset()};

        // Setting the engine creates the sprite vertex buffer and loads the sprite textures
        system.Engine = Engine;

        // Warming up and advancing the simulation must not throw
        system.Invoking(s => s.Poke()).Should().NotThrow();
        system.Invoking(s => s.Poke()).Should().NotThrow();
    }

    [Fact]
    public void SpreadsParticlesSpawnedInOneStepAlongTheirPath()
    {
        var parameters = new CpuParticleParameters {LifeTime = 10, Size = 1};
        using var system = new PokableParticleSystem
        {
            Preset = new()
            {
                LowerParameters1 = parameters,
                UpperParameters1 = parameters,
                SpawnRate = 100,
                Gravity = new(0, 0, -1),
                WarmupTime = 0.1f // Two update steps, spawning about ten particles
            },
            LocalSpace = true
        };
        system.Engine = Engine;
        system.ApplyGravity();

        system.Poke();

        var depths = new List<double>();
        system.ForEachFirstLifeParticle(particle => depths.Add(particle.Position.Z));
        depths.Should().HaveCountGreaterThanOrEqualTo(9);
        depths.Should().OnlyHaveUniqueItems(because: "particles spawned within one step must not start out at the same place");
    }

    /// <summary>Exposes the protected per-frame update hook for testing.</summary>
    private sealed class PokableParticleSystem : CpuParticleSystem
    {
        public void Poke() => OnPreRender();

        /// <summary>Takes the preset's gravity into the emitter's frame, as rendering a frame would.</summary>
        public void ApplyGravity() => RecalcRenderTransform();
    }
}
