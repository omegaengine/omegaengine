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
using OmegaEngine.Foundation.Light;
using SlimDX.Direct3D9;
using Xunit;

namespace OmegaEngine.Graphics;

public class RenderTargetTest : EngineTestBase
{
    [Fact]
    public void EmptySizeCreatesFullscreenTexture()
    {
        using var renderTarget = new RenderTarget(Engine, Size.Empty);

        renderTarget.Texture.Should().NotBeNull();
    }

    [Fact]
    public void DisposeIsIdempotent()
    {
        var renderTarget = new RenderTarget(Engine, new Size(64, 64));

        renderTarget.Disposed.Should().BeFalse();
        renderTarget.Dispose();
        renderTarget.Disposed.Should().BeTrue();

        // Calling dispose a second time must not throw
        renderTarget.Invoking(rt => rt.Dispose()).Should().NotThrow();
    }

    [Fact]
    public void ImplicitlyConvertsToTexture()
    {
        using var renderTarget = new RenderTarget(Engine, new Size(64, 64));

        Texture texture = renderTarget;
        texture.Should().BeSameAs(renderTarget.Texture);
    }

    [Fact]
    public void SrgbWriteGammaEncodesTheOutput()
    {
        using var renderTarget = new RenderTarget(Engine, new Size(64, 64));

        var input = Color.FromArgb(255, 128, 128, 128);
        renderTarget.RenderTo(() =>
        {
            Engine.Device.Clear(ClearFlags.Target, Color.Black, 1, 0);
            Engine.State.AlphaBlend = EngineState.Opaque;
            Engine.State.SrgbWrite = true;
            Engine.DrawQuadColored(input);
            Engine.State.SrgbWrite = false;
        });

        // The vertex color reaches the ROP unmodified and is treated as a linear value there
        int expected = (int)(ColorUtils.LinearToSrgb(input.R / 255f) * 255 + 0.5f);
        ReadPixel(renderTarget).R.Should().BeCloseTo((byte)expected, 2);
    }

    [Fact]
    public void SrgbWriteDoesNotAffectTheOutputWhenOff()
    {
        using var renderTarget = new RenderTarget(Engine, new Size(64, 64));

        var input = Color.FromArgb(255, 128, 128, 128);
        renderTarget.RenderTo(() =>
        {
            Engine.Device.Clear(ClearFlags.Target, Color.Black, 1, 0);
            Engine.State.AlphaBlend = EngineState.Opaque;
            Engine.State.SrgbWrite = false;
            Engine.DrawQuadColored(input);
        });

        ReadPixel(renderTarget).R.Should().BeCloseTo(input.R, 2);
    }

    /// <summary>
    /// Pins down how the hardware combines <c>SRGBWRITEENABLE</c> with alpha blending, which the engine's
    /// additive multi-light passes depend on.
    /// </summary>
    /// <remarks>
    /// The Direct3D 9 specification leaves this to the driver:
    /// classic DX9 parts encode the source before blending it with the still-encoded destination (gamma space),
    /// while DX10-class parts exposing the D3D9 API generally decode the destination, blend in linear space and re-encode.
    /// Both are acceptable, so this test only asserts that the result matches one of the two.
    /// </remarks>
    [Fact]
    public void SrgbWriteBlendingMatchesAKnownModel()
    {
        using var renderTarget = new RenderTarget(Engine, new Size(64, 64));

        const float linearValue = 0.25f;
        var input = Color.FromArgb(255, (int)(linearValue * 255), (int)(linearValue * 255), (int)(linearValue * 255));
        renderTarget.RenderTo(() =>
        {
            Engine.Device.Clear(ClearFlags.Target, Color.Black, 1, 0);
            Engine.State.SrgbWrite = true;
            Engine.State.AlphaBlend = EngineState.AdditiveBlending;
            Engine.DrawQuadColored(input);
            Engine.DrawQuadColored(input);
            Engine.State.AlphaBlend = EngineState.Opaque;
            Engine.State.SrgbWrite = false;
        });

        float source = input.R / 255f;

        // Encoding the source before adding it to the (still encoded) destination
        byte gammaSpacePrediction = (byte)Math.Min(255, 2 * Encode(source));

        // Decoding the destination, adding in linear space and re-encoding
        byte linearSpacePrediction = Encode(source * 2);

        byte actual = ReadPixel(renderTarget).R;
        new[] {gammaSpacePrediction, linearSpacePrediction}
           .Should().Contain(candidate => Math.Abs(candidate - actual) <= 3,
                $"blending with sRGB writes must follow either the gamma-space model ({gammaSpacePrediction}) or the linear-space one ({linearSpacePrediction}), but the readback was {actual}");

        static byte Encode(float linear) => (byte)(ColorUtils.LinearToSrgb(linear) * 255 + 0.5f);
    }

    /// <summary>
    /// Copies a <see cref="RenderTarget"/> into system memory and reads its top-left pixel.
    /// </summary>
    private Color ReadPixel(RenderTarget renderTarget)
    {
        var description = renderTarget.Texture.GetLevelDescription(0);
        using var systemSurface = Surface.CreateOffscreenPlain(
            Engine.Device, description.Width, description.Height, description.Format, Pool.SystemMemory);
        Engine.Device.GetRenderTargetData(renderTarget.Surface, systemSurface);

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
}
