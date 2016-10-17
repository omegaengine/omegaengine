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
using NanoByte.Common.Undo;

namespace AlphaFramework.Editor.World.Commands
{
    /// <summary>
    /// Modifies texture-map data in a <see cref="ITerrain"/>.
    /// </summary>
    public class ModifyTextureMap : PreExecutedCommand
    {
        #region Variables
        private readonly ITerrain _terrain;
        private readonly Point _start;
        private readonly byte[,] _oldPartialData, _newPartialData;
        private readonly Action _refreshHandler;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command for modifying a rectangular area of the texture-map in a <see cref="ITerrain"/>.
        /// </summary>
        /// <param name="terrain">The <see cref="ITerrain"/> to modify.</param>
        /// <param name="offset">The top-left coordinates of the area to modify.</param>
        /// <param name="oldPartialData">The texture-map data of the area before it was modified. Do not modify this array after calling this method!</param>
        /// <param name="newPartialData">The texture-map data of the area after it was modified. Do not modify this array after calling this method!</param>
        /// <param name="refreshHandler">Called when the <see cref="OmegaEngine.Graphics.Renderables.Terrain"/> needs to be reset.</param>
        public ModifyTextureMap(ITerrain terrain, Point offset, byte[,] oldPartialData, byte[,] newPartialData, Action refreshHandler)
        {
            #region Sanity checks
            if (terrain == null) throw new ArgumentNullException(nameof(terrain));
            if (oldPartialData == null) throw new ArgumentNullException(nameof(oldPartialData));
            if (newPartialData == null) throw new ArgumentNullException(nameof(newPartialData));
            if (refreshHandler == null) throw new ArgumentNullException(nameof(refreshHandler));
            if (oldPartialData.GetLength(0) != newPartialData.GetLength(0) || oldPartialData.GetLength(1) != newPartialData.GetLength(1)) throw new ArgumentException(Resources.PartialDataArrayDimensionsNotEqual, nameof(newPartialData));
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
        /// <summary>
        /// Applies the <see cref="_newPartialData"/> to <see cref="ITerrain.TextureMap"/>.
        /// </summary>
        protected override void OnRedo()
        {
            for (int x = 0; x < _newPartialData.GetLength(0); x++)
            {
                for (int y = 0; y < _newPartialData.GetLength(1); y++)
                    _terrain.TextureMap[_start.X + x, _start.Y + y] = _newPartialData[x, y];
            }
            _refreshHandler();
        }

        /// <summary>
        /// Applies the <see cref="_oldPartialData"/> to <see cref="ITerrain.TextureMap"/>.
        /// </summary>
        protected override void OnUndo()
        {
            for (int x = 0; x < _oldPartialData.GetLength(0); x++)
            {
                for (int y = 0; y < _oldPartialData.GetLength(1); y++)
                    _terrain.TextureMap[_start.X + x, _start.Y + y] = _oldPartialData[x, y];
            }
            _refreshHandler();
        }
        #endregion
    }
}
