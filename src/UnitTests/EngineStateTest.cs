/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using AwesomeAssertions;
using SlimDX.Direct3D9;
using Xunit;

namespace OmegaEngine;

/// <summary>
/// Contains test methods for <see cref="EngineState"/>.
/// </summary>
public class EngineStateTest : EngineTestBase
{
    [Fact]
    public void SrgbWriteDefaultsToOff()
    {
        Engine.State.SrgbWrite.Should().BeFalse();
        Engine.Device.GetRenderState(RenderState.SrgbWriteEnable).Should().Be(0);
    }

    [Fact]
    public void SrgbWriteReachesTheDevice()
    {
        Engine.State.SrgbWrite = true;

        Engine.State.SrgbWrite.Should().BeTrue();
        Engine.Device.GetRenderState(RenderState.SrgbWriteEnable).Should().NotBe(0);

        Engine.State.SrgbWrite = false;

        Engine.State.SrgbWrite.Should().BeFalse();
        Engine.Device.GetRenderState(RenderState.SrgbWriteEnable).Should().Be(0);
    }

    [Fact]
    public void SrgbTextureDefaultsToOff()
    {
        Engine.State.SrgbTexture.Should().BeFalse();
        Engine.Device.GetSamplerState(0, SamplerState.SrgbTexture).Should().Be(0);
    }

    [Fact]
    public void SrgbTextureReachesTheDevice()
    {
        Engine.State.SrgbTexture = true;

        Engine.State.SrgbTexture.Should().BeTrue();
        Engine.Device.GetSamplerState(0, SamplerState.SrgbTexture).Should().NotBe(0);

        Engine.State.SrgbTexture = false;

        Engine.State.SrgbTexture.Should().BeFalse();
        Engine.Device.GetSamplerState(0, SamplerState.SrgbTexture).Should().Be(0);
    }

    [Fact]
    public void SrgbCapabilitiesAreSupported()
    {
        // Expected on any hardware we test on; there is no fallback path
        Engine.Capabilities.SrgbRead.Should().BeTrue();
        Engine.Capabilities.SrgbWrite.Should().BeTrue();
    }
}
