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
        model.IsVisible(camera, forcedPerspectiveDistance: 10_000);

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
        model.IsVisible(camera, forcedPerspectiveDistance: 10_000);

        model.WorldBoundingSphere!.Value.Radius.Should().BeApproximately(originalRadius * 2, 0.001f);
    }

    [Fact]
    public void AutoScaleDistanceIsIgnoredWhileTheBodyHasChildren()
    {
        using var model = Model.Box(Engine, XMaterial.Default, new(2, 2, 2));
        float originalRadius = model.WorldBoundingSphere!.Value.Radius;

        model.AutoScaleDistance = 100;
        var camera = new ArcballCamera {Radius = 200, Size = new Size(800, 600)};
        model.IsVisible(camera, forcedPerspectiveDistance: 10_000);
        model.WorldBoundingSphere!.Value.Radius.Should().BeApproximately(originalRadius * 2, 0.001f);

        // Auto-scaling is a leaf-only camera effect, so it must stop applying once the body becomes a parent
        using var child = new Pivot();
        model.Children.Add(child);
        model.IsVisible(camera, forcedPerspectiveDistance: 10_000);
        model.WorldBoundingSphere!.Value.Radius.Should().BeApproximately(originalRadius, 0.001f);

        // ... and start applying again once it is a leaf again
        model.Children.Remove(child);
        model.IsVisible(camera, forcedPerspectiveDistance: 10_000);
        model.WorldBoundingSphere!.Value.Radius.Should().BeApproximately(originalRadius * 2, 0.001f);
    }

    /// <summary>
    /// Returns where the <paramref name="model"/>'s origin is actually rendered when looked at with the <paramref name="camera"/>.
    /// </summary>
    private static DoubleVector3 GetRenderedPosition(Model model, Camera camera, float forcedPerspectiveDistance)
    {
        model.IsVisible(camera, forcedPerspectiveDistance);
        var transform = model.WorldTransform;
        return new(transform.M41, transform.M42, transform.M43);
    }

    /// <summary>
    /// Returns how far from the <paramref name="camera"/> the <paramref name="model"/>'s origin is actually rendered.
    /// </summary>
    private static double GetRenderedDistance(Model model, Camera camera, float forcedPerspectiveDistance)
        => (GetRenderedPosition(model, camera, forcedPerspectiveDistance) - camera.Position).Length();

    /// <summary>
    /// Returns by how much the <paramref name="model"/> is scaled around the <paramref name="camera"/> by forced perspective.
    /// </summary>
    private static double GetRenderedScaling(Model model, Camera camera, float forcedPerspectiveDistance)
        => GetRenderedDistance(model, camera, forcedPerspectiveDistance) / (model.WorldPosition - camera.Position).Length();

    /// <summary>
    /// Returns how far from the <paramref name="camera"/> the surface of the <paramref name="sphere"/> is, without forced perspective.
    /// </summary>
    private static double GetSurfaceDistance(SlimDX.BoundingSphere sphere, Camera camera)
        => (new DoubleVector3(sphere.Center.X, sphere.Center.Y, sphere.Center.Z) - camera.Position).Length() - sphere.Radius;

    /// <summary>
    /// A sphere whose bounding sphere has exactly the given <paramref name="radius"/>.
    /// </summary>
    private Model Sphere(float radius)
        => Model.Sphere(Engine, XMaterial.Default, radius, slices: 8, stacks: 8);

    /// <summary>
    /// Returns the distance from the camera a <paramref name="distance"/> beyond <paramref name="maxDistance"/> is pulled in to, far short of the far clip plane.
    /// </summary>
    private static double PullIn(double distance, double maxDistance)
        => maxDistance * (1 + Math.Log(distance / maxDistance));

    /// <summary>
    /// A far clip plane far enough away for <see cref="PullIn"/> to hold.
    /// </summary>
    private const float DistantFarClip = 1e12f;

    [Fact]
    public void ForcedPerspectiveKeepsDistancesInOrder()
    {
        using var model = Sphere(radius: 10);
        model.ForcedPerspective = true;

        var near = new ArcballCamera {Radius = 1010, FarClip = DistantFarClip, Size = new Size(800, 600)};
        var far = new ArcballCamera {Radius = 5010, FarClip = DistantFarClip, Size = new Size(800, 600)};

        GetRenderedDistance(model, near, 100).Should().BeApproximately(1010 * PullIn(1000, 100) / 1000, 0.01);
        GetRenderedDistance(model, far, 100).Should().BeApproximately(5010 * PullIn(5000, 100) / 5000, 0.01);
    }

    [Fact]
    public void ForcedPerspectiveFollowsTheDistance()
    {
        using var model = Sphere(radius: 10);
        model.ForcedPerspective = true;

        var camera = new ArcballCamera {Radius = 5010, FarClip = DistantFarClip, Size = new Size(800, 600)};
        GetRenderedDistance(model, camera, 100).Should().BeApproximately(5010 * PullIn(5000, 100) / 5000, 0.01);
        GetRenderedDistance(model, camera, 1_000).Should().BeApproximately(5010 * PullIn(5000, 1_000) / 5000, 0.01);
    }

    [Fact]
    public void ForcedPerspectiveApproachesFarClip()
    {
        using var model = Sphere(radius: 10);
        model.ForcedPerspective = true;

        double previousSurfaceDistance = 100;
        foreach (float radius in new[] {10_000f, 100_000f, 10_000_000f})
        {
            var camera = new ArcballCamera {MaxRadius = radius, Radius = radius, FarClip = 1_000, Size = new Size(800, 600)};
            double scaling = GetRenderedScaling(model, camera, 100);

            // Still in order, but the far side never reaches the far clip plane
            double surfaceDistance = (radius - 10) * scaling;
            surfaceDistance.Should().BeGreaterThan(previousSurfaceDistance);
            ((radius + 10) * scaling).Should().BeLessThan(camera.FarClip);
            previousSurfaceDistance = surfaceDistance;
        }
    }

    [Fact]
    public void ForcedPerspectiveKeepsTheFarSideWithinFarClip()
    {
        using var model = Sphere(radius: 500);
        model.ForcedPerspective = true;

        // Pulling the surface in to at least 950 would put the far side at 5500 * 950 / 4500 > 1000, so it is pulled in further
        var camera = new ArcballCamera {Radius = 5_000, FarClip = 1_000, Size = new Size(800, 600)};
        double scaling = GetRenderedScaling(model, camera, 950);
        (5_500 * scaling).Should().BeLessThanOrEqualTo(camera.FarClip);
        (4_500 * scaling).Should().BeLessThan(950);
    }

    [Fact]
    public void ForcedPerspectiveMeasuresAutoScaledLeaves()
    {
        using var model = Sphere(radius: 10);
        model.AutoScaleDistance = 100;
        model.ForcedPerspective = true;

        // Auto-scaled by 10_000 / 100, so the surface is 9_000 away
        var camera = new ArcballCamera {Radius = 10_000, FarClip = DistantFarClip, Size = new Size(800, 600)};
        GetRenderedDistance(model, camera, 1_000).Should().BeApproximately(10_000 * PullIn(9_000, 1_000) / 9_000, 0.01);
    }

    [Fact]
    public void ForcedPerspectiveLeavesCloseRenderablesAlone()
    {
        using var model = Sphere(radius: 10);
        model.ForcedPerspective = true;

        // The center is beyond the distance, but the surface is not
        var camera = new ArcballCamera {Radius = 105, Size = new Size(800, 600)};
        GetRenderedDistance(model, camera, 100).Should().BeApproximately(105, 0.01);
    }

    [Fact]
    public void ForcedPerspectiveLeavesRenderablesWithoutItAlone()
    {
        using var model = Sphere(radius: 10);

        var camera = new ArcballCamera {Radius = 5_000, FarClip = 1_000, Size = new Size(800, 600)};
        GetRenderedDistance(model, camera, 100).Should().BeApproximately(5_000, 0.01);
        model.IsVisible(camera, 100).Should().BeFalse("it is still culled by the far clip plane");
    }

    [Fact]
    public void ForcedPerspectivePullsInTheWholeSubtreeAlike()
    {
        using var parent = new Pivot {ForcedPerspective = true};
        using var center = Sphere(radius: 10);
        using var offCenter = Sphere(radius: 10);
        offCenter.Position = new(0, 300, 0);
        parent.Children.Add(center);
        parent.Children.Add(offCenter);

        var camera = new ArcballCamera {Radius = 5_000, FarClip = 1_000, Size = new Size(800, 600)};
        var renderedCenter = GetRenderedPosition(center, camera, 100);
        var renderedOffCenter = GetRenderedPosition(offCenter, camera, 100);

        // Both children are scaled around the camera by the same factor, so they keep their places relative to each other
        double scaling = (renderedCenter - camera.Position).Length() / 5_000;
        (renderedOffCenter - renderedCenter).Length().Should().BeApproximately(300 * scaling, 0.001);

        // Measured to the surface of the sphere enclosing both children, not to either child
        var subtree = parent.SubtreeBoundingSphere!.Value;
        double surfaceDistance = GetSurfaceDistance(subtree, camera);
        (surfaceDistance * scaling).Should().BeGreaterThan(100);
        ((surfaceDistance + 2 * subtree.Radius) * scaling).Should().BeLessThan(camera.FarClip);

        // Pulled in from beyond the far clip plane
        center.IsVisible(camera, 100).Should().BeTrue();
        offCenter.IsVisible(camera, 100).Should().BeTrue();
        parent.SubtreeInFrustum(camera).Should().BeTrue();
    }

    [Fact]
    public void ForcedPerspectivePullsInSubtreeSurface()
    {
        using var parent = new Pivot {ForcedPerspective = true};
        using var center = Sphere(radius: 10);
        using var offCenter = Sphere(radius: 10);
        offCenter.Position = new(0, 300, 0);
        parent.Children.Add(center);
        parent.Children.Add(offCenter);

        // The pulled-in scaling is cached per ancestor, so check it follows the camera
        foreach (float radius in new[] {5_000f, 10_000f})
        {
            var camera = new ArcballCamera {Radius = radius, FarClip = DistantFarClip, Size = new Size(800, 600)};
            double scaling = GetRenderedScaling(center, camera, 100);
            GetRenderedScaling(offCenter, camera, 100).Should().BeApproximately(scaling, 1e-6);
            double surfaceDistance = GetSurfaceDistance(parent.SubtreeBoundingSphere!.Value, camera);
            (surfaceDistance * scaling).Should().BeApproximately(PullIn(surfaceDistance, 100), 0.01);
        }
    }

    [Fact]
    public void NestedForcedPerspectiveAppliesOnce()
    {
        using var parent = new Pivot {ForcedPerspective = true};
        using var child = Sphere(radius: 10);
        child.ForcedPerspective = true;
        parent.Children.Add(child);

        // The parent pulls in its whole subtree, measured to the child's surface 9_990 away; the child adds nothing on top
        var camera = new ArcballCamera {Radius = 10_000, FarClip = DistantFarClip, Size = new Size(800, 600)};
        GetRenderedScaling(child, camera, 100).Should().BeApproximately(PullIn(9_990, 100) / 9_990, 1e-6);
    }

    [Fact]
    public void ForcedPerspectiveOfAncestorDisablesFarClipCullingForDescendants()
    {
        using var root = new Pivot();
        using var middle = Sphere(radius: 10);
        using var leaf = Sphere(radius: 10);
        root.Children.Add(middle);
        middle.Children.Add(leaf);

        var camera = new ArcballCamera {Radius = 5_000, FarClip = 1_000, Size = new Size(800, 600)};
        middle.SubtreeInFrustum(camera).Should().BeFalse();
        leaf.IsVisible(camera, 100).Should().BeFalse();

        // Neither the middle body nor its subtree uses forced perspective itself, but the root pulls them in
        root.ForcedPerspective = true;
        middle.SubtreeInFrustum(camera).Should().BeTrue();
        leaf.IsVisible(camera, 100).Should().BeTrue();
    }

    [Fact]
    public void ForcedPerspectiveAppliesToParentsAndTheirChildren()
    {
        using var parent = Sphere(radius: 10);
        using var child = Sphere(radius: 10);
        child.Position = new(0, 300, 0);
        parent.Children.Add(child);
        parent.ForcedPerspective = true;

        var camera = new ArcballCamera {Radius = 10_000, FarClip = 100_000, Size = new Size(800, 600)};
        var renderedParent = GetRenderedPosition(parent, camera, 100);
        var renderedChild = GetRenderedPosition(child, camera, 100);

        // The parent is no leaf, but is pulled in all the same, together with its child
        double scaling = (renderedParent - camera.Position).Length() / 10_000;
        scaling.Should().BeLessThan(0.1);
        (renderedChild - renderedParent).Length().Should().BeApproximately(300 * scaling, 0.001);
    }

    [Fact]
    public void ForcedPerspectiveIsPreciseFarFromTheWorldOrigin()
    {
        using var parent = Sphere(radius: 10);
        using var child = Sphere(radius: 10);
        child.Position = new(0, 300, 0);
        parent.Children.Add(child);
        parent.Position = new(1e9, 0, 0);
        parent.ForcedPerspective = true;

        // Far beyond what single precision can resolve to within 0.01, so this only holds if the floating origin is applied in double precision
        var camera = new ArcballCamera {Target = parent.Position, Radius = 10_000, FarClip = DistantFarClip, Size = new Size(800, 600)};
        parent.SetFloatingOrigin(camera);

        // Measured to the surface of the sphere enclosing both, which lives in floating space just like the camera here
        var subtree = parent.SubtreeBoundingSphere!.Value;
        double surfaceDistance = SlimDX.Vector3.Distance(subtree.Center, parent.ApplyFloatingOriginTo(camera.Position)) - subtree.Radius;
        double scaling = PullIn(surfaceDistance, 100) / surfaceDistance;

        // Scaled around the camera, far away from the world origin
        foreach (var model in new[] {parent, child})
        {
            var expected = (DoubleVector3)model.ApplyFloatingOriginTo(camera.Position + (model.WorldPosition - camera.Position) * scaling);
            (GetRenderedPosition(model, camera, 100) - expected).Length().Should().BeLessThan(0.01);
        }
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

        child.IsVisible(camera, forcedPerspectiveDistance: 10_000).Should().BeTrue();

        // Moving the parent takes the child out of the frustum, even though the child's own position is unchanged
        parent.Position = new(0, 0, 10_000);
        child.IsVisible(camera, forcedPerspectiveDistance: 10_000).Should().BeFalse();
    }

    [Fact]
    public void IsVisibleIsFalseWhenHiddenOrFullyTransparent()
    {
        using var model = Model.Box(Engine, XMaterial.Default);
        var camera = new ArcballCamera {Radius = 20, Size = new Size(800, 600)};

        model.Visible = false;
        model.IsVisible(camera, forcedPerspectiveDistance: 10_000).Should().BeFalse();

        model.Visible = true;
        model.Alpha = EngineState.Invisible;
        model.IsVisible(camera, forcedPerspectiveDistance: 10_000).Should().BeFalse();
    }

    [Fact]
    public void ReassigningSurfaceShaderDoesNotAccumulateChildren()
    {
        using var model = Model.Box(Engine, XMaterial.Default);
        int childCount = model.RegisteredChildCount;

        for (int i = 0; i < 100; i++)
            model.SurfaceShader = Engine.DefaultShader;
        model.RegisteredChildCount.Should().Be(childCount, "assigning the current shader again is a no-op");

        model.SurfaceShader = null;
        model.RegisteredChildCount.Should().Be(childCount - 1, "the replaced shader must no longer be referenced");

        model.SurfaceShader = Engine.DefaultShader;
        model.RegisteredChildCount.Should().Be(childCount);
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
        model.IsVisible(camera, forcedPerspectiveDistance: 10_000).Should().BeTrue();

        // Moving it far beyond the far clip plane must cull it
        model.Position = new(0, 0, 10_000);
        model.IsVisible(camera, forcedPerspectiveDistance: 10_000).Should().BeFalse();
    }
}
