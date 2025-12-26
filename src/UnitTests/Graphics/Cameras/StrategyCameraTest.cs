/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using FluentAssertions;
using OmegaEngine.Foundation.Geometry;
using Xunit;

namespace OmegaEngine.Graphics.Cameras;

/// <summary>
/// Contains test methods for <see cref="StrategyCamera"/>.
/// </summary>
public class StrategyCameraTest
{
    [Fact]
    public void TestNavigatePan()
    {
        var camera = new StrategyCamera(heightController: _ => 0)
        {
            Radius = 10,
            MovementSensitivity = 0.01
        };

        camera.Navigate(translation: new(0, 10, 0));

        camera.Target.Y.Should().BeApproximately(0, precision: 0.001); // Height controlled
        camera.Target.Z.Should().BeApproximately(1, precision: 0.001);
    }

    [Fact]
    public void TestNavigateRotatedPan()
    {
        var camera = new StrategyCamera(heightController: _ => 0)
        {
            Rotation = 90,
            MovementSensitivity = 0.01,
            Radius = 10
        };

        camera.Navigate(translation: new(10, 0, 0));

        camera.Target.X.Should().BeApproximately(0, precision: 0.001);
        camera.Target.Y.Should().BeApproximately(0, precision: 0.001); // Height controlled
        camera.Target.Z.Should().BeApproximately(-1, precision: 0.001);
    }

    [Fact]
    public void TestNavigateZoom()
    {
        var camera = new StrategyCamera(heightController: _ => 0)
        {
            Radius = 10,
            MinRadius = 2,
            MaxRadius = 100
        };

        double initialRadius = camera.Radius;

        camera.Navigate(translation: new(0, 0, 1));
        camera.Radius.Should().BeLessThan(initialRadius);
    }

    [Fact]
    public void TestNavigateRotation()
    {
        var camera = new StrategyCamera(heightController: _ => 0);

        camera.Navigate(rotation: new(45, 0, 0));
        camera.Rotation.Should().BeApproximately(45, precision: 0.001);
    }

    [Fact]
    public void TestTargetHeightControl()
    {
        var camera = new StrategyCamera(heightController: _ => 0)
        {
            Target = new DoubleVector3(10, 100, 20), // Y value should be overridden
            Radius = 10
        };

        // Target Y should be controlled by height controller (0)
        camera.Target.Y.Should().Be(0);
        camera.Target.X.Should().Be(10);
        camera.Target.Z.Should().Be(20);
    }

    [Fact]
    public void TestRotationWraparound()
    {
        var camera = new StrategyCamera(heightController: _ => 0) { Rotation = 370 };
        camera.Rotation.Should().BeApproximately(10, precision: 0.001);
    }

    [Fact]
    public void TestAutomaticPitchCalculation()
    {
        var camera = new StrategyCamera(heightController: _ => 0)
        {
            Radius = 10,
            MinRadius = 5,
            MaxRadius = 20,
            MinPitch = 30,
            MaxPitch = 60
        };

        var position1 = camera.Position;

        // Change radius to test pitch changes
        camera.Radius = 15;
        var position2 = camera.Position;

        // Position should be different due to different pitch and radius
        // Pitch is interpolated between MinPitch and MaxPitch based on radius
        // The Y component changes with both radius and pitch
        position1.Y.Should().NotBe(position2.Y);

        // Verify approximate Y values: Y = Radius * sin(pitch)
        // At Radius=10: Y ≈ 6.43
        position1.Y.Should().BeApproximately(6.43, precision: 0.1);
        // At Radius=15: Y ≈ 11.49
        position2.Y.Should().BeApproximately(11.49, precision: 0.1);
    }

    [Fact]
    public void TestCameraStaysAboveTerrain()
    {
        const double terrainHeight = 5;
        var camera = new StrategyCamera(heightController: _ => terrainHeight)
        {
            Radius = 2, // Very small radius
            MinRadius = 1
        };

        // Camera Y position should be at or above terrain height
        camera.Position.Y.Should().BeGreaterOrEqualTo(terrainHeight);
    }
}
