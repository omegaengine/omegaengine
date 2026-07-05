/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using FluentAssertions;
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
    public void SphereHasGeometry()
    {
        using var model = Model.Sphere(Engine, XMaterial.Default, radius: 2);

        model.NumberSubsets.Should().BeGreaterThan(0);
        model.VertexCount.Should().BeGreaterThan(0);
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
}
