/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using JetBrains.Annotations;
using NanoByte.Common;
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
    /// <param name="rotation">The angle to rotate by in degrees.</param>
    /// <returns>The rotated <see cref="Vector2"/>.</returns>
    [Pure]
    public static Vector2 Rotate(this Vector2 value, float rotation)
    {
        double phi = rotation.DegreeToRadian();
        return new(
            (float)(value.X * Math.Cos(phi) - value.Y * Math.Sin(phi)),
            (float)(value.X * Math.Sin(phi) + value.Y * Math.Cos(phi)));
    }

    /// <summary>
    /// Rotates a <see cref="DoubleVector3"/> around an arbitrary axis.
    /// </summary>
    /// <param name="value">The original vector.</param>
    /// <param name="axis">The axis to rotate around.</param>
    /// <param name="rotation">The angle to rotate by in degrees.</param>
    /// <returns>The rotated <see cref="DoubleVector3"/>.</returns>
    [Pure]
    public static DoubleVector3 RotateAroundAxis(this DoubleVector3 value, DoubleVector3 axis, double rotation)
    {
        double phi = rotation.DegreeToRadian();
        double cosA = Math.Cos(phi);
        double sinA = Math.Sin(phi);
        return (float)cosA * value +
               (float)sinA * axis.CrossProduct(value) +
               (float)(1 - cosA) * axis.DotProduct(value) * axis;
    }

    /// <summary>
    /// Performs linear interpolation between two vectors.
    /// </summary>
    /// <param name="vector1">The starting vector.</param>
    /// <param name="vector2">The ending vector.</param>
    /// <param name="factor">Interpolation factor between 0 and 1.</param>
    /// <returns>The interpolated vector.</returns>
    [Pure]
    public static DoubleVector3 Lerp(DoubleVector3 vector1, DoubleVector3 vector2, double factor)
        => vector1 * (1 - factor) + vector2 * factor;

    /// <summary>
    /// Performs smooth (trigonometric) interpolation between two or more values
    /// </summary>
    /// <param name="factor">A factor between 0 and <paramref name="values"/>.Length</param>
    /// <param name="values">The value checkpoints</param>
    [Pure]
    public static Vector4 InterpolateTrigonometric(float factor, params Vector4[] values)
    {
        #region Sanity checks
        if (values == null) throw new ArgumentNullException(nameof(values));
        if (values.Length < 2) throw new ArgumentException(Properties.Resources.AtLeast2Values);
        #endregion

        // Handle value overflows
        if (factor <= 0) return values[0];
        if (factor >= values.Length - 1) return values[^1];

        // Isolate index shift from factor
        int index = (int)factor;

        // Remove index shift from factor
        factor -= index;

        // Apply sinus smoothing to factor
        factor = -0.5f * (float)Math.Cos(factor * Math.PI) + 0.5f;

        return new(
            values[index].X + factor * (values[index + 1].X - values[index].X),
            values[index].Y + factor * (values[index + 1].Y - values[index].Y),
            values[index].Z + factor * (values[index + 1].Z - values[index].Z),
            values[index].W + factor * (values[index + 1].W - values[index].W));
    }
}
