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
using System.Diagnostics.CodeAnalysis;
using Common.Undo;
using TerrainSample.Presentation;
using TerrainSample.World.Terrains;

namespace TerrainSample.Editor.World.Commands
{
    /// <summary>
    /// Abstract base class for commands that load new map data into a <see cref="Terrain"/>.
    /// </summary>
    internal abstract class ImportMap : FirstExecuteCommand
    {
        #region Variables
        // ReSharper disable InconsistentNaming
        protected readonly Terrain _terrain;
        protected readonly string _fileName;
        // ReSharper restore InconsistentNaming

        private readonly Action _refreshHandler;
        private byte[,] _undoMapData, _redoMapData;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command for loading map data into a <see cref="Terrain"/>.
        /// </summary>
        /// <param name="terrain">The <see cref="Terrain"/> to load new map data into.</param>
        /// <param name="fileName">The file to load the map data from.</param>
        /// <param name="refreshHandler">Called when the <see cref="Presenter"/> needs to be reset.</param>
        protected ImportMap(Terrain terrain, string fileName, Action refreshHandler)
        {
            #region Sanity checks
            if (terrain == null) throw new ArgumentNullException("terrain");
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            if (refreshHandler == null) throw new ArgumentNullException("refreshHandler");
            #endregion

            _terrain = terrain;
            _fileName = fileName;
            _refreshHandler = refreshHandler;
        }
        #endregion

        //--------------------//

        #region Execute
        /// <summary>
        /// Imports the map data
        /// </summary>
        protected override void OnFirstExecute()
        {
            // Backup current state for undo
            _undoMapData = MapData;

            LoadMap();

            // Update rendering
            _refreshHandler();
        }
        #endregion

        #region Redo
        /// <summary>
        /// Restores the imported map data
        /// </summary>
        protected override void OnRedo()
        {
            // Backup current state for undo
            _undoMapData = MapData;

            // Restore redo-backup and then clear it
            MapData = _redoMapData;
            _redoMapData = null;

            // Update rendering
            _refreshHandler();
        }
        #endregion

        #region Undo
        /// <summary>
        /// Restores the original map data
        /// </summary>
        protected override void OnUndo()
        {
            // Backup current state for redo
            _redoMapData = MapData;

            // Restore undo-backup and then clear it
            MapData = _undoMapData;
            _undoMapData = null;

            // Update rendering
            _refreshHandler();
        }
        #endregion

        //--------------------//

        #region Terrain access
        /// <summary>
        /// Override to point to the appropriate <see cref="Terrain"/> array map
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This property provides direct access to the underlying array without any cloning involved")]
        protected abstract byte[,] MapData { get; set; }

        /// <summary>
        /// Override to load the map data from a file into the <see cref="Terrain"/>
        /// </summary>
        protected abstract void LoadMap();
        #endregion
    }
}
