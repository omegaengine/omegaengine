/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using AwesomeAssertions;
using OmegaEngine.Foundation.Geometry;
using Vortice.Direct3D9;
using Xunit;

namespace OmegaEngine.Graphics.Meshes;

public class SkinInfoTest : EngineTestBase
{
    [StructLayout(LayoutKind.Sequential)]
    private record struct PositionNormal(Vector3 Position, Vector3 Normal);

    [Fact]
    public void SkinsVertexes()
    {
        var skinInfo = new SkinInfo(vertexCount: 4, ["Bone0", "Bone1"], [Matrix4x4.Identity, Matrix4x4.Identity],
            [(0, 0, 1), (0, 2, 0.5f), (1, 1, 1), (1, 2, 0.5f)]);
        skinInfo.BoneCount.Should().Be(2);

        using var source = new Mesh(Engine.Device, faceCount: 0, vertexCount: 4, VertexFormat.Position);
        source.WriteVertexBuffer<Vector3>([new(1, 0, 0), new(0, 1, 0), new(0, 0, 1), new(5, 5, 5)]);
        using var destination = source.Clone();

        skinInfo.UpdateSkinnedMesh([Matrix4x4.CreateTranslation(1, 0, 0), Matrix4x4.CreateTranslation(0, 2, 0)], boneInvTransposeTransforms: null, source, destination);

        destination.GetPoints().Should().Equal(
            new Vector3(2, 0, 0),
            new Vector3(0, 3, 0),
            new Vector3(0.5f, 1, 1),
            new Vector3(5, 5, 5)); // Not influenced by any bone
    }

    [Fact]
    public void SkinsNormalsWithInverseTranspose()
    {
        var skinInfo = new SkinInfo(vertexCount: 1, ["Bone"], [Matrix4x4.Identity], [(0, 0, 1)]);

        using var source = new Mesh(Engine.Device, faceCount: 0, vertexCount: 1, VertexFormat.Position | VertexFormat.Normal);
        source.WriteVertexBuffer<PositionNormal>([new(Vector3.Zero, Vector3.UnitX)]);
        using var destination = source.Clone();

        var rotation = Matrix4x4.CreateRotationZ(MathF.PI / 2);
        skinInfo.UpdateSkinnedMesh([rotation], [Matrix4x4.Transpose(VectorMath.Invert(rotation))], source, destination);

        var normal = destination.ReadVertexBuffer<PositionNormal>()[0].Normal;
        Vector3.Distance(normal, Vector3.UnitY).Should().BeLessThan(1e-5f);
    }
}
