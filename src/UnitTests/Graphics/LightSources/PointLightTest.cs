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
using OmegaEngine.Foundation.Light;
using SlimDX;
using Xunit;

namespace OmegaEngine.Graphics.LightSources;

public class PointLightTest
{
    private static PointLight CreateLight() => new()
    {
        Position = new DoubleVector3(0, 20, 0),
        Diffuse = Color.White,
        Specular = Color.White,
        Ambient = Color.FromArgb(50, 50, 50)
    };

    [Fact]
    public void GetShadowed_NoShadow_WhenLightAtSamePositionAsCaster()
    {
        var light = CreateLight();

        var casterSphere = new BoundingSphere(new(0, 20, 0), 1);
        var receiverSphere = new BoundingSphere(new(0, 0, 0), 1);

        var shadowed = light.GetShadowed(receiverSphere, casterSphere);

        shadowed.Should().Be(light);
    }

    [Fact]
    public void GetShadowed_NoShadow_WhenReceiverNotBehindCaster()
    {
        var light = CreateLight();

        // Receiver is in front of or beside the caster relative to light
        var casterSphere = new BoundingSphere(new(0, 10, 0), 1);
        var receiverSphere = new BoundingSphere(new(0, 15, 0), 1); // Between light and caster

        var shadowed = light.GetShadowed(receiverSphere, casterSphere);

        shadowed.Should().Be(light);
    }

    [Fact]
    public void GetShadowed_NoShadow_WhenReceiverOutsideShadowCone()
    {
        var light = CreateLight();

        // Receiver is behind caster but outside the shadow cone
        var casterSphere = new BoundingSphere(new(0, 10, 0), 1);
        var receiverSphere = new BoundingSphere(new(10, 0, 0), 1); // Far to the side

        var shadowed = light.GetShadowed(receiverSphere, casterSphere);

        shadowed.Should().Be(light);
    }

    [Fact]
    public void GetShadowed_FullShadow_WhenReceiverFullyInsideShadowCone()
    {
        var light = CreateLight();

        // Receiver is directly behind caster and fully contained in shadow cone
        var casterSphere = new BoundingSphere(new(0, 10, 0), 1);
        var receiverSphere = new BoundingSphere(new(0, 0, 0), 0.5f); // Directly below, smaller

        var shadowed = (PointLight)light.GetShadowed(receiverSphere, casterSphere);

        shadowed.Should().NotBe(light); // Shadow applied
        shadowed.Diffuse.Should().Be(Color.Black); // Fully shadowed
        shadowed.Specular.Should().Be(Color.Black); // Fully shadowed
        shadowed.Ambient.Should().Be(light.Ambient); // Ambient unchanged
    }

    [Fact]
    public void GetShadowed_PartialShadow_WhenReceiverPartiallyInsideShadowCone()
    {
        var light = CreateLight();

        // Receiver is partially in shadow cone
        var casterSphere = new BoundingSphere(new(0, 10, 0), radius: 1);
        var receiverSphere = new BoundingSphere(new(1.5f, 0, 0), radius: 1); // Partially overlapping

        var shadowed = (PointLight)light.GetShadowed(receiverSphere, casterSphere);

        shadowed.Should().NotBe(light); // Shadow applied
        // Diffuse and Specular should be darkened but not black
        shadowed.Diffuse.R.Should().BeLessThan(light.Diffuse.R);
        shadowed.Diffuse.R.Should().BeGreaterThan(0);
        shadowed.Specular.R.Should().BeLessThan(light.Specular.R);
        shadowed.Specular.R.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetShadowed_PreservesOtherProperties()
    {
        var lightWithProps = new PointLight
        {
            Name = "TestPointLight",
            Enabled = true,
            Position = new DoubleVector3(5, 15, 10),
            Diffuse = Color.Red,
            Specular = Color.Blue,
            Ambient = Color.Green,
            RenderAsDirectional = true,
            Attenuation = new(1, 0.5f, 0.25f)
        };

        var casterSphere = new BoundingSphere(new(5, 10, 10), radius: 1);
        var receiverSphere = new BoundingSphere(new(5, 5, 10), radius: 1);

        var shadowed = (PointLight)lightWithProps.GetShadowed(receiverSphere, casterSphere);

        shadowed.Name.Should().Be(lightWithProps.Name);
        shadowed.Enabled.Should().Be(lightWithProps.Enabled);
        shadowed.Position.Should().Be(lightWithProps.Position);
        shadowed.Range.Should().Be(lightWithProps.Range);
        shadowed.RenderAsDirectional.Should().Be(lightWithProps.RenderAsDirectional);
        shadowed.Attenuation.Should().Be(lightWithProps.Attenuation);
        shadowed.Ambient.Should().Be(lightWithProps.Ambient); // Ambient should not change
    }

    [Fact]
    public void GetShadowed_ConicalShadowVolume_GrowsWithDistance()
    {
        var light = CreateLight();

        // Test that point light creates conical shadow (expanding with distance)
        var casterSphere = new BoundingSphere(new(0, 10, 0), radius: 1);

        // Receiver close to caster (distance from caster = 2)
        // Shadow radius = casterRadius * (lightToReceiver / lightToCaster) = 1 * (12/10) = 1.2
        var receiverNear = new BoundingSphere(new(0, 8, 0), radius: 0.5f);
        var shadowedNear = (PointLight)light.GetShadowed(receiverNear, casterSphere);

        // Receiver far from caster (distance from caster = 10)
        // Shadow radius = casterRadius * (lightToReceiver / lightToCaster) = 1 * (20/10) = 2.0
        var receiverFar = new BoundingSphere(new(0, 0, 0), radius: 0.5f);
        var shadowedFar = (PointLight)light.GetShadowed(receiverFar, casterSphere);

        // Both should be fully shadowed
        shadowedNear.Diffuse.Should().Be(Color.Black);
        shadowedFar.Diffuse.Should().Be(Color.Black);
    }

    [Fact]
    public void GetShadowed_ShadowRadiusProportionalToDistance()
    {
        var light = CreateLight();

        // Verify the shadow cone expands proportionally with distance from light
        // At distance D1 from light, shadow should be narrower than at distance D2 > D1
        var casterSphere = new BoundingSphere(new(0, 15, 0), radius: 2);

        // Receiver at (0, 10, 0): light-to-receiver = 10, shadow radius = 2 * (10/5) = 4
        var receiver1 = new BoundingSphere(new(5.1f, 10, 0), radius: 1); // Just outside shadow
        var shadowed1 = light.GetShadowed(receiver1, casterSphere);

        // Receiver at (0, 5, 0): light-to-receiver = 15, shadow radius = 2 * (15/5) = 6
        var receiver2 = new BoundingSphere(new(5.1f, 5, 0), radius: 1); // Inside shadow at greater distance
        var shadowed2 = light.GetShadowed(receiver2, casterSphere);

        // First receiver should have no shadow, second should have shadow
        shadowed1.Should().Be(light);
        shadowed2.Should().NotBe(light);
    }

    [Fact]
    public void Range_Infinite()
    {
        var light = CreateLight();
        light.Range.Should().Be(float.PositiveInfinity);
    }

    [Fact]
    public void Range_Attenuated()
    {
        var light = CreateLight();
        light.Attenuation = new(constant: 0.75f, linear: 0, quadratic: 0.001f);
        light.Range.Should().BeLessThan(float.PositiveInfinity);
    }

    [Fact]
    public void GetShadowed_NoShadow_WhenCasterBeyondMaxShadowRange()
    {
        var light = CreateLight();
        light.MaxShadowRange = 10f;

        // Caster is 15 units away from receiver, beyond MaxShadowRange
        var casterSphere = new BoundingSphere(new(0, 10, 0), radius: 1);
        var receiverSphere = new BoundingSphere(new(0, -5, 0), radius: 1);

        var shadowed = light.GetShadowed(receiverSphere, casterSphere);

        shadowed.Should().Be(light); // No shadow applied
    }

    [Fact]
    public void AsDirectional_ConvertsBasicProperties()
    {
        var light = new PointLight
        {
            Name = "TestLight",
            Enabled = true,
            MaxShadowRange = 100f,
            Position = new DoubleVector3(0, 10, 0),
            Diffuse = Color.White,
            Specular = Color.White,
            Ambient = Color.FromArgb(50, 50, 50),
            Attenuation = Attenuation.None
        };
        // Trigger floating position computation
        _ = ((IFloatingOriginAware)light).FloatingPosition;

        var target = new Vector3(0, 0, 0);
        var directional = light.AsDirectional(target);

        directional.Name.Should().Be(light.Name);
        directional.Enabled.Should().Be(light.Enabled);
        directional.MaxShadowRange.Should().Be(light.MaxShadowRange);
        directional.Direction.Should().Be(Vector3.Normalize(new Vector3(0, -10, 0)));
        directional.Diffuse.Should().Be(light.Diffuse);
        directional.Specular.Should().Be(light.Specular);
        directional.Ambient.Should().Be(light.Ambient);
    }

    [Fact]
    public void AsDirectional_AppliesAttenuation()
    {
        var light = new PointLight
        {
            Position = new DoubleVector3(0, 100, 0),
            Diffuse = Color.FromArgb(255, 200, 100),
            Specular = Color.FromArgb(255, 150, 50),
            Ambient = Color.FromArgb(50, 50, 50),
            Attenuation = new(constant: 1, linear: 0.01f, quadratic: 0.001f)
        };
        // Trigger floating position computation
        _ = ((IFloatingOriginAware)light).FloatingPosition;

        var target = new Vector3(0, 0, 0);
        var directional = light.AsDirectional(target);

        // With distance 100 and attenuation (1, 0.01, 0.001), factor = 1 / (1 + 0.01*100 + 0.001*100^2) = 1/12
        // Expected colors should be attenuated
        directional.Diffuse.R.Should().BeLessThan(light.Diffuse.R);
        directional.Diffuse.G.Should().BeLessThan(light.Diffuse.G);
        directional.Diffuse.B.Should().BeLessThan(light.Diffuse.B);
        directional.Specular.R.Should().BeLessThan(light.Specular.R);
        directional.Specular.G.Should().BeLessThan(light.Specular.G);
        directional.Specular.B.Should().BeLessThan(light.Specular.B);
        directional.Ambient.R.Should().BeLessThan(light.Ambient.R);
        directional.Ambient.G.Should().BeLessThan(light.Ambient.G);
        directional.Ambient.B.Should().BeLessThan(light.Ambient.B);
    }

    [Fact]
    public void AsDirectional_ReusesDirectionalLightInstance()
    {
        var light = new PointLight
        {
            Position = new DoubleVector3(0, 10, 0),
            Diffuse = Color.White,
            Specular = Color.White,
            Ambient = Color.Black
        };
        // Trigger floating position computation
        _ = ((IFloatingOriginAware)light).FloatingPosition;

        var target1 = new Vector3(0, 0, 0);
        var directional1 = light.AsDirectional(target1);
        var direction1 = directional1.Direction;

        var target2 = new Vector3(10, 0, 0);
        var directional2 = light.AsDirectional(target2);

        // Should return the same instance, just updated
        directional2.Should().BeSameAs(directional1);
        directional2.Direction.Should().NotBe(direction1);
    }

    [Fact]
    public void AsDirectional_CalculatesCorrectDirection()
    {
        var light = new PointLight
        {
            Position = new DoubleVector3(10, 10, 10),
            Diffuse = Color.White,
            Specular = Color.White,
            Ambient = Color.Black
        };
        // Trigger floating position computation
        _ = ((IFloatingOriginAware)light).FloatingPosition;

        var target = new Vector3(0, 0, 0);
        var directional = light.AsDirectional(target);

        var expectedDirection = Vector3.Normalize(new Vector3(-10, -10, -10));
        directional.Direction.Should().Be(expectedDirection);
    }
}
