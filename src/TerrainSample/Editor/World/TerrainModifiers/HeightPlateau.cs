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
using Common;
using Presentation;
using World;
using EngineTerrain = OmegaEngine.Graphics.Renderables.Terrain;

namespace AlphaEditor.World.TerrainModifiers
{
    /// <summary>
    /// Interactivley turns a <see cref="Terrain"/> area into a plateau (all points have the same height).
    /// </summary>
    internal sealed class HeightPlateau : Height
    {
        /// <summary>
        /// Creates a new terrain plateau creator.
        /// </summary>
        /// <param name="terrain">The <see cref="Terrain"/> to modify.</param>
        /// <param name="engineTerrain">The <see cref="EngineTerrain"/> to live-update while modifying.</param>
        /// <param name="refreshHandler">Called when the <see cref="Presenter"/> needs to be reset.</param>
        public HeightPlateau(Terrain terrain, EngineTerrain engineTerrain, Action refreshHandler)
            : base(terrain, engineTerrain, refreshHandler + (() => terrain.LightAngleMapsOutdated = true)) // Mark the shadow maps for update
        {}

        /// <inheritdoc/>
        protected override void ModifyTerrain(Point offset, TerrainBrush brush, byte[,] oldData, byte[,] newData)
        {
            var heightMap = Terrain.HeightMap;
            byte centerHeight = heightMap[offset.X + brush.Size / 2, offset.Y + brush.Size / 2];

            // Iterate through intersection of [0,area.Size) and [-offset,heightMap-offset)
            for (int x = Math.Max(0, -offset.X); x < Math.Min(brush.Size, heightMap.GetLength(0) - offset.X); x++)
            {
                for (int y = Math.Max(0, -offset.Y); y < Math.Min(brush.Size, heightMap.GetLength(1) - offset.Y); y++)
                {
                    oldData[x, y] = heightMap[offset.X + x, offset.Y + y];

                    // Make flat and write back
                    double factor = brush.Factor(x, y);
                    newData[x, y] = heightMap[offset.X + x, offset.Y + y] =
                        (byte)(factor * centerHeight + (1 - factor) * oldData[x, y]);
                }
            }
        }
    }
}
