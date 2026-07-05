/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using FluentAssertions;
using SlimDX.Direct3D9;
using Xunit;

namespace OmegaEngine.Graphics;

public class RenderTargetTest : EngineTestBase
{
    [Fact]
    public void CreatesTextureWithRequestedSize()
    {
        using var renderTarget = new RenderTarget(Engine, new Size(256, 128));

        renderTarget.Texture.Should().NotBeNull();
    }

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
}
