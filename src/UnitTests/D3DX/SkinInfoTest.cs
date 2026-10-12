/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Numerics;
using System.Runtime.InteropServices;
using AwesomeAssertions;
using Vortice.Direct3D9;
using Xunit;

namespace OmegaEngine.D3DX;

public class SkinInfoTest : EngineTestBase
{
    [Fact]
    public void StoresBones()
    {
        using var skinInfo = new SkinInfo(vertexCount: 3, VertexFormat.Position, boneCount: 2);
        skinInfo.BoneCount.Should().Be(2);

        skinInfo.SetBoneName(1, "Arm");
        skinInfo.GetBoneName(1).Should().Be("Arm");

        var offset = Matrix4x4.CreateTranslation(1, 2, 3);
        skinInfo.SetBoneOffsetMatrix(1, offset);
        skinInfo.GetBoneOffsetMatrix(1).Should().Be(offset);
    }

    [Fact]
    public void SkinsVertices()
    {
        using var skinInfo = new SkinInfo(vertexCount: 3, VertexFormat.Position, boneCount: 2);
        skinInfo.SetBoneInfluence(0, [0, 2], [1, 0.5f]);
        skinInfo.SetBoneInfluence(1, [1, 2], [1, 0.5f]);

        var source = new[] {new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1)};
        var destination = new Vector3[3];
        var sourceHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
        var destinationHandle = GCHandle.Alloc(destination, GCHandleType.Pinned);
        try
        {
            skinInfo.UpdateSkinnedMesh(
                [Matrix4x4.CreateTranslation(1, 0, 0), Matrix4x4.CreateTranslation(0, 2, 0)],
                boneInvTransposeTransforms: null,
                sourceHandle.AddrOfPinnedObject(), destinationHandle.AddrOfPinnedObject());
        }
        finally
        {
            sourceHandle.Free();
            destinationHandle.Free();
        }

        destination.Should().Equal(new Vector3(2, 0, 0), new Vector3(0, 3, 0), new Vector3(0.5f, 1, 1));
    }
}
