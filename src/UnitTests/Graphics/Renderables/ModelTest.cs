/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using AwesomeAssertions;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics.Cameras;
using SlimDX;
using Xunit;

namespace OmegaEngine.Graphics.Renderables;

public class ModelTest : EngineTestBase
{
    [Fact]
    public void BoxHasGeometry()
    {
        using var model = Model.Box(Engine, XMaterial.Default);

        model.NumberSubsets.Should().BeGreaterThan(0);
        model.VertexCount.Should().BeGreaterThan(0);
        model.BoundingSphere.Should().NotBeNull();
    }

    [Fact]
    public void IntersectsHitsBox()
    {
        using var model = Model.Box(Engine, XMaterial.Default, new Vector3(2, 2, 2));

        // Ray along +Z through the origin, where the box (centered, size 2) sits
        var ray = new Ray(new Vector3(0, 0, -10), Vector3.UnitZ);
        model.Intersects(ray, out float distance).Should().BeTrue();
        distance.Should().BeGreaterThan(0);
    }

    [Fact]
    public void IntersectsMissesOutsideBox()
    {
        using var model = Model.Box(Engine, XMaterial.Default, new Vector3(2, 2, 2));

        // Ray parallel to +Z but far away from the box
        var ray = new Ray(new Vector3(50, 50, -10), Vector3.UnitZ);
        model.Intersects(ray, out float _).Should().BeFalse();
    }

    /// <summary>
    /// Far beyond what single precision can resolve to within 0.001.
    /// </summary>
    private static readonly DoubleVector3 FarAway = new(1e9, 0, 0);

    /// <summary>
    /// A box (size 2) far away from the world origin.
    /// </summary>
    private Model FarAwayBox()
    {
        var model = Model.Box(Engine, XMaterial.Default, new Vector3(2, 2, 2));
        model.Position = FarAway;
        return model;
    }

    [Fact]
    public void IntersectsWorldFarFromOrigin()
    {
        using var model = FarAwayBox();

        model.Intersects(FarAway + new DoubleVector3(0, 0, -10), Vector3.UnitZ, out float distance).Should().BeTrue();
        distance.Should().BeApproximately(9, 0.001f);
    }

    [Fact]
    public void IntersectsWorldMissesOutsideBox()
    {
        using var model = FarAwayBox();

        model.Intersects(FarAway + new DoubleVector3(0, 50, -10), Vector3.UnitZ, out float _).Should().BeFalse();
    }

    [Fact]
    public void IntersectsWorldIgnoresFloatingOrigin()
    {
        using var model = FarAwayBox();
        model.SetFloatingOrigin(FarAway + new DoubleVector3(-5, 3, 7));

        model.Intersects(FarAway + new DoubleVector3(0, 0, -10), Vector3.UnitZ, out float distance).Should().BeTrue();
        distance.Should().BeApproximately(9, 0.001f);
    }

    [Fact]
    public void IntersectsWorldIgnoresDistanceCompression()
    {
        using var model = FarAwayBox();
        var camera = new ArcballCamera {Target = model.Position, Radius = 10_000, FarClip = 1e12f, Size = new Size(800, 600)};
        model.SetFloatingOrigin(camera);
        model.IsVisible(camera, distanceCompressionStart: 100);

        // Make sure the box really is rendered pulled in towards the camera
        var transform = model.WorldTransform;
        double renderedDistance = (new Vector3(transform.M41, transform.M42, transform.M43) - model.ApplyFloatingOriginTo(camera.Position)).Length();
        renderedDistance.Should().BeLessThan(1_000);

        model.Intersects(FarAway + new DoubleVector3(0, 0, -10), Vector3.UnitZ, out float distance).Should().BeTrue();
        distance.Should().BeApproximately(9, 0.001f);
    }

    [Fact]
    public void IntersectsWorldAppliesParentTransform()
    {
        using var parent = FarAwayBox();
        parent.Rotation = Quaternion.RotationAxis(Vector3.UnitY, (float)Math.PI / 2);
        parent.SetScale(2);

        // Long along its own Z axis, which the parent turns into the world X axis
        using var child = Model.Box(Engine, XMaterial.Default, new Vector3(2, 2, 6));
        child.Position = new(0, 0, 5);
        parent.Children.Add(child);

        // The parent moves the child to (10, 0, 0) and turns it to span 12 along X and 4 along Z
        var origin = FarAway + new DoubleVector3(10, 0, -10);
        child.Intersects(origin, Vector3.UnitZ, out float distance).Should().BeTrue();
        distance.Should().BeApproximately(8, 0.001f);
        child.Intersects(origin + new DoubleVector3(5, 0, 0), Vector3.UnitZ, out float _).Should().BeTrue();
        child.Intersects(origin + new DoubleVector3(7, 0, 0), Vector3.UnitZ, out float _).Should().BeFalse();
    }
}
