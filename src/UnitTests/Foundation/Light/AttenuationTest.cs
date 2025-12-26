/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using FluentAssertions;
using Xunit;

namespace OmegaEngine.Foundation.Light;

/// <summary>
/// Contains test methods for <see cref="Attenuation"/>.
/// </summary>
public class AttenuationTest
{
    [Fact]
    public void ApplyNone()
    {
        Attenuation.None.Apply(distance: 100).Should().Be(1);
    }

    [Fact]
    public void ApplyLinear()
    {
        var attenuation = new Attenuation(constant: 1, linear: 1, quadratic: 0);
        attenuation.Apply(distance: 0).Should().Be(1); // At source: 1/(1+0+0) = 1
        attenuation.Apply(distance: 1).Should().Be(0.5f); // 1/(1+1+0) = 0.5
        attenuation.Apply(distance: 3).Should().Be(0.25f); // 1/(1+3+0) = 0.25
    }

    [Fact]
    public void ApplyQuadratic()
    {
        var attenuation = new Attenuation(constant: 1, linear: 0, quadratic: 1);
        attenuation.Apply(distance: 0).Should().Be(1); // At source
        attenuation.Apply(distance: 1).Should().Be(0.5f); // 1/(1+0+1) = 0.5
    }

    [Fact]
    public void TestApplyClamps()
    {
        var attenuation = new Attenuation(constant: 0.1f, linear: 0, quadratic: 0); // Would give >1 without clamping
        attenuation.Apply(distance: 0).Should().Be(1); // Clamped to 1
    }
}
