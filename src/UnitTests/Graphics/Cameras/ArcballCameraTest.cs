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
/// Contains test methods for <see cref="ArcballCamera"/>.
/// </summary>
public class ArcballCameraTest
{
    [Fact]
    public void TestNavigatePan()
    {
        var camera = new ArcballCamera
        {
            Radius = 10,
            MovementSensitivity = 0.01
        };

        camera.Navigate(translation: new(10, 0, 0));

        camera.Target.X.Should().BeApproximately(-1, precision: 0.001);
        camera.Target.Y.Should().BeApproximately(0, precision: 0.001);
        camera.Target.Z.Should().BeApproximately(0, precision: 0.001);
    }

    [Fact]
    public void TestNavigateRotatedPan()
    {
        var camera = new ArcballCamera
        {
            Yaw = 90,
            Radius = 10,
            MovementSensitivity = 0.01
        };

        camera.Navigate(translation: new(10, 0, 0));

        camera.Target.X.Should().BeApproximately(0, precision: 0.001);
        camera.Target.Y.Should().BeApproximately(0, precision: 0.001);
        camera.Target.Z.Should().BeApproximately(1, precision: 0.001);
    }

    [Fact]
    public void TestNavigateZoom()
    {
        var camera = new ArcballCamera
        {
            Radius = 10,
            MinRadius = 2,
            MaxRadius = 100
        };

        double initialRadius = camera.Radius;
        camera.Navigate(translation: DoubleVector3.UnitZ);
        camera.Radius.Should().BeLessThan(initialRadius);
    }

    [Fact]
    public void TestNavigateRotation()
    {
        var camera = new ArcballCamera();

        camera.Navigate(rotation: new(45, -10, 0));
        camera.Yaw.Should().BeApproximately(45, precision: 0.001);
        camera.Pitch.Should().BeApproximately(10, precision: 0.001);
    }

    [Fact]
    public void TestPositionRelativeToTarget()
    {
        var camera = new ArcballCamera { Radius = 10 };
        (camera.Position - camera.Target).Length().Should().BeApproximately(10, precision: 0.1);
    }

    [Fact]
    public void TestRadiusConstraints()
    {
        var camera = new ArcballCamera
        {
            MinRadius = 5,
            MaxRadius = 50,
            Radius = 10
        };

        // Try to set radius below minimum
        camera.Radius = 2;
        camera.Radius.Should().Be(5);

        // Try to set radius above maximum
        camera.Radius = 100;
        camera.Radius.Should().Be(50);
    }

    [Fact]
    public void TestYawWraparound()
    {
        var camera = new ArcballCamera { Yaw = 370 };
        camera.Yaw.Should().BeApproximately(10, precision: 0.001);
    }

    [Fact]
    public void TestPitchGimbalLockPrevention()
    {
        var camera = new ArcballCamera { Pitch = 90 };

        // Pitch should be very close to 90 but not exactly 90
        camera.Pitch.Should().NotBe(90);
        camera.Pitch.Should().BeGreaterThan(89.9);
        camera.Pitch.Should().BeLessThan(90.1);
    }

    [Fact]
    public void TestRollRotation()
    {
        var camera = new ArcballCamera { Roll = 45 };

        camera.Navigate(rotation: new(0, 0, 15));
        camera.Roll.Should().BeApproximately(60, precision: 0.001);
    }
}
