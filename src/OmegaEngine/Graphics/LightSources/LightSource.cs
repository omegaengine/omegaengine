/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Drawing;
using NanoByte.Common;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Foundation.Light;
using OmegaEngine.Graphics.Renderables;
using SlimDX;

namespace OmegaEngine.Graphics.LightSources;

/// <summary>
/// A light source that illuminates <see cref="PositionableRenderable"/>s in a <see cref="Scene"/>.
/// </summary>
/// <seealso cref="Scene.Lights"/>
public abstract class LightSource
{
    /// <summary>
    /// Text value to make it easier to identify a particular camera
    /// </summary>
    [Description("Text value to make it easier to identify a particular light source"), Category("Design")]
    public string? Name { get; set; }

    public override string ToString()
    {
        string value = GetType().Name;
        if (!string.IsNullOrEmpty(Name))
            value += $": {Name}";
        return value;
    }

    /// <summary>
    /// Shall the light source affect its surroundings?
    /// </summary>
    [Description("Shall the light source affect its enabled?"), Category("Behavior")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The maximum distance between shadow casters and receivers to consider.
    /// </summary>
    /// <remarks>Lower values can improve performance by excluding distant shadow casters.</remarks>
    [Description("The maximum distance between shadow casters and receivers to consider"), Category("Behavior")]
    public float MaxShadowRange { get; set; } = float.PositiveInfinity;

    private Color _diffuse = Color.White;

    /// <summary>
    /// The diffuse color this light source emits
    /// </summary>
    [Description("The diffuse color this light source emits"), Category("Appearance")]
    public Color Diffuse { get => _diffuse; set => _diffuse = value.DropAlpha(); }

    private Color _specular = Color.Gray;

    /// <summary>
    /// The specular color this light source emits
    /// </summary>
    [Description("The specular color this light source emits"), Category("Appearance")]
    public Color Specular { get => _specular; set => _specular = value.DropAlpha(); }

    private Color _ambient = Color.Black;

    /// <summary>
    /// The ambient color this light source emits
    /// </summary>
    [Description("The ambient color this light source emits"), Category("Appearance")]
    public Color Ambient { get => _ambient; set => _ambient = value.DropAlpha(); }

    /// <summary>
    /// Creates a copy of this light source with simple shadowing applied.
    /// </summary>
    /// <param name="receiverSphere">The bounding sphere of the shadow receiver in world space.</param>
    /// <param name="casterSphere">The bounding sphere of the shadow caster in world space.</param>
    [Pure]
    public abstract LightSource GetShadowed(BoundingSphere receiverSphere, BoundingSphere casterSphere);

    /// <summary>
    /// Calculates the shadow intensity.
    /// </summary>
    /// <param name="receiverSphere">Bounding sphere of the shadow receiver in floating world space.</param>
    /// <param name="shadowRay">Ray pointing from the light source to the shadow caster in floating world space.</param>
    /// <param name="shadowRadius">The radius of the shadow at the shadow receiver.</param>
    /// <returns>A value from 0 (fully lit) to 1 (fully shadowed).</returns>
    protected static float GetShadowFactor(BoundingSphere receiverSphere, Ray shadowRay, float shadowRadius)
    {
        float distance = shadowRay.PerpendicularDistance(receiverSphere.Center);
        float receiverRadius = receiverSphere.Radius;

        // Circles don't overlap
        if (distance >= shadowRadius + receiverRadius) return 0;

        // Shadow receiver is fully contained within shadow
        if (distance + receiverRadius <= shadowRadius) return 1;

        // Shadow is fully contained within shadow receiver
        if (distance + shadowRadius <= receiverRadius) return 0;

        float shadowRadiusSquared = shadowRadius * shadowRadius;
        float receiverRadiusSquared = receiverRadius * receiverRadius;
        float distanceSquared = distance * distance;

        // Avoid division by zero
        if (distanceSquared < 0.0001)
            return shadowRadius >= receiverRadius ? 1 : shadowRadiusSquared / receiverRadiusSquared;

        float angle1 = (float)Math.Acos(Math.Max(-1, Math.Min(1, (distanceSquared + shadowRadiusSquared - receiverRadiusSquared) / (2 * distance * shadowRadius))));
        float angle2 = (float)Math.Acos(Math.Max(-1, Math.Min(1, (distanceSquared + receiverRadiusSquared - shadowRadiusSquared) / (2 * distance * receiverRadius))));
        float intersectionArea = shadowRadiusSquared * angle1 + receiverRadiusSquared * angle2 - 0.5f * (shadowRadiusSquared * (float)Math.Sin(2 * angle1) + receiverRadiusSquared * (float)Math.Sin(2 * angle2));
        float circle2Area = (float)Math.PI * receiverRadiusSquared;

        return (intersectionArea / circle2Area).Clamp();
    }
}
