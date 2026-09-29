/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using AwesomeAssertions;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.LightSources;
using OmegaEngine.Graphics.Renderables;
using Xunit;

namespace OmegaEngine.Graphics;

/// <summary>
/// Tests that render passes are communicated via <see cref="RenderContext"/> instead of by modifying renderables.
/// </summary>
/// <remarks><see cref="Engine.Render()"/> skips all work while the render target is invisible, so these tests show the window.</remarks>
public class RenderContextTest : EngineTestBase
{
    [Fact]
    public void SpecialPassesOverrideConfiguredEffect()
    {
        foreach (var configured in new[] {SurfaceEffect.Plain, SurfaceEffect.FixedFunction, SurfaceEffect.Shader})
        {
            RenderContext.GetSurfaceEffect(configured, RenderPass.Glow, lighting: true, shaderAvailable: true).Should().Be(SurfaceEffect.Glow);
            RenderContext.GetSurfaceEffect(configured, RenderPass.Depth, lighting: true, shaderAvailable: true).Should().Be(SurfaceEffect.Depth);
        }
    }

    [Fact]
    public void SceneKeepsConfiguredEffectWhenEverythingIsAvailable()
    {
        foreach (var configured in new[] {SurfaceEffect.Plain, SurfaceEffect.FixedFunction, SurfaceEffect.Shader, SurfaceEffect.Glow, SurfaceEffect.Depth})
            RenderContext.GetSurfaceEffect(configured, RenderPass.Scene, lighting: true, shaderAvailable: true).Should().Be(configured);
    }

    [Fact]
    public void SceneFallsBackToPlainWithoutLighting()
    {
        RenderContext.GetSurfaceEffect(SurfaceEffect.Shader, RenderPass.Scene, lighting: false, shaderAvailable: true).Should().Be(SurfaceEffect.Plain);
        RenderContext.GetSurfaceEffect(SurfaceEffect.FixedFunction, RenderPass.Scene, lighting: false, shaderAvailable: true).Should().Be(SurfaceEffect.Plain);
        RenderContext.GetSurfaceEffect(SurfaceEffect.Shader, RenderPass.Scene, lighting: false, shaderAvailable: false).Should().Be(SurfaceEffect.Plain);
    }

    [Fact]
    public void SceneFallsBackToFixedFunctionWithoutShader()
        => RenderContext.GetSurfaceEffect(SurfaceEffect.Shader, RenderPass.Scene, lighting: true, shaderAvailable: false).Should().Be(SurfaceEffect.FixedFunction);

    [Fact]
    public void UnlitViewDoesNotModifySurfaceEffect()
    {
        var model = Model.Box(Engine, XMaterial.Default, new(2, 2, 2));
        model.SurfaceEffect = SurfaceEffect.Shader;
        using var scene = new Scene {Positionables = {model}};
        AddView(scene, lighting: false);

        Engine.Render(elapsedGameTime: 0, noPresent: true);

        model.RenderCount.Should().BeGreaterThan(0, "the model must actually have been rendered");
        model.SurfaceEffect.Should().Be(SurfaceEffect.Shader, "falling back to plain rendering must not stick once lighting is enabled again");
    }

    [Fact]
    public void MissingShaderDoesNotModifySurfaceEffect()
    {
        var model = Model.Box(Engine, XMaterial.Default, new(2, 2, 2));
        model.SurfaceEffect = SurfaceEffect.Shader;
        model.SurfaceShader = null;
        using var scene = new Scene {Positionables = {model}};
        AddView(scene, lighting: true);

        Engine.Render(elapsedGameTime: 0, noPresent: true);

        model.RenderCount.Should().BeGreaterThan(0, "the model must actually have been rendered");
        model.SurfaceEffect.Should().Be(SurfaceEffect.Shader, "falling back to the fixed-function pipeline must not stick once a shader is set again");
    }

    [Fact]
    public void GlowViewDoesNotModifySurfaceEffect()
    {
        Engine.Effects.PostScreenEffects = true;
        var model = Model.Box(Engine, XMaterial.Default, new(2, 2, 2));
        model.SurfaceEffect = SurfaceEffect.Shader;
        using var scene = new Scene {Positionables = {model}};
        AddView(scene, lighting: true).SetupGlow();

        Engine.Render(elapsedGameTime: 0, noPresent: true);

        model.RenderCount.Should().BeGreaterThan(0, "the model must actually have been rendered");
        model.SurfaceEffect.Should().Be(SurfaceEffect.Shader);
    }

    private View AddView(Scene scene, bool lighting)
    {
        Engine.Target.Show();

        scene.Lights.Add(new DirectionalLight {Direction = new(0, -1, 0), Diffuse = Color.White});
        var view = new View(scene, new ArcballCamera {Radius = 20, NearClip = 1, FarClip = 100})
        {
            Name = "Render context test",
            Lighting = lighting,
            BackgroundColor = Color.CornflowerBlue
        };
        Engine.Views.Add(view);
        return view;
    }
}
