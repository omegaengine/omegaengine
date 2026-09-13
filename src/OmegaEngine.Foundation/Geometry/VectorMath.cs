/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using JetBrains.Annotations;
using SlimDX;

namespace OmegaEngine.Foundation.Geometry;

/// <summary>
/// Math helpers related to vectors.
/// </summary>
public static class VectorMath
{
    /// <summary>
    /// Calculates a unit vector using spherical coordinates.
    /// </summary>
    /// <param name="inclination">Angle away from positive Z axis in radians. Values from 0 to Pi.</param>
    /// <param name="azimuth">Angle away from positive X axis in radians. Values from 0 to 2*Pi.</param>
    [Pure]
    public static Vector3 UnitVector(double inclination, double azimuth) => new(
        (float)(Math.Sin(inclination) * Math.Cos(azimuth)),
        (float)(Math.Sin(inclination) * Math.Sin(azimuth)),
        (float)Math.Cos(inclination));

    /// <summary>
    /// Rotates <paramref name="from"/> towards <paramref name="to"/> by <paramref name="factor"/> of the angle between them.
    /// </summary>
    public static DoubleVector3 Slerp(DoubleVector3 from, DoubleVector3 to, double factor)
    {
        (var axis, double rotation) = from.GetRotationTo(to);
        return from.RotateAroundAxis(axis, rotation * factor);
    }

    /// <summary>
    /// Maps a 0-255 byte value to a 0�-180� angle in radians.
    /// </summary>
    [Pure]
    public static double ByteToAngle(this byte b) => b / 255.0 * Math.PI;

    /// <summary>
    /// Maps a vector of 0-255 byte values to a vector of 0�-180� angles in radians.
    /// </summary>
    [Pure]
    public static Vector4 ByteToAngle(this ByteVector4 vector) => new(
        (float)vector.X.ByteToAngle(),
        (float)vector.Y.ByteToAngle(),
        (float)vector.Z.ByteToAngle(),
        (float)vector.W.ByteToAngle());

    /// <summary>
    /// Rotates a <see cref="Vector2"/> by <paramref name="rotation"/> around the origin.
    /// </summary>
    /// <param name="value">The original vector.</param>
    /// <param name="rotation">The angle to rotate by in radians.</param>
    /// <returns>The rotated <see cref="Vector2"/>.</returns>
    [Pure]
    public static Vector2 Rotate(this Vector2 value, float rotation)
        => new(
            (float)(value.X * Math.Cos(rotation) - value.Y * Math.Sin(rotation)),
            (float)(value.X * Math.Sin(rotation) + value.Y * Math.Cos(rotation)));

    /// <summary>
    /// Rotates a <see cref="DoubleVector3"/> around an arbitrary axis.
    /// </summary>
    /// <param name="value">The original vector.</param>
    /// <param name="axis">The axis to rotate around.</param>
    /// <param name="rotation">The angle to rotate by in radians.</param>
    /// <returns>The rotated <see cref="DoubleVector3"/>.</returns>
    [Pure]
    public static DoubleVector3 RotateAroundAxis(this DoubleVector3 value, DoubleVector3 axis, double rotation)
        => Math.Cos(rotation) * value +
           Math.Sin(rotation) * axis.CrossProduct(value) +
           (1 - Math.Cos(rotation)) * axis.DotProduct(value) * axis;

    /// <summary>
    /// How short the cross product of two unit vectors has to get before it no longer determines a plane between them.
    /// </summary>
    private const double DegenerateCross = 1e-9;

    /// <summary>
    /// Gets the minimal rotation required to rotate one direction vector into another.
    /// </summary>
    /// <param name="from">The starting direction vector.</param>
    /// <param name="to">The target direction vector.</param>
    /// <returns>A tuple containing the unit rotation axis and the rotation angle in radians.</returns>
    [Pure]
    public static (DoubleVector3 axis, double rotation) GetRotationTo(this DoubleVector3 from, DoubleVector3 to)
    {
        from = from.Normalize();
        to = to.Normalize();
        var cross = from.CrossProduct(to);
        double sine = cross.Length(), dot = from.DotProduct(to);

        // Nothing left of the cross product to take an axis from, so any perpendicular will do
        if (sine < DegenerateCross)
            return dot < 0 ? (from.AnyPerpendicular(), Math.PI) : (default, 0);

        // Accurate all the way to 0 and Pi, unlike Acos() of the dot product alone
        return (cross.Normalize(), Math.Atan2(sine, dot));
    }

    /// <summary>
    /// Rotates a vector from an old to a new reference frame.
    /// </summary>
    /// <param name="vector">The vector to be adjusted.</param>
    /// <param name="from">A vector describing the old reference frame.</param>
    /// <param name="to">A vector describing the new reference frame.</param>
    [Pure]
    public static DoubleVector3 AdjustReference(this DoubleVector3 vector, DoubleVector3 from, DoubleVector3 to)
    {
        (var axis, double rotation) = from.GetRotationTo(to);
        return vector.RotateAroundAxis(axis, rotation);
    }

    /// <summary>
    /// Computes the perpendicular distance of this ray to a point.
    /// </summary>
    public static float PerpendicularDistance(this Vector2Ray ray, Vector2 point)
    {
        var toPoint = point - ray.Position;
        float projection = Vector2.Dot(toPoint, ray.Direction);
        var projectedPoint = ray.Position + ray.Direction * projection;
        return (point - projectedPoint).Length();
    }

    /// <summary>
    /// Computes the perpendicular distance of this ray to a point.
    /// </summary>
    public static float PerpendicularDistance(this Ray ray, Vector3 point)
    {
        var toPoint = point - ray.Position;
        float projection = Vector3.Dot(toPoint, ray.Direction);
        var projectedPoint = ray.Position + ray.Direction * projection;
        return (point - projectedPoint).Length();
    }

    /// <summary>
    /// Returns a normalized vector perpendicular to <paramref name="vector"/>.
    /// </summary>
    public static DoubleVector3 AnyPerpendicular(this DoubleVector3 vector)
    {
        const double alignmentThreshold = 0.9;
        var vn = vector.Normalize();

        // Choose an axis that is not nearly parallel to vn
        var axis = Math.Abs(vn.Y) < alignmentThreshold
            ? DoubleVector3.UnitY
            : Math.Abs(vn.X) < alignmentThreshold
                ? DoubleVector3.UnitX
                : DoubleVector3.UnitZ;

        return vn.CrossProduct(axis).Normalize();
    }
}
