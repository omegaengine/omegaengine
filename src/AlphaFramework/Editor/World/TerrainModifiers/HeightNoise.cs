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
using OmegaEngine;

namespace AlphaFramework.Editor.World.TerrainModifiers
{
    /// <summary>
    /// Interactivley adds height noise to a <see cref="ITerrain"/>.
    /// </summary>
    public sealed class HeightNoise : Height
    {
        /// <summary>The maximum amplitude of the noise to generate.</summary>
        private readonly double _amplitude;

        /// <summary>The frequency of the noise to generate.</summary>
        private readonly double _frequency;

        /// <summary>
        /// Creates a new terrain height noise generator.
        /// </summary>
        /// <param name="terrain">The <see cref="ITerrain"/> to modify.</param>
        /// <param name="engineTerrain">The <see cref="OmegaEngine.Graphics.Renderables.Terrain"/> to live-update while modifying.</param>
        /// <param name="refreshHandler">Called when the presenter needs to be reset.</param>
        /// <param name="amplitude">The maximum amplitude of the noise to generate.</param>
        /// <param name="frequency">The frequency of the noise to generate.</param>
        public HeightNoise(ITerrain terrain, OmegaEngine.Graphics.Renderables.Terrain engineTerrain, Action refreshHandler, double amplitude, double frequency)
            : base(terrain, engineTerrain, refreshHandler)
        {
            _amplitude = amplitude;
            _frequency = frequency;
        }

        /// <inheritdoc/>
        protected override void ModifyTerrain(Point offset, TerrainBrush brush, byte[,] oldData, byte[,] newData)
        {
            #region Sanity checks
            if (oldData == null) throw new ArgumentNullException("oldData");
            if (newData == null) throw new ArgumentNullException("newData");
            #endregion

            var noise = new PerlinNoise {InitAmplitude = _amplitude, InitFrequency = _frequency};
            var heightMap = Terrain.HeightMap;

            // Iterate through intersection of [0,area.Size) and [-offset,heightMap-offset)
            for (int x = Math.Max(0, -offset.X); x < Math.Min(brush.Size, heightMap.Width - offset.X); x++)
            {
                for (int y = Math.Max(0, -offset.Y); y < Math.Min(brush.Size, heightMap.Height - offset.Y); y++)
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
