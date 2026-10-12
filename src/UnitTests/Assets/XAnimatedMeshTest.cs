/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Linq;
using AwesomeAssertions;
using OmegaEngine.Graphics.Renderables;
using SlimDX;
using Xunit;

namespace OmegaEngine.Assets;

public class XAnimatedMeshTest : EngineTestBase
{
    private const string DwarfMesh = "Test/Dwarf/Dwarf.x";

    [Fact]
    public void LoadsMeshHierarchy()
    {
        var mesh = XAnimatedMesh.Get(Engine, DwarfMesh);

        mesh.FrameCount.Should().BeGreaterThan(0);
        mesh.Containers.Should().NotBeEmpty();
        mesh.Containers.Should().Contain(c => c.Materials.Length > 0, "the mesh has materials");

        var boundingSphere = mesh.BoundingSphere;
        boundingSphere.Should().NotBeNull();
        boundingSphere!.Value.Radius.Should().BeGreaterThan(0);
    }

    [Fact]
    public void UpdatingAndRenderingDoesNotThrow()
    {
        var mesh = XAnimatedMesh.Get(Engine, DwarfMesh);
        using var model = new PokableModel(mesh) {Name = "Dwarf"};
        model.Engine = Engine;

        model.NumberSubsets.Should().BeGreaterThan(0);

        // Building the pose and (for skinned meshes) skinning must not throw
        model.Invoking(m => m.Poke()).Should().NotThrow();

        // Exercise the animation controls when the mesh actually has animations
        if (!model.AnimationSetNames.IsDefaultOrEmpty)
        {
            model.PlayAnimation(0);
            model.Invoking(m => m.Poke()).Should().NotThrow();
            model.CrossFadeTo(model.AnimationSetNames[^1]);
            model.Invoking(m => m.Poke()).Should().NotThrow();
        }
    }

    [Fact]
    public void PlaysAndBlendsFrameAnimation()
    {
        var mesh = XAnimatedMesh.Get(Engine, "Test/Animated/Spin.x");

        // The fixture has a frame hierarchy with two animation sets, listed in file order
        mesh.AnimationSetNames.Should().Equal("Spin", "Rest");

        // The first set actually rotates the frame over time
        var spin = mesh.AnimationSets[0];
        spin.GetTransformation(spin.GetPeriodicPosition(0.5), 0).Rotation.Should().NotBe(Quaternion.Identity);

        // The rigid mesh container must be attached to the animated frame, or it would not move
        mesh.Containers.Should().ContainSingle().Which.FrameIndex.Should().BeGreaterThan(0);

        using var model = new PokableModel(mesh) {Name = "Spin"};
        model.Engine = Engine;

        // The first animation is auto-selected on construction
        model.CurrentAnimation.Should().Be(mesh.AnimationSetNames[0]);

        // Sampling a single track must not throw
        model.Invoking(m => m.Poke()).Should().NotThrow();

        // Blending between two tracks must not throw
        model.CrossFadeTo(1);
        model.Invoking(m => m.Poke()).Should().NotThrow();
    }

    [Fact]
    public void SkinsAnimatedMesh()
    {
        var mesh = XAnimatedMesh.Get(Engine, "Test/Animated/SkinnedWave.x");

        // The fixture is a skinned mesh (two bones) with one animation
        mesh.Containers.Should().Contain(c => c.SkinInfo != null);
        mesh.Containers.First(c => c.SkinInfo != null).BoneOffsets.Length.Should().Be(2);
        mesh.AnimationSetNames.Should().NotBeEmpty();

        using var model = new PokableModel(mesh) {Name = "SkinnedWave"};
        model.Engine = Engine;

        // Advancing the animation performs CPU skinning (SkinInfo.UpdateSkinnedMesh) and must not throw
        model.Invoking(m => m.Poke()).Should().NotThrow();
    }

    /// <summary>Exposes the protected per-frame update hook for testing.</summary>
    private sealed class PokableModel(XAnimatedMesh mesh) : AnimatedModel(mesh)
    {
        public void Poke() => OnPreRender();
    }
}
