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

/// <summary>
/// Tests the render hierarchy built from <see cref="PositionableRenderable.Children"/>.
/// </summary>
/// <remarks>Uses <see cref="Pivot"/>s and <see cref="Probe"/>s, so none of this needs a Direct3D device.</remarks>
public class SceneGraphTest
{
    /// <summary>A renderable without geometry that has a bounding sphere, so it contributes to subtree bounds.</summary>
    private sealed class Probe : PositionableRenderable
    {
        public Probe(Vector3 center = default) => BoundingSphere = new(center, radius: 1);
    }

    private static void ShouldBe(DoubleVector3 actual, double x, double y, double z)
    {
        actual.X.Should().BeApproximately(x, 0.001);
        actual.Y.Should().BeApproximately(y, 0.001);
        actual.Z.Should().BeApproximately(z, 0.001);
    }

    private static void ShouldEnclose(PositionableRenderable subtree, BoundingSphere inner)
    {
        var sphere = subtree.SubtreeBoundingSphere ?? throw new InvalidOperationException("No subtree bounding sphere");
        (Vector3.Distance(sphere.Center, inner.Center) + inner.Radius).Should().BeLessThanOrEqualTo(sphere.Radius + 0.001f);

        var box = subtree.SubtreeBoundingBox ?? throw new InvalidOperationException("No subtree bounding box");
        for (int i = 0; i < 3; i++)
        {
            (inner.Center[i] - inner.Radius).Should().BeGreaterThanOrEqualTo(box.Minimum[i] - 0.001f);
            (inner.Center[i] + inner.Radius).Should().BeLessThanOrEqualTo(box.Maximum[i] + 0.001f);
        }
    }

    [Fact]
    public void ChildPositionIsRelativeToItsParent()
    {
        using var parent = new Pivot {Position = new(10, 0, 0)};
        using var child = new Pivot {Position = new(0, 5, 0)};
        parent.Children.Add(child);

        child.Position.Should().Be(new DoubleVector3(0, 5, 0), "the local position is kept as-is");
        ShouldBe(child.WorldPosition, 10, 5, 0);
    }

    [Fact]
    public void CompositionWorksAcrossThreeLevels()
    {
        using var root = new Pivot {Position = new(10, 0, 0)};
        using var middle = new Pivot {Position = new(0, 5, 0)};
        using var leaf = new Pivot {Position = new(0, 0, 3)};
        root.Children.Add(middle);
        middle.Children.Add(leaf);

        ShouldBe(leaf.WorldPosition, 10, 5, 3);
    }

    [Fact]
    public void ChildPositionIsRotatedByItsParent()
    {
        using var parent = new Pivot {Rotation = Quaternion.RotationYawPitchRoll((float)(Math.PI / 2), 0, 0)};
        using var child = new Pivot {Position = new(1, 0, 0)};
        parent.Children.Add(child);

        // A 90° yaw turns the parent's +X axis into the world's -Z axis
        ShouldBe(child.WorldPosition, 0, 0, -1);
    }

    [Fact]
    public void ChildPositionIsScaledByItsParent()
    {
        using var parent = new Pivot {Position = new(0, 100, 0), Scale = new(2, 2, 2)};
        using var child = new Pivot {Position = new(1, 0, 0)};
        parent.Children.Add(child);

        ShouldBe(child.WorldPosition, 2, 100, 0);
    }

    [Fact]
    public void MovingAParentMovesItsDescendants()
    {
        using var root = new Pivot();
        using var middle = new Pivot {Position = new(0, 5, 0)};
        using var leaf = new Pivot {Position = new(0, 0, 3)};
        root.Children.Add(middle);
        middle.Children.Add(leaf);

        // Read once, so any stale cache would be observable
        ShouldBe(leaf.WorldPosition, 0, 5, 3);

        root.Position = new(10, 0, 0);

        ShouldBe(leaf.WorldPosition, 10, 5, 3);
    }

    [Fact]
    public void ReparentingKeepsTheLocalTransform()
    {
        using var first = new Pivot {Position = new(10, 0, 0)};
        using var second = new Pivot {Position = new(100, 0, 0)};
        using var child = new Pivot {Position = new(1, 0, 0)};

        first.Children.Add(child);
        ShouldBe(child.WorldPosition, 11, 0, 0);

        second.Children.Add(child);

        first.Children.Should().BeEmpty("an element lives in exactly one collection");
        second.Children.Should().Equal(child);
        child.Position.Should().Be(new DoubleVector3(1, 0, 0), "the local position is kept, so the world position changes");
        ShouldBe(child.WorldPosition, 101, 0, 0);
    }

    [Fact]
    public void RemovingFromAParentMakesItARootAgain()
    {
        using var parent = new Pivot {Position = new(10, 0, 0)};
        using var child = new Pivot {Position = new(1, 0, 0)};
        parent.Children.Add(child);

        parent.Children.Remove(child).Should().BeTrue();

        ShouldBe(child.WorldPosition, 1, 0, 0);
    }

    [Fact]
    public void CyclesAreRejected()
    {
        using var grandParent = new Pivot();
        using var parent = new Pivot();
        using var child = new Pivot();
        grandParent.Children.Add(parent);
        parent.Children.Add(child);

        Assert.Throws<ArgumentException>(() => child.Children.Add(child));
        Assert.Throws<ArgumentException>(() => child.Children.Add(parent));
        Assert.Throws<ArgumentException>(() => child.Children.Add(grandParent));
    }

    [Fact]
    public void OnlyTheRootCarriesTheFloatingOrigin()
    {
        using var root = new Pivot {Position = new(10, 0, 0)};
        using var child = new Pivot {Position = new(0, 5, 0)};
        root.Children.Add(child);

        root.SetFloatingOrigin(new DoubleVector3(100, 0, 0));

        // The origin is applied exactly once, at the root
        root.GetFloatingPosition().X.Should().BeApproximately(-90, 0.001f);
        child.GetFloatingPosition().X.Should().BeApproximately(-90, 0.001f);
        child.GetFloatingPosition().Y.Should().BeApproximately(5, 0.001f);

        // Descendants read the root's origin instead of one of their own
        child.GetFloatingOrigin().Should().Be(new DoubleVector3(100, 0, 0));
    }

    [Fact]
    public void WorldPositionIsIndependentOfTheFloatingOrigin()
    {
        using var root = new Pivot {Position = new(10, 0, 0)};
        using var child = new Pivot {Position = new(0, 5, 0)};
        root.Children.Add(child);

        root.SetFloatingOrigin(new DoubleVector3(100, 200, 300));
        ShouldBe(child.WorldPosition, 10, 5, 0);

        root.SetFloatingOrigin(new DoubleVector3(-1000, 0, 7));
        ShouldBe(child.WorldPosition, 10, 5, 0);
    }

    [Fact]
    public void ToWorldResolvesLocalOffsets()
    {
        using var parent = new Pivot
        {
            Position = new(10, 0, 0),
            Rotation = Quaternion.RotationYawPitchRoll((float)(Math.PI / 2), 0, 0)
        };

        // A 90° yaw turns the parent's +X axis into the world's -Z axis
        ShouldBe(parent.ToWorld(new(1, 0, 0)), 10, 0, -1);
    }

    [Fact]
    public void SubtreeBoundsEncloseAllDescendants()
    {
        using var root = new Pivot {Position = new(10, 0, 0)};
        using var first = new Probe {Position = new(20, 0, 0)};
        using var second = new Probe {Position = new(0, -5, 3)};
        using var grandChild = new Probe {Position = new(0, 0, -30)};
        root.Children.Add(first);
        root.Children.Add(second);
        second.Children.Add(grandChild);

        ShouldEnclose(root, first.WorldBoundingSphere!.Value);
        ShouldEnclose(root, second.WorldBoundingSphere!.Value);
        ShouldEnclose(root, grandChild.WorldBoundingSphere!.Value);
    }

    [Fact]
    public void MovingAGrandChildUpdatesTheRootsSubtreeBounds()
    {
        using var root = new Pivot();
        using var middle = new Pivot {Position = new(0, 5, 0)};
        using var leaf = new Probe();
        root.Children.Add(middle);
        middle.Children.Add(leaf);

        // Read once, so any stale cache would be observable
        ShouldEnclose(root, leaf.WorldBoundingSphere!.Value);

        leaf.Position = new(100, 0, 0);

        ShouldEnclose(root, leaf.WorldBoundingSphere!.Value);
    }

    [Fact]
    public void AddingAndRemovingChildrenUpdatesSubtreeBounds()
    {
        using var root = new Pivot();
        using var near = new Probe();
        using var far = new Probe {Position = new(50, 0, 0)};
        root.Children.Add(near);
        root.SubtreeBoundingSphere!.Value.Radius.Should().BeApproximately(1, 0.001f);

        root.Children.Add(far);
        ShouldEnclose(root, far.WorldBoundingSphere!.Value);

        root.Children.Remove(far);
        root.SubtreeBoundingSphere!.Value.Radius.Should().BeApproximately(1, 0.001f);
    }

    [Fact]
    public void EmptyPivotsDoNotPreventCulling()
    {
        using var root = new Pivot();
        using var empty = new Pivot();
        using var leaf = new Probe();
        root.Children.Add(empty);
        root.Children.Add(leaf);

        empty.SubtreeBoundingSphere.Should().BeNull("there is nothing to enclose");
        ShouldEnclose(root, leaf.WorldBoundingSphere!.Value);
    }

    [Fact]
    public void AutoScaledLeafMakesItsAncestorsUncullable()
    {
        using var root = new Pivot();
        using var leaf = new Probe();
        root.Children.Add(leaf);
        root.SubtreeBoundingSphere.Should().NotBeNull();

        leaf.AutoScaleDistance = 100;
        root.SubtreeBoundingSphere.Should().BeNull("auto-scaling grows without bound as the camera moves away");
        root.SubtreeBoundingBox.Should().BeNull("auto-scaling grows without bound as the camera moves away");

        leaf.AutoScaleDistance = null;
        root.SubtreeBoundingSphere.Should().NotBeNull();
    }

    [Fact]
    public void ForcedPerspectiveLeafDisablesFarClipCullingForItsAncestors()
    {
        using var root = new Pivot();
        using var leaf = new Probe();
        root.Children.Add(leaf);
        root.SubtreeIgnoresFarClip.Should().BeFalse();

        leaf.ForcedPerspectiveDistance = 100;
        root.SubtreeIgnoresFarClip.Should().BeTrue();
        root.SubtreeBoundingSphere.Should().NotBeNull("forced perspective only disables far clip culling");
    }

    [Fact]
    public void BillboardedLeafSubtreeBoundsAreCameraIndependent()
    {
        using var root = new Pivot {Position = new(10, 0, 0)};
        using var leaf = new Probe(center: new(3, 0, 0)) {Billboard = BillboardMode.Spherical};
        root.Children.Add(leaf);
        var expected = root.SubtreeBoundingSphere!.Value;

        leaf.IsVisible(new ArcballCamera {Radius = 20, Size = new Size(800, 600)});
        var firstCenter = leaf.WorldBoundingSphere!.Value.Center;
        root.SubtreeBoundingSphere.Should().Be(expected, "the subtree bounds cover every possible billboard rotation");
        ShouldEnclose(root, leaf.WorldBoundingSphere!.Value);

        leaf.IsVisible(new ArcballCamera {Radius = 20, Yaw = 90, Pitch = 45, Size = new Size(800, 600)});
        leaf.WorldBoundingSphere!.Value.Center.Should().NotBe(firstCenter, "the billboard turns with the camera");
        root.SubtreeBoundingSphere.Should().Be(expected, "the subtree bounds cover every possible billboard rotation");
        ShouldEnclose(root, leaf.WorldBoundingSphere!.Value);
    }
}
