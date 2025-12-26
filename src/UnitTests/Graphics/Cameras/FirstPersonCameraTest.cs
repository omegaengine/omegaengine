/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using FluentAssertions;
using Xunit;

namespace OmegaEngine.Graphics.Cameras;

/// <summary>
/// Contains test methods for <see cref="FirstPersonCamera"/>.
/// </summary>
public class FirstPersonCameraTest
{
    [Fact]
    public void TestNavigateTranslation()
    {
        var camera = new FirstPersonCamera();

        camera.Navigate(translation: new(0, 0, 10));

        camera.Position.X.Should().BeApproximately(0, precision: 0.001);
        camera.Position.Y.Should().BeApproximately(0, precision: 0.001);
        camera.Position.Z.Should().BeApproximately(10, precision: 0.001);
    }

    [Fact]
    public void TestNavigateRotatedTranslation()
    {
        var camera = new FirstPersonCamera
        {
            Yaw = 90 // Facing right
        };

        camera.Navigate(translation: new(0, 0, 10));

        camera.Position.X.Should().BeApproximately(10, precision: 0.001);
        camera.Position.Y.Should().BeApproximately(0, precision: 0.001);
        camera.Position.Z.Should().BeApproximately(0, precision: 0.001);
    }

    [Fact]
    public void TestNavigateCustomWorldUp()
    {
        var camera = new FirstPersonCamera
        {
            WorldUp = new(1, 0, 0) // Right becomes up
        };

        camera.Navigate(translation: new(0, 0, 10));

        camera.Position.X.Should().BeApproximately(0, precision: 0.001);
        camera.Position.Y.Should().BeApproximately(0, precision: 0.001);
        camera.Position.Z.Should().BeApproximately(10, precision: 0.001);
    }

    [Fact]
    public void TestNavigateRotation()
    {
        var camera = new FirstPersonCamera();

        camera.Navigate(rotation: new(45, 30, 0));
        camera.Yaw.Should().BeApproximately(45, precision: 0.001);
        camera.Pitch.Should().BeApproximately(30, precision: 0.001);
    }

    [Fact]
    public void TestPitchClamp()
    {
        var camera = new FirstPersonCamera();

        camera.Pitch = 100;
        camera.Pitch.Should().BeApproximately(90, precision: 0.001);

        camera.Pitch = -100;
        camera.Pitch.Should().BeApproximately(-90, precision: 0.001);
    }

    [Fact]
    public void TestYawWraparound()
    {
        var camera = new FirstPersonCamera();

        camera.Yaw = 370;
        camera.Yaw.Should().BeApproximately(10, precision: 0.001);

        camera.Yaw = -10;
        camera.Yaw.Should().BeApproximately(350, precision: 0.001);
    }
}
