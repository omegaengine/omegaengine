/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage.SlimDX;
using OmegaGUI.Model;

namespace AlphaFramework.Editor.Gui
{
    /// <summary>
    /// Allows a user to edit language localizations for the GUI
    /// </summary>
    public partial class LanguageEditor : UndoCloneTab<XmlDictionary>
    {
        #region Constructor
        /// <summary>
        /// Creates a new GUI language editor.
        /// </summary>
        /// <param name="filePath">The path to the file to be edited.</param>
        /// <param name="overwrite"><c>true</c> if an existing file supposed to be overwritten when <see cref="Tab.SaveFile"/> is called.</param>
        public LanguageEditor(string filePath, bool overwrite)
        {
            InitializeComponent();

            FilePath = filePath;
            _overwrite = overwrite;

            dataGrid.CellValueChanged += OnDataGridViewCellEvent;
            dataGrid.NewRowNeeded += OnDataGridViewRowEvent;
            dataGrid.UserDeletedRow += OnDataGridViewRowEvent;
        }
        #endregion

        //--------------------//

        #region Handlers
        /// <inheritdoc/>
        protected override void OnInitialize()
        {
            #region File handling
            if (Path.IsPathRooted(FilePath))
            {
                _fullPath = FilePath;
                if (!_overwrite && File.Exists(_fullPath))
                { // Load existing file
                    Log.Info("Load file: " + _fullPath);
                    Content = LocaleFile.Load(_fullPath);
                }
                else
                { // Create new file
                    Log.Info("Create file: " + _fullPath);
                    Content = new XmlDictionary {{"Test", "A"}};
                    LocaleFile.Save(_fullPath, Content);
                }
            }
            else
            { // File name only? Might not save to same dir loaded from!
                Log.Info("Load file: " + FilePath);
                Content = LocaleFile.FromContent(FilePath);
                _fullPath = ContentManager.CreateFilePath("GUI/Language", FilePath);
            }
            #endregion

            base.OnInitialize();
        }

        /// <inheritdoc/>
        protected override void OnSaveFile()
        {
            // Sort alphabetically before saving
            Content.Sort();
            // OnUpdate() will automatically be called

            Log.Info("Save file: " + _fullPath);
            string directory = Path.GetDirectoryName(_fullPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            LocaleFile.Save(_fullPath, Content);

            base.OnSaveFile();
        }

        /// <inheritdoc/>
        protected override void OnDelete()
        {
            if (dataGrid.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGrid.SelectedRows)
                    dataGrid.Rows.Remove(row);
                OnChange();
            }
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            #region Set up DataGrid
            // Remove event handler delegates before updating to prevent loops
            dataGrid.CellValueChanged -= OnDataGridViewCellEvent;
            dataGrid.NewRowNeeded -= OnDataGridViewRowEvent;
            dataGrid.UserDeletedRow -= OnDataGridViewRowEvent;

            dataGrid.DataSource = Content;

            // Restore the event handler delegates
            dataGrid.CellValueChanged += OnDataGridViewCellEvent;
            dataGrid.NewRowNeeded += OnDataGridViewRowEvent;
            dataGrid.UserDeletedRow += OnDataGridViewRowEvent;
            #endregion

            base.OnUpdate();
        }
        #endregion

        //--------------------//

        #region DataGrid events
        private void OnDataGridViewRowEvent(object sender, DataGridViewRowEventArgs e)
        {
            OnChange();
        }

        private void OnDataGridViewCellEvent(object sender, DataGridViewCellEventArgs e)
        {
            OnChange();
        }

        private void OnDataGridDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            Msg.Inform(this, e.Exception.Message, MsgSeverity.Warn);
        }
        #endregion
    }
}
