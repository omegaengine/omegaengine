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
using OmegaEngine.Foundation.Design;
using SlimDX;

namespace OmegaEngine.Foundation.Geometry;

/// <summary>
/// Defines a plane in three dimensions with <see cref="double"/> distance accuracy.
/// </summary>
/// <param name="point">A point that lies along the plane.</param>
/// <param name="normal">The normal vector of the plane.</param>
[TypeConverter(typeof(DoublePlaneConverter))]
public struct DoublePlane(DoubleVector3 point, Vector3 normal) : IEquatable<DoublePlane>
{
    /// <summary>
    /// A point that lies along the plane.
    /// </summary>
    [Description("A point that lies along the plane.")]
    public DoubleVector3 Point { get; } = point;

    private Vector3 _normal = normal;

    /// <summary>
    /// The normal vector of the plane.
    /// </summary>
    [Description("The normal vector of the plane.")]
    public Vector3 Normal { get => _normal; set => _normal = Vector3.Normalize(value); }

    /// <summary>
    /// Returns a single-precision standard <see cref="Plane"/> after subtracting an offset value.
    /// </summary>
    /// <param name="offset">This value is subtracted from the double-precision data before it is casted to single-precision.</param>
    /// <returns>The newly positioned <see cref="Plane"/>.</returns>
    [Pure]
    public Plane ApplyOffset(DoubleVector3 offset) => new(Point.ApplyOffset(offset), _normal);

    /// <inheritdoc/>
    public override string ToString() => $"({Point} => {Normal})";

    /// <inheritdoc/>
    public bool Equals(DoublePlane other) => other.Point == Point && other.Normal == Normal;

    public static bool operator ==(DoublePlane left, DoublePlane right) => left.Equals(right);
    public static bool operator !=(DoublePlane left, DoublePlane right) => !left.Equals(right);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is DoublePlane plane && Equals(plane);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Point, Normal);
}
