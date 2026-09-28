/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using AwesomeAssertions;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.LightSources;
using OmegaEngine.Graphics.Renderables;
using SlimDX;
using Xunit;

namespace OmegaEngine.Graphics;

public class SceneTest : EngineTestBase
{
    [Fact]
    public void PropagatesEngineToPositionablesAndDisposesThem()
    {
        var model = new Model(XMesh.Get(Engine, "Test/Box/Normal/Normal.x"));
        model.IsEngineSet.Should().BeFalse();

        var scene = new Scene { Positionables = { model } };

        // Setting the scene's engine propagates it through the collection to the model
        scene.Engine = Engine;
        model.IsEngineSet.Should().BeTrue();

        // Disposing the scene disposes its contained renderables
        scene.Dispose();
        model.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void PropagatesEngineToNestedRenderablesAndDisposesThem()
    {
        var pivot = new Pivot();
        var model = new Model(XMesh.Get(Engine, "Test/Box/Normal/Normal.x"));
        pivot.Children.Add(model);

        var scene = new Scene { Positionables = { pivot } };

        // The engine reaches renderables nested below a root
        scene.Engine = Engine;
        model.IsEngineSet.Should().BeTrue();

        // Disposing the scene disposes the whole hierarchy
        scene.Dispose();
        pivot.IsDisposed.Should().BeTrue();
        model.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void FailedAddLeavesThePreviousParentIntact()
    {
        var pivot = new Pivot();
        var child = new Pivot();
        pivot.Children.Add(child);
        using var scene = new Scene { Engine = Engine, Positionables = { pivot } };

        child.Dispose();
        Assert.Throws<ObjectDisposedException>(() => scene.Positionables.Add(child));

        pivot.Children.Should().Equal(child);
        scene.Positionables.Should().Equal(pivot);
    }

    /// <summary>A renderable without geometry that has a bounding sphere, so it can cast shadows.</summary>
    private sealed class Probe : PositionableRenderable
    {
        public Probe(float radius) => BoundingSphere = new(default, radius);
    }

    [Fact]
    public void AppliesShadowsFromNestedCasters()
    {
        Engine.Effects.Shadows = true;

        var light = new DirectionalLight {Direction = new(0, -1, 0), Diffuse = Color.White, Specular = Color.White};
        var caster = new Probe(radius: 2) {Position = new(0, 2, 0), ShadowCaster = true};
        var inner = new Pivot {Position = new(0, 3, 0), Children = {caster}};
        var outer = new Pivot {Position = new(0, 5, 0), Children = {inner}};
        using var scene = new Scene {Engine = Engine, Positionables = {outer}, Lights = {light}};

        // Directly below the caster, which sits at (0, 10, 0) in world space
        var receiver = new BoundingSphere(new(0, 0, 0), radius: 1);

        void ShouldBeShadowed() => scene.GetEffectiveLights(receiver, shadowing: true).Should().ContainSingle()
                                        .Which.Diffuse.ToArgb().Should().Be(Color.Black.ToArgb());
        void ShouldBeLit() => scene.GetEffectiveLights(receiver, shadowing: true).Should().Equal(light);

        ShouldBeShadowed();
        scene.GetEffectiveLights(receiver, shadowing: false).Should().Equal(light);

        // Changes to the flag and the hierarchy must be picked up, not served from a stale caster list
        caster.ShadowCaster = false;
        ShouldBeLit();
        caster.ShadowCaster = true;
        ShouldBeShadowed();

        inner.Children.Remove(caster);
        ShouldBeLit();
        outer.Children.Add(caster);
        ShouldBeShadowed();

        scene.Positionables.Remove(outer);
        ShouldBeLit();
        outer.Dispose();
    }
}
