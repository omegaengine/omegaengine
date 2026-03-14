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

namespace AlphaFramework.Editor.World.Commands;

/// <summary>
/// Abstract base class for commands that load new map data into a <see cref="ITerrain"/>.
/// </summary>
/// <param name="terrain">The <see cref="ITerrain"/> to load new map data into.</param>
/// <param name="fileName">The file to load the map data from.</param>
/// <param name="refreshHandler">Called when the presenter needs to be reset.</param>
/// <typeparam name="T">The type of the map data to be imported.</typeparam>
public abstract class ImportMap<T>(ITerrain terrain, string fileName, Action refreshHandler)
    : FirstExecuteCommand
    where T : class
{
    protected readonly ITerrain Terrain = terrain;
    protected readonly string FileName = fileName;

    private T? _undoMapData, _redoMapData;

    /// <summary>
    /// Imports the map data
    /// </summary>
    protected override void OnFirstExecute()
    {
        // Backup current state for undo
        _undoMapData = MapData;

        LoadMap();

        // Update rendering
        refreshHandler();
    }

    /// <summary>
    /// Restores the imported map data
    /// </summary>
    protected override void OnRedo()
    {
        // Backup current state for undo
        _undoMapData = MapData;

        // Restore redo-backup and then clear it
        MapData = _redoMapData!;
        _redoMapData = null;

        // Update rendering
        refreshHandler();
    }

    /// <summary>
    /// Restores the original map data
    /// </summary>
    protected override void OnUndo()
    {
        // Backup current state for redo
        _redoMapData = MapData;

        // Restore undo-backup and then clear it
        MapData = _undoMapData!;
        _undoMapData = null;

        // Update rendering
        refreshHandler();
    }

    /// <summary>
    /// Override to point to the appropriate <see cref="ITerrain"/> array map
    /// </summary>
    protected abstract T MapData { get; set; }

    /// <summary>
    /// Override to load the map data from a file into the <see cref="ITerrain"/>
    /// </summary>
    protected abstract void LoadMap();
}
