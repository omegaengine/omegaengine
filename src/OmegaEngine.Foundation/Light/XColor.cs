/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using JetBrains.Annotations;
using SlimDX;

namespace OmegaEngine.Foundation.Light;

/// <summary>
/// Stores ARGB-colors as byte values but also surfaces them as float values.
/// </summary>
/// <remarks>
/// This class can be used to serialize ARGB-color values (unlike <see cref="Color"/> all fields are writable).
/// It provides implicit casts to and from <see cref="Color"/>.
/// </remarks>
[XmlInclude(typeof(Color))]
[StructLayout(LayoutKind.Sequential)]
public struct XColor(byte a, byte r, byte g, byte b) : IEquatable<XColor>
{
    [XmlAttribute]
    public byte A { get; set; } = a;

    [XmlAttribute]
    public byte R { get; set; } = r;

    [XmlAttribute]
    public byte G { get; set; } = g;

    [XmlAttribute]
    public byte B { get; set; } = b;

    [XmlIgnore]
    public float Red { get => (float)R / 255; set => R = (byte)(value * 255); }

    [XmlIgnore]
    public float Green { get => (float)G / 255; set => G = (byte)(value * 255); }

    [XmlIgnore]
    public float Blue { get => (float)B / 255; set => B = (byte)(value * 255); }

    [XmlIgnore]
    public float Alpha { get => (float)A / 255; set => A = (byte)(value * 255); }

    public XColor(float red, float green, float blue, float alpha)
        : this((byte)(alpha * 255), (byte)(red * 255), (byte)(green * 255), (byte)(blue * 255))
    {}

    /// <summary>
    /// Removes the alpha channel from the color (setting it to full opacity).
    /// </summary>
    [Pure]
    public XColor DropAlpha() => new(255, R, G, B);

    [Pure]
    public Color4 ToColorValue() => new(Alpha, Red, Green, Blue);

    /// <inheritdoc/>
    public override string ToString() => ((Color)this).ToString();

    public static implicit operator XColor(Color color) => new(color.A, color.R, color.G, color.B);
    public static implicit operator Color(XColor color) => Color.FromArgb(color.A, color.R, color.G, color.B);

    public static bool operator ==(XColor color1, XColor color2) => (Color)color1 == (Color)color2;
    public static bool operator !=(XColor color1, XColor color2) => (Color)color1 != (Color)color2;

    public static bool operator ==(XColor color1, Color color2) => (Color)color1 == color2;
    public static bool operator !=(XColor color1, Color color2) => (Color)color1 != color2;

    public static bool operator ==(Color color1, XColor color2) => color1 == (Color)color2;
    public static bool operator !=(Color color1, XColor color2) => color1 != (Color)color2;

    /// <inheritdoc/>
    public bool Equals(XColor other) => (Color)this == (Color)other;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj switch
        {
            XColor a => a == this,
            Color b => b == this,
            _ => false
        };

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(A, R, G, B);
}
