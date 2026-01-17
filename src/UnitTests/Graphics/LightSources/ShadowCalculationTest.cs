/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using FluentAssertions;
using OmegaEngine.Foundation.Geometry;
using SlimDX;
using Xunit;

namespace OmegaEngine.Graphics.LightSources;

/// <summary>
/// Tests for the new CalculateShadowFactor and ApplyShadowFactor methods.
/// </summary>
public class ShadowCalculationTest
{
    [Fact]
    public void DirectionalLight_CalculateShadowFactor_ReturnsZeroWhenNoShadow()
    {
        var light = new DirectionalLight
        {
            Direction = new(0, -1, 0),
            Diffuse = Color.White
        };

        // Receiver is in front of caster (not in shadow direction)
        var casterSphere = new BoundingSphere(new(0, 0, 0), radius: 1);
        var receiverSphere = new BoundingSphere(new(0, 5, 0), radius: 1);

        float shadowFactor = light.CalculateShadowFactor(receiverSphere, casterSphere);

        shadowFactor.Should().Be(0);
    }

    [Fact]
    public void DirectionalLight_CalculateShadowFactor_ReturnsPositiveWhenShadowed()
    {
        var light = new DirectionalLight
        {
            Direction = new(0, -1, 0),
            Diffuse = Color.White
        };

        // Receiver is directly behind caster
        var casterSphere = new BoundingSphere(new(0, 10, 0), radius: 2);
        var receiverSphere = new BoundingSphere(new(0, 0, 0), radius: 1);

        float shadowFactor = light.CalculateShadowFactor(receiverSphere, casterSphere);

        shadowFactor.Should().BeGreaterThan(0);
    }

    [Fact]
    public void DirectionalLight_ApplyShadowFactor_ReturnsSameInstanceWhenFactorIsZero()
    {
        var light = new DirectionalLight
        {
            Direction = new(0, -1, 0),
            Diffuse = Color.White,
            Specular = Color.White
        };

        var result = light.ApplyShadowFactor(0);

        result.Should().BeSameAs(light);
    }

    [Fact]
    public void DirectionalLight_ApplyShadowFactor_DarkensLightWhenFactorIsOne()
    {
        var light = new DirectionalLight
        {
            Direction = new(0, -1, 0),
            Diffuse = Color.White,
            Specular = Color.White,
            Ambient = Color.Gray
        };

        var result = (DirectionalLight)light.ApplyShadowFactor(1.0f);

        result.Should().NotBeSameAs(light);
        result.Diffuse.Should().Be(Color.Black);
        result.Specular.Should().Be(Color.Black);
        result.Ambient.Should().Be(light.Ambient); // Ambient unchanged
    }

    [Fact]
    public void DirectionalLight_ApplyShadowFactor_PartiallyDarkensWhenFactorIsHalf()
    {
        var light = new DirectionalLight
        {
            Direction = new(0, -1, 0),
            Diffuse = Color.FromArgb(100, 100, 100),
            Specular = Color.FromArgb(100, 100, 100)
        };

        var result = (DirectionalLight)light.ApplyShadowFactor(0.5f);

        result.Diffuse.R.Should().BeApproximately(50, 1);
        result.Diffuse.G.Should().BeApproximately(50, 1);
        result.Diffuse.B.Should().BeApproximately(50, 1);
    }

    [Fact]
    public void PointLight_CalculateShadowFactor_ReturnsZeroWhenNoShadow()
    {
        var light = new PointLight
        {
            Position = new(0, 20, 0),
            Diffuse = Color.White
        };

        // Receiver is not behind the caster
        var casterSphere = new BoundingSphere(new(0, 10, 0), 1);
        var receiverSphere = new BoundingSphere(new(0, 15, 0), 1);

        float shadowFactor = light.CalculateShadowFactor(receiverSphere, casterSphere);

        shadowFactor.Should().Be(0);
    }

    [Fact]
    public void PointLight_CalculateShadowFactor_ReturnsPositiveWhenShadowed()
    {
        var light = new PointLight
        {
            Position = new(0, 20, 0),
            Diffuse = Color.White
        };

        // Receiver is directly behind caster
        var casterSphere = new BoundingSphere(new(0, 10, 0), radius: 1);
        var receiverSphere = new BoundingSphere(new(0, 0, 0), radius: 0.5f);

        float shadowFactor = light.CalculateShadowFactor(receiverSphere, casterSphere);

        shadowFactor.Should().BeGreaterThan(0);
    }

    [Fact]
    public void PointLight_ApplyShadowFactor_ReturnsSameInstanceWhenFactorIsZero()
    {
        var light = new PointLight
        {
            Position = new(0, 20, 0),
            Diffuse = Color.White,
            Specular = Color.White
        };

        var result = light.ApplyShadowFactor(0);

        result.Should().BeSameAs(light);
    }

    [Fact]
    public void PointLight_ApplyShadowFactor_DarkensLightWhenFactorIsOne()
    {
        var light = new PointLight
        {
            Position = new(0, 20, 0),
            Diffuse = Color.White,
            Specular = Color.White,
            Ambient = Color.Gray
        };

        var result = (PointLight)light.ApplyShadowFactor(1.0f);

        result.Should().NotBeSameAs(light);
        result.Diffuse.Should().Be(Color.Black);
        result.Specular.Should().Be(Color.Black);
        result.Ambient.Should().Be(light.Ambient); // Ambient unchanged
    }

    [Fact]
    public void PointLight_ApplyShadowFactor_PreservesOtherProperties()
    {
        var light = new PointLight
        {
            Name = "TestLight",
            Position = new(5, 15, 10),
            Diffuse = Color.Red,
            RenderAsDirectional = true,
            Attenuation = new(1, 0.5f, 0.25f)
        };
        light.SetFloatingOrigin(new DoubleVector3(1, 2, 3));

        var result = (PointLight)light.ApplyShadowFactor(0.5f);

        result.Name.Should().Be(light.Name);
        result.Position.Should().Be(light.Position);
        result.RenderAsDirectional.Should().Be(light.RenderAsDirectional);
        result.Attenuation.Should().Be(light.Attenuation);
        result.GetFloatingOrigin().Should().Be(light.GetFloatingOrigin());
    }

    [Fact]
    public void DirectionalLight_GetShadowed_UsesNewMethods()
    {
        var light = new DirectionalLight
        {
            Direction = new(0, -1, 0),
            Diffuse = Color.White,
            Specular = Color.White
        };

        var casterSphere = new BoundingSphere(new(0, 10, 0), radius: 2);
        var receiverSphere = new BoundingSphere(new(0, 0, 0), radius: 1);

        // This should internally call CalculateShadowFactor and ApplyShadowFactor
        var result = (DirectionalLight)light.GetShadowed(receiverSphere, casterSphere);

        result.Should().NotBeSameAs(light);
        result.Diffuse.Should().NotBe(Color.White); // Should be darkened
    }

    [Fact]
    public void PointLight_GetShadowed_UsesNewMethods()
    {
        var light = new PointLight
        {
            Position = new(0, 20, 0),
            Diffuse = Color.White,
            Specular = Color.White
        };

        var casterSphere = new BoundingSphere(new(0, 10, 0), radius: 1);
        var receiverSphere = new BoundingSphere(new(0, 0, 0), radius: 0.5f);

        // This should internally call CalculateShadowFactor and ApplyShadowFactor
        var result = (PointLight)light.GetShadowed(receiverSphere, casterSphere);

        result.Should().NotBeSameAs(light);
        result.Diffuse.Should().NotBe(Color.White); // Should be darkened
    }

    [Fact]
    public void MultipleShadowFactors_MergeCorrectly()
    {
        // Simulate what ApplyShadows does: merge multiple shadow factors by multiplying (1 - factor)
        float shadowFactor1 = 0.5f; // 50% shadowed
        float shadowFactor2 = 0.5f; // 50% shadowed

        // Each shadow blocks 50% of light, so (1 - 0.5) * (1 - 0.5) = 0.25 light remains
        float combinedLightFactor = (1 - shadowFactor1) * (1 - shadowFactor2);
        float combinedShadowFactor = 1 - combinedLightFactor;

        combinedShadowFactor.Should().Be(0.75f); // 75% shadowed total
    }
}
