/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using AlphaEditor.Properties;
using Common;
using Common.Undo;

namespace AlphaEditor
{
    /// <summary>
    /// A base class for all editor windows that have undo-functionality based on <see cref="IUndoCommand"/> logging (using the command pattern)
    /// </summary>
    /// <remarks>All changes to the content must be performed via the <see cref="ExecuteCommand"/> interface to be handled by the undo-system</remarks>
    // Note: Cannot be abstract to prevent WinForms designer problems
    public class UndoCommandTab : UndoTab<IUndoCommand>, ICommandExecutor
    {
        #region Constructor
        // Note: This prevents instatiation without making class abstract
        /// <inheritdoc/>
        protected UndoCommandTab()
        {}
        #endregion

        //--------------------//

        #region Command
        /// <inheritdoc/>
        public void ExecuteCommand(IUndoCommand command)
        {
            #region Sanity checks
            if (command == null) throw new ArgumentNullException("command");
            #endregion

            // Execute the command and store it for later undo
            try
            {
                command.Execute();
            }
                #region Error handling
            catch (FileNotFoundException ex)
            {
                Msg.Inform(this, Resources.FileNotFound + "\n" + ex.FileName, MsgSeverity.Warn);
                return;
            }
            catch (IOException ex)
            {
                Msg.Inform(this, Resources.FileNotLoadable + "\n" + ex.Message, MsgSeverity.Warn);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, Resources.FileNotLoadable + "\n" + ex.Message, MsgSeverity.Warn);
                return;
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, Resources.FileDamaged + "\n" + ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
                return;
            }
            #endregion

            UndoBackups.Push(command);

            buttonUndo.Enabled = true;

            OnUpdate();
            OnChange();
        }
        #endregion

        //--------------------//

        #region Undo
        /// <inheritdoc />
        protected override void OnUndo()
        {
            // Remove last command from the undo list, execute it and add it to the redo list
            IUndoCommand lastCommand = UndoBackups.Pop();
            lastCommand.Undo();
            RedoBackups.Push(lastCommand);
        }
        #endregion

        #region Redo
        /// <inheritdoc />
        protected override void OnRedo()
        {
            // Remove last command from the redo list, execute it and add it to the undo list
            IUndoCommand lastCommand = RedoBackups.Pop();
            lastCommand.Execute();
            UndoBackups.Push(lastCommand);
        }
        #endregion
    }
}
