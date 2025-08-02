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

namespace AlphaFramework.Editor.World.TerrainModifiers;

/// <summary>
/// Interactively turns a <see cref="ITerrain"/> area into a plateau (all points have the same height).
/// </summary>
public sealed class HeightPlateau : Height
{
    /// <summary>
    /// Creates a new terrain plateau creator.
    /// </summary>
    /// <param name="terrain">The <see cref="ITerrain"/> to modify.</param>
    /// <param name="engineTerrain">The <see cref="OmegaEngine.Graphics.Renderables.Terrain"/> to live-update while modifying.</param>
    /// <param name="refreshHandler">Called when the presenter needs to be reset.</param>
    public HeightPlateau(ITerrain terrain, OmegaEngine.Graphics.Renderables.Terrain engineTerrain, Action refreshHandler)
        : base(terrain, engineTerrain, refreshHandler + (() => terrain.OcclusionIntervalMapOutdated = true)) // Mark the shadow maps for update
    {}

    /// <inheritdoc/>
    protected override void ModifyTerrain(Point offset, TerrainBrush brush, byte[,] oldData, byte[,] newData)
    {
        #region Sanity checks
        if (oldData == null) throw new ArgumentNullException(nameof(oldData));
        if (newData == null) throw new ArgumentNullException(nameof(newData));
        #endregion

        var heightMap = Terrain.HeightMap;
        byte centerHeight = heightMap[offset.X + brush.Size / 2, offset.Y + brush.Size / 2];

        // Iterate through intersection of [0,area.Size) and [-offset,heightMap-offset)
        for (int x = Math.Max(0, -offset.X); x < Math.Min(brush.Size, heightMap.Width - offset.X); x++)
        {
            for (int y = Math.Max(0, -offset.Y); y < Math.Min(brush.Size, heightMap.Height - offset.Y); y++)
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
