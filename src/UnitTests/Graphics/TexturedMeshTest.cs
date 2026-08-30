/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using AwesomeAssertions;
using OmegaEngine.Graphics.VertexDecl;
using Xunit;

namespace OmegaEngine.Graphics;

public class TexturedMeshTest : EngineTestBase
{
    [Fact]
    public void QuadHasFourVertexesAndTwoFaces()
    {
        using var mesh = TexturedMesh.Quad(Engine.Device, new(4, 4));

        mesh.VertexCount.Should().Be(4);
        mesh.FaceCount.Should().Be(2);
    }

    [Fact]
    public void BoxHasTwentyFourVertexesAndTwelveFaces()
    {
        using var mesh = TexturedMesh.Box(Engine.Device, new(2, 2, 2));

        mesh.VertexCount.Should().Be(24);
        mesh.FaceCount.Should().Be(12);
    }

    [Fact]
    public void BoxTextureCoordinatesStayWithinUnitSquare()
    {
        using var mesh = TexturedMesh.Box(Engine.Device, new(2, 2, 2));

        var vertexes = mesh.ReadVertexBuffer<PositionNormalTextured>();
        vertexes.Should().OnlyContain(v => v.Tu >= 0f && v.Tu <= 1f && v.Tv >= 0f && v.Tv <= 1f);
    }

    [Fact]
    public void SphereHasExpectedVertexAndFaceCount()
    {
        using var mesh = TexturedMesh.Sphere(Engine.Device, radius: 2, slices: 8, stacks: 4);

        mesh.VertexCount.Should().Be((8 + 1) * (4 + 1));
        mesh.FaceCount.Should().Be(8 * 4 * 2);
    }

    [Fact]
    public void DiscHasExpectedVertexAndFaceCount()
    {
        using var mesh = TexturedMesh.Disc(Engine.Device, radiusInner: 1, radiusOuter: 2, height: 1, segments: 8);

        mesh.VertexCount.Should().Be(8 * 4);
        mesh.FaceCount.Should().Be(8 * 8);
    }

    [Fact]
    public void GeneratingTbnProducesNonZeroTangents()
    {
        using var mesh = TexturedMesh.Box(Engine.Device, new(2, 2, 2), tbn: true);

        var vertexes = mesh.ReadVertexBuffer<PositionNormalTangentBinormalTextured>();
        vertexes.Should().Contain(v => v.Tangent != default);
    }

    [Fact]
    public void GetPointsHandlesMeshesWithCustomVertexDeclaration()
    {
        using var mesh = TexturedMesh.Box(Engine.Device, new(2, 2, 2), tbn: true);

        // A mesh carrying tangents has no equivalent FVF, so deriving the stride from the vertex format yields garbage
        var points = mesh.GetPoints();

        points.Should().HaveCount(mesh.VertexCount);
        SlimDX.BoundingBox.FromPoints(points).Diagonal().Length()
              .Should().BeApproximately(new SlimDX.Vector3(2, 2, 2).Length(), 0.001f);
    }

    [Fact]
    public void GeneratingTbnKeepsExistingTangents()
    {
        var mesh = TexturedMesh.Box(Engine.Device, new(2, 2, 2), tbn: true);
        try
        {
            var before = mesh.ReadVertexBuffer<PositionNormalTangentBinormalTextured>();

            TexturedMeshUtils.GenerateTBN(Engine.Device, ref mesh, weldVertexes: true);

            mesh.ReadVertexBuffer<PositionNormalTangentBinormalTextured>()
                .Should().Equal(before);
        }
        finally
        {
            mesh.Dispose();
        }
    }

    [Fact]
    public void GeneratingTbnReplacesZeroedTangents()
    {
        var mesh = TexturedMesh.Box(Engine.Device, new(2, 2, 2), tbn: true);
        try
        {
            var vertexes = mesh.ReadVertexBuffer<PositionNormalTangentBinormalTextured>();
            for (int i = 0; i < vertexes.Length; i++)
            {
                vertexes[i].Tangent = default;
                vertexes[i].Binormal = default;
            }
            mesh.WriteVertexBuffer(vertexes);

            TexturedMeshUtils.GenerateTBN(Engine.Device, ref mesh);

            mesh.ReadVertexBuffer<PositionNormalTangentBinormalTextured>()
                .Should().Contain(v => v.Tangent != default);
        }
        finally
        {
            mesh.Dispose();
        }
    }
}
