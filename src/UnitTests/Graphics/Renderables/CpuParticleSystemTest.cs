/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using FluentAssertions;
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

    /// <summary>Exposes the protected per-frame update hook for testing.</summary>
    private sealed class PokableParticleSystem : CpuParticleSystem
    {
        public void Poke() => OnPreRender();
    }
}
