/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using JetBrains.Annotations;
using OmegaEngine.Properties;

namespace OmegaEngine.Values
{
    /// <summary>
    /// A 2D grid of nibble (half a <see cref="byte"/> / 4 bits) values that can be stored in 16 colors PNG files.
    /// </summary>
    public class NibbleGrid : Grid<byte>
    {
        /// <inheritdoc/>
        public NibbleGrid(int width, int height)
            : base(width, height)
        {}

        /// <inheritdoc/>
        public NibbleGrid([NotNull] byte[,] data)
            : base(data)
        {}

        /// <summary>
        /// Gets or sets a value in the grid.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"><paramref name="x"/> or <paramref name="y"/> are out of bounds.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A value larger than 15 is set.</exception>
        public override byte this[int x, int y]
        {
            get => Data[x, y];
            set
            {
                if (value > 15) throw new ArgumentOutOfRangeException(nameof(value));
                Data[x, y] = value;
            }
        }

        /// <summary>
        /// Loads a grid from a PNG stream.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Any of the colors in the imag  is not part of the classic 16 colors palette.</exception>
        public static NibbleGrid Load([NotNull] Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            #endregion

            try
            {
                using (var bitmap = (Bitmap)Image.FromStream(stream))
                {
                    var data = new byte[bitmap.Width, bitmap.Height];
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        for (int y = 0; y < bitmap.Height; y++)
                            data[x, y] = PaletteLookup(bitmap.GetPixel(x, y));
                    }
                    return new(data);
                }
            }
            #region Error handling
            catch (ArgumentException ex)
            {
                throw new IOException(Resources.NotAnImage, ex);
            }
            #endregion
        }

        /// <summary>
        /// Identifies a color in the classic 16 colors palette.
        /// </summary>
        /// <param name="color">One of the classic 16 colors.</param>
        /// <returns>A number between 0 and 15.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="color"/> is not part of the classic 16 colors palette.</exception>
        private static byte PaletteLookup(Color color)
        {
            if (color.EqualsIgnoreAlpha(Color.Black)) return 0; // (0,0,0)
            if (color.EqualsIgnoreAlpha(Color.Maroon)) return 1; // (128,0,0)
            if (color.EqualsIgnoreAlpha(Color.Green)) return 2; // (0,128,0)
            if (color.EqualsIgnoreAlpha(Color.Olive)) return 3; // (128,128,0)
            if (color.EqualsIgnoreAlpha(Color.Navy)) return 4; // (0,0,128)
            if (color.EqualsIgnoreAlpha(Color.Purple)) return 5; // (128,0,128)
            if (color.EqualsIgnoreAlpha(Color.Teal)) return 6; // (0,128,128)
            if (color.EqualsIgnoreAlpha(Color.Gray)) return 7; // (128,128,128)
            if (color.EqualsIgnoreAlpha(Color.Silver)) return 8; // (192,192,192)
            if (color.EqualsIgnoreAlpha(Color.Red)) return 9; // (255,0,0)
            if (color.EqualsIgnoreAlpha(Color.Lime)) return 10; // (0,255,0)
            if (color.EqualsIgnoreAlpha(Color.Yellow)) return 11; // (255,255,0)
            if (color.EqualsIgnoreAlpha(Color.Blue)) return 12; // (0,0,255)
            if (color.EqualsIgnoreAlpha(Color.Fuchsia)) return 13; // (255,0,255)
            if (color.EqualsIgnoreAlpha(Color.Aqua)) return 14; // (0,255,255)
            if (color.EqualsIgnoreAlpha(Color.White)) return 15; // (255,255,255)
            throw new ArgumentOutOfRangeException(nameof(color));
        }

        /// <inheritdoc/>
        public override unsafe Bitmap GenerateBitmap()
        {
            var bitmap = new Bitmap(Width, Height, PixelFormat.Format4bppIndexed);
            var bitmapData = bitmap.LockBits(new(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            try
            {
                var pointer = (byte*)bitmapData.Scan0;
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < (Width + 1) / 2; x++)
                        *(pointer + x) = MathUtils.CombineHiLoByte(ClampedRead(x * 2, y), ClampedRead(x * 2 + 1, y));
                    pointer += bitmapData.Stride;
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
            return bitmap;
        }
    }
}
