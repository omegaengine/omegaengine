/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using AlphaEditor.GUI;
using AlphaEditor.Graphics;
using AlphaEditor.Properties;
using Common;
using Common.Controls;
using Common.Storage;
using OmegaGUI.Model;

namespace AlphaEditor
{
    /// <summary>
    /// An extendable basis for an editor main window.
    /// </summary>
    /// <remarks>Implement the hooks <see cref="Restart"/>, <see cref="LaunchGame"/> and <see cref="ChangeLanguage"/>.</remarks>
    public partial class MainFormBase : Form, IToastProvider
    {
        #region Properties
        private AsyncWaitDialog _waitDialog;
        private bool _loading;

        /// <summary>
        /// Indicates the editor is currently loading something and the user must wait.
        /// </summary>
        public bool Loading
        {
            get { return _loading; }
            set
            {
                // Show a "Loading..." dialog in a separate thread
                UpdateHelper.Do(ref _loading, value, delegate
                {
                    if (value)
                    {
                        _waitDialog = new AsyncWaitDialog("AlphEditor - " + Resources.Loading, Icon);
                        _waitDialog.Start();
                    }
                    else
                    {
                        _waitDialog.Stop();
                        _waitDialog = null;
                    }
                });
            }
        }
        #endregion

        #region Startup
        /// <inheritdoc/>
        protected MainFormBase()
        {
            InitializeComponent();

            // Moved hotkeys out of WinForms designer / resource file to prevent problems with localized versions of Visual Studio
            menuFileCloseTab.ShortcutKeys = Keys.Control | Keys.F4;
            menuFileSwitchTab.ShortcutKeys = Keys.Control | Keys.Tab;
            menuFileSave.ShortcutKeys = Keys.Control | Keys.S;
            menuFileModProperties.ShortcutKeys = Keys.Control | Keys.Shift | Keys.P;
            menuFilePackageMod.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            menuFileTestInGame.ShortcutKeys = Keys.Control | Keys.Shift | Keys.X;
            menuEdit.ShortcutKeys = Keys.Control | Keys.Y;
            menuEditUndo.ShortcutKeys = Keys.Control | Keys.Z;
            menuEditRedo.ShortcutKeys = Keys.Control | Keys.Y;
            menuEditDelete.ShortcutKeys = Keys.Shift | Keys.Delete;
            toolGuiLanguage.ShortcutKeys = Keys.Control | Keys.Shift | Keys.L;
            toolGuiCutscene.ShortcutKeys = Keys.Control | Keys.Shift | Keys.C;
        }

        private void MainFormBase_Load(object sender, EventArgs e)
        {
            Text = AboutBox.AssemblyTitle;
            if (ModInfo.Current == null)
                Text += " - Main game";
            else
            {
                Text += " - Mod: " + ModInfo.Current.Name;
                menuFileModProperties.Enabled = menuFilePackageMod.Enabled = true;
            }
        }
        #endregion

        #region Tab control
        protected readonly Dictionary<Tab, ToolStripButton> Tabs = new Dictionary<Tab, ToolStripButton>();

        #region Add tab
        protected void AddTab(Tab tab)
        {
            // Load the new tab into the MainForm
            tabPanel.Controls.Add(tab);
            tab.Dock = DockStyle.Fill;

            // Create a new button for the tab in the TabStrip
            var tabButton = (ToolStripButton)tabStrip.Items.Add(Resources.Loading + @"     ");
            tab.TextChanged += delegate { tabButton.Text = tab.Text + @"   "; };
            tabButton.BackColor = Color.DarkGray;
            tabButton.Click += delegate { ShowTab(tab); };

            // Hookup the tab with the closing system
            tab.TabClosed += delegate { RemoveTab(tab); };

            // Add the tab to the list and display it
            Tabs.Add(tab, tabButton);
            Loading = true;
            try
            {
                ShowTab(tab);
            }
            finally
            {
                Loading = false;
            }
        }
        #endregion

        #region Show tab
        protected void ShowTab(Tab tab)
        {
            // Hide and uncheck all tabs
            foreach (Tab oldTab in Tabs.Keys)
                if (oldTab != tab) oldTab.Hide();
            foreach (ToolStripButton oldTabButton in Tabs.Values)
                oldTabButton.Checked = false;

            // Open/load the selected tab
            try
            {
                tab.Open(this);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            // Activate tab
            Tabs[tab].Checked = true;

            // Turn on the tab-related menu commands
            ToogleMenuCommands(true);
        }
        #endregion

        #region Remove tab
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect", Justification = "GC.Collect is only called after tab-closes, when a lot of long-lived objects have turned into garbage")]
        private void RemoveTab(Tab tab)
        {
            // Remove the tab from all list
            tabPanel.Controls.Remove(tab);
            tabStrip.Items.Remove(Tabs[tab]); // Remove the toolstrip entry
            Tabs.Remove(tab);

            // Open the last tab in the list
            Tab lastTab = null;
            foreach (Tab oldTab in Tabs.Keys)
                lastTab = oldTab;
            if (lastTab != null) ShowTab(lastTab);

            // Turn off the tab-related menu commands when there are no more tabs left
            if (Tabs.Count == 0) ToogleMenuCommands(false);

            // Cause cache to be flushed, so Garbage Collectio can work
            PerformLayout();
            tabStrip.PerformLayout();

            // After a Tab has closed a lot of garbage will be left in Generation 2.
            // We should run Garbage Collection now, so we don't keep on wasting a large chunk of memory.
            GC.Collect();
        }
        #endregion

        #region Current tab
        /// <summary>
        /// The currently visible tab
        /// </summary>
        protected Tab CurrentTab
        {
            get
            {
                foreach (var pair in Tabs)
                    if (pair.Value.Checked) return pair.Key;
                return null;
            }
        }
        #endregion

        #endregion

        #region Toast messages
        /// <inheritdoc/>
        public void ShowToast(string message)
        {
            labelToast.Text = message;
            labelToast.Visible = true;
            timerToastReset.Start();
        }

        private void labelToast_Click(object sender, EventArgs e)
        {
            timerToastReset.Stop();
            labelToast.Visible = false;
        }

        private void timerToastReset_Tick(object sender, EventArgs e)
        {
            timerToastReset.Stop();
            labelToast.Visible = false;
        }
        #endregion

        #region Exit
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Make sure all tabs are closed before the main window closes
            while (CurrentTab != null)
            {
                if (!CurrentTab.RequestClose())
                {
                    e.Cancel = true;
                    //Program.Restart = false;
                    return;
                }
                Application.DoEvents();
            }

            // Reset mod info
            ContentManager.ModDir = null;
            ModInfo.Current = null;
        }

        /// <summary>
        /// Restarts the editor to allow the user to choose a different mod.
        /// </summary>
        protected virtual void Restart()
        {}
        #endregion

        #region Game
        /// <summary>
        /// Launches the main game with the currently active mod (arguments automatically set).
        /// </summary>
        /// <param name="arguments">Additional arguments to be passed; may be <see langword="null"/>.</param>
        /// <exception cref="Win32Exception">Thrown if the game executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the game executable is damaged.</exception>
        protected virtual void LaunchGame(string arguments)
        {}
        #endregion

        //--------------------//

        #region Editors

        #region Particle System
        private void CpuParticleSystemEditor()
        {
            // Get the file path
            string path;
            bool overwrite;
            if (!FileSelectorDialog.TryGetPath("Graphics/CpuParticleSystem", ".xml", out path, out overwrite)) return;

            // Don't open a file twice
            foreach (Tab tab in Tabs.Keys)
            {
                var previousEditor = tab as CpuParticleSystemEditor;
                if (previousEditor != null && previousEditor.FilePath == path)
                {
                    ShowTab(tab);
                    return;
                }
            }

            AddTab(new CpuParticleSystemEditor(path, overwrite));
        }

        private void GpuParticleSystemEditor()
        {
            // Get the file path
            string path;
            bool overwrite;
            if (!FileSelectorDialog.TryGetPath("Graphics/GpuParticleSystem", ".xml", out path, out overwrite)) return;

            // Don't open a file twice
            foreach (Tab tab in Tabs.Keys)
            {
                var previousEditor = tab as GpuParticleSystemEditor;
                if (previousEditor != null && previousEditor.FilePath == path)
                {
                    ShowTab(tab);
                    return;
                }
            }

            AddTab(new GpuParticleSystemEditor(path, overwrite));
        }
        #endregion

        #region GUI
        private void GuiEditor()
        {
            // Get the file path
            string path;
            bool overwrite;
            if (!FileSelectorDialog.TryGetPath("GUI", ".xml", out path, out overwrite)) return;

            // Don't open a file twice
            foreach (Tab tab in Tabs.Keys)
            {
                var previousEditor = tab as GuiEditor;
                if (previousEditor != null && previousEditor.FilePath == path)
                {
                    ShowTab(tab);
                    return;
                }
            }

            AddTab(new GuiEditor(path, overwrite));
        }

        private void LanguageEditor()
        {
            // Get the file path
            string path;
            bool overwrite;
            if (!FileSelectorDialog.TryGetPath("GUI/Language", LocaleFile.FileExt, out path, out overwrite)) return;

            // Don't open a file twice
            foreach (Tab tab in Tabs.Keys)
            {
                var previousEditor = tab as LanguageEditor;
                if (previousEditor != null && previousEditor.FilePath == path)
                {
                    ShowTab(tab);
                    return;
                }
            }

            AddTab(new LanguageEditor(path, overwrite));
        }

        private void CutsceneEditor()
        {
            // Get the file path
            string path;
            bool overwrite;
            if (!FileSelectorDialog.TryGetPath("GUI/Cutscenes", ".xml", out path, out overwrite)) return;

            // Don't open a file twice
            foreach (Tab tab in Tabs.Keys)
            {
                var previousEditor = tab as CutsceneEditor;
                if (previousEditor != null && previousEditor.FilePath == path)
                {
                    ShowTab(tab);
                    return;
                }
            }

            AddTab(new CutsceneEditor(path, overwrite));
        }
        #endregion

        #endregion

        #region Menu
        private void ToogleMenuCommands(bool enabled)
        {
            // Disable inappropriate commands if no tabs are open
            menuFileCloseTab.Enabled = menuFileSwitchTab.Enabled = menuFileSave.Enabled =
                menuEditUndo.Enabled = menuEditRedo.Enabled = menuEditDelete.Enabled =
                    menuEditUndo.Enabled = menuEditRedo.Enabled = enabled;
        }

        #region File
        private void menuFileCloseTab_Click(object sender, EventArgs e)
        {
            CurrentTab.RequestClose();
        }

        private void menuFileSwitchTab_Click(object sender, EventArgs e)
        {
            // Copy the tabs to a more powerfull list
            var currentTabs = new List<Tab>(Tabs.Keys);

            // Find the current tab index and move one to the left
            int index = currentTabs.LastIndexOf(CurrentTab) + 1;

            // Select this new tab (handle overflow by starting back at zero)
            ShowTab(currentTabs[currentTabs.Count > index ? index : 0]);
        }

        private void menuFileSave_Click(object sender, EventArgs e)
        {
            CurrentTab.SaveFile();
        }

        private void menuFileModProperties_Click(object sender, EventArgs e)
        {
            ModPropertyDialog.EditMod(ModInfo.Current, ModInfo.CurrentLocation);
        }

        private void menuFilePackageMod_Click(object sender, EventArgs e)
        {
            ModPackageDialog.PackageMod(ModInfo.Current, ModInfo.CurrentLocation);
        }

        private void menuFileTestInGame_Click(object sender, EventArgs e)
        {
            if (!Msg.OkCancel(this, Resources.TestInGame, MsgSeverity.Info, Resources.TestInGameContinue, null))
                return;

            try
            {
                LaunchGame(null);
            }
                #region Error handling
            catch (Win32Exception)
            {
                Msg.Inform(this, Resources.GameEXENotFound, MsgSeverity.Error);
            }
            catch (BadImageFormatException)
            {
                Msg.Inform(this, Resources.GameEXENotFound, MsgSeverity.Error);
            }
            #endregion
        }

        private void menuFileSwitchMod_Click(object sender, EventArgs e)
        {
            Restart();
        }

        private void menuFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        #region Edit
        private void menuEditUndo_Click(object sender, EventArgs e)
        {
            CurrentTab.Undo();
        }

        private void menuEditRedo_Click(object sender, EventArgs e)
        {
            CurrentTab.Redo();
        }

        private void menuEditDelete_Click(object sender, EventArgs e)
        {
            CurrentTab.Delete();
        }
        #endregion

        #region Language
        private void menuLanguageEnglish_Click(object sender, EventArgs e)
        {
            menuLanguageEnglish.Checked = true;
            menuLanguageGerman.Checked = false;
            ChangeLanguage("en");
        }

        private void menuLanguageGerman_Click(object sender, EventArgs e)
        {
            menuLanguageEnglish.Checked = false;
            menuLanguageGerman.Checked = true;
            ChangeLanguage("de");
        }

        /// <summary>
        /// Applies a new language to the editor GUI, game logic, etc..
        /// </summary>
        /// <param name="language">A two-letter ISO language code.</param>
        protected virtual void ChangeLanguage(string language)
        {}
        #endregion

        #region Help
        private void menuHelpAbout_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog(this);
        }
        #endregion

        #endregion

        #region Toolbar
        private void toolParticleSystemCpu_Click(object sender, EventArgs e)
        {
            CpuParticleSystemEditor();
        }

        private void toolParticleSystemGpu_Click(object sender, EventArgs e)
        {
            GpuParticleSystemEditor();
        }

        private void toolGuiEditor_Click(object sender, EventArgs e)
        {
            GuiEditor();
        }

        private void toolGuiLanguage_Click(object sender, EventArgs e)
        {
            LanguageEditor();
        }

        private void toolGuiCutscene_Click(object sender, EventArgs e)
        {
            CutsceneEditor();
        }
        #endregion
    }
}
