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
using Presentation;
using SlimDX;
using World;
using EngineTerrain = OmegaEngine.Graphics.Renderables.Terrain;

namespace AlphaEditor.World.TerrainModifiers
{
    /// <summary>
    /// Abstract base class for interactivley changing the height of an area on a <see cref="Terrain"/>.
    /// </summary>
    internal abstract class Height : Base
    {
        /// <summary>The <see cref="EngineTerrain"/> to live-update while modifying.</summary>
        private readonly EngineTerrain _engineTerrain;

        /// <summary>Called when the <see cref="Presenter"/> needs to be reset.</summary>
        private readonly Action _refreshHandler;

        /// <summary>
        /// Creates a new terrain height modifier.
        /// </summary>
        /// <param name="terrain">The <see cref="Terrain"/> to modify.</param>
        /// <param name="engineTerrain">The <see cref="OmegaEngine.Graphics.Renderables.Terrain"/> to live-update while modifying.</param>
        /// <param name="refreshHandler">Called when the <see cref="Presenter"/> needs to be reset.</param>
        protected Height(Terrain terrain, EngineTerrain engineTerrain, Action refreshHandler) : base(terrain)
        {
            #region Sanity checks
            if (engineTerrain == null) throw new ArgumentNullException("engineTerrain");
            if (refreshHandler == null) throw new ArgumentNullException("refreshHandler");
            #endregion

            _engineTerrain = engineTerrain;
            _refreshHandler = refreshHandler;
        }

        /// <inheritdoc/>
        public override void Apply(Vector2 terrainCoords, TerrainBrush brush)
        {
            // Calculate top-left point of area to modify
            var offset = new Point(
                (int)Math.Round(terrainCoords.X / Terrain.Size.StretchH) - brush.Size / 2,
                (int)Math.Round(terrainCoords.Y / Terrain.Size.StretchH) - brush.Size / 2);

            var oldData = new byte[brush.Size, brush.Size];
            var newData = new byte[brush.Size, brush.Size];
            ModifyTerrain(offset, brush, oldData, newData);

            OldData.AddFirst(offset, oldData);
            NewData.AddLast(offset, newData);
            _engineTerrain.ModifyHeight(offset, newData); // Live-update engine terrain
        }

        /// <summary>
        /// Modifies the <see cref="Terrain"/> and fills arrays with undo/redo data.
        /// </summary>
        /// <param name="offset">The <see cref="Terrain.HeightMap"/> index that corresponds to the top-left corner of the area to modify.</param>
        /// <param name="brush">The shape and size of the area to the lower-right of <paramref name="offset"/> to modify.</param>
        /// <param name="oldData">An array to be filled with data from <see cref="Terrain.HeightMap"/> before the modification. Both dimensions must be equal to <see cref="TerrainBrush.Size"/>.</param>
        /// <param name="newData">An array to be filled with data from <see cref="Terrain.HeightMap"/> after the modification. Both dimensions must be equal to <see cref="TerrainBrush.Size"/>.</param>
        protected abstract void ModifyTerrain(Point offset, TerrainBrush brush, byte[,] oldData, byte[,] newData);

        /// <inheritdoc/>
        public override IUndoCommand GetCommand()
        {
            return new Commands.ModifyHeightMap(Terrain,
                NewData.TotalArea.Location, OldData.GetArray(Terrain.HeightMap), NewData.GetArray(Terrain.HeightMap),
                _refreshHandler);
        }
    }
}
