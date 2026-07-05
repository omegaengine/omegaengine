/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using FluentAssertions;
using OmegaEngine.Assets;
using Xunit;

namespace OmegaEngine.Graphics.Renderables;

public class SimpleSkyboxTest : EngineTestBase
{
    [Fact]
    public void FromAssetSetHoldsReferencesToAllSixFacesWhenTopAndBottomExist()
    {
        // The "green" test fixture has all 6 faces (rt, lf, up, dn, ft, bk)
        using var skybox = SimpleSkybox.FromAssetSet(Engine, "green", "jpg");

        XTexture.Get(Engine, "Skybox/green/up.jpg").ReferenceCount.Should().Be(1);
        XTexture.Get(Engine, "Skybox/green/dn.jpg").ReferenceCount.Should().Be(1);
    }

    [Fact]
    public void FromAssetSetFallsBackToCardboardStyleWhenTopOrBottomMissing()
    {
        // The "red" test fixture is missing dn.jpg, so must fall back to a 4-sided skybox without top/bottom
        using var skybox = SimpleSkybox.FromAssetSet(Engine, "red", "jpg");

        XTexture.Get(Engine, "Skybox/red/rt.jpg").ReferenceCount.Should().Be(1);
        XTexture.Get(Engine, "Skybox/red/ft.jpg").ReferenceCount.Should().Be(1);
    }

    [Fact]
    public void DisposeReleasesTextureReferences()
    {
        var skybox = SimpleSkybox.FromAssetSet(Engine, "green", "jpg");
        var texture = XTexture.Get(Engine, "Skybox/green/rt.jpg");
        texture.ReferenceCount.Should().Be(1);

        skybox.Dispose();

        texture.ReferenceCount.Should().Be(0);
    }
}
