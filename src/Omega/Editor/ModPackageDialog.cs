/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using AlphaEditor.Properties;
using Common;
using ICSharpCode.SharpZipLib.Zip;

namespace AlphaEditor
{
    /// <summary>
    /// Allows the user to create a redistributable package of a Mod
    /// </summary>
    public partial class ModPackageDialog : Form
    {
        #region Constants
        /// <summary>
        /// The file extensions when a mod is stored in a package
        /// </summary>
        public static string FileExt { get { return ModInfo.FileExt + "Package"; } }
        #endregion

        #region Variables
        private string _modProjectFile, _modDirPath;
        private readonly FastZipEvents _fastZipEvents = new FastZipEvents();
        private readonly FastZip _fastZip;
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public ModPackageDialog()
        {
            InitializeComponent();
            saveFileDialog.DefaultExt = FileExt;
            saveFileDialog.Filter = string.Format("Mod package (*{0})|*{0}", FileExt);

            _fastZip = new FastZip(_fastZipEvents);
            _fastZipEvents.Progress += ((sender, e) =>
                e.ContinueRunning = !backgroundWorker.CancellationPending);
        }
        #endregion

        #region Static access
        /// <summary>
        /// Packages a mod into an auto-install ZIP file
        /// </summary>
        /// <param name="info">The mod information to be edited</param>
        /// <param name="path">The path of the mod info file</param>
        public static void PackageMod(ModInfo info, string path)
        {
            #region Sanity checks
            if (info == null) throw new ArgumentNullException("info");
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            var dialog = new ModPackageDialog
            {
                _modProjectFile = path,
                _modDirPath = Path.GetDirectoryName(path),
                saveFileDialog = {FileName = info.Name + FileExt}
            };
            dialog.ShowDialog();
        }
        #endregion

        //--------------------//

        #region Form
        private void ModPackageDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Don't close window while ZIP process is still running
            if (backgroundWorker.IsBusy)
            {
                // Disable cancel button for optical reasons
                buttonCancel.Enabled = false;
                Application.DoEvents();

                // Start cancelling the ZIP process
                backgroundWorker.CancelAsync();

                // Wait until the process has stopped
                while (backgroundWorker.IsBusy) Application.DoEvents();
            }
        }
        #endregion

        #region Start
        private void buttonStart_Click(object sender, EventArgs e)
        {
            saveFileDialog.ShowDialog(this);
        }

        private void saveFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            // Prevent glitches when creating the package within the mod directory
            if (saveFileDialog.FileName.Contains(_modDirPath + Path.DirectorySeparatorChar))
            {
                Msg.Inform(this, Resources.PackageOutsideMod, MsgSeverity.Warn);
                return;
            }

            // Update form
            labelInfo.Visible = buttonStart.Visible = false;
            buttonCancel.Enabled = false; // ToDo: Implement proper cancel function
            labelWait.Visible = true;

            // Start ZIPing
            backgroundWorker.RunWorkerAsync();
        }
        #endregion

        #region BackgroundWorker
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string modInfoFile = Path.Combine(_modDirPath, "ModInfo.xml");

            try
            {
                // Temporarily rename _modProjectFile to modInfoFile to fit a generic In-ZIP naming scheme
                if (File.Exists(modInfoFile)) File.Delete(modInfoFile);
                File.Move(_modProjectFile, modInfoFile);

                try
                {
                    _fastZip.CreateZip(saveFileDialog.FileName, _modDirPath, true, null);
                }
                    #region Error handling
                catch (ZipException ex)
                {
                    Msg.Inform(this, Resources.FileNotSavable + "\n" + "ZIP: " + ex.Message, MsgSeverity.Error);
                    e.Cancel = true;
                }
                catch (IOException ex)
                {
                    if (ex.Source.StartsWith("ICSharpCode.SharpZipLib", StringComparison.Ordinal))
                        Msg.Inform(this, Resources.FileNotSavable + "\n" + "ZIP: " + ex.Message, MsgSeverity.Error);
                    else
                        Msg.Inform(this, Resources.FileNotSavable + "\n" + ex.Message, MsgSeverity.Error);
                    e.Cancel = true;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Msg.Inform(this, Resources.FileNotSavable + "\n" + ex.Message, MsgSeverity.Error);
                    e.Cancel = true;
                }
                #endregion

                File.Move(modInfoFile, _modProjectFile);
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, Resources.FileNotSavable + "\n" + ex.Message, MsgSeverity.Warn);
                e.Cancel = true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, Resources.FileNotSavable + "\n" + ex.Message, MsgSeverity.Warn);
                e.Cancel = true;
            }
            #endregion
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled) Msg.Inform(this, Resources.ModPackaged, MsgSeverity.Info);
            Close();
        }
        #endregion
    }
}
