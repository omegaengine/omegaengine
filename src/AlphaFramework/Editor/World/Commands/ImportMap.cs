/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using AlphaFramework.World.Terrains;
using Common.Undo;

namespace AlphaFramework.Editor.World.Commands
{
    /// <summary>
    /// Abstract base class for commands that load new map data into a <see cref="ITerrain"/>.
    /// </summary>
    public abstract class ImportMap : FirstExecuteCommand
    {
        #region Variables
        // ReSharper disable InconsistentNaming
        protected readonly ITerrain _terrain;
        protected readonly string _fileName;
        // ReSharper restore InconsistentNaming

        private readonly Action _refreshHandler;
        private byte[,] _undoMapData, _redoMapData;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command for loading map data into a <see cref="ITerrain"/>.
        /// </summary>
        /// <param name="terrain">The <see cref="ITerrain"/> to load new map data into.</param>
        /// <param name="fileName">The file to load the map data from.</param>
        /// <param name="refreshHandler">Called when the presenter needs to be reset.</param>
        protected ImportMap(ITerrain terrain, string fileName, Action refreshHandler)
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
        /// Override to point to the appropriate <see cref="ITerrain"/> array map
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This property provides direct access to the underlying array without any cloning involved")]
        protected abstract byte[,] MapData { get; set; }

        /// <summary>
        /// Override to load the map data from a file into the <see cref="ITerrain"/>
        /// </summary>
        protected abstract void LoadMap();
        #endregion
    }
}
