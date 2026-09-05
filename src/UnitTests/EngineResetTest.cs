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
using Xunit;

namespace OmegaEngine;

/// <summary>
/// Tests resetting the Direct3D device, as triggered by changing <see cref="Engine.Config"/>.
/// </summary>
/// <remarks><see cref="Engine.Render()"/> skips all work while the render target is invisible, so these tests show the window.</remarks>
public class EngineResetTest : EngineTestBase
{
    private static readonly Size NewSize = new(160, 120);

    /// <summary>
    /// Makes the render target visible, changes the resolution and renders one frame to perform the pending reset.
    /// </summary>
    private void ResizeAndRender()
    {
        Engine.Target.Show();
        Engine.Config = Engine.Config with {TargetSize = NewSize};
        Engine.Render(elapsedGameTime: 0, noPresent: true);
    }

    [Fact]
    public void ChangingConfigSchedulesReset()
    {
        Engine.NeedsReset.Should().BeFalse();

        Engine.Config = Engine.Config with {TargetSize = NewSize};

        Engine.NeedsReset.Should().BeTrue();
    }

    [Fact]
    public void RenderPerformsPendingResetAndAppliesNewSize()
    {
        ResizeAndRender();

        Engine.NeedsReset.Should().BeFalse();
        Engine.RenderSize.Should().Be(NewSize);
    }

    [Fact]
    public void ResetRaisesDeviceLostAndDeviceReset()
    {
        int lost = 0, reset = 0;
        Engine.DeviceLost += () => lost++;
        Engine.DeviceReset += () => reset++;

        ResizeAndRender();

        lost.Should().Be(1);
        reset.Should().Be(1);
    }

    [Fact]
    public void ResetResizesRenderTargets()
    {
        using var renderTarget = new RenderTarget(Engine, Size.Empty);

        ResizeAndRender();

        // A fullscreen render target tracks the back buffer size, so it must have been recreated
        var description = renderTarget.Texture.GetLevelDescription(0);
        new Size(description.Width, description.Height).Should().Be(NewSize);
    }

    [Fact]
    public void RenderingKeepsWorkingAfterReset()
    {
        ResizeAndRender();

        Engine.Invoking(engine => engine.Render(elapsedGameTime: 0, noPresent: true)).Should().NotThrow();
        Engine.NeedsReset.Should().BeFalse();
    }
}
