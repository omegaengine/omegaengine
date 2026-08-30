/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Xml.Serialization;
using SlimDX;

#if NETFRAMEWORK
using OmegaEngine.Foundation.Design;
using System.Drawing.Design;
#endif

namespace OmegaEngine.Foundation.Geometry;

/// <summary>
/// Rotation around three axes, in degrees.
/// </summary>
/// <param name="yaw">Rotation around the Y axis, in degrees.</param>
/// <param name="pitch">Rotation around the X axis, in degrees.</param>
/// <param name="roll">Rotation around the Z axis, in degrees.</param>
#if NETFRAMEWORK
[TypeConverter(typeof(Design.RotationConverter))]
#endif
public struct Rotation(float yaw = 0, float pitch = 0, float roll = 0) : IEquatable<Rotation>
{
    /// <summary>
    /// Rotation around the Y axis, in degrees.
    /// </summary>
    [XmlAttribute, DefaultValue(0f)]
    [Description("Rotation around the Y axis, in degrees.")]
#if NETFRAMEWORK
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
#endif
    public float Yaw { get; set; } = yaw;

    /// <summary>
    /// Rotation around the X axis, in degrees.
    /// </summary>
    [XmlAttribute, DefaultValue(0f)]
    [Description("Rotation around the X axis, in degrees.")]
#if NETFRAMEWORK
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
#endif
    public float Pitch { get; set; } = pitch;

    /// <summary>
    /// Rotation around the Z axis, in degrees.
    /// </summary>
    [XmlAttribute, DefaultValue(0f)]
    [Description("Rotation around the Z axis, in degrees.")]
#if NETFRAMEWORK
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
#endif
    public float Roll { get; set; } = roll;

    /// <summary>Convert <see cref="Rotation"/> into <see cref="Quaternion"/></summary>
    public static implicit operator Quaternion(Rotation rotation) => Quaternion.RotationYawPitchRoll(
        rotation.Yaw.DegreeToRadian(),
        rotation.Pitch.DegreeToRadian(),
        rotation.Roll.DegreeToRadian());

    /// <summary>Convert <see cref="Rotation"/> into <see cref="Matrix"/></summary>
    public static implicit operator Matrix(Rotation rotation) => Matrix.RotationYawPitchRoll(
        rotation.Yaw.DegreeToRadian(),
        rotation.Pitch.DegreeToRadian(),
        rotation.Roll.DegreeToRadian());

    /// <inheritdoc/>
    public bool Equals(Rotation other) => other.Yaw == Yaw && other.Pitch == Pitch && other.Roll == Roll;

    public static bool operator ==(Rotation left, Rotation right) => left.Equals(right);
    public static bool operator !=(Rotation left, Rotation right) => !left.Equals(right);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Rotation rotation && Equals(rotation);
    }

    /// <inheritdoc/>
    public readonly override int GetHashCode() => HashCode.Combine(Yaw, Pitch, Roll);
}
