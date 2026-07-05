/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using FluentAssertions;
using OmegaEngine.Graphics.VertexDecl;
using Xunit;

namespace OmegaEngine.Graphics;

public class BufferUtilsTest : EngineTestBase
{
    [Fact]
    public void VertexBufferRoundTrips()
    {
        PositionTextured[] vertexes =
        [
            new(0, 0, 0, 0, 0),
            new(1, 0, 0, 1, 0),
            new(0, 1, 0, 0, 1)
        ];

        using var buffer = Engine.Device.CreateVertexBuffer(vertexes, PositionTextured.Format);

        buffer.Read<PositionTextured>().Should().BeEquivalentTo(vertexes);
    }

    [Fact]
    public void IndexBuffer16BitRoundTrips()
    {
        short[] indexes = [0, 1, 2, 2, 1, 0];

        using var buffer = Engine.Device.CreateIndexBuffer(indexes);

        buffer.Read16Bit().Should().Equal(indexes);
    }

    [Fact]
    public void IndexBuffer32BitRoundTrips()
    {
        int[] indexes = [0, 1, 2, 2, 1, 0];

        using var buffer = Engine.Device.CreateIndexBuffer(indexes);

        buffer.Read32Bit().Should().Equal(indexes);
    }

    [Fact]
    public void MeshVertexAndIndexBuffersRoundTrip()
    {
        using var mesh = TexturedMesh.Quad(Engine.Device, new(2, 2));

        mesh.GetPoints().Should().HaveCount(mesh.VertexCount);
        mesh.ReadIndexBuffer().Should().HaveCount(mesh.FaceCount * 3);
    }
}
