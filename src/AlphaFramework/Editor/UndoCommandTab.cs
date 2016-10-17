/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using AlphaFramework.Editor.Properties;
using NanoByte.Common;
using NanoByte.Common.Undo;

namespace AlphaFramework.Editor
{
    /// <summary>
    /// A base class for all editor windows that have undo-functionality based on <see cref="IUndoCommand"/> logging (using the command pattern)
    /// </summary>
    /// <remarks>All changes to the content must be performed via the <see cref="ExecuteCommand"/> interface to be handled by the undo-system</remarks>
    // Note: Cannot be abstract to prevent WinForms designer problems
    public class UndoCommandTab : UndoTab<IUndoCommand>
    {
        #region Constructor
        // Note: This prevents instatiation without making class abstract
        /// <inheritdoc/>
        protected UndoCommandTab()
        {}
        #endregion

        //--------------------//

        #region Command
        /// <summary>
        /// Executes a <see cref="IUndoCommand"/> using this tab's undo stack.
        /// </summary>
        protected void ExecuteCommand(IUndoCommand command)
        {
            #region Sanity checks
            if (command == null) throw new ArgumentNullException(nameof(command));
            #endregion

            command.Execute();
            UndoBackups.Push(command);

            buttonUndo.Enabled = true;

            OnUpdate();
            OnChange();
        }

        /// <summary>
        /// Executes a <see cref="IUndoCommand"/> and automatically displays message boxes for common exception types.
        /// </summary>
        protected void ExecuteCommandSafe(IUndoCommand command)
        {
            try
            {
                ExecuteCommand(command);
            }
                #region Error handling
            catch (FileNotFoundException ex)
            {
                Msg.Inform(this, Resources.FileNotFound + "\n" + ex.FileName, MsgSeverity.Warn);
            }
            catch (IOException ex)
            {
                Msg.Inform(this, Resources.FileNotLoadable + "\n" + ex.Message, MsgSeverity.Warn);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, Resources.FileNotLoadable + "\n" + ex.Message, MsgSeverity.Warn);
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, Resources.FileDamaged + "\n" + ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
            }
            #endregion
        }
        #endregion

        //--------------------//

        #region Undo
        /// <inheritdoc/>
        protected override void OnUndo()
        {
            // Remove last command from the undo list, execute it and add it to the redo list
            IUndoCommand lastCommand = UndoBackups.Pop();
            lastCommand.Undo();
            RedoBackups.Push(lastCommand);
        }
        #endregion

        #region Redo
        /// <inheritdoc/>
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
