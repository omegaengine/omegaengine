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

        model.IntersectsWorld(FarAway + new DoubleVector3(0, 0, -10), Vector3.UnitZ, out float distance).Should().BeTrue();
        distance.Should().BeApproximately(9, 0.001f);
    }

    [Fact]
    public void IntersectsWorldMissesOutsideBox()
    {
        using var model = FarAwayBox();

        model.IntersectsWorld(FarAway + new DoubleVector3(0, 50, -10), Vector3.UnitZ, out float _).Should().BeFalse();
    }

    [Fact]
    public void IntersectsWorldIgnoresFloatingOrigin()
    {
        using var model = FarAwayBox();
        model.SetFloatingOrigin(FarAway + new DoubleVector3(-5, 3, 7));

        model.IntersectsWorld(FarAway + new DoubleVector3(0, 0, -10), Vector3.UnitZ, out float distance).Should().BeTrue();
        distance.Should().BeApproximately(9, 0.001f);
    }

    [Fact]
    public void IntersectsWorldIgnoresDistanceCompression()
    {
        using var model = FarAwayBox();
        CompressDistance(model);

        model.IntersectsWorld(FarAway + new DoubleVector3(0, 0, -10), Vector3.UnitZ, out float distance).Should().BeTrue();
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
        child.IntersectsWorld(origin, Vector3.UnitZ, out float distance).Should().BeTrue();
        distance.Should().BeApproximately(8, 0.001f);
        child.IntersectsWorld(origin + new DoubleVector3(5, 0, 0), Vector3.UnitZ, out float _).Should().BeTrue();
        child.IntersectsWorld(origin + new DoubleVector3(7, 0, 0), Vector3.UnitZ, out float _).Should().BeFalse();
    }

    [Fact]
    public void IntersectsWorldAppliesPreTransform()
    {
        using var model = FarAwayBox();
        model.PreTransform = Matrix.Translation(5, 0, 0);
        model.SetScale(2);

        // The scaling also applies to the pre-transform, so the box spans 8 to 12 along X
        model.IntersectsWorld(FarAway + new DoubleVector3(10, 0, -10), Vector3.UnitZ, out float distance).Should().BeTrue();
        distance.Should().BeApproximately(8, 0.001f);
        model.IntersectsWorld(FarAway + new DoubleVector3(5, 0, -10), Vector3.UnitZ, out float _).Should().BeFalse();
    }

    /// <summary>
    /// Renders <see cref="FarAwayBox"/> pulled in towards a camera far away from it by distance compression.
    /// </summary>
    /// <returns>The camera position, the box's rendered center and its rendered scaling, all in floating space.</returns>
    private static (Vector3 camera, Vector3 center, float scaling) CompressDistance(Model model)
    {
        var camera = new ArcballCamera {Target = model.Position, Radius = 10_000, FarClip = 1e12f, Size = new Size(800, 600)};
        model.SetFloatingOrigin(camera);
        model.IsVisible(camera, distanceCompressionStart: 100);

        var transform = model.RenderTransform;
        var center = new Vector3(transform.M41, transform.M42, transform.M43);
        var cameraPosition = model.ApplyFloatingOriginTo(camera.Position);
        (center - cameraPosition).Length().Should().BeLessThan(1_000, because: "the box should be rendered pulled in towards the camera");

        return (cameraPosition, center, new Vector3(transform.M11, transform.M12, transform.M13).Length());
    }

    [Fact]
    public void IntersectsRenderedOffCameraRayUnderDistanceCompression()
    {
        using var model = FarAwayBox();
        var (camera, center, scaling) = CompressDistance(model);

        // A ray that does not pass through the camera, so distance compression does change what it hits
        var sideways = Vector3.Normalize(Vector3.Cross(center - camera, Vector3.UnitY));
        model.Intersects(new Ray(center + sideways, -sideways), out float distance).Should().BeTrue();
        // The ray passes through the center of the axis-aligned box, so it enters through the face of the axis it is most aligned with
        float maxComponent = Math.Max(Math.Abs(sideways.X), Math.Max(Math.Abs(sideways.Y), Math.Abs(sideways.Z)));
        distance.Should().BeApproximately(1 - scaling / maxComponent, 0.001f);
    }

    [Fact]
    public void IntersectsPositionUndoesDistanceCompression()
    {
        using var model = FarAwayBox();
        var (camera, center, _) = CompressDistance(model);

        model.Intersects(new Ray(camera, Vector3.Normalize(center - camera)), out DoubleVector3 position).Should().BeTrue();

        // On the surface of the physical box (size 2), not of the rendered one
        double distanceFromCenter = (position - model.Position).Length();
        distanceFromCenter.Should().BeGreaterThan(0.999).And.BeLessThan(Math.Sqrt(3) + 0.001);
    }
}
