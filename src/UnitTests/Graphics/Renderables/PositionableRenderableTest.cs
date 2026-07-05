/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using FluentAssertions;
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
