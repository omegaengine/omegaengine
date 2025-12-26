/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using FluentAssertions;
using Xunit;

namespace OmegaEngine.Foundation.Geometry;

/// <summary>
/// Contains test methods for <see cref="DoubleVector3"/>.
/// </summary>
public class DoubleVector3Test
{
    [Fact]
    public void TestAddition()
    {
        var v1 = new DoubleVector3(1, 2, 3);
        var v2 = new DoubleVector3(4, 5, 6);
        var result = v1 + v2;
        result.Should().Be(new DoubleVector3(5, 7, 9));
    }

    [Fact]
    public void TestSubtraction()
    {
        var v1 = new DoubleVector3(4, 5, 6);
        var v2 = new DoubleVector3(1, 2, 3);
        var result = v1 - v2;
        result.Should().Be(new DoubleVector3(3, 3, 3));
    }

    [Fact]
    public void TestMultiplication()
    {
        var vector = new DoubleVector3(1, 2, 3);
        (vector * 2.0).Should().Be(new DoubleVector3(2, 4, 6));
        (2.0 * vector).Should().Be(new DoubleVector3(2, 4, 6));
        (vector * 2.0f).Should().Be(new DoubleVector3(2, 4, 6));
        (2.0f * vector).Should().Be(new DoubleVector3(2, 4, 6));
    }

    [Fact]
    public void TestDivision()
    {
        var vector = new DoubleVector3(2, 4, 6);
        (vector / 2.0).Should().Be(new DoubleVector3(1, 2, 3));
        (vector / 2.0f).Should().Be(new DoubleVector3(1, 2, 3));
    }

    [Fact]
    public void TestDotProduct()
    {
        var v1 = new DoubleVector3(1, 2, 3);
        var v2 = new DoubleVector3(4, 5, 6);
        v1.DotProduct(v2).Should().Be(32); // 1*4 + 2*5 + 3*6 = 4 + 10 + 18 = 32
    }

    [Fact]
    public void TestCrossProduct()
    {
        var v1 = new DoubleVector3(1, 0, 0);
        var v2 = new DoubleVector3(0, 1, 0);
        var result = v1.CrossProduct(v2);
        result.Should().Be(new DoubleVector3(0, 0, 1));
    }

    [Fact]
    public void TestLength()
    {
        new DoubleVector3(3, 4, 0).Length().Should().Be(5);
        new DoubleVector3(0, 0, 0).Length().Should().Be(0);
    }

    [Fact]
    public void TestNormalize()
    {
        var vector = new DoubleVector3(3, 4, 0);
        var normalized = vector.Normalize();
        normalized.Length().Should().BeApproximately(1.0, 1e-10);
        normalized.X.Should().BeApproximately(0.6, 1e-10);
        normalized.Y.Should().BeApproximately(0.8, 1e-10);
    }

    [Fact]
    public void TestNormalizeZeroVector()
    {
        var vector = new DoubleVector3(0, 0, 0);
        var normalized = vector.Normalize();
        normalized.Should().Be(new DoubleVector3(0, 0, 0));
    }

    [Fact]
    public void TestLerp()
    {
        var start = new DoubleVector3(0, 0, 0);
        var end = new DoubleVector3(10, 20, 30);

        DoubleVector3.Lerp(start, end, 0).Should().Be(start);
        DoubleVector3.Lerp(start, end, 1).Should().Be(end);
        DoubleVector3.Lerp(start, end, 0.5).Should().Be(new DoubleVector3(5, 10, 15));
    }

    [Fact]
    public void TestEquality()
    {
        var v1 = new DoubleVector3(1, 2, 3);
        var v2 = new DoubleVector3(1, 2, 3);
        var v3 = new DoubleVector3(1, 2, 4);

        (v1 == v2).Should().BeTrue();
        (v1 != v3).Should().BeTrue();
        v1.Equals(v2).Should().BeTrue();
        v1.Equals(v3).Should().BeFalse();
    }
}
