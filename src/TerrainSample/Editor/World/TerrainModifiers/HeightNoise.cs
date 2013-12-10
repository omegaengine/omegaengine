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
using Common.Utils;
using OmegaEngine;
using TerrainSample.Presentation;
using TerrainSample.World.Terrains;

namespace TerrainSample.Editor.World.TerrainModifiers
{
    /// <summary>
    /// Interactivley adds height noise to a <see cref="Terrain"/>.
    /// </summary>
    internal sealed class HeightNoise : Height
    {
        /// <summary>The maximum amplitude of the noise to generate.</summary>
        private readonly double _amplitude;

        /// <summary>The frequency of the noise to generate.</summary>
        private readonly double _frequency;

        /// <summary>
        /// Creates a new terrain height noise generator.
        /// </summary>
        /// <param name="terrain">The <see cref="Terrain"/> to modify.</param>
        /// <param name="engineTerrain">The <see cref="OmegaEngine.Graphics.Renderables.Terrain"/> to live-update while modifying.</param>
        /// <param name="refreshHandler">Called when the <see cref="Presenter"/> needs to be reset.</param>
        /// <param name="amplitude">The maximum amplitude of the noise to generate.</param>
        /// <param name="frequency">The frequency of the noise to generate.</param>
        public HeightNoise(Terrain terrain, OmegaEngine.Graphics.Renderables.Terrain engineTerrain, Action refreshHandler, double amplitude, double frequency)
            : base(terrain, engineTerrain, refreshHandler)
        {
            _amplitude = amplitude;
            _frequency = frequency;
        }

        /// <inheritdoc/>
        protected override void ModifyTerrain(Point offset, TerrainBrush brush, byte[,] oldData, byte[,] newData)
        {
            var noise = new PerlinNoise {InitAmplitude = _amplitude, InitFrequency = _frequency};
            var heightMap = Terrain.HeightMap;

            // Iterate through intersection of [0,area.Size) and [-offset,heightMap-offset)
            for (int x = Math.Max(0, -offset.X); x < Math.Min(brush.Size, heightMap.GetLength(0) - offset.X); x++)
            {
                for (int y = Math.Max(0, -offset.Y); y < Math.Min(brush.Size, heightMap.GetLength(1) - offset.Y); y++)
                {
                    oldData[x, y] = heightMap[offset.X + x, offset.Y + y];

                    // Add noise and write back
                    newData[x, y] = heightMap[offset.X + x, offset.Y + y] =
                        (byte)(oldData[x, y] + (noise.Function2D(x, y) * brush.Factor(x, y))).Clamp(byte.MinValue, byte.MaxValue);
                }
            }
        }
    }
}
