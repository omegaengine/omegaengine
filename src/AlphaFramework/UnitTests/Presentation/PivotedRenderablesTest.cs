/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using AwesomeAssertions;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Renderables;
using SlimDX;
using Xunit;

namespace AlphaFramework.Presentation;

/// <summary>
/// Contains test methods for <see cref="PivotedRenderables"/>.
/// </summary>
/// <remarks>Nothing here touches the GPU, so no <see cref="OmegaEngine.Engine"/> is needed.</remarks>
public class PivotedRenderablesTest : IDisposable
{
    /// <summary>A renderable without geometry whose bounding sphere reveals where the hierarchy places it.</summary>
    private sealed class Probe : PositionableRenderable
    {
        public Probe() => BoundingSphere = new(new Vector3(), radius: 1);

        /// <summary>The center of the bounding sphere after the full hierarchy transform.</summary>
        public Vector3 WorldCenter => WorldBoundingSphere!.Value.Center;
    }

    private static readonly Quaternion QuarterTurn = Quaternion.RotationYawPitchRoll((float)(Math.PI / 2), 0, 0);

    private readonly Scene _scene = new();
    private readonly PivotedRenderables _renderables;
    private readonly object _owner = new();

    public PivotedRenderablesTest()
    {
        _renderables = new(_scene.Positionables);
    }

    public void Dispose() => _scene.Dispose();

    private static void ShouldBe(Vector3 actual, float x, float y, float z)
    {
        actual.X.Should().BeApproximately(x, 0.001f);
        actual.Y.Should().BeApproximately(y, 0.001f);
        actual.Z.Should().BeApproximately(z, 0.001f);
    }

    /// <summary>Places the group where the owner sits, like a presenter's update callback would.</summary>
    private void MoveOwner(DoubleVector3 position, Quaternion rotation)
    {
        var anchor = _renderables.AnchorFor(_owner)!;
        anchor.Position = position;
        anchor.Rotation = rotation;
    }

    [Fact]
    public void SingleRenderableGetsNoPivot()
    {
        var probe = _renderables.AddTo(new Probe(), _owner);

        _scene.Positionables.Should().ContainSingle("a lone renderable carries the owner's transform itself")
              .Which.Should().BeSameAs(probe);
        _scene.Positionables.OfType<Pivot>().Should().BeEmpty();
        _renderables.AnchorFor(_owner).Should().BeSameAs(probe);
    }

    [Fact]
    public void CollapsedRenderableKeepsItsPlacement()
    {
        _renderables.AddTo(new Probe {Position = new(5, 0, 0)}, _owner);

        MoveOwner(new(100, 0, 0), QuarterTurn);

        // A quarter turn around Y takes the offset from the owner's +X axis to the world's -Z axis
        ShouldBe(((Probe)_renderables.AnchorFor(_owner)!).WorldCenter, 100, 0, -5);
    }

    [Fact]
    public void SecondRenderablePromotesTheGroupToAPivot()
    {
        var first = _renderables.AddTo(new Probe {Position = new(5, 0, 0)}, _owner, "Owner");
        MoveOwner(new(100, 0, 0), QuarterTurn);

        var second = _renderables.AddTo(new Probe {Position = new(0, 7, 0)}, _owner, "Owner");

        var pivot = _renderables.AnchorFor(_owner).Should().BeOfType<Pivot>().Subject;
        _scene.Positionables.Should().ContainSingle("the pivot replaces the collapsed renderable at the scene root")
              .Which.Should().BeSameAs(pivot);
        pivot.Children.Should().Equal(first, second);

        // Promotion must not move anything that was already placed
        ShouldBe(first.WorldCenter, 100, 0, -5);
        ShouldBe(second.WorldCenter, 100, 7, 0);
    }

    [Fact]
    public void PivotForPromotesAndKeepsTheGroupPivoted()
    {
        var probe = _renderables.AddTo(new Probe {Position = new(5, 0, 0)}, _owner, "Owner");
        MoveOwner(new(100, 0, 0), QuarterTurn);

        // Lights and sounds resolve their position through the pivot, so they must not see the renderable's own placement
        var pivot = _renderables.PivotFor(_owner, "Owner");

        pivot.Name.Should().Be("Owner");
        _renderables.AnchorFor(_owner).Should().BeSameAs(pivot);
        pivot.Children.Should().Equal(probe);
        ShouldBe(probe.WorldCenter, 100, 0, -5);

        // The group stays pivoted even after the renderable is gone again, because the light is still attached
        _renderables.Remove(probe).Should().BeTrue();
        probe.Dispose();
        _renderables.AnchorFor(_owner).Should().BeSameAs(pivot);
        _scene.Positionables.Should().ContainSingle().Which.Should().BeSameAs(pivot);
    }

    [Fact]
    public void PromotionKeepsTheAnchorScaleAndLaterPreTransformChanges()
    {
        var probe = _renderables.AddTo(new Probe {Position = new(5, 0, 0)}, _owner);
        var anchor = _renderables.AnchorFor(_owner)!;
        anchor.Position = new(100, 0, 0);
        anchor.Scale = new(2, 2, 2);
        probe.PreTransform *= Matrix.Translation(0, 1, 0);
        ShouldBe(probe.WorldCenter, 110, 2, 0);

        _renderables.AddTo(new Probe(), _owner);

        _renderables.AnchorFor(_owner)!.Scale.Should().Be(new Vector3(2, 2, 2));
        ShouldBe(probe.WorldCenter, 110, 2, 0);
    }

    [Fact]
    public void LocalTransformIsTheSameCollapsedAndPivoted()
    {
        var probe = _renderables.AddTo(new Probe {Position = new(5, 0, 0), PreTransform = Matrix.Scaling(2, 2, 2)}, _owner);
        var collapsed = _renderables.LocalTransformOf(probe);

        _renderables.AddTo(new Probe(), _owner);

        _renderables.LocalTransformOf(probe).Should().Be(collapsed);
    }

    [Fact]
    public void RemovingTheLastRenderableDropsTheGroup()
    {
        var probe = _renderables.AddTo(new Probe(), _owner);
        var second = _renderables.AddTo(new Probe(), _owner);
        var pivot = _renderables.AnchorFor(_owner)!;

        _renderables.Remove(probe);
        _renderables.Remove(second);
        probe.Dispose();
        second.Dispose();

        _renderables.AnchorFor(_owner).Should().BeNull();
        _scene.Positionables.Should().BeEmpty("the pivot is disposed along with the group");
        pivot.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void ReleaseDisposesAPinnedPivot()
    {
        var pivot = _renderables.PivotFor(_owner, "Owner");
        _scene.Positionables.Should().ContainSingle("a group can consist of nothing but an attachment point")
              .Which.Should().BeSameAs(pivot);

        _renderables.Release(_owner);

        _renderables.AnchorFor(_owner).Should().BeNull();
        _scene.Positionables.Should().BeEmpty();
        pivot.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void ReleaseDoesNotDisposeGroupedRenderables()
    {
        using var probe = _renderables.AddTo(new Probe(), _owner);
        var pivot = _renderables.PivotFor(_owner);

        _renderables.Release(_owner);

        pivot.IsDisposed.Should().BeTrue();
        probe.IsDisposed.Should().BeFalse("the renderable is still owned by whoever added it");
        _scene.Positionables.Should().BeEmpty();
    }
}
