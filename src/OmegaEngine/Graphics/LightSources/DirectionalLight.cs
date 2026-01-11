/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Diagnostics.Contracts;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Foundation.Light;
using SlimDX;

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

    /// <inheritdoc/>
    [Pure]
    public override LightSource GetShadowed(BoundingSphere receiverSphere, BoundingSphere casterSphere)
    {
        if (Vector3.Dot(receiverSphere.Center - casterSphere.Center, Direction) <= 0)
            return this; // Receiver is not in shadow direction

        var shadowRay = new Vector3Ray(casterSphere.Center, Direction);
        float shadowFactor = GetShadowFactor(receiverSphere, shadowRay, casterSphere.Radius);

        if (shadowFactor == 0) return this;
        return new DirectionalLight
        {
            Name = Name,
            Enabled = Enabled,
            Diffuse = Diffuse.Multiply(1 - shadowFactor),
            Specular = Specular.Multiply(1 - shadowFactor),
            Ambient = Ambient,
            Direction = Direction
        };
    }
}
