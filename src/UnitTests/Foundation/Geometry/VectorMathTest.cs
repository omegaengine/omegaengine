/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using FluentAssertions;
using SlimDX;
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
        var vector = DoubleVector3.UnitX;
        var axis = DoubleVector3.UnitZ;
        var rotated = vector.RotateAroundAxis(axis, Math.PI / 2);

        rotated.X.Should().BeApproximately(0, precision: 1e-10);
        rotated.Y.Should().BeApproximately(1, precision: 1e-10);
        rotated.Z.Should().BeApproximately(0, precision: 1e-10);
    }

    [Fact]
    public void TestGetRotationTo()
    {
        var from = DoubleVector3.UnitX;
        var to = DoubleVector3.UnitY;
        (var axis, double rotation) = from.GetRotationTo(to);

        rotation.Should().BeApproximately(Math.PI / 2, 1e-10);
        axis.Z.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void TestGetRotationToOpposite()
    {
        var from = DoubleVector3.UnitX;
        var to = new DoubleVector3(-1, 0, 0);
        (var axis, double rotation) = from.GetRotationTo(to);

        rotation.Should().BeApproximately(Math.PI, 1e-9);
        axis.Length().Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void TestAdjustReference()
    {
        var vector = DoubleVector3.UnitX;
        var from = DoubleVector3.UnitX;
        var to = DoubleVector3.UnitY;

        var adjusted = vector.AdjustReference(from, to);
        adjusted.X.Should().BeApproximately(0, 1e-10);
        adjusted.Y.Should().BeApproximately(1, 1e-10);
    }
    [Fact]
    public void PerpendicularDistance_Vector2_PointOnRay_ReturnsZero()
    {
        var ray = new Vector2Ray(position: new Vector2(0, 0), direction: new Vector2(1, 0));
        var pointOnRay = new Vector2(5, 0);

        float distance = ray.PerpendicularDistance(pointOnRay);
        distance.Should().BeApproximately(0, precision: 0.0001f);
    }

    [Fact]
    public void PerpendicularDistance_Vector2_PointOffsetPerpendicularly_ReturnsCorrectDistance()
    {
        var ray = new Vector2Ray(position: new Vector2(2, 0), direction: new Vector2(1, 0));
        var point = new Vector2(5, 4);

        float distance = ray.PerpendicularDistance(point);
        distance.Should().BeApproximately(4, precision: 0.0001f);
    }

    [Fact]
    public void PerpendicularDistance_Vector2_PointBehindRay_StillComputesPerpendicularDistance()
    {
        var ray = new Vector2Ray(position: new Vector2(0, 2), direction: new Vector2(1, 0));
        var point = new Vector2(-2, 5);

        float distance = ray.PerpendicularDistance(point);
        distance.Should().BeApproximately(3, precision: 0.0001f);
    }

    [Fact]
    public void PerpendicularDistance_Vector3_PointOnRay_ReturnsZero()
    {
        var ray = new Ray(position: new Vector3(2, 0, 0), direction: new Vector3(1, 0, 0));
        var pointOnRay = new Vector3(7, 0, 0);

        float distance = ray.PerpendicularDistance(pointOnRay);
        distance.Should().BeApproximately(0, precision: 0.0001f);
    }

    [Fact]
    public void PerpendicularDistance_Vector3_PointOffsetPerpendicularly_ReturnsCorrectDistance()
    {
        var ray = new Ray(position: new Vector3(0, 2, 0), direction: new Vector3(1, 0, 0));
        var point = new Vector3(3, 6, 0);

        float distance = ray.PerpendicularDistance(point);
        distance.Should().BeApproximately(4, precision: 0.0001f);
    }

    [Fact]
    public void PerpendicularDistance_Vector3_PointBehindRay_StillComputesPerpendicularDistance()
    {
        var ray = new Ray(position: new Vector3(0, 0, 2), direction: new Vector3(1, 0, 0));
        var point = new Vector3(-2, 3, 2);

        float distance = ray.PerpendicularDistance(point);
        distance.Should().BeApproximately(3, precision: 0.0001f);
    }
}
