/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using Common;
using Common.Storage;

namespace AlphaFramework.Editor.Gui
{
    /// <summary>
    /// Allows a user to design a cutscene
    /// </summary>
    public partial class CutsceneEditor : UndoCommandTab
    {
        #region Variables
        //
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new cutscene editor.
        /// </summary>
        /// <param name="filePath">The path to the file to be edited.</param>
        /// <param name="overwrite"><see langword="true"/> if an existing file supposed to be overwritten when <see cref="Tab.SaveFile"/> is called.</param>
        public CutsceneEditor(string filePath, bool overwrite)
        {
            InitializeComponent();

            FilePath = filePath;
            _overwrite = overwrite;
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
                    // ToDo: Load file
                }
                else
                { // Create new file
                    Log.Info("Create file: " + _fullPath);
                    // ToDo: default values
                    // ToDo: Save file
                }
            }
            else
            { // File name only? Might not save to same dir loaded from!
                Log.Info("Load file: " + FilePath);
                _fullPath = ContentManager.CreateFilePath("GUI/Language", FilePath);
                // ToDo: Load file
            }
            #endregion

            base.OnInitialize();
        }

        /// <inheritdoc/>
        protected override void OnSaveFile()
        {
            Log.Info("Save file: " + _fullPath);
            string directory = Path.GetDirectoryName(_fullPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            // ToDo: Implement

            base.OnSaveFile();
        }
        #endregion
    }
}
