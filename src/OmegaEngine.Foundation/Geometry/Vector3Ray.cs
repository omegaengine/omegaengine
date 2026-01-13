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
/// Defines a ray in three dimensions, specified by an origin and a direction.
/// </summary>
/// <param name="position">A point along the ray.</param>
/// <param name="direction">A unit vector specifying the direction in which the ray is pointing (automatically normalized).</param>
[TypeConverter(typeof(Vector3RayConverter))]
public struct Vector3Ray(Vector3 position, Vector3 direction) : IEquatable<Vector3Ray>
{
    /// <summary>
    /// A point along the ray.
    /// </summary>
    [Description("A point along the ray.")]
    public Vector3 Position { get; } = position;

    private Vector3 _direction = Vector3.Normalize(direction);

    /// <summary>
    /// A unit vector specifying the direction in which the ray is pointing (automatically normalized).
    /// </summary>
    [Description("A unit vector specifying the direction in which the ray is pointing (automatically normalized).")]
    public Vector3 Direction { readonly get => _direction; set => _direction = Vector3.Normalize(value); }

    /// <inheritdoc/>
    public readonly override string ToString() => string.Format(CultureInfo.InvariantCulture, "({0} => {1})", Position, Direction);

    /// <inheritdoc/>
    public bool Equals(Vector3Ray other) => other.Direction == Direction && other.Position == Position;

    public static bool operator ==(Vector3Ray left, Vector3Ray right) => left.Equals(right);
    public static bool operator !=(Vector3Ray left, Vector3Ray right) => !left.Equals(right);

    /// <summary>
    /// Computes the perpendicular distance of this ray to a point.
    /// </summary>
    public float PerpendicularDistance(Vector3 point)
    {
        var toPoint = point - Position;
        float projection = Vector3.Dot(toPoint, Direction);
        var projectedPoint = Position + Direction * projection;
        return (point - projectedPoint).Length();
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Vector3Ray ray && Equals(ray);
    }

    /// <inheritdoc/>
    public readonly override int GetHashCode() => HashCode.Combine(Direction, Position);
}
