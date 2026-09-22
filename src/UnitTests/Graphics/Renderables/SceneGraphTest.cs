/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using AwesomeAssertions;
using OmegaEngine.Foundation.Geometry;
using SlimDX;
using Xunit;

namespace OmegaEngine.Graphics.Renderables;

/// <summary>
/// Tests the render hierarchy built from <see cref="PositionableRenderable.Children"/>.
/// </summary>
/// <remarks>Uses <see cref="Pivot"/>s, so none of this needs a Direct3D device.</remarks>
public class SceneGraphTest
{
    private static void ShouldBe(DoubleVector3 actual, double x, double y, double z)
    {
        actual.X.Should().BeApproximately(x, 0.001);
        actual.Y.Should().BeApproximately(y, 0.001);
        actual.Z.Should().BeApproximately(z, 0.001);
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
}
