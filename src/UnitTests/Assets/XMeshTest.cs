/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using FluentAssertions;
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
}
