/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using AwesomeAssertions;
using SlimDX;
using Xunit;

namespace OmegaEngine.Graphics.Renderables;

public class CpuParticlePresetTest
{
    [Theory]
    [InlineData(0, 1)] // No friction
    [InlineData(1, 0.1875)] // Friction stronger than gravity
    [InlineData(1.5, 2)]
    public void BoundingSphereEnclosesTheLongestLivedParticle(float friction, float gravity)
    {
        var preset = new CpuParticlePreset
        {
            LowerParameters1 = new() {LifeTime = 1, Friction = friction},
            UpperParameters1 = new() {LifeTime = 1, Friction = friction},
            Gravity = new(0, 0, -gravity)
        };

        var sphere = preset.CalculateBoundingSphere();
        var end = Simulate(preset.Gravity, friction, lifeTime: 1);
        Vector3.Distance(sphere.Center, end).Should().BeLessThanOrEqualTo(sphere.Radius * 1.01f, "the sphere follows the exact path, which stepping overshoots slightly");
        sphere.Radius.Should().BeLessThan(end.Length(), "the sphere should fit the particle's path rather than enclose it with a wide margin");
    }

    [Fact]
    public void BoundingSphereWithoutGravityIsCentered()
    {
        var preset = new CpuParticlePreset
        {
            LowerParameters1 = new() {LifeTime = 1, Friction = 1},
            UpperParameters1 = new() {LifeTime = 1, Friction = 1},
            SpawnRadius = 0.5f
        };

        var sphere = preset.CalculateBoundingSphere();
        sphere.Center.Should().Be(new Vector3());
        sphere.Radius.Should().BePositive();
    }

    /// <summary>Steps a particle starting at rest the way <see cref="CpuParticleSystem"/> and <see cref="CpuParticle"/> do.</summary>
    private static Vector3 Simulate(Vector3 gravity, float friction, float lifeTime)
    {
        const int steps = 1000;
        float step = lifeTime / steps;
        var position = new Vector3();
        var velocity = new Vector3();
        for (int i = 0; i < steps; i++)
        {
            velocity += gravity * step;
            velocity -= velocity * friction * step;
            position += velocity * step;
        }
        return position;
    }
}
