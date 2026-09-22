/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using JetBrains.Annotations;
using SlimDX;

namespace OmegaEngine.Foundation.Light;

/// <summary>
/// Convert colors to different formats, interpolate, invert, ...
/// </summary>
public static class ColorUtils
{
    /// <summary>
    /// Removes the alpha channel from the color (setting it to full opacity).
    /// </summary>
    [Pure]
    public static Color DropAlpha(this Color color)
        => color.A == 255
            ? color // Preserve well-known color names when possible
            : Color.FromArgb(255, color);

    /// <summary>
    /// Compares two colors ignoring the alpha channel and the name
    /// </summary>
    [Pure]
    public static bool EqualsIgnoreAlpha(this Color color1, Color color2) => color1.R == color2.R && color1.G == color2.G && color1.B == color2.B;

    /// <summary>
    /// Multiplies a color's RGB channels with a scalar factor between 0 and 1, preserving the alpha channel
    /// </summary>
    [Pure]
    public static Color Multiply(this Color color, float factor)
    {
        factor = factor.Clamp();

        // Preserve well-known color names when possible
        if (factor == 1) return color;
        if (factor == 0 && color.A == 255) return Color.Black;
        if (color == Color.Black) return color;

        return Color.FromArgb(
            color.A,
            (byte)(color.R * factor),
            (byte)(color.G * factor),
            (byte)(color.B * factor));
    }

    /// <summary>
    /// Interpolates between two colors
    /// </summary>
    /// <param name="factor">The proportion of the two colors between 0 (only first color) and 1 (only second color)</param>
    /// <param name="color1">The first color value</param>
    /// <param name="color2">The second color value</param>
    [Pure]
    public static Color Interpolate(float factor, Color color1, Color color2)
    {
        factor = factor.Clamp();
        return Color.FromArgb(
            (byte)(color1.A * (1.0f - factor) + color2.A * factor),
            (byte)(color1.R * (1.0f - factor) + color2.R * factor),
            (byte)(color1.G * (1.0f - factor) + color2.G * factor),
            (byte)(color1.B * (1.0f - factor) + color2.B * factor));
    }

    #region sRGB
    /// <summary>
    /// Converts a single color channel from sRGB (gamma) encoding to linear space
    /// </summary>
    /// <param name="value">The sRGB-encoded channel value, usually between 0 and 1</param>
    /// <remarks>Uses the exact piecewise IEC 61966-2-1 curve, not an approximate <c>pow(value, 2.2)</c></remarks>
    [Pure]
    public static float SrgbToLinear(float value)
        => value <= 0.04045f
            ? value / 12.92f
            : (float)Math.Pow((value + 0.055) / 1.055, 2.4);

    /// <summary>
    /// Converts a single color channel from linear space to sRGB (gamma) encoding
    /// </summary>
    /// <param name="value">The linear channel value, usually between 0 and 1</param>
    /// <remarks>Uses the exact piecewise IEC 61966-2-1 curve, not an approximate <c>pow(value, 1/2.2)</c></remarks>
    [Pure]
    public static float LinearToSrgb(float value)
        => value <= 0.0031308f
            ? value * 12.92f
            : (float)(1.055 * Math.Pow(value, 1.0 / 2.4) - 0.055);

    /// <summary>
    /// Converts a color from sRGB (gamma) encoding to linear space, leaving the alpha channel untouched
    /// </summary>
    [Pure]
    public static Color4 SrgbToLinear(this Color4 color)
        => new(color.Alpha, SrgbToLinear(color.Red), SrgbToLinear(color.Green), SrgbToLinear(color.Blue));

    /// <summary>
    /// Converts a color from linear space to sRGB (gamma) encoding, leaving the alpha channel untouched
    /// </summary>
    [Pure]
    public static Color4 LinearToSrgb(this Color4 color)
        => new(color.Alpha, LinearToSrgb(color.Red), LinearToSrgb(color.Green), LinearToSrgb(color.Blue));

    /// <summary>
    /// Converts an sRGB-encoded (i.e., as authored) <see cref="Color"/> to a linear-space <see cref="Color4"/>
    /// </summary>
    /// <remarks>
    /// The result must never be converted back to an 8-bit <see cref="Color"/>: linear values crowd into the
    /// bottom of the byte range (sRGB 40 becomes linear 0.0212, i.e. byte 5), destroying precision in the darks.
    /// Linearize only at the boundary where the value is handed to the GPU as a float, <see cref="Color4"/> or DWORD.
    /// </remarks>
    [Pure]
    public static Color4 SrgbToLinear(this Color color) => new Color4(color).SrgbToLinear();

    /// <summary>
    /// Multiplies a color's RGB channels with a scalar factor between 0 and 1 in linear space, preserving the alpha channel
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="Multiply"/> this scales the light intensity the color represents rather than its
    /// gamma-encoded byte values, so a factor of 0.5 halves the actual brightness.
    /// </remarks>
    [Pure]
    public static Color MultiplyLinear(this Color color, float factor)
    {
        factor = factor.Clamp();

        // Preserve well-known color names when possible
        if (factor == 1) return color;
        if (factor == 0 && color.A == 255) return Color.Black;
        if (color == Color.Black) return color;

        var linear = color.SrgbToLinear();
        return Color.FromArgb(
            color.A,
            ToByte(LinearToSrgb(linear.Red * factor)),
            ToByte(LinearToSrgb(linear.Green * factor)),
            ToByte(LinearToSrgb(linear.Blue * factor)));
    }

    /// <summary>
    /// Interpolates between two colors in linear space
    /// </summary>
    /// <param name="factor">The proportion of the two colors between 0 (only first color) and 1 (only second color)</param>
    /// <param name="color1">The first color value</param>
    /// <param name="color2">The second color value</param>
    /// <remarks>
    /// Unlike <see cref="Interpolate"/> this blends the light intensities the colors represent rather than their
    /// gamma-encoded byte values, so the midpoint is the physical average of the two.
    /// </remarks>
    [Pure]
    public static Color InterpolateLinear(float factor, Color color1, Color color2)
    {
        factor = factor.Clamp();
        var linear1 = color1.SrgbToLinear();
        var linear2 = color2.SrgbToLinear();
        return Color.FromArgb(
            (byte)(color1.A * (1.0f - factor) + color2.A * factor),
            ToByte(LinearToSrgb(linear1.Red * (1.0f - factor) + linear2.Red * factor)),
            ToByte(LinearToSrgb(linear1.Green * (1.0f - factor) + linear2.Green * factor)),
            ToByte(LinearToSrgb(linear1.Blue * (1.0f - factor) + linear2.Blue * factor)));
    }

    private static int ToByte(float value) => (int)(value.Clamp() * 255 + 0.5f);
    #endregion
}
