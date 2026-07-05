/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using FluentAssertions;
using Xunit;

namespace OmegaEngine.Assets;

public class XTextureTest : EngineTestBase
{
    private const string Texture = "flag.png";

    [Fact]
    public void LoadsTexture()
    {
        var texture = XTexture.Get(Engine, Texture);

        texture.Texture.Should().NotBeNull();
    }

    [Fact]
    public void GetReturnsCachedInstance()
    {
        var first = XTexture.Get(Engine, Texture);
        var second = XTexture.Get(Engine, Texture);

        second.Should().BeSameAs(first);
    }

    [Fact]
    public void GetWithNullIdReturnsNull()
        => XTexture.Get(Engine, null).Should().BeNull();
}
