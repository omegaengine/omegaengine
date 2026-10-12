/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using AwesomeAssertions;
using System.Numerics;
using Vortice.Mathematics;
using Xunit;

namespace OmegaEngine.Graphics.Renderables;

public class BoundingBodyUtilsTests
{
    [Fact]
    public void TransformBoundingBox_WithIdentityMatrix_ShouldRemainUnchanged()
    {
        var box = new BoundingBox(min: new(-1, -2, -3), max: new(1, 2, 3));
        var result = box.Transform(Matrix4x4.Identity);

        result.Min.Should().Be(box.Min);
        result.Max.Should().Be(box.Max);
    }

    [Fact]
    public void TransformBoundingBox_WithTranslation_ShouldMoveCenterCorrectly()
    {
        var box = new BoundingBox(min: new(0, 0, 0), max: new(2, 2, 2));
        var translation = Matrix4x4.CreateTranslation(5, -3, 10);

        var result = box.Transform(translation);

        result.Min.Should().Be(new Vector3(5, -3, 10));
        result.Max.Should().Be(new Vector3(7, -1, 12));
    }

    [Fact]
    public void TransformBoundingBox_WithNonUniformScale_ShouldExpandCorrectly()
    {
        var box = new BoundingBox(min: new(-1, -1, -1), max: new(1, 1, 1));
        var scale = Matrix4x4.CreateScale(2, 3, 4);

        var result = box.Transform(scale);

        result.Min.Should().Be(new Vector3(-2, -3, -4));
        result.Max.Should().Be(new Vector3(2, 3, 4));
    }

    [Fact]
    public void TransformBoundingBox_WithRotation_ShouldExpandToCorrectAabb()
    {
        var box = new BoundingBox(min: new(-1, -2, -3), max: new(1,  2,  3));
        var result = box.Transform(Matrix4x4.CreateRotationZ((float)(Math.PI / 2)));

        const float eps = 1e-5f;
        result.Min.X.Should().BeApproximately(-2, eps);
        result.Min.Y.Should().BeApproximately(-1, eps);
        result.Min.Z.Should().BeApproximately(-3, eps);
        result.Max.X.Should().BeApproximately(2, eps);
        result.Max.Y.Should().BeApproximately(1, eps);
        result.Max.Z.Should().BeApproximately(3, eps);
    }

    [Fact]
    public void TransformBoundingSphere_WithIdentityMatrix_ShouldRemainUnchanged()
    {
        var sphere = new BoundingSphere(center: new(1, 2, 3), radius: 5);
        var result = sphere.Transform(Matrix4x4.Identity);

        result.Center.Should().Be(sphere.Center);
        result.Radius.Should().Be(sphere.Radius);
    }

    [Fact]
    public void TransformBoundingSphere_WithTranslation_ShouldMoveCenter()
    {
        var sphere = new BoundingSphere(center: new(0, 0, 0), radius: 3);
        var translation = Matrix4x4.CreateTranslation(10, -5, 2);

        var result = sphere.Transform(translation);

        result.Center.Should().Be(new Vector3(10, -5, 2));
        result.Radius.Should().Be(3);
    }

    [Fact]
    public void TransformBoundingSphere_WithNonUniformScale_ShouldUseMaxAxisScale()
    {
        var sphere = new BoundingSphere(center: new(0, 0, 0), radius: 3);
        var scale = Matrix4x4.CreateScale(2, 5, 3);

        var result = sphere.Transform(scale);

        result.Radius.Should().Be(15);
    }
}
