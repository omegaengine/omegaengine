/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NanoByte.Common;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Foundation.Light;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using SlimDX;
using System.ComponentModel;

namespace OmegaEngine.Graphics;

/// <summary>
/// A light source that has a fixed position and shines uniformly in all directions.
/// </summary>
public sealed class PointLight : LightSource, IPositionableOffset
{
    /// <summary>
    /// Shall this light source be converted to a pseudo-directional source for each individual <see cref="PositionableRenderable"/> before passing it to shaders?
    /// </summary>
    [Description("Shall this light source be converted to a pseudo-directional source for each individual PositionableRenderable before passing it to shaders?"), Category("Behavior")]
    public bool DirectionalForShader { get; set; }

    private DoubleVector3 _position;

    /// <summary>
    /// The position of the light source
    /// </summary>
    [Description("The position of the light source"), Category("Layout")]
    public DoubleVector3 Position { get => _position; set => value.To(ref _position, ref _effectivePositionDirty); }

    private DoubleVector3 _positionOffset;

    /// <summary>
    /// A value to be added to <see cref="Position"/> in order gain <see cref="IPositionableOffset.EffectivePosition"/> - auto-updated by <see cref="View.Render"/> to the negative <see cref="Camera.Position"/>
    /// </summary>
    DoubleVector3 IPositionableOffset.Offset { get => _positionOffset; set => value.To(ref _positionOffset, ref _effectivePositionDirty); }

    private bool _effectivePositionDirty;
    private Vector3 _effectivePosition;

    /// <summary>
    /// The body's position in render space, based on <see cref="Position"/>
    /// </summary>
    /// <remarks>Constantly changes based on the values set for <see cref="IPositionableOffset.Offset"/></remarks>
    Vector3 IPositionableOffset.EffectivePosition
    {
        get
        {
            if (_effectivePositionDirty)
            {
                _effectivePosition = _position.ApplyOffset(((IPositionableOffset)this).Offset);
                _effectivePositionDirty = false;
            }
            return _effectivePosition;
        }
    }

    /// <summary>
    /// Stores an offset used by game logic positioning code. Ignore by the engine itself!
    /// </summary>
    [Description("Stores an offset used by game logic positioning code. Ignore by the engine itself!"), Category("Layout")]
    public Vector3 Shift { get; set; }

    /// <summary>
    /// The maximum distance at which the light source has an effect.
    /// </summary>
    [Description("The maximum distance at which the light source has an effect."), Category("Behavior")]
    public float Range { get; set; } = 1000;

    /// <summary>
    /// Factors describing the attenuation of light intensity over distance.
    /// </summary>
    [Description("Factors describing the attenuation of light intensity over distance. (1,0,0) for no attenuation."), Category("Behavior")]
    public Attenuation Attenuation { get; set; } = Attenuation.None;

    /// <summary>
    /// Checks whether the light source is within effective range of a given <paramref name="position"/> with a specified <paramref name="tolerance"/>.
    /// </summary>
    internal bool IsInRange(DoubleVector3 position, float tolerance)
        => (Position - position).Length() <= Range + tolerance;

    private DirectionalLight? _directional;

    /// <summary>
    /// Converts the point light source to a directional light source.
    /// </summary>
    /// <param name="target">The target location being lit.</param>
    /// <returns>A re-used light source. Updates and returns the same instance on subsequent calls.</returns>
    internal DirectionalLight AsDirectional(DoubleVector3 target)
    {
        _directional ??= new();

        _directional.Name = Name;
        _directional.Enabled = Enabled;

        var delta = target - Position;
        _directional.Direction = (Vector3)delta.Normalize();

        float attenuation = Attenuation.Apply((float)delta.Length());
        _directional.Diffuse = Diffuse.Multiply(attenuation);
        _directional.Specular = Specular.Multiply(attenuation);
        _directional.Ambient = Ambient.Multiply(attenuation);

        return _directional;
    }
}
