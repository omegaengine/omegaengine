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
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.LightSources;

/// <summary>
/// A light source that has a fixed position and shines uniformly in all directions.
/// </summary>
public sealed class PointLight : LightSource, IFloatingOriginAware
{
    /// <summary>
    /// Shall this light source be converted to a pseudo-directional source for each individual <see cref="PositionableRenderable"/> before rendering?
    /// </summary>
    [Description("Shall this light source be converted to a pseudo-directional source for each individual PositionableRenderable before rendering?"), Category("Behavior")]
    public bool RenderAsDirectional { get; set; }

    private DoubleVector3 _position;

    /// <summary>
    /// The position of the light source in world space.
    /// </summary>
    /// <remarks>While <see cref="AttachedTo"/> is set, this is recalculated from <see cref="Offset"/> by <see cref="UpdatePosition"/> and setting it has no lasting effect.</remarks>
    [Description("The position of the light source"), Category("Layout")]
    public DoubleVector3 Position { get => _position; set => value.To(ref _position, ref _floatingPositionDirty); }

    /// <summary>
    /// A <see cref="PositionableRenderable"/> this light source is attached to; <c>null</c> to use <see cref="Position"/> as an absolute world position.
    /// </summary>
    /// <seealso cref="Offset"/>
    [Browsable(false)]
    public PositionableRenderable? AttachedTo { get; set; }

    /// <summary>
    /// The offset from <see cref="AttachedTo"/> in its local coordinate system. Ignored while <see cref="AttachedTo"/> is <c>null</c>.
    /// </summary>
    [Description("The offset from the renderable the light source is attached to, in its local coordinate system"), Category("Layout")]
    public Vector3 Offset { get; set; }

    /// <summary>
    /// Recalculates <see cref="Position"/> from <see cref="AttachedTo"/> and <see cref="Offset"/>. Does nothing while not attached.
    /// </summary>
    /// <remarks>Called by <see cref="View.Render"/> for every view with <see cref="View.Lighting"/> enabled, before the view uses its lights.</remarks>
    internal void UpdatePosition()
    {
        if (AttachedTo is {} parent) Position = parent.ToWorld((DoubleVector3)Offset);
    }

    private DoubleVector3 _floatingOrigin;

    /// <summary>
    /// A value to be added to <see cref="Position"/> in order gain <see cref="IFloatingOriginAware.FloatingPosition"/> - auto-updated by <see cref="View.Render"/> to the negative <see cref="Camera.Position"/>
    /// </summary>
    DoubleVector3 IFloatingOriginAware.FloatingOrigin { get => _floatingOrigin; set => value.To(ref _floatingOrigin, ref _floatingPositionDirty); }

    private bool _floatingPositionDirty;
    private Vector3 _floatingPositionCached;

    /// <summary>
    /// The body's position in render space, based on <see cref="Position"/>
    /// </summary>
    /// <remarks>Constantly changes based on the values set for <see cref="IFloatingOriginAware.FloatingOrigin"/></remarks>
    Vector3 IFloatingOriginAware.FloatingPosition
    {
        get
        {
            if (_floatingPositionDirty)
            {
                _floatingPositionCached = this.ApplyFloatingOriginTo(_position);
                _floatingPositionDirty = false;
            }
            return _floatingPositionCached;
        }
    }

    /// <summary>
    /// Factors describing the attenuation of light intensity over distance.
    /// </summary>
    [Description("Factors describing the attenuation of light intensity over distance."), Category("Behavior")]
    public Attenuation Attenuation { get; set; } = Attenuation.None;

    /// <summary>
    /// The maximum distance at which the light source has an effect.
    /// </summary>
    [Description("The maximum distance at which the light source has an effect."), Category("Behavior")]
    public float Range => Attenuation.Range(minIntensity: 0.02f);

    private DirectionalLight? _directional;

    /// <summary>
    /// Determines whether the light source is in range of the given bounding sphere.
    /// </summary>
    /// <param name="boundingSphere">A bounding sphere in floating world space.</param>
    internal bool IsInRange(BoundingSphere boundingSphere)
    {
        float distance = (this.GetFloatingPosition() - boundingSphere.Center).Length();
        return distance <= Range + boundingSphere.Radius;
    }

    /// <summary>
    /// Converts the point light source to a directional light source.
    /// </summary>
    /// <param name="target">The floating target location being lit.</param>
    /// <returns>A re-used light source. Updates and returns the same instance on subsequent calls.</returns>
    internal DirectionalLight AsDirectional(Vector3 target)
    {
        _directional ??= new();

        _directional.Name = Name;
        _directional.Enabled = Enabled;
        _directional.MaxShadowRange = MaxShadowRange;

        var delta = target - this.GetFloatingPosition();
        float distance = delta.Length();
        _directional.Direction = Vector3.Normalize(delta);

        _directional.SourceRadius = SourceRadius;
        // Approximates the distance to the shadow casters with the distance to the lit target; negligible as long as the casters are close to the receivers relative to the light source's distance
        _directional.SourceDistance = distance;

        float attenuation = Attenuation.Apply(distance);
        _directional.Diffuse = Diffuse.MultiplyLinear(attenuation);
        _directional.Specular = Specular.MultiplyLinear(attenuation);
        _directional.Ambient = Ambient.MultiplyLinear(attenuation);

        return _directional;
    }

    /// <inheritdoc/>
    [Pure]
    public override LightSource GetShadowed(BoundingSphere receiverSphere, BoundingSphere casterSphere)
    {
        var lightPos = this.GetFloatingPosition();
        var lightToCaster = casterSphere.Center - lightPos;
        float lightToCasterDistance = lightToCaster.Length();

        if (lightToCasterDistance < 0.0001)
            return this; // Light at same position as caster

        var casterToReceiver = receiverSphere.Center - casterSphere.Center;
        if (casterToReceiver.Length() > MaxShadowRange)
            return this; // Shadow caster is too far away

        var lightDirection = lightToCaster / lightToCasterDistance;
        float projectionDistance = Vector3.Dot(casterToReceiver, lightDirection);
        if (projectionDistance <= 0)
            return this; // Receiver is not behind the caster

        var shadowRay = new Ray(casterSphere.Center, lightDirection);
        float shadowFactor = GetShadowFactor(receiverSphere, shadowRay, casterSphere.Radius, lightToCasterDistance, projectionDistance);

        if (shadowFactor == 0) return this;
        var lightSource = new PointLight
        {
            Name = Name,
            Enabled = Enabled,
            MaxShadowRange = MaxShadowRange,
            SourceRadius = SourceRadius,
            Diffuse = Diffuse.MultiplyLinear(1 - shadowFactor),
            Specular = Specular.MultiplyLinear(1 - shadowFactor),
            Ambient = Ambient,
            Position = Position,
            RenderAsDirectional = RenderAsDirectional,
            Attenuation = Attenuation
        };
        lightSource.SetFloatingOrigin(this.GetFloatingOrigin());
        return lightSource;
    }

    /// <inheritdoc/>
    internal override Light ToFfpLight() => new()
    {
        Type = LightType.Point,
        Position = this.GetFloatingPosition(),
        Range = Range,
        Attenuation0 = Attenuation.Constant,
        Attenuation1 = Attenuation.Linear,
        Attenuation2 = Attenuation.Quadratic,
        Diffuse = Diffuse.SrgbToLinear(),
        Specular = Specular.SrgbToLinear(),
        Ambient = Ambient.SrgbToLinear()
    };
}
