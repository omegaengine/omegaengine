/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using AlphaFramework.World;
using NanoByte.Common.Storage;
using NanoByte.Common.Undo;

namespace AlphaFramework.Editor.World.Commands;

/// <summary>
/// Common base for loading new XML data into a <see cref="IUniverse"/>.
/// </summary>
/// <param name="getUniverse">Called to get the current <typeparamref name="TUniverse"/> in the editor.</param>
/// <param name="setUniverse">Called to change the current <typeparamref name="TUniverse"/> in the editor.</param>
/// <param name="xmlData">The XML string to parse.</param>
/// <param name="refreshHandler">Called when the presenter needs to be reset.</param>
/// <typeparam name="TUniverse">The specific type of <see cref="IUniverse"/> to load XML data for.</typeparam>
public abstract class ImportXmlBase<TUniverse>(Func<TUniverse> getUniverse, Action<TUniverse> setUniverse, string xmlData, Action refreshHandler) : FirstExecuteCommand
    where TUniverse : class, IUniverse
{
    private TUniverse? _undoUniverse, _redoUniverse;

    /// <summary>
    /// Imports the XML data
    /// </summary>
    protected override void OnFirstExecute()
    {
        // Backup current state for undo
        _undoUniverse = getUniverse();
        Debug.Assert(_undoUniverse != null);

        // Create new universe from XML and partially restore old data
        var newUniverse = XmlStorage.FromXmlString<TUniverse>(xmlData);
        newUniverse.PostLoad(_undoUniverse.SourceFile);
        TransferNonXmlData(_undoUniverse, newUniverse);

        // Apply new data
        setUniverse(newUniverse);

        // Update rendering
        refreshHandler();
    }

    /// <summary>
    /// Transfers any non-serialized data from <paramref name="oldUniverse"/> to <paramref name="newUniverse"/>.
    /// </summary>
    protected abstract void TransferNonXmlData(TUniverse oldUniverse, TUniverse newUniverse);

    /// <summary>
    /// Restores the imported XML data
    /// </summary>
    protected override void OnRedo()
    {
        // Backup current state for undo
        _undoUniverse = getUniverse();

        // Restore redo-backup and then clear it
        setUniverse(_redoUniverse);
        _redoUniverse = null;

        // Update rendering
        refreshHandler();
    }

    /// <summary>
    /// Restores the original XML data
    /// </summary>
    protected override void OnUndo()
    {
        // Backup current state for redo
        _redoUniverse = getUniverse();

        // Restore undo-backup and then clear it
        setUniverse(_undoUniverse);
        _undoUniverse = null;

        // Update rendering
        refreshHandler();
    }
}
