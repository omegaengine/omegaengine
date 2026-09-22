/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using AwesomeAssertions;
using OmegaEngine.Graphics.Cameras;
using Xunit;

namespace OmegaEngine.Graphics.Renderables;

public class PositionableRenderableTest : EngineTestBase
{
    [Fact]
    public void WorldBoundingSphereFollowsPosition()
    {
        using var model = Model.Box(Engine, XMaterial.Default, new(2, 2, 2));

        model.Position = new(10, 5, -3);

        var center = model.WorldBoundingSphere!.Value.Center;
        center.X.Should().BeApproximately(10, 0.001f);
        center.Y.Should().BeApproximately(5, 0.001f);
        center.Z.Should().BeApproximately(-3, 0.001f);
    }

    [Fact]
    public void ScalingRecalculatesWorldBoundingSphere()
    {
        using var model = Model.Box(Engine, XMaterial.Default, new(2, 2, 2));
        float originalRadius = model.WorldBoundingSphere!.Value.Radius;

        model.SetScale(3);

        model.WorldBoundingSphere!.Value.Radius.Should().NotBe(originalRadius);
    }

    [Fact]
    public void AutoScaleDistanceKeepsNaturalSizeWhenClose()
    {
        using var model = Model.Box(Engine, XMaterial.Default, new(2, 2, 2));
        float originalRadius = model.WorldBoundingSphere!.Value.Radius;

        model.AutoScaleDistance = 100;

        // Camera closer than the start distance: the factor is clamped to 1, so no scaling occurs
        var camera = new ArcballCamera {Radius = 50, Size = new Size(800, 600)};
        model.IsVisible(camera);

        model.WorldBoundingSphere!.Value.Radius.Should().BeApproximately(originalRadius, 0.001f);
    }

    [Fact]
    public void AutoScaleDistanceScalesUpWhenFar()
    {
        using var model = Model.Box(Engine, XMaterial.Default, new(2, 2, 2));
        float originalRadius = model.WorldBoundingSphere!.Value.Radius;

        model.AutoScaleDistance = 100;

        // Camera at twice the start distance: factor = distance / AutoScaleDistance = 2
        var camera = new ArcballCamera {Radius = 200, Size = new Size(800, 600)};
        model.IsVisible(camera);

        model.WorldBoundingSphere!.Value.Radius.Should().BeApproximately(originalRadius * 2, 0.001f);
    }

    [Fact]
    public void AutoScaleDistanceIsIgnoredWhileTheBodyHasChildren()
    {
        using var model = Model.Box(Engine, XMaterial.Default, new(2, 2, 2));
        float originalRadius = model.WorldBoundingSphere!.Value.Radius;

        model.AutoScaleDistance = 100;
        var camera = new ArcballCamera {Radius = 200, Size = new Size(800, 600)};
        model.IsVisible(camera);
        model.WorldBoundingSphere!.Value.Radius.Should().BeApproximately(originalRadius * 2, 0.001f);

        // Auto-scaling is a leaf-only camera effect, so it must stop applying once the body becomes a parent
        using var child = new Pivot();
        model.Children.Add(child);
        model.IsVisible(camera);
        model.WorldBoundingSphere!.Value.Radius.Should().BeApproximately(originalRadius, 0.001f);

        // ... and start applying again once it is a leaf again
        model.Children.Remove(child);
        model.IsVisible(camera);
        model.WorldBoundingSphere!.Value.Radius.Should().BeApproximately(originalRadius * 2, 0.001f);
    }

    [Fact]
    public void ChildrenAreCulledIndependentlyOfTheirParent()
    {
        using var parent = new Pivot();
        using var child = Model.Box(Engine, XMaterial.Default, new(4, 4, 4));
        parent.Children.Add(child);

        var camera = new ArcballCamera
        {
            Radius = 20,
            NearClip = 1,
            FarClip = 100,
            Size = new Size(800, 600)
        };

        child.IsVisible(camera).Should().BeTrue();

        // Moving the parent takes the child out of the frustum, even though the child's own position is unchanged
        parent.Position = new(0, 0, 10_000);
        child.IsVisible(camera).Should().BeFalse();
    }

    [Fact]
    public void IsVisibleIsFalseWhenHiddenOrFullyTransparent()
    {
        using var model = Model.Box(Engine, XMaterial.Default);
        var camera = new ArcballCamera {Radius = 20, Size = new Size(800, 600)};

        model.Visible = false;
        model.IsVisible(camera).Should().BeFalse();

        model.Visible = true;
        model.Alpha = EngineState.Invisible;
        model.IsVisible(camera).Should().BeFalse();
    }

    [Fact]
    public void IsVisibleAppliesFrustumCulling()
    {
        using var model = Model.Box(Engine, XMaterial.Default, new(4, 4, 4));
        var camera = new ArcballCamera
        {
            Radius = 20,
            NearClip = 1,
            FarClip = 100,
            Size = new Size(800, 600)
        };

        // The model sits at the origin, well within the camera's near/far clip range
        model.Position = new();
        model.IsVisible(camera).Should().BeTrue();

        // Moving it far beyond the far clip plane must cull it
        model.Position = new(0, 0, 10_000);
        model.IsVisible(camera).Should().BeFalse();
    }
}
