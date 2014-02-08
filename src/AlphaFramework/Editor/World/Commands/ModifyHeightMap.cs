/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using AlphaFramework.Editor.Properties;
using AlphaFramework.World.Terrains;
using Common.Undo;

namespace AlphaFramework.Editor.World.Commands
{
    /// <summary>
    /// Modifies height-map data in a <see cref="ITerrain"/>.
    /// </summary>
    public class ModifyHeightMap : FirstExecuteCommand
    {
        #region Variables
        private readonly ITerrain _terrain;
        private readonly Point _start;
        private readonly byte[,] _oldPartialData, _newPartialData;
        private readonly Action _refreshHandler;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command for modifying a rectangular area of the height-map in a <see cref="ITerrain"/>.
        /// </summary>
        /// <param name="terrain">The <see cref="ITerrain"/> to modify.</param>
        /// <param name="offset">The top-left coordinates of the area to modify.</param>
        /// <param name="oldPartialData">The height-map data of the area before it was modified. Do not modify this array after calling this method!</param>
        /// <param name="newPartialData">The height-map data of the area after it was modified. Do not modify this array after calling this method!</param>
        /// <param name="refreshHandler">Called when the presenter needs to be reset.</param>
        public ModifyHeightMap(ITerrain terrain, Point offset, byte[,] oldPartialData, byte[,] newPartialData, Action refreshHandler)
        {
            #region Sanity checks
            if (terrain == null) throw new ArgumentNullException("terrain");
            if (oldPartialData == null) throw new ArgumentNullException("oldPartialData");
            if (newPartialData == null) throw new ArgumentNullException("newPartialData");
            if (refreshHandler == null) throw new ArgumentNullException("refreshHandler");
            if (oldPartialData.GetLength(0) != newPartialData.GetLength(0) || oldPartialData.GetLength(1) != newPartialData.GetLength(1)) throw new ArgumentException(Resources.PartialDataArrayDimensionsNotEqual, "newPartialData");
            #endregion

            _terrain = terrain;
            _start = offset;
            _oldPartialData = oldPartialData;
            _newPartialData = newPartialData;
            _refreshHandler = refreshHandler;
        }
        #endregion

        //--------------------//

        #region Undo/Redo
        protected override void OnFirstExecute()
        {
            // Terrain data has already been modified but entity positions have not been updated yet
            _refreshHandler();

            // Mark the shadow maps for update
            _terrain.OcclusionIntervalMapOutdated = true;
        }

        /// <summary>
        /// Applies the <see cref="_newPartialData"/> to <see cref="ITerrain.HeightMap"/>.
        /// </summary>
        protected override void OnRedo()
        {
            for (int x = 0; x < _newPartialData.GetLength(0); x++)
            {
                for (int y = 0; y < _newPartialData.GetLength(1); y++)
                    _terrain.HeightMap[_start.X + x, _start.Y + y] = _newPartialData[x, y];
            }

            OnFirstExecute();
        }

        /// <summary>
        /// Applies the <see cref="_oldPartialData"/> to <see cref="ITerrain.HeightMap"/>.
        /// </summary>
        protected override void OnUndo()
        {
            for (int x = 0; x < _oldPartialData.GetLength(0); x++)
            {
                for (int y = 0; y < _oldPartialData.GetLength(1); y++)
                    _terrain.HeightMap[_start.X + x, _start.Y + y] = _oldPartialData[x, y];
            }

            OnFirstExecute();
        }
        #endregion
    }
}
