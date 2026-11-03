/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using System.Linq;
using AwesomeAssertions;
using OmegaEngine.Graphics.VertexDecl;
using Xunit;

namespace OmegaEngine.Assets;

public class XMeshTest : EngineTestBase
{
    private const string BoxMesh = "Test/Box/Normal/Normal.x";
    private const string DwarfMesh = "Test/Dwarf/Dwarf.x";

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
    public void GlowMapDoublesAsEmissiveMapWhenThereIsNoDedicatedOne()
    {
        var mesh = XMesh.Get(Engine, DwarfMesh);

        var material = mesh.Materials.Single(x => x.GlowMap != null);
        material.EmissiveMap.Should().BeSameAs(material.GlowMap, "the subset has a glow map but no emissive map");
    }

    [Fact]
    public void GetReturnsCachedInstance()
    {
        var first = XMesh.Get(Engine, BoxMesh);
        var second = XMesh.Get(Engine, BoxMesh);

        second.Should().BeSameAs(first);
    }

    [Fact]
    public void GeneratesTangentsForNormalMappedMeshes()
    {
        var mesh = XMesh.Get(Engine, BoxMesh);

        mesh.Materials[0].NormalMap.Should().NotBeNull("the texture has a _normal companion");
        mesh.Mesh.ReadVertexBuffer<PositionNormalTangentBinormalTextured>()
            .Should().OnlyContain(x => x.Normal != default && x.Tangent != default);
    }

    [Fact]
    public void ThrowsForMissingFiles()
    {
        var action = () => XMesh.Get(Engine, "Test/Missing.x");
        action.Should().Throw<FileNotFoundException>();
    }
}
