/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using AwesomeAssertions;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.LightSources;
using OmegaEngine.Graphics.Renderables;
using SlimDX.Direct3D9;
using Xunit;

namespace OmegaEngine;

/// <summary>
/// Tests that the sRGB render states are confined to the scene pass.
/// </summary>
/// <remarks><see cref="Engine.Render()"/> skips all work while the render target is invisible, so these tests show the window.</remarks>
public class EngineSrgbRenderTest : EngineTestBase
{
    /// <summary>
    /// Builds a minimal scene with a lit body and adds it to the engine as a <see cref="View"/>.
    /// </summary>
    private View AddView()
    {
        Engine.Target.Show();

        var model = Model.Box(Engine, XMaterial.Default, new(2, 2, 2));
        var scene = new Scene
        {
            Positionables = {model},
            Lights = {new DirectionalLight {Direction = new(0, -1, 0), Diffuse = Color.White}}
        };
        var view = new View(scene, new ArcballCamera {Radius = 20, NearClip = 1, FarClip = 100})
        {
            Name = "sRGB test",
            Lighting = true,
            BackgroundColor = Color.CornflowerBlue
        };
        Engine.Views.Add(view);
        return view;
    }

    [Fact]
    public void ExtraRenderSeesSrgbTurnedOff()
    {
        AddView();

        int srgbWrite = -1, srgbTexture = -1;
        Engine.ExtraRender += () =>
        {
            srgbWrite = Engine.Device.GetRenderState(RenderState.SrgbWriteEnable);
            srgbTexture = Engine.Device.GetSamplerState(0, SamplerState.SrgbTexture);
        };

        Engine.Render(elapsedGameTime: 0, noPresent: true);

        // The GUI renders in gamma space and inherits whatever sampler state is live,
        // so a leak from the scene pass would wash out the whole HUD
        srgbWrite.Should().Be(0, "the GUI must not gamma-encode its output a second time");
        srgbTexture.Should().Be(0, "the GUI must not linearize its textures");
        srgbTexture.Should().NotBe(-1, "the ExtraRender hook must actually have run");
    }

    /// <summary>
    /// The background color is already gamma-encoded, so it must reach the render target unchanged.
    /// </summary>
    /// <remarks>
    /// Whether <see cref="SlimDX.Direct3D9.Device"/> clears honor <c>SrgbWriteEnable</c> is hardware-dependent:
    /// classic Direct3D 9 parts ignore it, DX10-class ones apply it and would brighten every view's background.
    /// <see cref="View.RenderBackground"/> therefore clears with sRGB writes off.
    /// </remarks>
    [Fact]
    public void BackgroundColorIsNotGammaEncoded()
    {
        var background = Color.FromArgb(255, 18, 20, 28);
        var view = AddView();
        view.BackgroundColor = background;

        Engine.Render(elapsedGameTime: 0, noPresent: true);

        // The body sits at the center of the view, so the top-left pixel is background
        var pixel = ReadBackBufferPixel();
        pixel.R.Should().BeCloseTo(background.R, 2);
        pixel.G.Should().BeCloseTo(background.G, 2);
        pixel.B.Should().BeCloseTo(background.B, 2);
    }

    private Color ReadBackBufferPixel()
    {
        var description = Engine.Device.GetBackBuffer(0, 0).Description;
        using var systemSurface = Surface.CreateOffscreenPlain(
            Engine.Device, description.Width, description.Height, description.Format, Pool.SystemMemory);
        Engine.Device.GetRenderTargetData(Engine.Device.GetBackBuffer(0, 0), systemSurface);

        var data = systemSurface.LockRectangle(LockFlags.ReadOnly).Data;
        try
        {
            return Color.FromArgb(data.Read<int>());
        }
        finally
        {
            systemSurface.UnlockRectangle();
        }
    }

    [Fact]
    public void SrgbWriteDoesNotSurviveTheFrame()
    {
        AddView();

        Engine.Render(elapsedGameTime: 0, noPresent: true);

        Engine.State.SrgbWrite.Should().BeFalse();
        Engine.Device.GetRenderState(RenderState.SrgbWriteEnable).Should().Be(0);
    }

    [Fact]
    public void SceneIsRenderedWithSrgbWriteOn()
    {
        var view = AddView();

        // Sampled from inside the scene pass via a body that is rendered after the background
        int srgbWrite = -1;
        view.Scene.Positionables.Add(new Probe(() => srgbWrite = Engine.Device.GetRenderState(RenderState.SrgbWriteEnable)));

        Engine.Render(elapsedGameTime: 0, noPresent: true);

        srgbWrite.Should().NotBe(-1, "the probe must actually have been rendered");
        srgbWrite.Should().NotBe(0, "all shading in the scene pass happens in linear space and must be gamma-encoded on write");
    }

    /// <summary>
    /// Particle systems render in gamma space, but must hand the scene pass back with sRGB writes on.
    /// </summary>
    [Fact]
    public void ParticleSystemRestoresSrgbWrite()
    {
        var view = AddView();

        int srgbWriteAfter = -1, srgbTextureAfter = -1;
        view.Scene.Positionables.Add(new ProbingParticleSystem(() =>
        {
            srgbWriteAfter = Engine.Device.GetRenderState(RenderState.SrgbWriteEnable);
            srgbTextureAfter = Engine.Device.GetSamplerState(0, SamplerState.SrgbTexture);
        })
        {
            Preset = CpuParticlePreset.FromContent("CleanFire.xml")
        });

        Engine.Render(elapsedGameTime: 0, noPresent: true);

        srgbWriteAfter.Should().NotBe(-1, "the particle system must actually have been rendered");
        srgbWriteAfter.Should().NotBe(0, "bodies rendered after the particle system still shade in linear space");
        srgbTextureAfter.Should().Be(0, "particle textures are sampled without linearization");
    }

    /// <summary>A <see cref="CpuParticleSystem"/> that reports device state right after rendering its particles.</summary>
    private sealed class ProbingParticleSystem(System.Action sampleAfter) : CpuParticleSystem
    {
        internal override void Render(Camera camera, GetEffectiveLights? getEffectiveLights = null)
        {
            base.Render(camera, getEffectiveLights);
            sampleAfter();
        }
    }

    /// <summary>A <see cref="PositionableRenderable"/> that only reports device state while the scene pass runs.</summary>
    private sealed class Probe(System.Action sample) : PositionableRenderable
    {
        internal override void Render(Camera camera, GetEffectiveLights? getEffectiveLights = null)
        {
            base.Render(camera, getEffectiveLights);
            sample();
        }
    }
}
