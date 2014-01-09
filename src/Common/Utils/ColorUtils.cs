/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Drawing;
using LuaInterface;

namespace Common.Utils
{
    /// <summary>
    /// Convert colors to different formats, interpolate, invert, ...
    /// </summary>
    public static class ColorUtils
    {
        /// <summary>
        /// Compares two colors ignoring the alpha channel and the name
        /// </summary>
        [LuaGlobal(Name = "CompareColor", Description = "Compares two colors ignoring the alpha channel and the name")]
        public static bool EqualsIgnoreAlpha(this Color color1, Color color2)
        {
            return color1.R == color2.R && color1.G == color2.G && color1.B == color2.B;
        }

        /// <summary>
        /// Interpolates between two colors
        /// </summary>
        /// <param name="factor">The proportion of the two colors between 0 (only first color) and 1 (only second color)</param>
        /// <param name="color1">The first color value</param>
        /// <param name="color2">The second color value</param>
        [LuaGlobal(Name = "InterpolateColor", Description = "Interpolates between two colors")]
        public static Color Interpolate(float factor, Color color1, Color color2)
        {
            factor = factor.Clamp();
            return Color.FromArgb(
                (byte)(color1.A * (1.0f - factor) + color2.A * factor),
                (byte)(color1.R * (1.0f - factor) + color2.R * factor),
                (byte)(color1.G * (1.0f - factor) + color2.G * factor),
                (byte)(color1.B * (1.0f - factor) + color2.B * factor));
        }

        /// <summary>
        /// Returns one of the classic 16 colors identified by a number
        /// </summary>
        /// <param name="colorNumber">A number between 0 and 15</param>
        /// <returns>The color associated to <paramref name="colorNumber"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="colorNumber"/> is larger than 15</exception>
        public static Color SixteenColors(byte colorNumber)
        {
            #region Sanity checks
            if (colorNumber > 15) throw new ArgumentOutOfRangeException("colorNumber");
            #endregion

            switch (colorNumber)
            {
                default:
                    return Color.Black; // (0,0,0)
                case 1:
                    return Color.Gray; // (128,128,128)
                case 2:
                    return Color.Maroon; // (128,0,0)
                case 3:
                    return Color.Red; // (255,0,0)
                case 4:
                    return Color.Green; // (0,128,0)
                case 5:
                    return Color.Lime; // (0,255,0)
                case 6:
                    return Color.Olive; // (128,128,0)
                case 7:
                    return Color.Yellow; // (255,255,0)
                case 8:
                    return Color.Navy; // (0,0,128)
                case 9:
                    return Color.Blue; // (0,0,255)
                case 10:
                    return Color.Purple; // (128,0,128)
                case 11:
                    return Color.Fuchsia; // (255,0,255)
                case 12:
                    return Color.Teal; // (0,128,128)
                case 13:
                    return Color.Aqua; // (0,255,255)
                case 14:
                    return Color.Silver; // (192,192,192)
                case 15:
                    return Color.White; // (255,255,255)
            }
        }

        /// <summary>
        /// Identifies one of the classic 16 colors with a number
        /// </summary>
        /// <param name="color">One of the classic 16 colors</param>
        /// <returns>A number between 0 and 15</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="color"/> is not one of the classic 16 colors</exception>
        public static byte SixteenColors(Color color)
        {
            if (color.EqualsIgnoreAlpha(Color.Black)) return 0; // (0,0,0)
            if (color.EqualsIgnoreAlpha(Color.Gray)) return 1; // (128,128,128)
            if (color.EqualsIgnoreAlpha(Color.Maroon)) return 2; // (128,0,0)
            if (color.EqualsIgnoreAlpha(Color.Red)) return 3; // (255,0,0)
            if (color.EqualsIgnoreAlpha(Color.Green)) return 4; // (0,128,0)
            if (color.EqualsIgnoreAlpha(Color.Lime)) return 5; // (0,255,0)
            if (color.EqualsIgnoreAlpha(Color.Olive)) return 6; // (128,128,0)
            if (color.EqualsIgnoreAlpha(Color.Yellow)) return 7; // (255,255,0)
            if (color.EqualsIgnoreAlpha(Color.Navy)) return 8; // (0,0,128)
            if (color.EqualsIgnoreAlpha(Color.Blue)) return 9; // (0,0,255)
            if (color.EqualsIgnoreAlpha(Color.Purple)) return 10; // (128,0,128)
            if (color.EqualsIgnoreAlpha(Color.Fuchsia)) return 11; // (255,0,255)
            if (color.EqualsIgnoreAlpha(Color.Teal)) return 12; // (0,128,128)
            if (color.EqualsIgnoreAlpha(Color.Aqua)) return 13; // (0,255,255)
            if (color.EqualsIgnoreAlpha(Color.Silver)) return 14; // (192,192,192)
            if (color.EqualsIgnoreAlpha(Color.White)) return 15; // (255,255,255)
            throw new ArgumentOutOfRangeException("color");
        }
    }
}
