/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using AlphaFramework.World.Terrains;
using NanoByte.Common;

namespace AlphaFramework.Editor.World.TerrainModifiers
{
    /// <summary>
    /// Interactivley smoothes a <see cref="ITerrain"/> area using a Gaussian filter.
    /// </summary>
    public sealed class HeightSmooth : Height
    {
        /// <summary>The smoothing filter kernel.</summary>
        private readonly double[] _kernel;

        /// <summary>
        /// Creates a new terrain height smoother.
        /// </summary>
        /// <param name="terrain">The <see cref="ITerrain"/> to modify.</param>
        /// <param name="engineTerrain">The <see cref="OmegaEngine.Graphics.Renderables.Terrain"/> to live-update while modifying.</param>
        /// <param name="refreshHandler">Called when the presenter needs to be reset.</param>
        /// <param name="sigma">The standard deviation of the Gaussian distribution.</param>
        public HeightSmooth(ITerrain terrain, OmegaEngine.Graphics.Renderables.Terrain engineTerrain, Action refreshHandler, double sigma)
            : base(terrain, engineTerrain, refreshHandler)
        {
            _kernel = MathUtils.GaussKernel(sigma, Math.Max(3, (int)(6 * sigma) - 1));
        }

        /// <inheritdoc/>
        protected override void ModifyTerrain(Point offset, TerrainBrush brush, byte[,] oldData, byte[,] newData)
        {
            #region Sanity checks
            if (oldData == null) throw new ArgumentNullException("oldData");
            if (newData == null) throw new ArgumentNullException("newData");
            #endregion

            var heightMap = Terrain.HeightMap;

            // Iterate through intersection of [0,area.Size) and [-offset,heightMap-offset
            int xMin = Math.Max(0, -offset.X);
            int xMax = Math.Min(brush.Size, heightMap.Width - offset.X);
            int yMin = Math.Max(0, -offset.Y);
            int yMax = Math.Min(brush.Size, heightMap.Height - offset.Y);

            // Use separated Gaussian filter with additional write-back between steps
            // Additional write-back is required because data points outside of the modified area may be queried

            // ReSharper disable AccessToModifiedClosure
            for (int x = xMin; x < xMax; x++)
            {
                for (int y = yMin; y < yMax; y++)
                {
                    // Filter horizontal
                    double factor = brush.Factor(x, y);
                    double filteredValue = GetFiltered(i => heightMap.ClampedRead(offset.X + x + i, offset.Y + y));
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
                    double filteredValue = GetFiltered(i => heightMap.ClampedRead(offset.X + x, offset.Y + y + i));
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
        /// Retreives a single filtered value.
        /// </summary>
        /// <param name="getValue">Called to retreive surrounding base values with relative indexes.</param>
        private double GetFiltered(Func<int, byte> getValue)
        {
            double result = 0;
            // ReSharper disable LoopCanBeConvertedToQuery
            for (int i = 0; i < _kernel.Length; i++)
                result += _kernel[i] * getValue(i - _kernel.Length / 2);
            return result;
            // ReSharper restore LoopCanBeConvertedToQuery
        }
        #endregion
    }
}
