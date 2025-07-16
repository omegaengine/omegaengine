/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using AlphaFramework.Editor.Properties;
using NanoByte.Common;
using OmegaEngine.Storage;

namespace AlphaFramework.Editor
{
    /// <summary>
    /// Allows the user to select a Mod to edit
    /// </summary>
    public sealed partial class ModSelectorForm : Form
    {
        #region Structs
        private struct ModPath
        {
            public readonly string FullPath;

            public ModPath(string fullPath)
            {
                FullPath = fullPath;
            }

            public override string ToString()
            {
                return Path.GetFileNameWithoutExtension(FullPath ?? "");
            }
        }
        #endregion

        #region Variables
        private readonly IList<string> _recentMods;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new mod selection form.
        /// </summary>
        /// <param name="allowEditMain"><c>true</c> to allow the user to edit the main game as well; <c>false</c> to allow only mods to be edited.</param>
        /// <param name="recentMods">An externally stored list of recently opened mods.</param>
        public ModSelectorForm(bool allowEditMain, IList<string> recentMods = null)
        {
            InitializeComponent();
            Text = AboutBox.AssemblyTitle;

            buttonMainGame.Visible = allowEditMain;
            _recentMods = recentMods ?? new List<string>();

            openFileDialog.DefaultExt = ModInfo.FileExt;
            openFileDialog.Filter = "Mod info (*" + ModInfo.FileExt + ")|*" + ModInfo.FileExt;
        }
        #endregion

        #region Startup
        private void ModSelector_Load(object sender, EventArgs e)
        {
            foreach (string modPath in _recentMods)
                listBoxRecent.Items.Add(new ModPath(modPath));
        }
        #endregion

        //--------------------//

        #region Helpers
        /// <summary>
        /// Displays a message box for an exception, unwrapping it to be more usefull if it comes from XML serialization.
        /// </summary>
        /// <param name="ex">The exception to report.</param>
        private void ReportPotentialXmlError(InvalidOperationException ex)
        {
            if (ex.Source.StartsWith("System.Xml", StringComparison.Ordinal) && ex.InnerException != null)
            {
                string postion = ex.Message.GetRightPartAtFirstOccurrence('(').GetLeftPartAtFirstOccurrence(')');
                string message = $"{Resources.FileDamaged}\nXML ({postion}): {ex.InnerException.Message}";
                Msg.Inform(this, message, MsgSeverity.Warn);
                return;
            }

            Msg.Inform(this, ex.Message, MsgSeverity.Warn);
        }
        #endregion

        //--------------------//

        #region Open mod
        private void OpenMod(string path)
        {
            if (!File.Exists(path))
            {
                _recentMods.Remove(path);
                listBoxRecent.Items.Remove(new ModPath(path));

                Msg.Inform(this, Resources.FileNotFound + "\n" + path, MsgSeverity.Warn);
                return;
            }

            Log.Info("Open mod: " + path);

            try
            {
                ModInfo.Current = XmlStorage.LoadXml<ModInfo>(path);
            }
                #region Error handling
            catch (ArgumentException)
            {
                Msg.Inform(this, Resources.InvalidFilePath, MsgSeverity.Warn);
                return;
            }
            catch (InvalidOperationException ex)
            {
                ReportPotentialXmlError(ex);
                throw;
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
            #endregion

            string directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory))
            {
                Msg.Inform(this, Resources.NotFoundModContentDir, MsgSeverity.Warn);
                return;
            }
            ModInfo.CurrentLocation = path;
            ContentManager.ModDir = new(directory);

            #region Recently used List
            // Remove the entry from the list if it is already there
            _recentMods.Remove(path);

            // Now add the entry to the top of the list
            _recentMods.Insert(0, path);

            // Make sure the list doesn't get too long
            while (_recentMods.Count > 10)
                _recentMods.RemoveAt(_recentMods.Count - 1);
            #endregion

            Close();
        }
        #endregion

        //--------------------//

        #region New mod
        private void buttonNew_Click(object sender, EventArgs e)
        {
            string location = ModPropertyDialog.CreateMod();
            if (!string.IsNullOrEmpty(location))
                OpenMod(location);
        }
        #endregion

        #region Existing mod
        private void buttonOpen_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog(this);
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            OpenMod(openFileDialog.FileName);
        }
        #endregion

        #region Main game
        private void buttonMainGame_Click(object sender, EventArgs e)
        {
            Log.Info("Open main game");

            // Signalize the user didn't just cancel
            ModInfo.MainGame = true;

            Close();
        }
        #endregion

        #region Recent mods
        private void listBoxRecent_DoubleClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(listBoxRecent.Text))
                OpenMod(((ModPath)listBoxRecent.SelectedItem).FullPath);
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            listBoxRecent.Items.Clear();
            _recentMods.Clear();
        }
        #endregion
    }
}
