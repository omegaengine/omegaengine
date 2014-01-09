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
using OmegaEngine.Graphics.Renderables;
using SlimDX;

namespace AlphaFramework.Editor.World.TerrainModifiers
{
    /// <summary>
    /// Abstract base class for interactivley changing the height of an area on a <see cref="ITerrain"/>.
    /// </summary>
    public abstract class Height : Base
    {
        /// <summary>The <see cref="Terrain"/> to live-update while modifying.</summary>
        private readonly Terrain _engineTerrain;

        /// <summary>Called when the presenter needs to be reset.</summary>
        private readonly Action _refreshHandler;

        /// <summary>
        /// Creates a new terrain height modifier.
        /// </summary>
        /// <param name="terrain">The <see cref="ITerrain"/> to modify.</param>
        /// <param name="engineTerrain">The <see cref="OmegaEngine.Graphics.Renderables.Terrain"/> to live-update while modifying.</param>
        /// <param name="refreshHandler">Called when the presenter needs to be reset.</param>
        protected Height(ITerrain terrain, Terrain engineTerrain, Action refreshHandler)
            : base(terrain)
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
        /// Modifies the <see cref="ITerrain"/> and fills arrays with undo/redo data.
        /// </summary>
        /// <param name="offset">The <see cref="ITerrain.HeightMap"/> index that corresponds to the top-left corner of the area to modify.</param>
        /// <param name="brush">The shape and size of the area to the lower-right of <paramref name="offset"/> to modify.</param>
        /// <param name="oldData">An array to be filled with data from <see cref="ITerrain.HeightMap"/> before the modification. Both dimensions must be equal to <see cref="TerrainBrush.Size"/>.</param>
        /// <param name="newData">An array to be filled with data from <see cref="ITerrain.HeightMap"/> after the modification. Both dimensions must be equal to <see cref="TerrainBrush.Size"/>.</param>
        protected abstract void ModifyTerrain(Point offset, TerrainBrush brush, byte[,] oldData, byte[,] newData);

        /// <inheritdoc/>
        public override IUndoCommand GetCommand()
        {
            return new ModifyHeightMap(Terrain,
                NewData.TotalArea.Location, OldData.GetArray(Terrain.HeightMap), NewData.GetArray(Terrain.HeightMap),
                _refreshHandler);
        }
    }
}
