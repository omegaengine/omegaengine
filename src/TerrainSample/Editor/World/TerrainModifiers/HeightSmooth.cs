/*
 * Copyright 2006-2012 Bastian Eicher
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
using Common;
using Common.Utils;
using Presentation;
using World;
using EngineTerrain = OmegaEngine.Graphics.Renderables.Terrain;

namespace AlphaEditor.World.TerrainModifiers
{
    /// <summary>
    /// Interactivley smoothes a <see cref="Terrain"/> area using a Gaussian filter.
    /// </summary>
    internal sealed class HeightSmooth : Height
    {
        /// <summary>The smoothing filter kernel.</summary>
        private readonly double[] _kernel;

        /// <summary>
        /// Creates a new terrain height smoother.
        /// </summary>
        /// <param name="terrain">The <see cref="Terrain"/> to modify.</param>
        /// <param name="engineTerrain">The <see cref="EngineTerrain"/> to live-update while modifying.</param>
        /// <param name="refreshHandler">Called when the <see cref="Presenter"/> needs to be reset.</param>
        /// <param name="sigma">The standard deviation of the Gaussian distribution.</param>
        public HeightSmooth(Terrain terrain, EngineTerrain engineTerrain, SimpleEventHandler refreshHandler, double sigma)
            : base(terrain, engineTerrain, refreshHandler)
        {
            _kernel = MathUtils.GaussKernel(sigma, Math.Max(3, (int)(6 * sigma) - 1));
        }

        /// <inheritdoc/>
        protected override void ModifyTerrain(Point offset, TerrainBrush brush, byte[,] oldData, byte[,] newData)
        {
            var heightMap = Terrain.HeightMap;

            // Iterate through intersection of [0,area.Size) and [-offset,heightMap-offset
            int xMin = Math.Max(0, -offset.X);
            int xMax = Math.Min(brush.Size, heightMap.GetLength(0) - offset.X);
            int yMin = Math.Max(0, -offset.Y);
            int yMax = Math.Min(brush.Size, heightMap.GetLength(1) - offset.Y);

            // Use separated Gaussian filter with additional write-back between steps
            // Additional write-back is required because data points outside of the modified area may be queried

            // ReSharper disable AccessToModifiedClosure
            for (int x = xMin; x < xMax; x++)
            {
                for (int y = yMin; y < yMax; y++)
                {
                    // Filter horizontal
                    double factor = brush.Factor(x, y);
                    double filteredValue = GetFiltered(i => ClampedRead(heightMap, offset.X + x + i, offset.Y + y));
                    byte previousValue = heightMap[offset.X + x, offset.Y + y];
                    newData[x, y] = (byte)(factor * filteredValue + (1 - factor) * previousValue);
                }
            }

            for (int x = xMin; x < xMax; x++)
            {
                for (int y = yMin; y < yMax; y++)
                {
                    oldData[x, y] = heightMap[offset.X + x, offset.Y + y];

                    // Write back
                    heightMap[offset.X + x, offset.Y + y] = newData[x, y];
                }
            }

            for (int x = xMin; x < xMax; x++)
            {
                for (int y = yMin; y < yMax; y++)
                {
                    // Filter vertical
                    double factor = brush.Factor(x, y);
                    double filteredValue = GetFiltered(i => ClampedRead(heightMap, offset.X + x, offset.Y + y + i));
                    byte previousValue = heightMap[offset.X + x, offset.Y + y];
                    newData[x, y] = (byte)(factor * filteredValue + (1 - factor) * previousValue);
                }
            }

            for (int x = xMin; x < xMax; x++)
            {
                for (int y = yMin; y < yMax; y++)
                {
                    // Write back
                    heightMap[offset.X + x, offset.Y + y] = newData[x, y];
                }
            }
            // ReSharper restore AccessToModifiedClosure
        }

        #region Helpers
        /// <summary>
        /// Reads a value from a 2D array and clamps to the nearest border if the index is out of bounds.
        /// </summary>
        private static T ClampedRead<T>(T[,] array, int x, int y)
        {
            return array[MathUtils.Clamp(x, 0, array.GetLength(0) - 1), MathUtils.Clamp(y, 0, array.GetLength(1) - 1)];
        }

        /// <summary>
        /// Retreives a single filtered value.
        /// </summary>
        /// <param name="getValue">Called to retreive surrounding base values with relative indexes.</param>
        private double GetFiltered(MapAction<int, byte> getValue)
        {
            double result = 0;
            for (int i = 0; i < _kernel.Length; i++)
                result += _kernel[i] * getValue(i - _kernel.Length / 2);
            return result;
        }
        #endregion
    }
}
