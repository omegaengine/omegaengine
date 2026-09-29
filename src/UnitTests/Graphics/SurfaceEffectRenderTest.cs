/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using AwesomeAssertions;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using Xunit;

namespace OmegaEngine.Graphics;

/// <summary>
/// Tests that rendering does not permanently modify <see cref="PositionableRenderable.SurfaceEffect"/>.
/// </summary>
/// <remarks><see cref="Engine.Render()"/> skips all work while the render target is invisible, so these tests show the window.</remarks>
public class SurfaceEffectRenderTest : EngineTestBase
{
    private void AddView(Scene scene, bool lighting)
        => Engine.Views.Add(new View(scene, new ArcballCamera {Radius = 20, NearClip = 1, FarClip = 100})
        {
            Name = lighting ? "Lit" : "Unlit",
            Lighting = lighting
        });

    [Theory]
    [InlineData(SurfaceEffect.FixedFunction)]
    [InlineData(SurfaceEffect.Shader)] // Without a SurfaceShader, so rendering falls back to fixed-function
    public void SceneSharedByLitAndUnlitViewKeepsSurfaceEffect(SurfaceEffect surfaceEffect)
    {
        var body = Model.Box(Engine, XMaterial.Default, new(2, 2, 2));
        body.SurfaceEffect = surfaceEffect;

        using var scene = new Scene {Positionables = {body}};
        Engine.Target.Show();
        AddView(scene, lighting: false);
        AddView(scene, lighting: true);

        Engine.Render(elapsedGameTime: 0, noPresent: true);

        body.RenderCount.Should().Be(2, "the body is rendered once by each view");
        body.SurfaceEffect.Should().Be(surfaceEffect, "the unlit view must not affect how the lit view renders the body");
    }
}
