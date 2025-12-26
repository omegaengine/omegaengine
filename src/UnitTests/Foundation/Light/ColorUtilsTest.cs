/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using FluentAssertions;
using Xunit;

namespace OmegaEngine.Foundation.Light;

/// <summary>
/// Contains test methods for <see cref="ColorUtils"/>.
/// </summary>
public class ColorUtilsTest
{
    [Fact]
    public void TestDropAlpha()
    {
        var color = Color.FromArgb(128, 255, 100, 50);
        var result = color.DropAlpha();
        
        result.A.Should().Be(255);
        result.R.Should().Be(255);
        result.G.Should().Be(100);
        result.B.Should().Be(50);
    }

    [Fact]
    public void TestEqualsIgnoreAlpha()
    {
        var color1 = Color.FromArgb(128, 100, 150, 200);
        var color2 = Color.FromArgb(255, 100, 150, 200);
        var color3 = Color.FromArgb(128, 100, 150, 201);

        color1.EqualsIgnoreAlpha(color2).Should().BeTrue();
        color1.EqualsIgnoreAlpha(color3).Should().BeFalse();
    }

    [Fact]
    public void TestMultiply()
    {
        var color = Color.FromArgb(200, 100, 150, 200);
        var result = color.Multiply(0.5f);
        
        result.A.Should().Be(100);
        result.R.Should().Be(50);
        result.G.Should().Be(75);
        result.B.Should().Be(100);
    }

    [Fact]
    public void TestMultiplyClamp()
    {
        var color = Color.FromArgb(200, 100, 150, 200);
        
        var result1 = color.Multiply(1.5f);
        result1.A.Should().Be(200);
        result1.R.Should().Be(100);

        var result2 = color.Multiply(-0.5f);
        result2.A.Should().Be(0);
        result2.R.Should().Be(0);
    }

    [Fact]
    public void TestInterpolate()
    {
        var color1 = Color.FromArgb(100, 0, 0, 0);
        var color2 = Color.FromArgb(200, 100, 100, 100);
        
        var result = ColorUtils.Interpolate(0.5f, color1, color2);
        result.A.Should().Be(150);
        result.R.Should().Be(50);
        result.G.Should().Be(50);
        result.B.Should().Be(50);
    }

    [Fact]
    public void TestInterpolateBoundaries()
    {
        var color1 = Color.Red;
        var color2 = Color.Blue;
        
        var result1 = ColorUtils.Interpolate(0, color1, color2);
        result1.R.Should().Be(color1.R);
        result1.B.Should().Be(color1.B);

        var result2 = ColorUtils.Interpolate(1, color1, color2);
        result2.R.Should().Be(color2.R);
        result2.B.Should().Be(color2.B);
    }

    [Fact]
    public void TestInterpolateClamp()
    {
        var color1 = Color.Red;
        var color2 = Color.Blue;
        
        var result1 = ColorUtils.Interpolate(1.5f, color1, color2);
        result1.Should().Be(ColorUtils.Interpolate(1.0f, color1, color2));

        var result2 = ColorUtils.Interpolate(-0.5f, color1, color2);
        result2.Should().Be(ColorUtils.Interpolate(0.0f, color1, color2));
    }
}
