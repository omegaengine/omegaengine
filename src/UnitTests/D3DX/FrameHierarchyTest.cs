/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Numerics;
using AwesomeAssertions;
using OmegaEngine.Foundation.Storage;
using Xunit;

namespace OmegaEngine.D3DX;

public class FrameHierarchyTest : EngineTestBase
{
    private FrameHierarchy Load(string id)
    {
        using var stream = ContentManager.GetFileStream("Meshes", id);
        return FrameHierarchy.FromStream(Engine.Device, stream, MeshFlags.SystemMemory);
    }

    [Fact]
    public void LoadsFramesAndMeshes()
    {
        using var hierarchy = Load("Test/Animated/Spin.x");

        var scene = hierarchy.RootFrames.Should().ContainSingle().Which;
        scene.Name.Should().Be("Scene");
        var bone = scene.Children.Should().ContainSingle().Which;
        bone.Name.Should().Be("Bone");

        var container = bone.MeshContainers.Should().ContainSingle().Which;
        hierarchy.MeshContainers.Should().Equal(container);
        container.Name.Should().Be("BoneMesh");
        container.Mesh.FaceCount.Should().Be(2);
        container.Materials.Should().ContainSingle();
        container.SkinInfo.Should().BeNull();

        hierarchy.BoundingSphere.Radius.Should().BeGreaterThan(0.5f);
    }

    [Fact]
    public void SamplesAnimationSets()
    {
        using var hierarchy = Load("Test/Animated/Spin.x");

        // D3DX lists the sets in reverse file order
        hierarchy.AnimationSets.Select(x => x.Name).Should().Equal("Rest", "Spin");

        var spin = hierarchy.AnimationSets[1];
        double period = spin.Period;
        period.Should().BePositive();
        spin.GetPeriodicPosition(period * 1.25).Should().BeApproximately(period * 0.25, 1e-9);
        spin.AnimationCount.Should().Be(1);
        spin.GetAnimationName(0).Should().Be("Bone", "animations are named after the frame they drive");

        var (scale, rotation, translation) = spin.GetTransformation(period / 2, 0);
        scale.Should().Be(Vector3.One);
        Math.Abs(rotation.Z).Should().BeApproximately(1, 1e-5f, "half the period is a half turn around Z");
        translation.Should().Be(Vector3.Zero);
    }

    [Fact]
    public void LoadsSkinInfo()
    {
        using var hierarchy = Load("Test/Animated/SkinnedWave.x");

        var skinInfo = hierarchy.MeshContainers.Should().ContainSingle().Which.SkinInfo!;
        skinInfo.Should().NotBeNull();
        skinInfo.BoneCount.Should().Be(2);

        var boneNames = Enumerable.Range(0, skinInfo.BoneCount).Select(skinInfo.GetBoneName).ToList();
        boneNames.Should().BeEquivalentTo("Bone0", "Bone1");
        skinInfo.GetBoneOffsetMatrix(boneNames.IndexOf("Bone1")).Translation.Should().Be(new Vector3(0, -1, 0));

        var wave = hierarchy.AnimationSets.Should().ContainSingle().Which;
        var (scale, _, translation) = wave.GetTransformation(0, 0);
        scale.Should().Be(Vector3.One);
        translation.Should().Be(new Vector3(0, 1, 0));
    }
}
