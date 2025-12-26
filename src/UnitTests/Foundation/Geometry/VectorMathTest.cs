/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using FluentAssertions;
using Xunit;

namespace OmegaEngine.Foundation.Geometry;

/// <summary>
/// Contains test methods for <see cref="VectorMath"/>.
/// </summary>
public class VectorMathTest
{
    [Fact]
    public void TestByteToAngle()
    {
        ((byte)0).ByteToAngle().Should().Be(0);
        ((byte)255).ByteToAngle().Should().BeApproximately(Math.PI, 1e-10);
        ((byte)128).ByteToAngle().Should().BeApproximately(Math.PI / 2, 0.01);
    }

    [Fact]
    public void TestRotateAroundAxis()
    {
        var vector = new DoubleVector3(1, 0, 0);
        var axis = new DoubleVector3(0, 0, 1);
        var rotated = vector.RotateAroundAxis(axis, Math.PI / 2);

        rotated.X.Should().BeApproximately(0, precision: 1e-10);
        rotated.Y.Should().BeApproximately(1, precision: 1e-10);
        rotated.Z.Should().BeApproximately(0, precision: 1e-10);
    }

    [Fact]
    public void TestGetRotationTo()
    {
        var from = new DoubleVector3(1, 0, 0);
        var to = new DoubleVector3(0, 1, 0);
        (var axis, double rotation) = from.GetRotationTo(to);

        rotation.Should().BeApproximately(Math.PI / 2, 1e-10);
        axis.Z.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void TestGetRotationToOpposite()
    {
        var from = new DoubleVector3(1, 0, 0);
        var to = new DoubleVector3(-1, 0, 0);
        (var axis, double rotation) = from.GetRotationTo(to);

        rotation.Should().BeApproximately(Math.PI, 1e-9);
        axis.Length().Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void TestAdjustReference()
    {
        var vector = new DoubleVector3(1, 0, 0);
        var from = new DoubleVector3(1, 0, 0);
        var to = new DoubleVector3(0, 1, 0);

        var adjusted = vector.AdjustReference(from, to);
        adjusted.X.Should().BeApproximately(0, 1e-10);
        adjusted.Y.Should().BeApproximately(1, 1e-10);
    }
}
