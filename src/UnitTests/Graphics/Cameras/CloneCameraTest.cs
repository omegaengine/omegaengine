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
/// Contains test methods for <see cref="CloneCamera"/>.
/// </summary>
public class CloneCameraTest
{
    [Fact]
    public void EffectiveClipPlane_ReflectsCurrentParentFloatingOrigin()
    {
        // Arrange: parent camera at origin, clone with a clip plane whose X-normal makes D
        // sensitive to floating-origin changes on the X axis.
        var parentCamera = new FirstPersonCamera();
        var cloneCamera = new CloneCamera(parentCamera)
        {
            ClipPlane = new DoublePlane(new(0, 0, 0), new(1, 0, 0))
        };

        // Force the initial cache to build (FO = origin, D should be 0).
        _ = cloneCamera.View;
        _ = cloneCamera.Projection;
        float initialD = cloneCamera.EffectiveClipPlane.D;

        // Act: move the parent camera far enough to trigger a floating-origin reset,
        // then simulate what WaterView.Render() does (reassign ParentCamera and
        // eagerly refresh caches).
        parentCamera.Position = new(200_000, 0, 0);
        cloneCamera.ParentCamera = parentCamera;
        _ = cloneCamera.View;
        _ = cloneCamera.Projection;

        // Assert: D must change because the clip plane's render-space X position has
        // shifted with the floating origin, proving EffectiveClipPlane stays in sync.
        cloneCamera.EffectiveClipPlane.D.Should().NotBeApproximately(initialD, precision: 1f);
    }

    [Fact]
    public void View_TracksParentCameraAfterParentCameraChanges()
    {
        // Arrange
        var parent1 = new FirstPersonCamera { Position = new(0, 0, 0) };
        var parent2 = new FirstPersonCamera { Position = new(100, 0, 0) };
        var cloneCamera = new CloneCamera(parent1);

        var view1 = cloneCamera.View;

        // Act: switch parent camera (as WaterView.Render() does each frame)
        cloneCamera.ParentCamera = parent2;
        _ = cloneCamera.View; // eager refresh

        var view2 = cloneCamera.View;

        // Assert: the cached view matrix should reflect the new parent camera's position
        view2.Should().NotBe(view1);
    }
}
