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
    /// A 2D grid of <see cref="ByteVector4"/> values that can be stored in ARGB PNG files.
    /// </summary>
    public class ByteVector4Grid : Grid<ByteVector4>
    {
        /// <inheritdoc/>
        public ByteVector4Grid(int width, int height)
            : base(width, height)
        {}

        /// <inheritdoc/>
        public ByteVector4Grid([NotNull] ByteVector4[,] data)
            : base(data)
        {}

        /// <summary>
        /// Loads a grid from a PNG stream.
        /// </summary>
        public static ByteVector4Grid Load([NotNull] Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            #endregion

            try
            {
                using var bitmap = (Bitmap)Image.FromStream(stream);
                var data = new ByteVector4[bitmap.Width, bitmap.Height];
                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        var color = bitmap.GetPixel(x, y);
                        data[x, y] = new(color.B, color.G, color.R, color.A);
                    }
                }
                return new(data);
            }
            #region Error handling
            catch (ArgumentException ex)
            {
                throw new IOException(Resources.NotAnImage, ex);
            }
            #endregion
        }

        /// <inheritdoc/>
        public override unsafe Bitmap GenerateBitmap()
        {
            var bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            var bitmapData = bitmap.LockBits(new(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            try
            {
                var pointer = (byte*)bitmapData.Scan0;
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        var blockPointer = pointer + 4 * x;
                        *blockPointer = Data[x, y].X;
                        *(blockPointer + 1) = Data[x, y].Y;
                        *(blockPointer + 2) = Data[x, y].Z;
                        *(blockPointer + 3) = Data[x, y].W;
                    }
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
