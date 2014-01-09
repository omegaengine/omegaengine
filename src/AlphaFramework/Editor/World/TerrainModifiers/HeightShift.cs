/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using AlphaFramework.World.Terrains;
using Common.Utils;

namespace AlphaFramework.Editor.World.TerrainModifiers
{
    /// <summary> 
    /// Interactivley raises or lowers all points of an area on a <see cref="ITerrain"/>.
    /// </summary>
    public sealed class HeightShift : Height
    {
        /// <summary>The value by which the terrain height is to be shifted.</summary>
        private readonly short _diff;

        /// <summary>
        /// Creates a new terrain height shifter.
        /// </summary>
        /// <param name="terrain">The <see cref="ITerrain"/> to modify.</param>
        /// <param name="engineTerrain">The <see cref="OmegaEngine.Graphics.Renderables.Terrain"/> to live-update while modifying.</param>
        /// <param name="refreshHandler">Called when the presenter needs to be reset.</param>
        /// <param name="diff">The value by which the terrain height is to be shifted.</param>
        public HeightShift(ITerrain terrain, OmegaEngine.Graphics.Renderables.Terrain engineTerrain, Action refreshHandler, short diff)
            : base(terrain, engineTerrain, refreshHandler)
        {
            _diff = diff;
        }

        /// <inheritdoc/>
        protected override void ModifyTerrain(Point offset, TerrainBrush brush, byte[,] oldData, byte[,] newData)
        {
            #region Sanity checks
            if (oldData == null) throw new ArgumentNullException("oldData");
            if (newData == null) throw new ArgumentNullException("newData");
            #endregion

            var heightMap = Terrain.HeightMap;

            // Iterate through intersection of [0,area.Size) and [-offset,heightMap-offset)
            for (int x = Math.Max(0, -offset.X); x < Math.Min(brush.Size, heightMap.GetLength(0) - offset.X); x++)
            {
                for (int y = Math.Max(0, -offset.Y); y < Math.Min(brush.Size, heightMap.GetLength(1) - offset.Y); y++)
                {
                    oldData[x, y] = heightMap[offset.X + x, offset.Y + y];

                    // Shift height and write back
                    newData[x, y] = heightMap[offset.X + x, offset.Y + y] =
                        (byte)(oldData[x, y] + (_diff * brush.Factor(x, y))).Clamp(byte.MinValue, byte.MaxValue);
                }
            }
        }
    }
}
