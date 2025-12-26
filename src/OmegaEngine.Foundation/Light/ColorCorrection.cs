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
using JetBrains.Annotations;
using NanoByte.Common;
using OmegaEngine.Foundation.Design;
using SlimDX;

namespace OmegaEngine.Foundation.Light;

/// <summary>
/// Color correction values for use in post-processing.
/// </summary>
[TypeConverter(typeof(ColorCorrectionConverter))]
public struct ColorCorrection : IEquatable<ColorCorrection>
{
    #region Constants
    /// <summary>
    /// No color change.
    /// </summary>
    public static readonly ColorCorrection Default = new(brightness: 1);
    #endregion

    private float _brightness;

    /// <summary>
    /// How bright the picture should be - values between 0 (black) and 5 (5x normal).
    /// </summary>
    [XmlAttribute, Description("How bright the picture should be - values between 0 (black) and 5 (5x normal).")]
    public float Brightness { readonly get => _brightness; set => _brightness = value.Clamp(0, 5); }

    private float _contrast;

    /// <summary>
    /// The contrast level of the picture - values between -5 and 5.
    /// </summary>
    [XmlAttribute, Description("The contrast level of the picture - values between -5 and 5.")]
    public float Contrast { readonly get => _contrast; set => _contrast = value.Clamp(-5, 5); }

    private float _saturation;

    /// <summary>
    /// The color saturation level of the picture - values between -5 and 5.
    /// </summary>
    [XmlAttribute, Description("The color saturation level of the picture - values between -5 and 5.")]
    public float Saturation { readonly get => _saturation; set => _saturation = value.Clamp(-5, 5); }

    private float _hue;

    /// <summary>
    /// The color hue rotation of the picture - values between 0 and 360.
    /// </summary>
    [XmlAttribute, DefaultValue(0f), Description("The color hue rotation of the picture - values between 0 and 360.")]
    public float Hue { readonly get => _hue; set => _hue = value.Clamp(0, 360); }

    /// <summary>
    /// Creates a new color correction structure.
    /// </summary>
    /// <param name="brightness">How bright the picture should be - values between 0 (black) and 5 (5x normal).</param>
    /// <param name="contrast">The contrast level of the picture - values between -5 and 5.</param>
    /// <param name="saturation">The color saturation level of the picture - values between -5 and 5.</param>
    /// <param name="hue">The color hue rotation of the picture - values between 0 and 360.</param>
    public ColorCorrection(float brightness = 1, float contrast = 1, float saturation = 1, float hue = 0)
        : this()
    {
        Brightness = brightness;
        Contrast = contrast;
        Saturation = saturation;
        Hue = hue;
    }

    /// <summary>
    /// Performs interpolation between two or more value sets with easing.
    /// </summary>
    /// <param name="factor">A factor between 0 and <paramref name="values"/>.Length.</param>
    /// <param name="values">The value checkpoints.</param>
    [Pure]
    public static ColorCorrection InterpolateEased(float factor, params ColorCorrection[] values)
    {
        #region Sanity checks
        if (values == null) throw new ArgumentNullException(nameof(values));
        #endregion

        if (factor <= 0) return values[0];
        if (factor >= values.Length - 1) return values[^1];

        int index = (int)factor;
        factor -= index;
        factor = (float)MathUtils.EaseInOut(factor);

        return new(
            MathUtils.Lerp(values[index].Brightness, values[index + 1].Brightness, factor),
            MathUtils.Lerp(values[index].Contrast, values[index + 1].Contrast, factor),
            MathUtils.Lerp(values[index].Saturation, values[index + 1].Saturation, factor),
            MathUtils.Lerp(values[index].Hue, values[index + 1].Hue, factor));
    }

    #region Conversion
    /// <inheritdoc/>
    public readonly override string ToString() => $"(Brightness: {Brightness}, Contrast: {Contrast}, Saturation: {Saturation}, Hue: {Hue})";

    /// <summary>Convert <see cref="ColorCorrection"/> into <see cref="Vector4"/></summary>
    public static explicit operator Vector4(ColorCorrection correction) => new(correction._brightness, correction._contrast, correction._saturation, correction._hue);

    /// <summary>Convert <see cref="Vector4"/> into see <see cref="ColorCorrection"/></summary>
    public static explicit operator ColorCorrection(Vector4 vector) => new(vector.X, vector.Y, vector.Z, vector.W);
    #endregion

    #region Equality
    /// <inheritdoc/>
    public bool Equals(ColorCorrection other) => other.Brightness == Brightness && other.Contrast == Contrast && other.Saturation == Saturation && other.Hue == Hue;

    public static bool operator ==(ColorCorrection left, ColorCorrection right) => left.Equals(right);
    public static bool operator !=(ColorCorrection left, ColorCorrection right) => !left.Equals(right);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return (obj.GetType() != typeof(ColorCorrection)) && Equals((ColorCorrection)obj);
    }

    /// <inheritdoc/>
    public readonly override int GetHashCode() => HashCode.Combine(_brightness, _contrast, _saturation, _hue);
    #endregion
}
