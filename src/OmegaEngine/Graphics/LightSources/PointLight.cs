/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Diagnostics.Contracts;
using NanoByte.Common;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Foundation.Light;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using SlimDX;

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
    /// The position of the light source
    /// </summary>
    [Description("The position of the light source"), Category("Layout")]
    public DoubleVector3 Position { get => _position; set => value.To(ref _position, ref _floatingPositionDirty); }

    private DoubleVector3 _floatingOrigin;

    /// <summary>
    /// A value to be added to <see cref="Position"/> in order gain <see cref="IFloatingOriginAware.FloatingPosition"/> - auto-updated by <see cref="View.Render"/> to the negative <see cref="Camera.Position"/>
    /// </summary>
    DoubleVector3 IFloatingOriginAware.FloatingOrigin { get => _floatingOrigin; set => value.To(ref _floatingOrigin, ref _floatingPositionDirty); }

    private bool _floatingPositionDirty;
    private Vector3 _floatingPosition;

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
                _floatingPosition = this.ApplyFloatingOriginTo(_position);
                _floatingPositionDirty = false;
            }
            return _floatingPosition;
        }
    }

    /// <summary>
    /// Stores an offset used by game logic positioning code. Ignore by the engine itself!
    /// </summary>
    [Description("Stores an offset used by game logic positioning code. Ignore by the engine itself!"), Category("Layout")]
    public Vector3 Shift { get; set; }

    /// <summary>
    /// Factors describing the attenuation of light intensity over distance.
    /// </summary>
    [Description("Factors describing the attenuation of light intensity over distance. (1,0,0) for no attenuation."), Category("Behavior")]
    public Attenuation Attenuation { get; set; } = Attenuation.None;

    /// <summary>
    /// The maximum distance at which the light source has an effect.
    /// </summary>
    [Description("The maximum distance at which the light source has an effect."), Category("Behavior")]
    public float Range => Attenuation.Range(minIntensity: 0.02f);

    private DirectionalLight? _directional;

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

        var delta = target - _floatingPosition;
        _directional.Direction = Vector3.Normalize(delta);

        float attenuation = Attenuation.Apply(delta.Length());
        _directional.Diffuse = Diffuse.Multiply(attenuation);
        _directional.Specular = Specular.Multiply(attenuation);
        _directional.Ambient = Ambient.Multiply(attenuation);

        _directional.MaxShadowRange = MaxShadowRange;

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

        var lightDirection = lightToCaster / lightToCasterDistance;
        var casterToReceiver = receiverSphere.Center - casterSphere.Center;
        float projectionDistance = Vector3.Dot(casterToReceiver, lightDirection);

        if (projectionDistance <= 0)
            return this; // Receiver is not behind the caster

        float lightToReceiverDistance = lightToCasterDistance + projectionDistance;
        float shadowRadius = casterSphere.Radius * (lightToReceiverDistance / lightToCasterDistance);

        var shadowRay = new Ray(casterSphere.Center, lightDirection);
        float shadowFactor = GetShadowFactor(receiverSphere, shadowRay, shadowRadius);

        if (shadowFactor == 0) return this;
        var lightSource = new PointLight
        {
            Name = Name,
            Enabled = Enabled,
            Diffuse = Diffuse.Multiply(1 - shadowFactor),
            Specular = Specular.Multiply(1 - shadowFactor),
            Ambient = Ambient,
            Position = Position,
            RenderAsDirectional = RenderAsDirectional,
            Attenuation = Attenuation,
            MaxShadowRange = MaxShadowRange
        };
        lightSource.SetFloatingOrigin(this.GetFloatingOrigin());
        return lightSource;
    }
}
