/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Windows.Forms;
using AlphaEditor.Properties;
using Common;
using Common.Storage;

namespace AlphaEditor
{
    /// <summary>
    /// Displays and edits information about a mod. Can also be used to create a new mod.
    /// </summary>
    public partial class ModPropertyDialog : Form
    {
        #region Variables
        private ModInfo _info;
        private string _location;
        #endregion

        #region Startup
        /// <inheritdoc/>
        public ModPropertyDialog()
        {
            InitializeComponent();
        }

        private void ModPropertyDialog_Load(object sender, EventArgs e)
        {
            if (_info == null)
            {
                // Enable location selection for new mods
                textLocation.Enabled = true;
                buttonBrowse.Enabled = true;
            }
            else
            {
                // Show info for existing mods
                textName.Text = _info.Name;
                textVersion.Text = _info.Version;
                textLocation.Text = _location;
                textAuthor.Text = _info.Author;
                textWebsite.Text = _info.Website;
                textDescription.Text = _info.Description;
            }
        }
        #endregion

        #region Static access
        /// <summary>
        /// Creates a new mod. Information is automatically saved to the XML info file.
        /// </summary>
        /// <returns>The path of the newly created mod info file, <see langword="null"/> if none was created</returns>
        public static string CreateMod()
        {
            var dialog = new ModPropertyDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string path = Path.Combine(dialog._location, dialog._info.Name + ModInfo.FileExt);
                XmlStorage.Save(path, dialog._info);
                return path;
            }
            return null;
        }

        /// <summary>
        /// Edits the properties of an existing mod. Changes are automatically saved to the XML info file.
        /// </summary>
        /// <param name="info">The mod information to be edited</param>
        /// <param name="path">The path of the mod info file</param>
        public static void EditMod(ModInfo info, string path)
        {
            var dialog = new ModPropertyDialog {_info = info, _location = Path.GetDirectoryName(path)};
            if (dialog.ShowDialog() == DialogResult.OK)
                XmlStorage.Save(path, ModInfo.Current);
        }
        #endregion

        //--------------------//

        #region Browse location
        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            folderBrowser.SelectedPath = textLocation.Text;
            folderBrowser.ShowDialog(this);
            textLocation.Text = folderBrowser.SelectedPath;
        }
        #endregion

        #region OK button
        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textName.Text))
            {
                errorProvider.SetError(textName, Resources.MissingModName);
                textName.Focus();
                return;
            }

            if (_info == null)
            {
                if (string.IsNullOrEmpty(textLocation.Text))
                {
                    errorProvider.SetError(textLocation, Resources.MissingStorageLocation);
                    textLocation.Focus();
                    return;
                }

                _location = textLocation.Text;

                // Check the directory is empty
                if (Directory.Exists(_location) && Directory.GetFiles(_location).Length > 0)
                {
                    if (!Msg.YesNo(this, Resources.DirectoryNotEmpty, MsgSeverity.Warn, Resources.DirectoryNotEmptyContinue, Resources.DirectoryNotEmptyCancel))
                    {
                        textLocation.Focus();
                        return;
                    }
                }

                // Try to create directory for new mod
                try
                {
                    Directory.CreateDirectory(_location);
                }
                    #region Error handling
                catch (ArgumentException)
                {
                    errorProvider.SetError(textLocation, Resources.InvalidDirectoryPath);
                    textLocation.Focus();
                    return;
                }
                catch (NotSupportedException)
                {
                    errorProvider.SetError(textLocation, Resources.InvalidDirectoryPath);
                    textLocation.Focus();
                    return;
                }
                catch (IOException)
                {
                    errorProvider.SetError(textLocation, Resources.DirectoryNotCreatable);
                    textLocation.Focus();
                    return;
                }
                catch (UnauthorizedAccessException)
                {
                    errorProvider.SetError(textLocation, Resources.DirectoryNotCreatable);
                    textLocation.Focus();
                    return;
                }
                #endregion

                _info = new ModInfo();
            }

            // Store mod information
            _info.Name = textName.Text;
            _info.Version = textVersion.Text;
            _info.Author = textAuthor.Text;
            _info.Website = textWebsite.Text;
            _info.Description = textDescription.Text;

            DialogResult = DialogResult.OK;
            Close();
        }
        #endregion

        #region Clear ErrorProvider messages
        private void textName_TextChanged(object sender, EventArgs e)
        {
            errorProvider.SetError(textName, null);
        }

        private void textLocation_TextChanged(object sender, EventArgs e)
        {
            errorProvider.SetError(textLocation, null);
        }
        #endregion
    }
}
