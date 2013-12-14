/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Common.Storage;
using Common.Undo;
using TemplateWorld;

namespace AlphaEditor.World.Commands
{
    /// <summary>
    /// Common base for loading new XML data into a <see cref="IUniverse"/>.
    /// </summary>
    /// <typeparam name="TUniverse">The specific type of <see cref="IUniverse"/> to load XML data for.</typeparam>
    public abstract class ImportXmlBase<TUniverse> : FirstExecuteCommand
        where TUniverse : class, IUniverse
    {
        #region Variables
        private readonly Func<TUniverse> _getUniverse;
        private readonly Action<TUniverse> _setUniverse;
        private readonly string _fileName;
        private readonly Action _refreshHandler;
        private TUniverse _undoUniverse, _redoUniverse;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command for loading XML data into a <typeparamref name="TUniverse"/>.
        /// </summary>
        /// <param name="getUniverse">Called to get the current <typeparamref name="TUniverse"/> in the editor.</param>
        /// <param name="setUniverse">Called to change the current <typeparamref name="TUniverse"/> in the editor.</param>
        /// <param name="fileName">The file to load the XML data from.</param>
        /// <param name="refreshHandler">Called when the presenter needs to be reset.</param>
        protected ImportXmlBase(Func<TUniverse> getUniverse, Action<TUniverse> setUniverse, string fileName, Action refreshHandler)
        {
            #region Sanity checks
            if (getUniverse == null) throw new ArgumentNullException("getUniverse");
            if (setUniverse == null) throw new ArgumentNullException("setUniverse");
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            if (refreshHandler == null) throw new ArgumentNullException("refreshHandler");
            #endregion

            _getUniverse = getUniverse;
            _setUniverse = setUniverse;
            _fileName = fileName;
            _refreshHandler = refreshHandler;
        }
        #endregion

        #region Execute
        /// <summary>
        /// Imports the XML data
        /// </summary>
        protected override void OnFirstExecute()
        {
            // Backup current state for undo
            _undoUniverse = _getUniverse();

            // Create new universe from XML and partially restore old data
            var newUniverse = XmlStorage.LoadXml<TUniverse>(_fileName);
            newUniverse.SourceFile = _undoUniverse.SourceFile;
            TransferNonXmlData(_undoUniverse, newUniverse);

            // Apply new data
            _setUniverse(newUniverse);

            // Update rendering
            _refreshHandler();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldOUniverse"></param>
        /// <param name="newUniverse"></param>
        protected abstract void TransferNonXmlData(TUniverse oldOUniverse, TUniverse newUniverse);
        #endregion

        #region Redo
        /// <summary>
        /// Restores the imported XML data
        /// </summary>
        protected override void OnRedo()
        {
            // Backup current state for undo
            _undoUniverse = _getUniverse();

            // Restore redo-backup and then clear it
            _setUniverse(_redoUniverse);
            _redoUniverse = null;

            // Update rendering
            _refreshHandler();
        }
        #endregion

        #region Undo
        /// <summary>
        /// Restores the original XML data
        /// </summary>
        protected override void OnUndo()
        {
            // Backup current state for redo
            _redoUniverse = _getUniverse();

            // Restore undo-backup and then clear it
            _setUniverse(_undoUniverse);
            _undoUniverse = null;

            // Update rendering
            _refreshHandler();
        }
        #endregion
    }
}
