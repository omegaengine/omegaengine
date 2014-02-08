/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using AlphaFramework.Editor.World.Commands;
using AlphaFramework.World.Terrains;
using Common.Undo;
using SlimDX;

namespace AlphaFramework.Editor.World.TerrainModifiers
{
    /// <summary>
    /// Interactivley changes the texture ID of an area on a <see cref="ITerrain"/>.
    /// </summary>
    public sealed class Texture : Base
    {
        /// <summary>Called when the <see cref="OmegaEngine.Graphics.Renderables.Terrain"/> needs to be reset.</summary>
        private readonly Action _refreshHandler;

        /// <summary>The new texture ID to set.</summary>
        private readonly byte _textureID;

        /// <summary>
        /// Creates a new texture index modifier.
        /// </summary>
        /// <param name="terrain">The <see cref="ITerrain"/> to modify.</param>
        /// <param name="refreshHandler">Called when the <see cref="OmegaEngine.Graphics.Renderables.Terrain"/> needs to be reset.</param>
        /// <param name="textureID">The new texture ID to set.</param>
        public Texture(ITerrain terrain, Action refreshHandler, byte textureID)
            : base(terrain)
        {
            #region Sanity checks
            if (refreshHandler == null) throw new ArgumentNullException("refreshHandler");
            #endregion

            _textureID = textureID;
            _refreshHandler = refreshHandler;
        }

        /// <inheritdoc/>
        public override void Apply(Vector2 terrainCoords, TerrainBrush brush)
        {
            // Handle texture 3x scale and calculate top-left point of area to modify
            brush = new TerrainBrush(brush.Size / 3, brush.Circle);
            var offset = new Point(
                (int)Math.Round(terrainCoords.X / Terrain.Size.StretchH / 3) - brush.Size / 2,
                (int)Math.Round(terrainCoords.Y / Terrain.Size.StretchH / 3) - brush.Size / 2);

            var oldData = new byte[brush.Size, brush.Size];
            var newData = new byte[brush.Size, brush.Size];
            if (!ModifyTerrain(offset, brush, oldData, newData)) return;

            OldData.AddFirst(offset, oldData);
            NewData.AddLast(offset, newData);
            _refreshHandler(); // Live-rebuild engine terrain
        }

        /// <summary>
        /// Modifies the <see cref="ITerrain"/> and fills arrays with undo/redo data.
        /// </summary>
        /// <param name="offset">The <see cref="ITerrain.TextureMap"/> index that corresponds to the top-left corner of the area to modify.</param>
        /// <param name="brush">The shape and size of the area to the lower-right of <paramref name="offset"/> to modify.</param>
        /// <param name="oldData">An array to be filled with data from <see cref="ITerrain.TextureMap"/> before the modification. Both dimensions must be equal to <see cref="TerrainBrush.Size"/>.</param>
        /// <param name="newData">An array to be filled with data from <see cref="ITerrain.TextureMap"/> after the modification. Both dimensions must be equal to <see cref="TerrainBrush.Size"/>.</param>
        /// <returns><see langword="true"/> if anything was changed; <see langword="false"/> if the <see cref="ITerrain"/> remains unchanged (because no changes were necessary).</returns>
        private bool ModifyTerrain(Point offset, TerrainBrush brush, byte[,] oldData, byte[,] newData)
        {
            bool changed = false;
            var textureMap = Terrain.TextureMap;

            // Iterate through intersection of [0,area.Size) and [-offset,textureMap-offset)
            for (int x = Math.Max(0, -offset.X); x < Math.Min(brush.Size, textureMap.Width - offset.X); x++)
            {
                for (int y = Math.Max(0, -offset.Y); y < Math.Min(brush.Size, textureMap.Height - offset.Y); y++)
                {
                    oldData[x, y] = textureMap[offset.X + x, offset.Y + y];

                    // Change texture ID and write back
                    if (brush.Contains(x, y))
                    {
                        newData[x, y] = _textureID;
                        if (newData[x, y] != oldData[x, y])
                        {
                            textureMap[offset.X + x, offset.Y + y] = newData[x, y];
                            changed = true;
                        }
                    }
                    else newData[x, y] = oldData[x, y];
                }
            }
            return changed;
        }

        /// <inheritdoc/>
        public override IUndoCommand GetCommand()
        {
            return new ModifyTextureMap(Terrain,
                NewData.TotalArea.Location, OldData.GetArray(Terrain.TextureMap), NewData.GetArray(Terrain.TextureMap),
                _refreshHandler);
        }
    }
}
