/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using OmegaEngine.Foundation.Design;
using SlimDX;

namespace OmegaEngine.Foundation.Geometry;

/// <summary>
/// Defines a three-component vector with <see cref="double"/> accuracy.
/// </summary>
[TypeConverter(typeof(DoubleVector3Converter))]
[StructLayout(LayoutKind.Sequential)]
public struct DoubleVector3 : IEquatable<DoubleVector3>
{
    /// <summary>
    /// Gets or sets the X component of the vector.
    /// </summary>
    [XmlAttribute, Description("Gets or sets the X component of the vector.")]
    public double X { get; set; }

    /// <summary>
    /// Gets or sets the Y component of the vector.
    /// </summary>
    [XmlAttribute, Description("Gets or sets the Y component of the vector.")]
    public double Y { get; set; }

    /// <summary>
    /// Gets or sets the Z component of the vector.
    /// </summary>
    [XmlAttribute, Description("Gets or sets the Z component of the vector.")]
    public double Z { get; set; }

    /// <summary>
    /// Creates a new vector.
    /// </summary>
    /// <param name="x">The X component.</param>
    /// <param name="y">The Y component.</param>
    /// <param name="z">The Z component.</param>
    public DoubleVector3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>Add <see cref="DoubleVector3"/> to <see cref="Vector3"/></summary>
    public static DoubleVector3 operator +(DoubleVector3 vector1, Vector3 vector2) => new(vector1.X + vector2.X, vector1.Y + vector2.Y, vector1.Z + vector2.Z);

    /// <summary>Add <see cref="Vector3"/> to <see cref="DoubleVector3"/></summary>
    public static DoubleVector3 operator +(Vector3 vector1, DoubleVector3 vector2) => new(vector1.X + vector2.X, vector1.Y + vector2.Y, vector1.Z + vector2.Z);

    /// <summary>Subtract <see cref="DoubleVector3"/> from <see cref="Vector3"/></summary>
    public static DoubleVector3 operator -(DoubleVector3 vector1, Vector3 vector2) => new(vector1.X - vector2.X, vector1.Y - vector2.Y, vector1.Z - vector2.Z);

    /// <summary>Subtract <see cref="DoubleVector3"/> from <see cref="Vector3"/></summary>
    public static DoubleVector3 operator -(Vector3 vector1, DoubleVector3 vector2) => new(vector1.X - vector2.X, vector1.Y - vector2.Y, vector1.Z - vector2.Z);

    /// <summary>Add <see cref="DoubleVector3"/> to <see cref="DoubleVector3"/></summary>
    public static DoubleVector3 operator +(DoubleVector3 vector1, DoubleVector3 vector2) => new(vector1.X + vector2.X, vector1.Y + vector2.Y, vector1.Z + vector2.Z);

    /// <summary>Subtract <see cref="DoubleVector3"/> from <see cref="DoubleVector3"/></summary>
    public static DoubleVector3 operator -(DoubleVector3 vector1, DoubleVector3 vector2) => new(vector1.X - vector2.X, vector1.Y - vector2.Y, vector1.Z - vector2.Z);

    /// <summary>Multiply <see cref="DoubleVector3"/> with <see cref="double"/></summary>
    public static DoubleVector3 operator *(DoubleVector3 vector, double scalar) => new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);

    /// <summary>Multiply <see cref="DoubleVector3"/> with <see cref="double"/></summary>
    public static DoubleVector3 operator *(double scalar, DoubleVector3 vector) => new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);

    /// <summary>Multiply <see cref="DoubleVector3"/> with <see cref="float"/></summary>
    public static DoubleVector3 operator *(float scalar, DoubleVector3 vector) => new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);

    /// <summary>Multiply <see cref="DoubleVector3"/> with <see cref="float"/></summary>
    public static DoubleVector3 operator *(DoubleVector3 vector, float scalar) => new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);

    /// <summary>
    /// Returns a single-precision standard Vector3 after subtracting an offset value
    /// </summary>
    /// <param name="offset">This value is subtracting from the double-precision data before it is casted to single-precision</param>
    /// <returns>The relative value</returns>
    public Vector3 ApplyOffset(DoubleVector3 offset) => new(
        (float)(X - offset.X),
        (float)(Y - offset.Y),
        (float)(Z - offset.Z));

    /// <summary>
    /// Calculates the dot product of this vector and <paramref name="vector"/>.
    /// </summary>
    /// <param name="vector">The second vector to calculate the dot product with.</param>
    /// <returns>this x <paramref name="vector"/></returns>
    public double DotProduct(DoubleVector3 vector) => X * vector.X + Y * vector.Y + Z * vector.Z;

    /// <summary>
    /// Calculates the length of the vector.
    /// </summary>
    public double Length() => Math.Sqrt(X * X + Y * Y + Z * Z);

    /// <summary>
    /// Maps X to X and Z to -Y. Drops Y.
    /// </summary>
    public Vector2 Flatten() => new((float)X, (float)-Z);

    /// <inheritdoc/>
    public override string ToString() => $"({X}, {Y}, {Z})";

    /// <summary>Convert <see cref="Vector3"/> into <see cref="DoubleVector3"/></summary>
    public static explicit operator DoubleVector3(Vector3 vector) => new(vector.X, vector.Y, vector.Z);

    /// <summary>Convert <see cref="DoubleVector3"/> into <see cref="Vector3"/></summary>
    public static explicit operator Vector3(DoubleVector3 vector) => new((float)vector.X, (float)vector.Y, (float)vector.Z);

    /// <inheritdoc/>
    public bool Equals(DoubleVector3 other) => other.X == X && other.Y == Y && other.Z == Z;

    public static bool operator ==(DoubleVector3 left, DoubleVector3 right) => left.Equals(right);
    public static bool operator !=(DoubleVector3 left, DoubleVector3 right) => !left.Equals(right);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is DoubleVector3 vector3 && Equals(vector3);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 7;
            hash = 97 * hash + ((int)X ^ ((int)X >> 32));
            hash = 97 * hash + ((int)Y ^ ((int)Y >> 32));
            hash = 97 * hash + ((int)Z ^ ((int)Z >> 32));
            return hash;
        }
    }
}
