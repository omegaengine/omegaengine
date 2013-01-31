/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace AlphaEditor
{
    /// <summary>
    /// A base class for all editor windows that have undo-functionality based on cloning their whole content
    /// </summary>
    /// <remarks>Calling <see cref="OnChange"/> will create an undo checkpoint (<see cref="_currentBackup"/>) after the fact, ready for the next change.
    /// The first checkpoint is created right at startup.</remarks>
    // Note: Cannot be abstract to prevent WinForms designer problems
    public class UndoCloneTab : UndoTab<ICloneable>
    {
        #region Variables
        /// <summary>
        /// The content to be handled by the undo-system
        /// </summary>
        protected ICloneable Content;

        /// <summary>
        /// A backup of the current state of <see cref="Content"/>
        /// </summary>
        /// <remarks>This is used to get the state of <see cref="Content"/> as it was before <see cref="OnChange"/> was called</remarks>
        private ICloneable _currentBackup;
        #endregion

        #region Constructor
        // Note: This prevents instatiation without making class abstract
        /// <inheritdoc/>
        protected UndoCloneTab()
        {}
        #endregion

        //--------------------//

        #region Handlers
        /// <summary>
        /// Called on startup to load the content for this tab from a file
        /// </summary>
        protected override void OnInitialize()
        {
            // Create an initial backup after loading
            if (Content != null) _currentBackup = (ICloneable)Content.Clone();
        }

        /// <summary>
        /// Mark the content of this tab as changed (needs to be saved) and create a new undo-backup
        /// </summary>
        protected override void OnChange()
        {
            // Move the last backup to the undo list and then create a new backup
            UndoBackups.Push(_currentBackup);
            _currentBackup = (ICloneable)Content.Clone();

            buttonUndo.Enabled = true;

            base.OnChange();
        }
        #endregion

        //--------------------//

        #region Undo
        /// <summary>
        /// Called to undo the last change
        /// </summary>
        protected override void OnUndo()
        {
            // Remove the last backup from the undo list, then add the current backup to the redo list
            ICloneable toRestore = UndoBackups.Pop();
            RedoBackups.Push(_currentBackup);

            // Restore the backup and update the current backup
            Content = (ICloneable)toRestore.Clone();
            _currentBackup = (ICloneable)Content.Clone();
        }
        #endregion

        #region Redo
        /// <summary>
        /// Called to redo the last undone change
        /// </summary>
        protected override void OnRedo()
        {
            // Remove the last backup from the redo list, then add the current backup to the undo list
            ICloneable toRestore = RedoBackups.Pop();
            UndoBackups.Push(_currentBackup);

            // Restore the backup and update the current backup
            Content = (ICloneable)toRestore.Clone();
            _currentBackup = (ICloneable)Content.Clone();
        }
        #endregion
    }
}
