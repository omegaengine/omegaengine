using System;
using Common;
using Common.Undo;
using World;

namespace AlphaEditor.World.Commands
{
    /// <summary>
    /// Command that loads new XML data into a <see cref="Universe"/>
    /// </summary>
    internal class ImportXml : FirstExecuteCommand
    {
        #region Variables
        private readonly Func<Universe> _getUniverse;
        private readonly Action<Universe> _setUniverse;
        private readonly string _fileName;
        private readonly Action _refreshHandler;
        private Universe _undoUniverse, _redoUniverse;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command for loading XML data into a <see cref="Universe"/>
        /// </summary>
        /// <param name="getUniverse">Called to get the current <see cref="Universe"/> in the editor</param>
        /// <param name="setUniverse">Called to change the current <see cref="Universe"/> in the editor</param>
        /// <param name="fileName">The file to load the XML data from</param>
        /// <param name="refreshHandler">Called every time the XML data is changed</param>
        public ImportXml(Func<Universe> getUniverse, Action<Universe> setUniverse, string fileName, Action refreshHandler)
        {
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

            // Backup non-XML data to keep from old universe
            var sourceFile = _undoUniverse.SourceFile;

            // Create new universe from XML and partially restore old data
            var newUniverse = Universe.LoadXml(_fileName);
            newUniverse.SourceFile = sourceFile;

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
