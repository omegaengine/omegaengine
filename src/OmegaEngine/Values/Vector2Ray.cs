/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Globalization;
using OmegaEngine.Values.Design;
using SlimDX;

namespace OmegaEngine.Values;

/// <summary>
/// Defines a ray in two dimensions, specified by a starting position and a direction.
/// </summary>
[TypeConverter(typeof(Vector2RayConverter))]
public struct Vector2Ray : IEquatable<Vector2Ray>
{
    /// <summary>
    /// Specifies the location of the ray's origin.
    /// </summary>
    [Description("Specifies the location of the ray's origin.")]
    public Vector2 Position { get; }

    private Vector2 _direction;

    /// <summary>
    /// A vector pointing along the ray - automatically normalized when set
    /// </summary>
    [Description("A unit vector specifying the direction in which the ray is pointing.")]
    public Vector2 Direction { get => _direction; set => _direction = Vector2.Normalize(value); }

    /// <summary>
    /// Creates a new ray
    /// </summary>
    /// <param name="point">A point along the ray</param>
    /// <param name="direction">A vector pointing along the ray - automatically normalized when set</param>
    public Vector2Ray(Vector2 point, Vector2 direction)
        : this()
    {
        Position = point;
        Direction = direction;
    }

    #region Conversion
    /// <inheritdoc/>
    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "({0} => {1})", Position, Direction);
    #endregion

    #region Equality
    /// <inheritdoc/>
    public bool Equals(Vector2Ray other) => other.Direction == Direction && other.Position == Position;

    public static bool operator ==(Vector2Ray left, Vector2Ray right) => left.Equals(right);
    public static bool operator !=(Vector2Ray left, Vector2Ray right) => !left.Equals(right);

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Vector2Ray ray && Equals(ray);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            return (Direction.GetHashCode() * 397) ^ Position.GetHashCode();
        }
    }
    #endregion
}
