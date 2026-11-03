/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Linq;
using System.Numerics;
using AwesomeAssertions;
using Xunit;

namespace OmegaEngine.Graphics.Meshes;

public class MeshImporterTest : EngineTestBase
{
    [Fact]
    public void LoadsStaticMeshWithMaterials()
    {
        var (mesh, materials) = MeshImporter.LoadStatic(Engine.Device, "Test/Box/Normal/Normal.x");
        using (mesh)
        {
            mesh.FaceCount.Should().BePositive();
            materials.Should().ContainSingle().Which.TextureFileName.Should().Be("Riverstone.png");
            mesh.GetDeclaration().Last().Should().Be(Vortice.Direct3D9.VertexElement.VertexDeclarationEnd);
        }
    }

    [Fact]
    public void MergesMeshesWithOneSubsetPerMaterial()
    {
        var (mesh, materials) = MeshImporter.LoadStatic(Engine.Device, "Test/Dwarf/Dwarf.x");
        using (mesh)
        {
            materials.Should().HaveCount(9);
            mesh.SubsetCount.Should().Be(materials.Length);
            Enumerable.Range(0, mesh.SubsetCount).Should().OnlyContain(subset => mesh.GetPoints(subset).Length > 0);
        }
    }

    [Fact]
    public void RejectsInvalidFiles()
    {
        var action = () => MeshImporter.LoadStatic(Engine.Device, "Test/Box/Normal/Riverstone.png");
        action.Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void LoadsFramesAndMeshes()
    {
        var hierarchy = MeshImporter.LoadHierarchy(Engine.Device, "Test/Animated/Spin.x");
        try
        {
            hierarchy.Frames.Select(x => x.Name).Should().Equal("Scene", "Bone");
            hierarchy.Frames[1].Parent.Should().Be(0);

            var container = hierarchy.Containers.Should().ContainSingle().Which;
            container.FrameIndex.Should().Be(1, "the mesh is attached to the bone frame");
            container.Mesh.FaceCount.Should().Be(2);
            container.Skin.Should().BeNull();

            hierarchy.BoundingSphere.Radius.Should().BeGreaterThan(0.5f);
        }
        finally
        {
            foreach (var container in hierarchy.Containers) container.Mesh.Dispose();
        }
    }

    [Fact]
    public void SamplesAnimations()
    {
        var hierarchy = MeshImporter.LoadHierarchy(Engine.Device, "Test/Animated/Spin.x");
        try
        {
            hierarchy.Animations.Select(x => x.Name).Should().Equal("Spin", "Rest");

            var spin = hierarchy.Animations[0];
            spin.Period.Should().BeApproximately(2, 1e-9, "200 ticks at 100 ticks per second");
            spin.GetPeriodicPosition(spin.Period * 1.25).Should().BeApproximately(spin.Period * 0.25, 1e-9);

            var channel = spin.Channels.Should().ContainSingle().Which;
            channel.FrameIndex.Should().Be(1, "animations drive the frame with their name");

            var (scale, rotation, translation) = channel.Sample(spin.Period / 2, fallback: default);
            scale.Should().Be(Vector3.One);
            Math.Abs(rotation.Z).Should().BeApproximately(1, 1e-5f, "half the period is a half turn around Z");
            translation.Should().Be(Vector3.Zero);
        }
        finally
        {
            foreach (var container in hierarchy.Containers) container.Mesh.Dispose();
        }
    }

    [Fact]
    public void LoadsSkinInfo()
    {
        var hierarchy = MeshImporter.LoadHierarchy(Engine.Device, "Test/Animated/SkinnedWave.x");
        try
        {
            var skinInfo = hierarchy.Containers.Should().ContainSingle().Which.Skin!;
            skinInfo.Should().NotBeNull();
            skinInfo.BoneNames.Should().BeEquivalentTo("Bone0", "Bone1");
            skinInfo.BoneOffsets[skinInfo.BoneNames.ToList().IndexOf("Bone1")].Translation.Should().Be(new Vector3(0, -1, 0));

            var wave = hierarchy.Animations.Should().ContainSingle().Which;
            var (scale, _, translation) = wave.Channels.Single().Sample(0, fallback: default);
            scale.Should().Be(Vector3.One);
            translation.Should().Be(new Vector3(0, 1, 0));
        }
        finally
        {
            foreach (var container in hierarchy.Containers) container.Mesh.Dispose();
        }
    }
}
