/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using FluentAssertions;
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

    [Fact]
    public void GetShadowed_NoShadow_WhenCasterBeyondMaxShadowRange()
    {
        var light = CreateLight();
        light.MaxShadowRange = 5f;

        // Caster is 10 units away from receiver, beyond MaxShadowRange
        var casterSphere = new BoundingSphere(new(0, 10, 0), radius: 1);
        var receiverSphere = new BoundingSphere(new(0, 0, 0), radius: 1);

        var shadowed = light.GetShadowed(receiverSphere, casterSphere);

        shadowed.Should().Be(light); // No shadow applied
    }
}
