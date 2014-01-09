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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using AlphaFramework.World.Properties;
using Common;
using Common.Utils;
using LuaInterface;

namespace AlphaFramework.World.Terrains
{
    partial class Terrain<TTemplate>
    {
        #region Load bitmap helper
        /// <summary>
        /// Parses the content of a <see cref="Stream"/> as a <see cref="Bitmap"/> an writes it into a 2D-array.
        /// </summary>
        /// <param name="targetMap">The array to write the bitmap to.</param>
        /// <param name="size">The expected size of the bitmap.</param>
        /// <param name="stream">The stream to read the bitmap from.</param>
        /// <param name="sixteenColors">If <see langword="true"/>, use <see cref="ColorUtils.SixteenColors(Color)"/>;
        /// if <see langword="false"/>, handle as grayscale</param>
        /// <exception cref="IOException">Thrown if the bitmap size does not match <paramref name="size"/>.</exception>
        private static void LoadBitmap(Stream stream, Size size, ref byte[,] targetMap, bool sixteenColors)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            try
            {
                using (var image = Image.FromStream(stream))
                using (var mapBitmap = new Bitmap(image))
                {
                    #region Sanity checks
                    if (mapBitmap.Size != size) throw new IOException(Resources.InvalidMapSize);
                    #endregion

                    // Copy Bitmap to Color-array for easier access
                    Color[,] pixels = BitmapUtils.GetColorArray(mapBitmap);

                    // Transfer data from Color-array to byte-array
                    targetMap = new byte[size.Width, size.Height];
                    for (int x = 0; x < targetMap.GetLength(0); x++)
                    {
                        for (int y = 0; y < targetMap.GetLength(1); y++)
                        {
                            if (sixteenColors)
                            {
                                // Associate the color with an index in the mapping table
                                targetMap[x, y] = ColorUtils.SixteenColors(pixels[x, y]);
                            }
                            else
                            {
                                // Get the value from the first color channel (all the same in grayscale)
                                targetMap[x, y] = pixels[x, y].R;
                            }
                        }
                    }
                }
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                throw new IOException(Resources.NotAnImage, ex);
            }
            #endregion
        }
        #endregion

        #region Load specific maps
        /// <inheritdoc/>
        [LuaHide]
        public void LoadHeightMap(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            Log.Info("Loading terrain height-map");
            try
            {
                LoadBitmap(stream, _size, ref _heightMap, false);
            }
            catch (IOException ex)
            {
                throw new IOException(Resources.HeightMapSizeEqualTerrain, ex);
            }
        }

        /// <inheritdoc/>
        public void LoadHeightMap(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var stream = File.Open(path, FileMode.Open))
                LoadHeightMap(stream);
        }

        /// <inheritdoc/>
        [LuaHide]
        public void LoadLightRiseAngleMap(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            Log.Info("Loading terrain light rise angle-map");
            try
            {
                LoadBitmap(stream, _size, ref _lightRiseAngleMap, false);
            }
            catch (IOException ex)
            {
                throw new IOException(Resources.AngleMapSizeEqualTerrain, ex);
            }
        }

        /// <inheritdoc/>
        public void LoadLightRiseAngleMap(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var stream = File.Open(path, FileMode.Open))
                LoadLightRiseAngleMap(stream);
        }

        /// <inheritdoc/>
        [LuaHide]
        public void LoadLightSetAngleMap(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            Log.Info("Loading terrain light set angle-map");
            try
            {
                LoadBitmap(stream, _size, ref _lightSetAngleMap, false);
            }
            catch (IOException ex)
            {
                throw new IOException(Resources.AngleMapSizeEqualTerrain, ex);
            }
        }

        /// <inheritdoc/>
        public void LoadLightSetAngleMap(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var stream = File.Open(path, FileMode.Open))
                LoadLightSetAngleMap(stream);
        }

        /// <inheritdoc/>
        [LuaHide]
        public void LoadTextureMap(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            Log.Info("Loading terrain texture-map");
            try
            {
                LoadBitmap(stream, new Size(_size.X / 3, _size.Y / 3), ref _textureMap, true);
            }
            catch (IOException ex)
            {
                throw new IOException(Resources.TextureMapSizeThirdOfTerrain, ex);
            }
        }

        /// <inheritdoc/>
        public void LoadTextureMap(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var stream = File.Open(path, FileMode.Open))
                LoadTextureMap(stream);
        }
        #endregion

        //--------------------//

        #region Save bitmap helper
        /// <summary>
        /// Prepares the content of a 2D-array as a <see cref="Bitmap"/> so that it can be stored in a <see cref="Stream"/> later on.
        /// </summary>
        /// <param name="sourceMap">The array to be stores.</param>
        /// <param name="sixteenColors">If <see langword="true"/>, use <see cref="ColorUtils.SixteenColors(byte)"/>;
        /// if <see langword="false"/>, handle as grayscale</param>
        /// <returns>A delegate to be called when the <see cref="Stream"/> for writing the bitmap is ready.</returns>
        private static Action<Stream> GetSaveBitmapDelegate(byte[,] sourceMap, bool sixteenColors = false)
        {
            // Transfer data from byte-array to Bitmap
            var saveBitmap = new Bitmap(sourceMap.GetLength(0), sourceMap.GetLength(1));
            for (int x = 0; x < sourceMap.GetLength(0); x++)
            {
                for (int y = 0; y < sourceMap.GetLength(1); y++)
                {
                    saveBitmap.SetPixel(x, y, sixteenColors
                        // Use the color defined in the legend for the texture-map
                        ? ColorUtils.SixteenColors(sourceMap[x, y])
                        // Store the same value in all three color channels to generate a grayscale image
                        : Color.FromArgb(255, sourceMap[x, y], sourceMap[x, y], sourceMap[x, y]));
                }
            }

            if (!sixteenColors)
            {
                // Convert to grayscale and dispose old bitmap
                Bitmap oldBitmap = saveBitmap;
                saveBitmap = BitmapUtils.ConvertTo8BppGrayscale(oldBitmap);
                oldBitmap.Dispose();
            }

            // Callback for actually storing the data
            return delegate(Stream stream)
            {
                // Use RAM buffer because writing a PNG directly to a ZIP won't work
                var memory = new MemoryStream();
                saveBitmap.Save(memory, ImageFormat.Png);
                saveBitmap.Dispose();
                memory.CopyTo(stream);
            };
        }
        #endregion

        #region Save specific maps
        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Creates a new delegate on each call")]
        [LuaHide]
        public Action<Stream> GetSaveHeightMapDelegate()
        {
            return GetSaveBitmapDelegate(_heightMap);
        }

        /// <inheritdoc/>
        public void SaveHeightMap(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            // Generate the bitmap and get a write delegate
            Action<Stream> writeHeightMap = GetSaveHeightMapDelegate();

            // Open a stream and use the write delegate to fill it
            using (var stream = File.Create(path))
                writeHeightMap(stream);
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Creates a new delegate on each call")]
        [LuaHide]
        public Action<Stream> GetSaveLightRiseAngleMapDelegate()
        {
            return GetSaveBitmapDelegate(_lightRiseAngleMap);
        }

        /// <inheritdoc/>
        public void SaveLightRiseAngleMap(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            // Generate the bitmap and get a write delegate
            Action<Stream> writeAngleMap = GetSaveLightRiseAngleMapDelegate();

            // Open a stream and use the write delegate to fill it
            using (var stream = File.Create(path))
                writeAngleMap(stream);
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Creates a new delegate on each call")]
        [LuaHide]
        public Action<Stream> GetSaveLightSetAngleMapDelegate()
        {
            return GetSaveBitmapDelegate(_lightSetAngleMap);
        }

        /// <inheritdoc/>
        public void SaveLightSetAngleMap(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            // Generate the bitmap and get a write delegate
            Action<Stream> writeAngleMap = GetSaveLightSetAngleMapDelegate();

            // Open a stream and use the write delegate to fill it
            using (var stream = File.Create(path))
                writeAngleMap(stream);
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Creates a new delegate on each call")]
        [LuaHide]
        public Action<Stream> GetSaveTextureMapDelegate()
        {
            return GetSaveBitmapDelegate(_textureMap, sixteenColors: true);
        }

        /// <inheritdoc/>
        public void SaveTextureMap(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            // Generate the bitmap and get a write delegate
            Action<Stream> writeTextureMap = GetSaveTextureMapDelegate();

            // Open a stream and use the write delegate to fill it
            using (var stream = File.Create(path))
                writeTextureMap(stream);
        }
        #endregion
    }
}
