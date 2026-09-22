/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Diagnostics.Contracts;
using OmegaEngine.Foundation.Light;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.LightSources;

/// <summary>
/// A light source that has no position and shines in one direction.
/// </summary>
public sealed class DirectionalLight : LightSource
{
    private Vector3 _direction;

    /// <summary>
    /// The direction light emitted by this light source travels in
    /// </summary>
    [Description("The direction light emitted by this light source travels in"), Category("Layout")]
    public Vector3 Direction { get => _direction; set => _direction = Vector3.Normalize(value); }

    /// <summary>
    /// The distance from the emitting body to the objects being lit.
    /// </summary>
    /// <remarks>Used together with <see cref="LightSource.SourceRadius"/> to taper shadow volumes into umbra cones. <see cref="float.PositiveInfinity"/> for an idealized parallel source, whose shadow volumes never converge.</remarks>
    [Description("The distance from the emitting body to the objects being lit"), Category("Layout")]
    public float SourceDistance { get; set; } = float.PositiveInfinity;

    /// <inheritdoc/>
    [Pure]
    public override LightSource GetShadowed(BoundingSphere receiverSphere, BoundingSphere casterSphere)
    {
        var casterToReceiver = receiverSphere.Center - casterSphere.Center;
        if (casterToReceiver.Length() > MaxShadowRange)
            return this; // Shadow caster is too far away

        float projectionDistance = Vector3.Dot(casterToReceiver, Direction);
        if (projectionDistance <= 0)
            return this; // Receiver is not in shadow direction

        var shadowRay = new Ray(casterSphere.Center, Direction);
        float shadowFactor = GetShadowFactor(receiverSphere, shadowRay, casterSphere.Radius, SourceDistance, projectionDistance);

        if (shadowFactor == 0) return this;
        return new DirectionalLight
        {
            Name = Name,
            Enabled = Enabled,
            MaxShadowRange = MaxShadowRange,
            SourceRadius = SourceRadius,
            SourceDistance = SourceDistance,
            Diffuse = Diffuse.MultiplyLinear(1 - shadowFactor),
            Specular = Specular.MultiplyLinear(1 - shadowFactor),
            Ambient = Ambient,
            Direction = Direction
        };
    }

    /// <inheritdoc/>
    internal override Light ToFfpLight() => new()
    {
        Type = LightType.Directional,
        Direction = Direction,
        Diffuse = Diffuse.SrgbToLinear(),
        Specular = Specular.SrgbToLinear(),
        Ambient = Ambient.SrgbToLinear()
    };
}
