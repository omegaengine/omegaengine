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
using AlphaEditor.Properties;
using Common;
using Common.Undo;
using World;

namespace AlphaEditor.World.Commands
{
    /// <summary>
    /// Modifies texture-map data in a <see cref="Terrain"/>.
    /// </summary>
    internal class ModifyTextureMap : PreExecutedCommand
    {
        #region Variables
        private readonly Terrain _terrain;
        private readonly Point _start;
        private readonly byte[,] _oldPartialData, _newPartialData;
        private readonly Action _refreshHandler;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command for modifying a rectangular area of the texture-map in a <see cref="Terrain"/>.
        /// </summary>
        /// <param name="terrain">The <see cref="Terrain"/> to modify.</param>
        /// <param name="offset">The top-left coordinates of the area to modify.</param>
        /// <param name="oldPartialData">The texture-map data of the area before it was modified. Do not modify this array after calling this method!</param>
        /// <param name="newPartialData">The texture-map data of the area after it was modified. Do not modify this array after calling this method!</param>
        /// <param name="refreshHandler">Called when the <see cref="OmegaEngine.Graphics.Renderables.Terrain"/> needs to be reset.</param>
        public ModifyTextureMap(Terrain terrain, Point offset, byte[,] oldPartialData, byte[,] newPartialData, Action refreshHandler)
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
        /// <summary>
        /// Applies the <see cref="_newPartialData"/> to <see cref="Terrain.TextureMap"/>.
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
        /// Applies the <see cref="_oldPartialData"/> to <see cref="Terrain.TextureMap"/>.
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
