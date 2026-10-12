/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using AwesomeAssertions;
using System.Numerics;
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

    [Fact]
    public void BoundingSphereOfStationaryParticlesCoversTheirBillboards()
    {
        var preset = new CpuParticlePreset
        {
            LowerParameters1 = new() {LifeTime = CpuParticleParameters.InfiniteFlag, Size = 2},
            UpperParameters1 = new() {LifeTime = CpuParticleParameters.InfiniteFlag, Size = 4}
        };

        var sphere = preset.CalculateBoundingSphere();
        sphere.Radius.Should().BeGreaterThanOrEqualTo(2, "the largest billboard is 4 units wide");
    }

    [Theory]
    [InlineData(1e-3f)]
    [InlineData(1e3f)]
    public void BoundingSphereIndependentOfWorldUnit(float scale)
    {
        CpuParticlePreset[] presets =
        [
            new() // Stationary particles with infinite lifetime
            {
                LowerParameters1 = new() {LifeTime = CpuParticleParameters.InfiniteFlag, Size = 3},
                UpperParameters1 = new() {LifeTime = CpuParticleParameters.InfiniteFlag, Size = 4}
            },
            new() // Drifting particles with infinite lifetime
            {
                LowerParameters1 = new() {LifeTime = CpuParticleParameters.InfiniteFlag, Size = 1},
                UpperParameters1 = new() {LifeTime = CpuParticleParameters.InfiniteFlag, Size = 1},
                SpawnRadius = 2, EmitterRepelRange = 3, RandomAcceleration = 4
            },
            new() // Particles with two finite lives
            {
                LowerParameters1 = new() {LifeTime = 1, Friction = 0.5f, Size = 1},
                UpperParameters1 = new() {LifeTime = 2, Friction = 0.5f, Size = 2},
                LowerParameters2 = new() {LifeTime = 1, Friction = 1, Size = 3},
                UpperParameters2 = new() {LifeTime = 1, Friction = 1, Size = 3},
                SpawnRadius = 0.5f, EmitterRepelRange = 1, EmitterRepelSpeed = 2, Gravity = new(0, 0, -3)
            }
        ];

        foreach (var preset in presets)
        {
            var expected = preset.CalculateBoundingSphere();
            var actual = Scale(preset, scale).CalculateBoundingSphere();

            float tolerance = expected.Radius * scale * 1e-5f;
            actual.Radius.Should().BeApproximately(expected.Radius * scale, tolerance);
            Vector3.Distance(actual.Center, expected.Center * scale).Should().BeLessThanOrEqualTo(tolerance);
        }
    }

    /// <summary>Scales all lengths in a preset uniformly, keeping times and ratios.</summary>
    private static CpuParticlePreset Scale(CpuParticlePreset preset, float scale) => preset with
    {
        LowerParameters1 = Scale(preset.LowerParameters1, scale),
        UpperParameters1 = Scale(preset.UpperParameters1, scale),
        LowerParameters2 = Scale(preset.LowerParameters2, scale),
        UpperParameters2 = Scale(preset.UpperParameters2, scale),
        SpawnRadius = preset.SpawnRadius * scale,
        EmitterRepelRange = preset.EmitterRepelRange * scale,
        EmitterRepelSpeed = preset.EmitterRepelSpeed * scale,
        EmitterSuctionRange = preset.EmitterSuctionRange * scale,
        EmitterSuctionSpeed = preset.EmitterSuctionSpeed * scale,
        Gravity = preset.Gravity * scale,
        RandomAcceleration = preset.RandomAcceleration * scale
    };

    private static CpuParticleParameters Scale(CpuParticleParameters parameters, float scale) => parameters with
    {
        Size = parameters.Size * scale,
        DeltaSize = parameters.DeltaSize * scale
    };

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
