/*
 * Copyright 2006-2012 Bastian Eicher
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
using Common;
using Common.Undo;
using Presentation;
using World;

namespace AlphaEditor.World.Commands
{
    /// <summary>
    /// Loads new XML data into a <see cref="Universe"/>.
    /// </summary>
    internal class ImportXml : FirstExecuteCommand
    {
        #region Variables
        private readonly Func<Universe> _getUniverse;
        private readonly Action<Universe> _setUniverse;
        private readonly string _fileName;
        private readonly SimpleEventHandler _refreshHandler;
        private Universe _undoUniverse, _redoUniverse;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command for loading XML data into a <see cref="Universe"/>.
        /// </summary>
        /// <param name="getUniverse">Called to get the current <see cref="Universe"/> in the editor.</param>
        /// <param name="setUniverse">Called to change the current <see cref="Universe"/> in the editor.</param>
        /// <param name="fileName">The file to load the XML data from.</param>
        /// <param name="refreshHandler">Called when the <see cref="Presenter"/> needs to be reset.</param>
        public ImportXml(Func<Universe> getUniverse, Action<Universe> setUniverse, string fileName, SimpleEventHandler refreshHandler)
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

        //--------------------//

        #region Execute
        /// <summary>
        /// Imports the XML data
        /// </summary>
        protected override void OnFirstExecute()
        {
            // Backup current state for undo
            _undoUniverse = _getUniverse();
            _undoUniverse.Terrain.LightAngleMapsOutdated = true;

            // Create new universe from XML and partially restore old data
            var newUniverse = Universe.LoadXml(_fileName);
            newUniverse.Terrain.LightAngleMapsOutdated = true;
            newUniverse.SourceFile = _undoUniverse.SourceFile;
            newUniverse.Terrain.HeightMap = _undoUniverse.Terrain.HeightMap;
            newUniverse.Terrain.TextureMap = _undoUniverse.Terrain.TextureMap;

            // Apply new data
            _setUniverse(newUniverse);

            // Update rendering
            _refreshHandler();
        }
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
