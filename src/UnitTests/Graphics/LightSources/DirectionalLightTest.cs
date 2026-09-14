/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using AwesomeAssertions;
using SlimDX;
using Xunit;

namespace OmegaEngine.Graphics.LightSources;

public class DirectionalLightTest
{
    private static DirectionalLight CreateLight() => new()
    {
        Direction = new(0, -1, 0),
        Diffuse = Color.White,
        Specular = Color.White,
        Ambient = Color.FromArgb(50, 50, 50)
    };

    [Fact]
    public void GetShadowed_NoShadow_WhenReceiverNotInShadowDirection()
    {
        var light = CreateLight();

        // Receiver is in front of caster (not in shadow direction)
        var casterSphere = new BoundingSphere(new(0, 0, 0), radius: 1);
        var receiverSphere = new BoundingSphere(new(0, 5, 0), radius: 1); // Above the caster, light points down

        var shadowed = light.GetShadowed(receiverSphere, casterSphere);

        shadowed.Should().Be(light);
    }

    [Fact]
    public void GetShadowed_NoShadow_WhenReceiverOutsideShadowCylinder()
    {
        var light = CreateLight();

        // Receiver is behind caster but outside the shadow cylinder
        var casterSphere = new BoundingSphere(new(0, 10, 0), radius: 1);
        var receiverSphere = new BoundingSphere(new(5, 0, 0), radius: 1); // Far to the side

        var shadowed = light.GetShadowed(receiverSphere, casterSphere);

        shadowed.Should().Be(light);
    }

    [Fact]
    public void GetShadowed_FullShadow_WhenReceiverFullyInsideShadowCylinder()
    {
        var light = CreateLight();

        // Receiver is directly behind caster and fully contained in shadow cylinder
        var casterSphere = new BoundingSphere(new(0, 10, 0), radius: 2);
        var receiverSphere = new BoundingSphere(new(0, 0, 0), radius: 1); // Directly below, smaller

        var shadowed = (DirectionalLight)light.GetShadowed(receiverSphere, casterSphere);

        shadowed.Should().NotBe(light); // Shadow applied
        shadowed.Diffuse.Should().Be(Color.Black); // Fully shadowed
        shadowed.Specular.Should().Be(Color.Black); // Fully shadowed
        shadowed.Ambient.Should().Be(light.Ambient); // Ambient unchanged
    }

    [Fact]
    public void GetShadowed_PartialShadow_WhenReceiverPartiallyInsideShadowCylinder()
    {
        var light = CreateLight();

        // Receiver is partially in shadow cylinder
        var casterSphere = new BoundingSphere(new(0, 10, 0), radius: 2);
        var receiverSphere = new BoundingSphere(new(2.5f, 0, 0), radius: 1); // Partially overlapping

        var shadowed = (DirectionalLight)light.GetShadowed(receiverSphere, casterSphere);

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
        var lightWithName = new DirectionalLight
        {
            Name = "TestLight",
            Enabled = true,
            Direction = new(1, -1, 0),
            Diffuse = Color.Red,
            Specular = Color.Blue,
            Ambient = Color.Green
        };

        var casterSphere = new BoundingSphere(new(0, 10, 0), radius: 2);
        var receiverSphere = new BoundingSphere(new(1, 0, 0), radius: 1);

        var shadowed = (DirectionalLight)lightWithName.GetShadowed(receiverSphere, casterSphere);

        shadowed.Name.Should().Be(lightWithName.Name);
        shadowed.Enabled.Should().Be(lightWithName.Enabled);
        shadowed.Direction.Should().Be(Vector3.Normalize(new(1, -1, 0)));
        shadowed.Ambient.Should().Be(lightWithName.Ambient); // Ambient should not change
    }

    [Fact]
    public void GetShadowed_CylindricalShadowVolume()
    {
        var light = CreateLight();

        // Test that directional light creates cylindrical shadow (constant radius)
        var casterSphere = new BoundingSphere(new(0, 10, 0), radius: 2);

        // Receiver close to caster
        var receiverNear = new BoundingSphere(new(0, 8, 0), radius: 1);
        var shadowedNear = (DirectionalLight)light.GetShadowed(receiverNear, casterSphere);

        // Receiver far from caster
        var receiverFar = new BoundingSphere(new(0, -10, 0), radius: 1);
        var shadowedFar = (DirectionalLight)light.GetShadowed(receiverFar, casterSphere);

        // Both should be fully shadowed since they're in the same cylindrical shadow
        shadowedNear.Diffuse.Should().Be(Color.Black);
        shadowedFar.Diffuse.Should().Be(Color.Black);
    }

    /// <summary>
    /// A light source whose umbra cone converges to a tip 20 units behind a caster with a radius of 2.
    /// </summary>
    private static DirectionalLight CreateTaperedLight()
    {
        var light = CreateLight();
        light.SourceRadius = 4;
        light.SourceDistance = 20;
        return light;
    }

    [Fact]
    public void GetShadowed_ConicalShadowVolume_ConvergesWithDistance()
    {
        var light = CreateTaperedLight();

        var casterSphere = new BoundingSphere(new(0, 10, 0), radius: 2);

        // Receiver 10 units behind the caster, halfway to the umbra tip: umbra radius = 2 + (10/20) * (2 - 4) = 1
        var receiverNear = new BoundingSphere(new(0, 0, 0), radius: 0.5f);
        var shadowedNear = (DirectionalLight)light.GetShadowed(receiverNear, casterSphere);

        // Receiver 30 units behind the caster, past the umbra tip: umbra radius = 2 + (30/20) * (2 - 4) = -1
        var receiverFar = new BoundingSphere(new(0, -20, 0), radius: 0.5f);
        var shadowedFar = (DirectionalLight)light.GetShadowed(receiverFar, casterSphere);

        shadowedNear.Diffuse.Should().Be(Color.Black); // Fully inside the umbra

        // Only the penumbra reaches past the tip
        shadowedFar.Diffuse.R.Should().BeLessThan(light.Diffuse.R);
        shadowedFar.Diffuse.R.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetShadowed_NoShadow_WhenBeyondUmbraTipAndOutsidePenumbra()
    {
        var light = CreateTaperedLight();

        // Receiver is past the umbra tip and beyond the penumbra radius of 2 + (30/20) * (2 + 4) = 11
        var casterSphere = new BoundingSphere(new(0, 10, 0), radius: 2);
        var receiverSphere = new BoundingSphere(new(12, -20, 0), radius: 0.5f);

        var shadowed = light.GetShadowed(receiverSphere, casterSphere);

        shadowed.Should().Be(light);
    }

    [Fact]
    public void GetShadowed_SourceRadiusIgnored_WhenSourceDistanceInfinite()
    {
        var light = CreateLight();
        light.SourceRadius = 4; // No effect without a finite SourceDistance

        var casterSphere = new BoundingSphere(new(0, 10, 0), radius: 2);
        var receiverSphere = new BoundingSphere(new(0, 0, 0), radius: 1);

        var shadowed = (DirectionalLight)light.GetShadowed(receiverSphere, casterSphere);

        shadowed.Diffuse.Should().Be(Color.Black); // Still a cylinder, so still fully shadowed
    }

    [Fact]
    public void GetShadowed_PreservesSourceProperties()
    {
        var light = CreateTaperedLight();

        var casterSphere = new BoundingSphere(new(0, 10, 0), radius: 2);
        var receiverSphere = new BoundingSphere(new(0, 0, 0), radius: 0.5f);

        var shadowed = (DirectionalLight)light.GetShadowed(receiverSphere, casterSphere);

        shadowed.Should().NotBe(light); // Shadow applied, so the assertions below are not vacuous
        shadowed.SourceRadius.Should().Be(light.SourceRadius);
        shadowed.SourceDistance.Should().Be(light.SourceDistance);
    }

    [Fact]
    public void GetShadowed_NoShadow_WhenCasterBeyondMaxShadowRange()
    {
        var light = CreateLight();
        light.MaxShadowRange = 5;

        // Caster is 10 units away from receiver, beyond MaxShadowRange
        var casterSphere = new BoundingSphere(new(0, 10, 0), radius: 1);
        var receiverSphere = new BoundingSphere(new(0, 0, 0), radius: 1);

        var shadowed = light.GetShadowed(receiverSphere, casterSphere);

        shadowed.Should().Be(light); // No shadow applied
    }
}
