/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using System.Linq;
using AwesomeAssertions;
using Xunit;

namespace OmegaEngine;

public class EngineCapabilitiesTest : EngineTestBase
{
    [Fact]
    public void GetFullscreenDisplayModeMatchesSupportedResolution()
    {
        var supported = Engine.Capabilities.DisplayModes.First();

        var mode = Engine.Capabilities.GetFullscreenDisplayMode(new(supported.Width, supported.Height));

        new Size(mode.Width, mode.Height).Should().Be(new Size(supported.Width, supported.Height));
        mode.RefreshRate.Should().BePositive("Direct3D9Ex rejects a fullscreen display mode without a valid refresh rate");
    }

    [Fact]
    public void GetFullscreenDisplayModeFallsBackToDesktopModeForUnsupportedResolution()
    {
        var mode = Engine.Capabilities.GetFullscreenDisplayMode(new(1, 1));

        Engine.Capabilities.CheckResolution(mode.Width, mode.Height).Should().BeTrue();
        mode.RefreshRate.Should().BePositive();
    }
}
