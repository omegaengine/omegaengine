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
    public void TestConstruction()
    {
        var attenuation = new Attenuation(1, 0.5f, 0.25f);
        attenuation.Constant.Should().Be(1);
        attenuation.Linear.Should().Be(0.5f);
        attenuation.Quadratic.Should().Be(0.25f);
    }

    [Fact]
    public void TestNoneConstant()
    {
        Attenuation.None.Constant.Should().Be(1);
        Attenuation.None.Linear.Should().Be(0);
        Attenuation.None.Quadratic.Should().Be(0);
    }

    [Fact]
    public void TestApply()
    {
        // No attenuation: should return 1 regardless of distance
        Attenuation.None.Apply(100).Should().Be(1);

        // Linear attenuation
        var linear = new Attenuation(1, 1, 0);
        linear.Apply(0).Should().Be(1); // At source: 1/(1+0+0) = 1
        linear.Apply(1).Should().Be(0.5f); // 1/(1+1+0) = 0.5
        linear.Apply(3).Should().Be(0.25f); // 1/(1+3+0) = 0.25

        // Quadratic attenuation
        var quadratic = new Attenuation(1, 0, 1);
        quadratic.Apply(0).Should().Be(1); // At source
        quadratic.Apply(1).Should().Be(0.5f); // 1/(1+0+1) = 0.5
    }

    [Fact]
    public void TestApplyClamps()
    {
        // Should clamp to [0, 1] range
        var attenuation = new Attenuation(0.1f, 0, 0); // Would give >1 without clamping
        attenuation.Apply(0).Should().Be(1); // Clamped to 1
    }

    [Fact]
    public void TestEquality()
    {
        var a1 = new Attenuation(1, 0.5f, 0.25f);
        var a2 = new Attenuation(1, 0.5f, 0.25f);
        var a3 = new Attenuation(2, 0.5f, 0.25f);

        (a1 == a2).Should().BeTrue();
        (a1 != a3).Should().BeTrue();
        a1.Equals(a2).Should().BeTrue();
        a1.Equals(a3).Should().BeFalse();
    }

    [Fact]
    public void TestHashCode()
    {
        var a1 = new Attenuation(1, 0.5f, 0.25f);
        var a2 = new Attenuation(1, 0.5f, 0.25f);
        
        a1.GetHashCode().Should().Be(a2.GetHashCode());
    }
}
