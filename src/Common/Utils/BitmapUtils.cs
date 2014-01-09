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
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Common.Utils
{
    /// <summary>
    /// Allows fast array-based access to bitmaps and grayscale storage.
    /// </summary>
    public static class BitmapUtils
    {
        #region Color array
        /// <summary>
        /// Returns a 2D array of <see cref="Color"/> structs representing the content of a bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to read</param>
        /// <returns>The 2D array of <see cref="Color"/> structs</returns>
        public static Color[,] GetColorArray(Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException("bitmap");

            #region Load parameters
            var colorIndex = new Hashtable();
            ColorPalette colorPalette = bitmap.Palette;
            PixelFormat pixelFormat = bitmap.PixelFormat;
            BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int stride = bmpData.Stride;
            int bytes = stride * bitmap.Height;
            #endregion

            #region Load pixels
            var bitmapData = new byte[bytes];
            Marshal.Copy(ptr, bitmapData, 0, bytes);
            bitmap.UnlockBits(bmpData);
            var color = new Color[bitmap.Width, bitmap.Height];

            switch (pixelFormat)
            {
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppArgb:
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                            color[x, y] = Color.FromArgb(bitmapData[y * stride + x * 4 + 3], bitmapData[y * stride + x * 4 + 2], bitmapData[y * stride + x * 4 + 1], bitmapData[y * stride + x * 4]);
                    }
                    break;

                case PixelFormat.Format24bppRgb:
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                            color[x, y] = Color.FromArgb(bitmapData[y * stride + x * 3 + 2], bitmapData[y * stride + x * 3 + 1], bitmapData[y * stride + x * 3]);
                    }
                    break;

                case PixelFormat.Format8bppIndexed:
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                            color[x, y] = colorPalette.Entries[bitmapData[y * stride + x]];
                    }
                    for (int i = 0; i < 256; i++)
                        colorIndex[colorPalette.Entries[i].ToArgb()] = i;
                    break;

                case PixelFormat.Format4bppIndexed:
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            color[x, y] = x % 2 == 0 ?
                                colorPalette.Entries[MathUtils.LoByte(bitmapData[y * stride + x / 2])] :
                                colorPalette.Entries[MathUtils.HiByte(bitmapData[y * stride + x / 2])];
                        }
                    }
                    for (byte i = 0; i < 16; i++)
                        colorIndex[colorPalette.Entries[i].ToArgb()] = i;
                    break;

                case PixelFormat.Format1bppIndexed:
                    int rest = bitmap.Width % 8;
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        byte bits;
                        int x;
                        for (x = 0; x < bitmap.Width - 8; x += 8)
                        {
                            bits = bitmapData[y * stride + x / 8];
                            color[x, y] = colorPalette.Entries[(bits & 128) / 128];
                            color[x + 1, y] = colorPalette.Entries[(bits & 64) / 64];
                            color[x + 2, y] = colorPalette.Entries[(bits & 32) / 32];
                            color[x + 3, y] = colorPalette.Entries[(bits & 16) / 16];
                            color[x + 4, y] = colorPalette.Entries[(bits & 8) / 8];
                            color[x + 5, y] = colorPalette.Entries[(bits & 4) / 4];
                            color[x + 6, y] = colorPalette.Entries[(bits & 2) / 2];
                            color[x + 7, y] = colorPalette.Entries[bits & 1];
                        }
                        bits = bitmapData[y * stride + x / 8];
                        int teiler = 128;
                        for (int i = 0; i < rest; i++)
                        {
                            color[x + i, y] = colorPalette.Entries[(bits & teiler) / teiler];
                            teiler /= 2;
                        }
                        for (int i = 0; i < 2; i++)
                            colorIndex[colorPalette.Entries[i].ToArgb()] = i;
                    }
                    break;
            }
            #endregion

            return color;
        }
        #endregion

        #region 24bpp Grayscale
        /// <summary>
        /// Converts any <see cref="Bitmap"/> to a 24-bit grayscale <see cref="Bitmap"/>
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> to be converted</param>
        /// <returns>The 24-bit grayscale <see cref="Bitmap"/>.</returns>
        public static Bitmap ConvertTo24BppGrayscale(Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException("bitmap");

            Color[,] color = GetColorArray(bitmap);
            var tmpBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
            BitmapData tmpBmpData = tmpBitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, tmpBitmap.PixelFormat);
            int tmpStride = tmpBmpData.Stride;
            var tmpBildDaten = new byte[bitmap.Height * tmpStride];
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color c = color[x, y];
                    var luma = (int)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);
                    Color tmpColor = Color.FromArgb(luma, luma, luma);
                    tmpBildDaten[y * tmpStride + x * 3 + 2] = tmpColor.R;
                    tmpBildDaten[y * tmpStride + x * 3 + 1] = tmpColor.G;
                    tmpBildDaten[y * tmpStride + x * 3] = tmpColor.B;
                }
            }
            IntPtr ptr = tmpBmpData.Scan0;
            Marshal.Copy(tmpBildDaten, 0, ptr, bitmap.Height * tmpStride);
            tmpBitmap.UnlockBits(tmpBmpData);
            return tmpBitmap;
        }
        #endregion

        #region 8bpp Grayscale
        /// <summary>
        /// Converts any <see cref="Bitmap"/> to a 8-bit grayscale <see cref="Bitmap"/>
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> to be converted</param>
        /// <returns>The 8-bit grayscale <see cref="Bitmap"/>.</returns>
        public static Bitmap ConvertTo8BppGrayscale(Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException("bitmap");

            var tmpBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format8bppIndexed);
            BitmapData data = tmpBitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, tmpBitmap.PixelFormat);
            int tmpStride = data.Stride;
            int tmpBytes = bitmap.Height * tmpStride;
            Color[,] tmpColor = GetColorArray(ConvertTo24BppGrayscale(bitmap));
            ColorPalette colPal = tmpBitmap.Palette;
            var grayColors = new Hashtable();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (grayColors.ContainsKey(tmpColor[x, y]))
                    {
                        var bisher = (int)(grayColors[tmpColor[x, y]]);
                        grayColors[tmpColor[x, y]] = bisher + 1;
                    }
                    else
                        grayColors[tmpColor[x, y]] = 1;
                }
            }
            int counter = 0;

            foreach (Color key in grayColors.Keys)
                colPal.Entries[counter++] = key;
            for (int i = 0; i < 256; i++)
                grayColors[colPal.Entries[i]] = i;
            var tmpBildDaten = new byte[tmpBytes];
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                    tmpBildDaten[(y * tmpStride) + x] = (byte)(int)grayColors[tmpColor[x, y]];
            }
            tmpBitmap.Palette = colPal;
            IntPtr ptr = data.Scan0;
            Marshal.Copy(tmpBildDaten, 0, ptr, tmpBytes);
            tmpBitmap.UnlockBits(data);
            return tmpBitmap;
        }
        #endregion
    }
}
