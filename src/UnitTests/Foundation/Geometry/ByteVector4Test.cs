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
/// Contains test methods for <see cref="ByteVector4"/>.
/// </summary>
public class ByteVector4Test
{
    [Fact]
    public void TestConstruction()
    {
        var vector = new ByteVector4(10, 20, 30, 40);
        vector.X.Should().Be(10);
        vector.Y.Should().Be(20);
        vector.Z.Should().Be(30);
        vector.W.Should().Be(40);
    }

    [Fact]
    public void TestEquality()
    {
        var v1 = new ByteVector4(1, 2, 3, 4);
        var v2 = new ByteVector4(1, 2, 3, 4);
        var v3 = new ByteVector4(1, 2, 3, 5);

        (v1 == v2).Should().BeTrue();
        (v1 != v3).Should().BeTrue();
        v1.Equals(v2).Should().BeTrue();
        v1.Equals(v3).Should().BeFalse();
    }

    [Fact]
    public void TestToString()
    {
        var vector = new ByteVector4(10, 20, 30, 40);
        vector.ToString().Should().Be("(10, 20, 30, 40)");
    }

    [Fact]
    public void TestBoundaryValues()
    {
        var vector = new ByteVector4(0, 255, 128, 1);
        vector.X.Should().Be(0);
        vector.Y.Should().Be(255);
        vector.Z.Should().Be(128);
        vector.W.Should().Be(1);
    }

    [Fact]
    public void TestHashCode()
    {
        var v1 = new ByteVector4(1, 2, 3, 4);
        var v2 = new ByteVector4(1, 2, 3, 4);
        var v3 = new ByteVector4(4, 3, 2, 1);

        v1.GetHashCode().Should().Be(v2.GetHashCode());
        v1.GetHashCode().Should().NotBe(v3.GetHashCode());
    }
}
