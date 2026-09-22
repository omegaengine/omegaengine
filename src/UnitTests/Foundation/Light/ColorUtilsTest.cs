/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using AwesomeAssertions;
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

        result.A.Should().Be(color.A); // unchanged
        result.R.Should().Be(50);
        result.G.Should().Be(75);
        result.B.Should().Be(100);
    }

    [Fact]
    public void TestMultiplyClamp()
    {
        var color = Color.FromArgb(200, 100, 150, 200);

        var result1 = color.Multiply(1.5f);
        result1.R.Should().Be(100);

        var result2 = color.Multiply(-0.5f);
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

    [Theory]
    [InlineData(0f, 0f)]
    [InlineData(1f, 1f)]
    [InlineData(0.5f, 0.21404114f)]
    [InlineData(0.04045f, 0.0031308f)] // The knee, where the linear and the power segment meet
    [InlineData(0.02f, 0.00154799f)] // Below the knee: plain division by 12.92
    [InlineData(0.1f, 0.01002289f)] // Above the knee
    public void TestSrgbToLinear(float srgb, float linear)
        => ColorUtils.SrgbToLinear(srgb).Should().BeApproximately(linear, 1e-6f);

    [Theory]
    [InlineData(0f, 0f)]
    [InlineData(1f, 1f)]
    [InlineData(0.21404114f, 0.5f)]
    [InlineData(0.0031308f, 0.04045f)] // The knee, where the linear and the power segment meet
    [InlineData(0.00154799f, 0.02f)] // Below the knee: plain multiplication by 12.92
    [InlineData(0.01002289f, 0.1f)] // Above the knee
    public void TestLinearToSrgb(float linear, float srgb)
        => ColorUtils.LinearToSrgb(linear).Should().BeApproximately(srgb, 1e-6f);

    [Fact]
    public void TestSrgbLinearRoundTrip()
    {
        for (int i = 0; i <= 255; i++)
        {
            float srgb = i / 255f;
            ColorUtils.LinearToSrgb(ColorUtils.SrgbToLinear(srgb)).Should().BeApproximately(srgb, 1e-5f);
        }
    }

    [Fact]
    public void TestSrgbToLinearIsMonotonic()
    {
        float previous = -1;
        for (int i = 0; i <= 255; i++)
        {
            float current = ColorUtils.SrgbToLinear(i / 255f);
            current.Should().BeGreaterThan(previous);
            previous = current;
        }
    }

    [Fact]
    public void TestSrgbToLinearColor()
    {
        var result = Color.FromArgb(128, 255, 128, 0).SrgbToLinear();

        result.Alpha.Should().BeApproximately(128 / 255f, 1e-6f); // Alpha is not a color channel
        result.Red.Should().BeApproximately(1f, 1e-6f);
        result.Green.Should().BeApproximately(ColorUtils.SrgbToLinear(128 / 255f), 1e-6f);
        result.Blue.Should().Be(0f);
    }

    [Fact]
    public void TestSrgbToLinearColor4RoundTrip()
    {
        var color = new SlimDX.Color4(0.5f, 0.25f, 0.75f, 1f);

        var result = color.SrgbToLinear().LinearToSrgb();

        result.Alpha.Should().BeApproximately(color.Alpha, 1e-5f);
        result.Red.Should().BeApproximately(color.Red, 1e-5f);
        result.Green.Should().BeApproximately(color.Green, 1e-5f);
        result.Blue.Should().BeApproximately(color.Blue, 1e-5f);
    }

    [Fact]
    public void TestMultiplyLinear()
    {
        var color = Color.FromArgb(200, 128, 128, 128);
        var result = color.MultiplyLinear(0.5f);

        result.A.Should().Be(color.A); // unchanged

        // Halving the light intensity is not the same as halving the byte value
        result.R.Should().Be((byte)(ColorUtils.LinearToSrgb(ColorUtils.SrgbToLinear(128 / 255f) * 0.5f) * 255 + 0.5f));
        result.R.Should().NotBe(64);
    }

    [Fact]
    public void TestMultiplyLinearBoundaries()
    {
        var color = Color.FromArgb(200, 100, 150, 200);

        color.MultiplyLinear(1).Should().Be(color);
        color.MultiplyLinear(1.5f).Should().Be(color);
        color.MultiplyLinear(-0.5f).R.Should().Be(0);
    }

    [Fact]
    public void TestInterpolateLinear()
    {
        var color1 = Color.FromArgb(100, 0, 0, 0);
        var color2 = Color.FromArgb(200, 255, 255, 255);

        var result = ColorUtils.InterpolateLinear(0.5f, color1, color2);

        result.A.Should().Be(150); // Alpha stays a plain linear blend
        result.R.Should().Be((byte)(ColorUtils.LinearToSrgb(0.5f) * 255 + 0.5f));
        result.R.Should().NotBe(127); // The physical midpoint is not the byte midpoint
    }

    [Fact]
    public void TestInterpolateLinearBoundaries()
    {
        var color1 = Color.FromArgb(255, 20, 40, 60);
        var color2 = Color.FromArgb(255, 200, 150, 100);

        // The round-trip through linear space may be off by one least significant bit
        AssertClose(ColorUtils.InterpolateLinear(0, color1, color2), color1);
        AssertClose(ColorUtils.InterpolateLinear(1, color1, color2), color2);
        AssertClose(ColorUtils.InterpolateLinear(-0.5f, color1, color2), color1);
        AssertClose(ColorUtils.InterpolateLinear(1.5f, color1, color2), color2);

        static void AssertClose(Color actual, Color expected)
        {
            actual.A.Should().Be(expected.A);
            actual.R.Should().BeCloseTo(expected.R, 1);
            actual.G.Should().BeCloseTo(expected.G, 1);
            actual.B.Should().BeCloseTo(expected.B, 1);
        }
    }
}
