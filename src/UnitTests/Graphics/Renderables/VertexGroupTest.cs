/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using FluentAssertions;
using OmegaEngine.Graphics.VertexDecl;
using SlimDX.Direct3D9;
using Xunit;

namespace OmegaEngine.Graphics.Renderables;

public class VertexGroupTest : EngineTestBase
{
    [Fact]
    public void QuadComputesBoundsOnEngineSet()
    {
        using var group = VertexGroup.Quad(4f);
        group.Engine = Engine; // triggers vertex-buffer creation and bounding-body computation

        var boundingSphere = group.BoundingSphere;
        boundingSphere.Should().NotBeNull();
        boundingSphere!.Value.Radius.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ColoredTriangleBuildsVertexBuffer()
    {
        PositionColored[] vertexes =
        [
            new(0, 0, 0, Color.Red),
            new(1, 0, 0, Color.Green),
            new(0, 1, 0, Color.Blue)
        ];
        using var group = new VertexGroup(PrimitiveType.TriangleList, vertexes);
        group.Engine = Engine;

        group.BoundingSphere.Should().NotBeNull();
    }
}
