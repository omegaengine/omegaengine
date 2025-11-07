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
using OmegaEngine.Foundation.Design;
using SlimDX;

namespace OmegaEngine.Foundation.Light;

/// <summary>
/// Factors describing the attenuation of light intensity over distance.
/// </summary>
/// <param name="constant">A constant factor multiplied with the color.</param>
/// <param name="linear">A constant factor multiplied with the color and the inverse distance.</param>
/// <param name="quadratic">A constant factor multiplied with the color and the inverse distance squared.</param>
[TypeConverter(typeof(AttenuationConverter))]
public struct Attenuation(float constant, float linear, float quadratic) : IEquatable<Attenuation>
{
    /// <summary>
    /// Value for no attenuation over distance.
    /// </summary>
    public static readonly Attenuation None = new(1, 0, 0);

    /// <summary>
    /// A constant factor multiplied with the color.
    /// </summary>
    [XmlAttribute, Description("A constant factor multiplied with the color.")]
    public float Constant { get; set; } = constant;

    /// <summary>
    /// A constant factor multiplied with the color and the inverse distance.
    /// </summary>
    [XmlAttribute, Description("A constant factor multiplied with the color and the inverse distance.")]
    public float Linear { get; set; } = linear;

    /// <summary>
    /// A constant factor multiplied with the color and the inverse distance squared.
    /// </summary>
    [XmlAttribute, Description("A constant factor multiplied with the color and the inverse distance squared.")]
    public float Quadratic { get; set; } = quadratic;

    /// <inheritdoc/>
    public override string ToString() => $"(Constant: {Constant}, Linear: {Linear}, Quadratic: {Quadratic})";

    /// <summary>Convert <see cref="Attenuation"/> into <see cref="Vector4"/></summary>
    public static explicit operator Vector4(Attenuation attenuation) => new(attenuation.Constant, attenuation.Linear, attenuation.Quadratic, 0);

    /// <summary>Convert <see cref="Vector4"/> into <see cref="Attenuation"/></summary>
    public static explicit operator Attenuation(Vector4 vector) => new(vector.X, vector.Y, vector.Z);

    /// <inheritdoc/>
    public bool Equals(Attenuation other) => other.Constant == Constant && other.Linear == Linear && other.Quadratic == Quadratic;

    public static bool operator ==(Attenuation left, Attenuation right) => left.Equals(right);
    public static bool operator !=(Attenuation left, Attenuation right) => !left.Equals(right);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Attenuation attenuation && Equals(attenuation);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Constant, Linear, Quadratic);
}
