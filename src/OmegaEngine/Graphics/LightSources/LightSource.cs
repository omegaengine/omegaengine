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
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Foundation.Light;
using OmegaEngine.Graphics.Renderables;
using SlimDX;
using SlimDX.Direct3D9;

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

    /// <summary>
    /// The radius of the body emitting the light.
    /// </summary>
    /// <remarks>Used to taper shadow volumes into umbra cones surrounded by penumbras. 0 for an idealized point source, whose shadow volumes never converge.</remarks>
    [Description("The radius of the body emitting the light"), Category("Behavior")]
    public float SourceRadius { get; set; }

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
    /// Calculates the shadow intensity, tapering the shadow volume into an umbra cone surrounded by a penumbra.
    /// </summary>
    /// <param name="receiverSphere">Bounding sphere of the shadow receiver in floating world space.</param>
    /// <param name="shadowRay">Ray pointing from the light source to the shadow caster in floating world space.</param>
    /// <param name="casterRadius">The radius of the shadow caster.</param>
    /// <param name="sourceDistance">The distance from the light source to the shadow caster; <see cref="float.PositiveInfinity"/> for an idealized parallel source.</param>
    /// <param name="projectionDistance">The distance from the shadow caster to the shadow receiver, projected onto the light direction.</param>
    /// <returns>A value from 0 (fully lit) to 1 (fully shadowed).</returns>
    /// <remarks>The penumbra is applied as a linear blend rather than an occlusion area computation, so it darkens uniformly instead of fading towards its outer edge.</remarks>
    protected float GetShadowFactor(BoundingSphere receiverSphere, Ray shadowRay, float casterRadius, float sourceDistance, float projectionDistance)
    {
        // A negative radius would make the penumbra narrower than the umbra, breaking the early-out below
        float sourceRadius = Math.Max(0, SourceRadius);

        // How far the shadow volume widens or narrows per unit of distance behind the caster; 0 for a parallel source
        float taper = sourceDistance > 0 ? projectionDistance / sourceDistance : 0;

        // The penumbra always widens, so missing it means missing the umbra too
        float penumbraFactor = GetShadowFactor(receiverSphere, shadowRay, casterRadius + taper * (casterRadius + sourceRadius));
        if (penumbraFactor == 0) return 0;

        // The umbra narrows when the source is larger than the caster and vanishes past the cone's tip
        float umbraRadius = casterRadius + taper * (casterRadius - sourceRadius);
        float umbraFactor = umbraRadius > 0 ? GetShadowFactor(receiverSphere, shadowRay, umbraRadius) : 0;

        return (umbraFactor + penumbraFactor) / 2;
    }

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

    /// <summary>
    /// Converts the light source to a fixed-function pipeline light source.
    /// </summary>
    internal abstract Light ToFfpLight();
}
