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
using Common.Undo;
using SlimDX;
using TerrainSample.Presentation;
using TerrainSample.World.Terrains;

namespace TerrainSample.Editor.World.TerrainModifiers
{
    /// <summary>
    /// Interactivley changes the texture ID of an area on a <see cref="Terrain"/>.
    /// </summary>
    internal sealed class Texture : Base
    {
        /// <summary>Called when the <see cref="OmegaEngine.Graphics.Renderables.Terrain"/> needs to be reset.</summary>
        private readonly Action _refreshHandler;

        /// <summary>The new texture ID to set.</summary>
        private readonly byte _textureID;

        /// <summary>
        /// Creates a new texture index modifier.
        /// </summary>
        /// <param name="terrain">The <see cref="Terrain"/> to modify.</param>
        /// <param name="refreshHandler">Called when the <see cref="OmegaEngine.Graphics.Renderables.Terrain"/> needs to be reset.</param>
        /// <param name="textureID">The new texture ID to set.</param>
        public Texture(Terrain terrain, Action refreshHandler, byte textureID) : base(terrain)
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
        /// Modifies the <see cref="Terrain"/> and fills arrays with undo/redo data.
        /// </summary>
        /// <param name="offset">The <see cref="Terrain.TextureMap"/> index that corresponds to the top-left corner of the area to modify.</param>
        /// <param name="brush">The shape and size of the area to the lower-right of <paramref name="offset"/> to modify.</param>
        /// <param name="oldData">An array to be filled with data from <see cref="Terrain.TextureMap"/> before the modification. Both dimensions must be equal to <see cref="TerrainBrush.Size"/>.</param>
        /// <param name="newData">An array to be filled with data from <see cref="Terrain.TextureMap"/> after the modification. Both dimensions must be equal to <see cref="TerrainBrush.Size"/>.</param>
        /// <returns><see langword="true"/> if anything was changed; <see langword="false"/> if the <see cref="Terrain"/> remains unchanged (because no changes were necessary).</returns>
        private bool ModifyTerrain(Point offset, TerrainBrush brush, byte[,] oldData, byte[,] newData)
        {
            bool changed = false;
            var textureMap = Terrain.TextureMap;

            // Iterate through intersection of [0,area.Size) and [-offset,textureMap-offset)
            for (int x = Math.Max(0, -offset.X); x < Math.Min(brush.Size, textureMap.GetLength(0) - offset.X); x++)
            {
                for (int y = Math.Max(0, -offset.Y); y < Math.Min(brush.Size, textureMap.GetLength(1) - offset.Y); y++)
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
            return new Commands.ModifyTextureMap(Terrain,
                NewData.TotalArea.Location, OldData.GetArray(Terrain.TextureMap), NewData.GetArray(Terrain.TextureMap),
                _refreshHandler);
        }
    }
}
