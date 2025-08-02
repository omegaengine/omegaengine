/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using FluentAssertions;
using Xunit;

namespace OmegaEngine;

/// <summary>
/// Contains test methods for <see cref="MathUtils"/>.
/// </summary>
public class MathUtilsTest
{
    [Fact]
    public void TestModuloDouble()
    {
        5.0.Modulo(3).Should().Be(2);
        4.0.Modulo(3).Should().Be(1);
        3.0.Modulo(3).Should().Be(0);
        2.0.Modulo(3).Should().Be(2);
        1.0.Modulo(3).Should().Be(1);
        0.0.Modulo(3).Should().Be(0);
        (-1.0).Modulo(3).Should().Be(2);
        (-2.0).Modulo(3).Should().Be(1);
        (-3.0).Modulo(3).Should().Be(0);
    }

    [Fact]
    public void TestModuloFloat()
    {
        5f.Modulo(3).Should().Be(2);
        4f.Modulo(3).Should().Be(1);
        3f.Modulo(3).Should().Be(0);
        2f.Modulo(3).Should().Be(2);
        1f.Modulo(3).Should().Be(1);
        0f.Modulo(3).Should().Be(0);
        (-1f).Modulo(3).Should().Be(2);
        (-2f).Modulo(3).Should().Be(1);
        (-3f).Modulo(3).Should().Be(0);
    }

    [Fact]
    public void TestModuloInt()
    {
        5.Modulo(3).Should().Be(2);
        4.Modulo(3).Should().Be(1);
        3.Modulo(3).Should().Be(0);
        2.Modulo(3).Should().Be(2);
        1.Modulo(3).Should().Be(1);
        0.Modulo(3).Should().Be(0);
        (-1).Modulo(3).Should().Be(2);
        (-2).Modulo(3).Should().Be(1);
        (-3).Modulo(3).Should().Be(0);
    }
}
