/*
 * Copyright 2006-2013 Bastian Eicher
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
using SlimDX;

namespace Common.Utils
{
    /// <summary>
    /// Convert colors to different formats, interpolate, invert, ...
    /// </summary>
    public static class ColorUtils
    {
        #region Conversions
        /// <summary>
        /// Converts a Color to a Vector4, assigning X=R, Y=G, Z=B, W=Alpha
        /// </summary>
        /// <param name="color">The Color</param>
        /// <returns>The Vector4</returns>
        public static Vector4 ColorToVector4(Color color)
        {
            return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }

        /// <summary>
        /// Converts a Vector4 to a Color, assigning R=X, G=Y, B=Z, Alpha=W
        /// </summary>
        /// <param name="vector">The Vector4</param>
        /// <returns>The Color</returns>
        public static Color Vector4ToColor(Vector4 vector)
        {
            return Color.FromArgb((int)(vector.W * 255), (int)(vector.X * 255), (int)(vector.Y * 255), (int)(vector.Z * 255));
        }
        #endregion

        #region Compare
        /// <summary>
        /// Compares two colors ignoring the alpha channel and the name
        /// </summary>
        [LuaGlobal(Name = "CompareColor", Description = "Compares two colors ignoring the alpha channel and the name")]
        public static bool Compare(Color color1, Color color2)
        {
            return color1.R == color2.R && color1.G == color2.G && color1.B == color2.B;
        }
        #endregion

        #region Add
        /// <summary>
        /// Adds two colors component-wise
        /// </summary>
        /// <param name="color1">Color 1</param>
        /// <param name="color2">Color 2</param>
        /// <returns>Return color</returns>
        [LuaGlobal(Name = "AddColors", Description = "Adds two colors component-wise")]
        public static Color Add(Color color1, Color color2)
        {
            // Get values from color1
            float redValue1 = color1.R / 255f;
            float greenValue1 = color1.G / 255f;
            float blueValue1 = color1.B / 255f;
            float alphaValue1 = color1.A / 255f;

            // Get values from color2
            float redValue2 = color2.R / 255f;
            float greenValue2 = color2.G / 255f;
            float blueValue2 = color2.B / 255f;
            float alphaValue2 = color2.A / 255f;

            // Multiply everything using our floats
            return Color.FromArgb(
                (byte)(MathUtils.Clamp(alphaValue1 + alphaValue2, 0, 1) * 255f),
                (byte)(MathUtils.Clamp(redValue1 + redValue2, 0, 1) * 255f),
                (byte)(MathUtils.Clamp(greenValue1 + greenValue2, 0, 1) * 255f),
                (byte)(MathUtils.Clamp(blueValue1 + blueValue2, 0, 1) * 255f));
        }

        /// <summary>
        /// Adds two colors component-wise
        /// </summary>
        /// <param name="color1">Color 1</param>
        /// <param name="color2">Color 2</param>
        /// <returns>Return color</returns>
        public static Color4 Add(Color4 color1, Color4 color2)
        {
            // Multiply everything using our floats
            return new Color4(
                MathUtils.Clamp(color1.Alpha + color2.Alpha, 0, 1),
                MathUtils.Clamp(color1.Red + color2.Red, 0, 1),
                MathUtils.Clamp(color1.Green + color2.Green, 0, 1),
                MathUtils.Clamp(color1.Blue + color2.Blue, 0, 1));
        }
        #endregion

        #region Multiply
        /// <summary>
        /// Multiplies a color with a scalar returns the resulting color
        /// </summary>
        /// <returns>The resulting Color</returns>
        public static Color Multiply(Color color, float scalar)
        {
            // Check if the color is black, multiplying won't do anything then
            if (color == Color.Black) return color;

            return Color.FromArgb(color.A, (int)(color.R * scalar), (int)(color.G * scalar), (int)(color.B * scalar));
        }

        /// <summary>
        /// Multiplies two colors and returns a vector
        /// </summary>
        /// <returns>Color as vector</returns>
        public static Vector4 Multiply(Color color1, Color color2)
        {
            // Check if any of the colors is white, multiplying won't do anything then
            if (color1 == Color.White) return ColorToVector4(color2);
            if (color2 == Color.White) return ColorToVector4(color1);

            // Get values from color1
            float redValue1 = color1.R / 255f;
            float greenValue1 = color1.G / 255f;
            float blueValue1 = color1.B / 255f;
            float alphaValue1 = color1.A / 255f;

            // Get values from color2
            float redValue2 = color2.R / 255f;
            float greenValue2 = color2.G / 255f;
            float blueValue2 = color2.B / 255f;
            float alphaValue2 = color2.A / 255f;

            // Multiply everything using our floats
            return new Vector4(
                MathUtils.Clamp(redValue1 * redValue2, 0, 1),
                MathUtils.Clamp(greenValue1 * greenValue2, 0, 1),
                MathUtils.Clamp(blueValue1 * blueValue2, 0, 1),
                MathUtils.Clamp(alphaValue1 * alphaValue2, 0, 1));
        }
        #endregion

        #region 16 colors
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
            if (Compare(color, Color.Black)) return 0; // (0,0,0)
            if (Compare(color, Color.Gray)) return 1; // (128,128,128)
            if (Compare(color, Color.Maroon)) return 2; // (128,0,0)
            if (Compare(color, Color.Red)) return 3; // (255,0,0)
            if (Compare(color, Color.Green)) return 4; // (0,128,0)
            if (Compare(color, Color.Lime)) return 5; // (0,255,0)
            if (Compare(color, Color.Olive)) return 6; // (128,128,0)
            if (Compare(color, Color.Yellow)) return 7; // (255,255,0)
            if (Compare(color, Color.Navy)) return 8; // (0,0,128)
            if (Compare(color, Color.Blue)) return 9; // (0,0,255)
            if (Compare(color, Color.Purple)) return 10; // (128,0,128)
            if (Compare(color, Color.Fuchsia)) return 11; // (255,0,255)
            if (Compare(color, Color.Teal)) return 12; // (0,128,128)
            if (Compare(color, Color.Aqua)) return 13; // (0,255,255)
            if (Compare(color, Color.Silver)) return 14; // (192,192,192)
            if (Compare(color, Color.White)) return 15; // (255,255,255)
            throw new ArgumentOutOfRangeException("color");
        }
        #endregion

        #region Interpolate
        /// <summary>
        /// Interpolates between two colors
        /// </summary>
        /// <param name="factor">The proportion of the two colors between 0 (only first color) and 1 (only second color)</param>
        /// <param name="color1">The first color value</param>
        /// <param name="color2">The second color value</param>
        [LuaGlobal(Name = "InterpolateColor", Description = "Interpolates between two colors")]
        public static Color Interpolate(float factor, Color color1, Color color2)
        {
            factor = MathUtils.Clamp(factor, 0, 1);
            return Color.FromArgb(
                (byte)(color1.A * (1.0f - factor) + color2.A * factor),
                (byte)(color1.R * (1.0f - factor) + color2.R * factor),
                (byte)(color1.G * (1.0f - factor) + color2.G * factor),
                (byte)(color1.B * (1.0f - factor) + color2.B * factor));
        }
        #endregion

        #region Invert
        /// <summary>
        /// Generates the absolute opposite of a color
        /// </summary>
        /// <param name="color">The color to be inverted</param>
        /// <returns>The inverted color</returns>
        [LuaGlobal(Name = "InvertColor", Description = "Generates the absolute opposite of a color")]
        public static Color Invert(Color color)
        {
            return Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);
        }
        #endregion
    }
}
