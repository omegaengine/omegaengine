/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using AwesomeAssertions;
using OmegaEngine.Graphics;
using SlimDX.Direct3D9;
using Xunit;

namespace OmegaEngine.Assets;

public class XMeshTest : EngineTestBase
{
    private const string BoxMesh = "Test/Box/Normal/Normal.x";

    [Fact]
    public void LoadsMeshWithMaterialsAndBounds()
    {
        var mesh = XMesh.Get(Engine, BoxMesh);

        mesh.Materials.IsDefaultOrEmpty.Should().BeFalse();
        mesh.Materials[0].DiffuseMap.Should().NotBeNull("the mesh references a diffuse texture");
        mesh.Mesh.VertexCount.Should().BeGreaterThan(0);

        var boundingSphere = mesh.BoundingSphere;
        boundingSphere.Should().NotBeNull();
        boundingSphere!.Value.Radius.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetReturnsCachedInstance()
    {
        var first = XMesh.Get(Engine, BoxMesh);
        var second = XMesh.Get(Engine, BoxMesh);

        second.Should().BeSameAs(first);
    }

    [Fact]
    public void RenderMeshIsWriteOnlyInDefaultPool()
    {
        var mesh = XMesh.Get(Engine, BoxMesh);

        var vertexBuffer = mesh.Mesh.VertexBuffer.Description;
        vertexBuffer.Pool.Should().Be(Pool.Default);
        vertexBuffer.Usage.Should().HaveFlag(Usage.WriteOnly);

        var indexBuffer = mesh.Mesh.IndexBuffer.Description;
        indexBuffer.Pool.Should().Be(Pool.Default);
        indexBuffer.Usage.Should().HaveFlag(Usage.WriteOnly);
    }

    [Fact]
    public void PickingMeshStaysInSystemMemoryAndIsReadable()
    {
        var mesh = XMesh.Get(Engine, BoxMesh);

        mesh.PickingMesh.Should().NotBeSameAs(mesh.Mesh);
        mesh.PickingMesh.VertexBuffer.Description.Pool.Should().Be(Pool.SystemMemory);

        // The whole point of the copy: the CPU can still read it
        mesh.PickingMesh.VertexCount.Should().Be(mesh.Mesh.VertexCount);
        mesh.PickingMesh.GetPoints().Should().HaveCount(mesh.PickingMesh.VertexCount);
    }
}
