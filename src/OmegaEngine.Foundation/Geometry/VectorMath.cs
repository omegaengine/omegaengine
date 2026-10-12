/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using JetBrains.Annotations;
using System.Numerics;
using Vortice.Mathematics;

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

    extension(ByteVector4 vector)
    {
        /// <summary>
        /// Maps a vector of 0-255 byte values to a vector of 0�-180� angles in radians.
        /// </summary>
        [Pure]
        public Vector4 ByteToAngle() => new(
            (float)vector.X.ByteToAngle(),
            (float)vector.Y.ByteToAngle(),
            (float)vector.Z.ByteToAngle(),
            (float)vector.W.ByteToAngle());
    }

    extension(Vector2 value)
    {
        /// <summary>
        /// Rotates a <see cref="Vector2"/> by <paramref name="rotation"/> around the origin.
        /// </summary>
        /// <param name="rotation">The angle to rotate by in radians.</param>
        /// <returns>The rotated <see cref="Vector2"/>.</returns>
        [Pure]
        public Vector2 Rotate(float rotation)
            => new(
                (float)(value.X * Math.Cos(rotation) - value.Y * Math.Sin(rotation)),
                (float)(value.X * Math.Sin(rotation) + value.Y * Math.Cos(rotation)));
    }

    /// <summary>
    /// How short the cross product of two unit vectors has to get before it no longer determines a plane between them.
    /// </summary>
    private const double DegenerateCross = 1e-9;

    extension(DoubleVector3 vector)
    {
        /// <summary>
        /// Gets the minimal rotation required to rotate one direction vector into another.
        /// </summary>
        /// <param name="to">The target direction vector.</param>
        /// <returns>A tuple containing the unit rotation axis and the rotation angle in radians.</returns>
        [Pure]
        public (DoubleVector3 axis, double rotation) GetRotationTo(DoubleVector3 to)
        {
            vector = vector.Normalize();
            to = to.Normalize();
            var cross = DoubleVector3.Cross(vector, to);
            double sine = cross.Length(), dot = DoubleVector3.Dot(vector, to);

            // Nothing left of the cross product to take an axis from, so any perpendicular will do
            if (sine < DegenerateCross)
                return dot < 0 ? (vector.AnyPerpendicular(), Math.PI) : (default, 0);

            // Accurate all the way to 0 and Pi, unlike Acos() of the dot product alone
            return (cross.Normalize(), Math.Atan2(sine, dot));
        }

        /// <summary>
        /// Rotates a vector from an old to a new reference frame.
        /// </summary>
        /// <param name="from">A vector describing the old reference frame.</param>
        /// <param name="to">A vector describing the new reference frame.</param>
        [Pure]
        public DoubleVector3 AdjustReference(DoubleVector3 from, DoubleVector3 to)
        {
            (var axis, double rotation) = from.GetRotationTo(to);
            return vector.RotateAroundAxis(axis, rotation);
        }

        /// <summary>
        /// Returns a normalized vector perpendicular to <paramref name="vector"/>.
        /// </summary>
        public DoubleVector3 AnyPerpendicular()
        {
            const double alignmentThreshold = 0.9;
            var vn = vector.Normalize();

            // Choose an axis that is not nearly parallel to vn
            var axis = Math.Abs(vn.Y) < alignmentThreshold
                ? DoubleVector3.UnitY
                : Math.Abs(vn.X) < alignmentThreshold
                    ? DoubleVector3.UnitX
                    : DoubleVector3.UnitZ;

            return DoubleVector3.Cross(vn, axis).Normalize();
        }

        /// <summary>
        /// Rotates a <see cref="DoubleVector3"/> around an arbitrary axis.
        /// </summary>
        /// <param name="axis">The axis to rotate around.</param>
        /// <param name="rotation">The angle to rotate by in radians.</param>
        /// <returns>The rotated <see cref="DoubleVector3"/>.</returns>
        [Pure]
        public DoubleVector3 RotateAroundAxis(DoubleVector3 axis, double rotation)
            => Math.Cos(rotation) * vector +
               Math.Sin(rotation) * DoubleVector3.Cross(axis, vector) +
               (1 - Math.Cos(rotation)) * DoubleVector3.Dot(axis, vector) * axis;
    }

    extension(Vector2Ray ray)
    {
        /// <summary>
        /// Computes the perpendicular distance of this ray to a point.
        /// </summary>
        public float PerpendicularDistance(Vector2 point)
        {
            var toPoint = point - ray.Position;
            float projection = Vector2.Dot(toPoint, ray.Direction);
            var projectedPoint = ray.Position + ray.Direction * projection;
            return (point - projectedPoint).Length();
        }
    }

    extension(Ray ray)
    {
        /// <summary>
        /// Computes the perpendicular distance of this ray to a point.
        /// </summary>
        public float PerpendicularDistance(Vector3 point)
        {
            var toPoint = point - ray.Position;
            float projection = Vector3.Dot(toPoint, ray.Direction);
            var projectedPoint = ray.Position + ray.Direction * projection;
            return (point - projectedPoint).Length();
        }
    }

    /// <summary>
    /// Transforms a coordinate by a matrix, including the division by the resulting W component.
    /// </summary>
    /// <remarks>Unlike <see cref="Vector3.Transform(Vector3,Matrix4x4)"/>, this also handles projection matrices.</remarks>
    [Pure]
    public static Vector3 TransformCoordinate(Vector3 coordinate, Matrix4x4 transform)
    {
        var result = Vector4.Transform(coordinate, transform);
        return new Vector3(result.X, result.Y, result.Z) / result.W;
    }

    /// <summary>
    /// Calculates the inverse of a matrix.
    /// </summary>
    /// <returns>The inverted matrix; all <see cref="float.NaN"/> if <paramref name="matrix"/> cannot be inverted.</returns>
    [Pure]
    public static Matrix4x4 Invert(Matrix4x4 matrix)
    {
        Matrix4x4.Invert(matrix, out var result);
        return result;
    }

    /// <summary>
    /// Scales a vector to a length of 1, leaving zero vectors unchanged instead of turning them into <see cref="float.NaN"/> like <see cref="Vector2.Normalize"/>.
    /// </summary>
    [Pure]
    public static Vector2 NormalizeOrZero(Vector2 vector)
    {
        float length = vector.Length();
        return length == 0 ? vector : vector / length;
    }

    /// <summary>
    /// Scales a vector to a length of 1, leaving zero vectors unchanged instead of turning them into <see cref="float.NaN"/> like <see cref="Vector3.Normalize"/>.
    /// </summary>
    [Pure]
    public static Vector3 NormalizeOrZero(Vector3 vector)
    {
        float length = vector.Length();
        return length == 0 ? vector : vector / length;
    }

    extension(Quaternion quaternion)
    {
        /// <summary>
        /// Gets the axis of the rotation described by a unit quaternion. Not normalized.
        /// </summary>
        [Pure]
        public Vector3 GetAxis() => new(quaternion.X, quaternion.Y, quaternion.Z);

        /// <summary>
        /// Gets the angle of the rotation described by a unit quaternion in radians.
        /// </summary>
        [Pure]
        public float GetAngle() => 2 * (float)Math.Acos(quaternion.W);
    }
}
