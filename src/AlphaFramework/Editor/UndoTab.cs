/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using AlphaFramework.Editor.Properties;

namespace AlphaFramework.Editor
{
    /// <summary>
    /// A base class for all editor windows that have undo-functionality
    /// </summary>
    /// <typeparam name="T">Data type for storing undo checkpoint information (e.g. cloned snapshots or command logs)</typeparam>
    public abstract class UndoTab<T> : Tab
    {
        #region Variables
        // ReSharper disable InconsistentNaming
        /// <summary>The undo button.</summary>
        protected readonly Button buttonUndo;

        /// <summary>The redo button.</summary>
        protected readonly Button buttonRedo;

        // ReSharper restore InconsistentNaming

        /// <summary>Entries used by the undo-system to undo changes</summary>
        protected readonly Stack<T> UndoBackups = new Stack<T>();

        /// <summary>Entries used by the undo-system to redo changes previously undone</summary>
        protected readonly Stack<T> RedoBackups = new Stack<T>();
        #endregion

        #region Constructor
        /// <inheritdoc/>
        protected UndoTab()
        {
            #region Undo Button
            buttonUndo = new Button
            {
                Name = "undoButton",
                FlatStyle = FlatStyle.Flat,
                Image = Resources.UndoButton,
                BackColor = Color.White,
                TabStop = false,
                Size = new Size(22, 22),
                Enabled = false
            };
            buttonUndo.Click += delegate { Undo(); };

            // Prevent button from getting focus
            buttonUndo.TabIndex = int.MaxValue;
            buttonUndo.GotFocus += delegate { FocusDefaultControl(); };

            Controls.Add(buttonUndo);
            Resize += delegate
            {
                buttonUndo.Top = Height - buttonUndo.Height - 1;
                buttonUndo.Left = Width - buttonUndo.Width - 1;
            };
            #endregion

            #region Redo Button
            buttonRedo = new Button
            {
                Name = "redoButton",
                FlatStyle = FlatStyle.Flat,
                Image = Resources.RedoButton,
                BackColor = Color.White,
                TabStop = false,
                Size = new Size(22, 22),
                Enabled = false
            };
            buttonRedo.Click += delegate { Redo(); };

            // Prevent button from getting focus
            buttonRedo.TabIndex = int.MaxValue;
            buttonRedo.GotFocus += delegate { FocusDefaultControl(); };

            Controls.Add(buttonRedo);
            Resize += delegate
            {
                buttonRedo.Top = buttonUndo.Top - buttonUndo.Height + 1;
                buttonRedo.Left = Width - buttonRedo.Width - 1;
            };
            #endregion
        }
        #endregion

        //--------------------//

        #region Handlers
        /// <inheritdoc />
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect", Justification = "GC.Collect is only called after undo-information is purged, when a lot of long-lived objects have turned into garbage")]
        protected override void OnSaveFile()
        {
            // Clear undo/redo backups after file was saved
            UndoBackups.Clear();
            RedoBackups.Clear();
            buttonUndo.Enabled = buttonRedo.Enabled = false;

            // After purging the undo/redo backups a lot of garbage will be left in Generation 2.
            // We should run Garbage Collection now, so we don't keep on wasting a large chunk of memory.
            GC.Collect();

            base.OnSaveFile();
        }

        /// <inheritdoc />
        protected override void OnChange()
        {
            // Prevent redos if the user changed something
            RedoBackups.Clear();
            buttonRedo.Enabled = false;

            base.OnChange();
        }
        #endregion

        //--------------------//

        #region Undo
        /// <inheritdoc />
        public override void Undo()
        {
            // Since this might be triggered by a hotkey instead of the actual button, we must check
            if (!buttonUndo.Enabled) return;

            OnUndo();

            // Only enable the buttons that still have a use
            if (UndoBackups.Count == 0)
            {
                buttonUndo.Enabled = false;
                Changed = false;
            }
            buttonRedo.Enabled = true;

            OnUpdate();
        }

        /// <summary>
        /// Hook to undo the last change
        /// </summary>
        protected abstract void OnUndo();
        #endregion

        #region Redo
        /// <inheritdoc />
        public override void Redo()
        {
            // Since this might be triggered by a hotkey instead of the actual button, we must check
            if (!buttonRedo.Enabled) return;

            OnRedo();

            // Mark as "to be saved" again
            Changed = true;

            // Only enable the buttons that still have a use
            buttonRedo.Enabled = (RedoBackups.Count > 0);
            buttonUndo.Enabled = true;

            OnUpdate();
        }

        /// <summary>
        /// Hook to redo the last undone change
        /// </summary>
        protected abstract void OnRedo();
        #endregion
    }
}
