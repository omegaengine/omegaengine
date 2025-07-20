/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using AlphaFramework.World.Terrains;
using NanoByte.Common.Undo;

namespace AlphaFramework.Editor.World.Commands
{
    /// <summary>
    /// Abstract base class for commands that load new map data into a <see cref="ITerrain"/>.
    /// </summary>
    /// <typeparam name="T">The type of the map data to be imported.</typeparam>
    public abstract class ImportMap<T> : FirstExecuteCommand
        where T : class
    {
        #region Variables
        protected readonly ITerrain Terrain;
        protected readonly string FileName;

        private readonly Action _refreshHandler;
        private T _undoMapData, _redoMapData;
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
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
            #endregion

            Terrain = terrain ?? throw new ArgumentNullException(nameof(terrain));
            FileName = fileName;
            _refreshHandler = refreshHandler ?? throw new ArgumentNullException(nameof(refreshHandler));
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
        protected abstract T MapData { get; set; }

        /// <summary>
        /// Override to load the map data from a file into the <see cref="ITerrain"/>
        /// </summary>
        protected abstract void LoadMap();
        #endregion
    }
}
