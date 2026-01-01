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

namespace OmegaEngine.Graphics.Renderables;

public class BoundingExtensionsTests
{
    [Fact]
    public void TransformBoundingBox_WithIdentityMatrix_ShouldRemainUnchanged()
    {
        var box = new BoundingBox(minimum: new(-1, -2, -3), maximum: new(1, 2, 3));
        var result = box.Transform(Matrix.Identity);

        result.Minimum.Should().Be(box.Minimum);
        result.Maximum.Should().Be(box.Maximum);
    }

    [Fact]
    public void TransformBoundingBox_WithTranslation_ShouldMoveCenterCorrectly()
    {
        var box = new BoundingBox(minimum: new(0, 0, 0), maximum: new(2, 2, 2));
        var translation = Matrix.Translation(5, -3, 10);

        var result = box.Transform(translation);

        result.Minimum.Should().Be(new Vector3(5, -3, 10));
        result.Maximum.Should().Be(new Vector3(7, -1, 12));
    }

    [Fact]
    public void TransformBoundingBox_WithUniformScale_ShouldScaleExtents()
    {
        var box = new BoundingBox(minimum: new(-1, -1, -1), maximum: new(1, 1, 1));
        var scale = Matrix.Scaling(scale: new(value: 2));

        var result = box.Transform(scale);

        result.Minimum.Should().Be(new Vector3(-2, -2, -2));
        result.Maximum.Should().Be(new Vector3(2, 2, 2));
    }

    [Fact]
    public void TransformBoundingBox_WithNonUniformScale_ShouldExpandCorrectly()
    {
        var box = new BoundingBox(minimum: new(-1, -1, -1), maximum: new(1, 1, 1));
        var scale = Matrix.Scaling(2, 3, 4);

        var result = box.Transform(scale);

        result.Minimum.Should().Be(new Vector3(-2, -3, -4));
        result.Maximum.Should().Be(new Vector3(2, 3, 4));
    }

    [Fact]
    public void TransformBoundingBox_WithRotation_ShouldExpandToCorrectAabb()
    {
        var box = new BoundingBox(minimum: new(-1, -2, -3), maximum: new(1,  2,  3));
        var result = box.Transform(Matrix.RotationZ((float)(Math.PI / 2)));

        const float eps = 1e-5f;
        result.Minimum.X.Should().BeApproximately(-2, eps);
        result.Minimum.Y.Should().BeApproximately(-1, eps);
        result.Minimum.Z.Should().BeApproximately(-3, eps);
        result.Maximum.X.Should().BeApproximately(2, eps);
        result.Maximum.Y.Should().BeApproximately(1, eps);
        result.Maximum.Z.Should().BeApproximately(3, eps);
    }

    [Fact]
    public void TransformBoundingSphere_WithIdentityMatrix_ShouldRemainUnchanged()
    {
        var sphere = new BoundingSphere(center: new(1, 2, 3), radius: 5);
        var result = sphere.Transform(Matrix.Identity);

        result.Center.Should().Be(sphere.Center);
        result.Radius.Should().Be(sphere.Radius);
    }

    [Fact]
    public void TransformBoundingSphere_WithTranslation_ShouldMoveCenter()
    {
        var sphere = new BoundingSphere(center: new(0, 0, 0), radius: 3);
        var translation = Matrix.Translation(10, -5, 2);

        var result = sphere.Transform(translation);

        result.Center.Should().Be(new Vector3(10, -5, 2));
        result.Radius.Should().Be(3);
    }

    [Fact]
    public void TransformBoundingSphere_WithUniformScale_ShouldScaleRadius()
    {
        var sphere = new BoundingSphere(center: new(0, 0, 0), radius: 3);
        var scale = Matrix.Scaling(scale: new(value: 2));

        var result = sphere.Transform(scale);

        result.Radius.Should().Be(6);
    }

    [Fact]
    public void TransformBoundingSphere_WithNonUniformScale_ShouldUseMaxAxisScale()
    {
        var sphere = new BoundingSphere(center: new(0, 0, 0), radius: 3);
        var scale = Matrix.Scaling(2, 5, 3);

        var result = sphere.Transform(scale);

        result.Radius.Should().Be(15);
    }

    [Fact]
    public void TransformBoundingSphere_WithZeroRadius_ShouldOnlyTranslate()
    {
        var sphere = new BoundingSphere(center: new(1, 2, 3), radius: 0);
        var translation = Matrix.Translation(5, 5, 5);

        var result = sphere.Transform(translation);

        result.Center.Should().Be(new Vector3(6, 7, 8));
        result.Radius.Should().Be(0);
    }
}
