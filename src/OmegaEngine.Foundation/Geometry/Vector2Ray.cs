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
using OmegaEngine.Foundation.Design;
using SlimDX;

namespace OmegaEngine.Foundation.Geometry;

/// <summary>
/// Defines a ray in two dimensions, specified by a starting position and a direction.
/// </summary>
/// <param name="position">A point along the ray.</param>
/// <param name="direction">A unit vector specifying the direction in which the ray is pointing (automatically normalized).</param>
[TypeConverter(typeof(Vector2RayConverter))]
public struct Vector2Ray(Vector2 position, Vector2 direction) : IEquatable<Vector2Ray>
{
    /// <summary>
    /// A point along the ray.
    /// </summary>
    [Description("A point along the ray.")]
    public Vector2 Position { get; } = position;

    private Vector2 _direction = Vector2.Normalize(direction);

    /// <summary>
    /// A unit vector specifying the direction in which the ray is pointing (automatically normalized).
    /// </summary>
    [Description("A unit vector specifying the direction in which the ray is pointing (automatically normalized).")]
    public Vector2 Direction { readonly get => _direction; set => _direction = Vector2.Normalize(value); }

    /// <inheritdoc/>
    public readonly override string ToString() => string.Format(CultureInfo.InvariantCulture, "({0} => {1})", Position, Direction);

    /// <inheritdoc/>
    public bool Equals(Vector2Ray other) => other.Direction == Direction && other.Position == Position;

    public static bool operator ==(Vector2Ray left, Vector2Ray right) => left.Equals(right);
    public static bool operator !=(Vector2Ray left, Vector2Ray right) => !left.Equals(right);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Vector2Ray ray && Equals(ray);
    }

    /// <inheritdoc/>
    public readonly override int GetHashCode() => HashCode.Combine(Direction, Position);
}
