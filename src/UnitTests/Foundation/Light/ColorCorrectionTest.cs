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
/// Contains test methods for <see cref="ColorCorrection"/>.
/// </summary>
public class ColorCorrectionTest
{
    [Fact]
    public void TestConstruction()
    {
        var correction = new ColorCorrection(1.5f, 0.5f, -0.5f, 90);
        correction.Brightness.Should().Be(1.5f);
        correction.Contrast.Should().Be(0.5f);
        correction.Saturation.Should().Be(-0.5f);
        correction.Hue.Should().Be(90);
    }

    [Fact]
    public void TestDefaultConstant()
    {
        ColorCorrection.Default.Brightness.Should().Be(1);
        ColorCorrection.Default.Contrast.Should().Be(1);
        ColorCorrection.Default.Saturation.Should().Be(1);
        ColorCorrection.Default.Hue.Should().Be(0);
    }

    [Fact]
    public void TestBrightnessClamp()
    {
        var correction = new ColorCorrection(brightness: 10); // > 5
        correction.Brightness.Should().Be(5);

        correction = new ColorCorrection(brightness: -1); // < 0
        correction.Brightness.Should().Be(0);
    }

    [Fact]
    public void TestContrastClamp()
    {
        var correction = new ColorCorrection(contrast: 10); // > 5
        correction.Contrast.Should().Be(5);

        correction = new ColorCorrection(contrast: -10); // < -5
        correction.Contrast.Should().Be(-5);
    }

    [Fact]
    public void TestSaturationClamp()
    {
        var correction = new ColorCorrection(saturation: 10); // > 5
        correction.Saturation.Should().Be(5);

        correction = new ColorCorrection(saturation: -10); // < -5
        correction.Saturation.Should().Be(-5);
    }

    [Fact]
    public void TestHueClamp()
    {
        var correction = new ColorCorrection(hue: 400); // > 360
        correction.Hue.Should().Be(360);

        correction = new ColorCorrection(hue: -10); // < 0
        correction.Hue.Should().Be(0);
    }

    [Fact]
    public void TestEquality()
    {
        var c1 = new ColorCorrection(1.5f, 0.5f, -0.5f, 90);
        var c2 = new ColorCorrection(1.5f, 0.5f, -0.5f, 90);
        var c3 = new ColorCorrection(2.0f, 0.5f, -0.5f, 90);

        (c1 == c2).Should().BeTrue();
        (c1 != c3).Should().BeTrue();
        c1.Equals(c2).Should().BeTrue();
        c1.Equals(c3).Should().BeFalse();
    }
}
