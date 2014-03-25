/*
 * Copyright 2006-2014 Bastian Eicher
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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AlphaFramework.Editor;
using AlphaFramework.Editor.Properties;
using Common;
using FrameOfReference.Editor.World;
using FrameOfReference.World;
using FrameOfReference.World.Config;

namespace FrameOfReference.Editor
{
    /// <summary>
    /// The main window of the editor, container for the editor tabs.
    /// </summary>
    public sealed partial class MainForm : MainFormBase
    {
        #region Startup
        /// <inheritdoc/>
        public MainForm()
        {
            InitializeComponent();

            // Moved hotkeys out of WinForms designer / resource file to prevent problems with localized versions of Visual Studio
            toolUniverseEntityEditor.ShortcutKeys = Keys.Control | Keys.Shift | Keys.E;
            toolUniverseItemEditor.ShortcutKeys = Keys.Control | Keys.Shift | Keys.M;
            toolUniverseTerrainEditor.ShortcutKeys = Keys.Control | Keys.Shift | Keys.T;

            switch (Settings.Current.General.Language)
            {
                case "de":
                    menuLanguageGerman.Checked = true;
                    break;
                default:
                    menuLanguageEnglish.Checked = true;
                    break;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Restore previous window layout
            if (Settings.Current.Editor.WindowSize != Size.Empty) Size = Settings.Current.Editor.WindowSize;
            if (Settings.Current.Editor.WindowMaximized) WindowState = FormWindowState.Maximized;

            // Open files passed as command-line arguments
            foreach (string file in Program.Args.Files.Where(file => file.EndsWith(Universe.FileExt, StringComparison.OrdinalIgnoreCase)))
                AddTab(new MapEditor(file, false));
        }
        #endregion

        #region Exit
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Store window layout
            Settings.Current.Editor.WindowMaximized = (WindowState == FormWindowState.Maximized);
            if (WindowState == FormWindowState.Normal) Settings.Current.Editor.WindowSize = Size;
        }

        /// <inheritdoc/>
        protected override void Restart()
        {
            Program.Restart = true;
            Close();
        }
        #endregion

        #region Game
        /// <summary>
        /// Launches the main game with the currently active mod (arguments automatically set).
        /// </summary>
        /// <param name="arguments">Additional arguments to be passed; may be <see langword="null"/>.</param>
        /// <exception cref="Win32Exception">Thrown if the game executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the game executable is damaged.</exception>
        protected override void LaunchGame(string arguments)
        {
            Program.LaunchGame(arguments);
        }
        #endregion

        //--------------------//

        #region Language
        /// <inheritdoc/>
        protected override void ChangeLanguage(string language)
        {
            // Set language and propagate change
            Settings.Current.General.Language = language;
            Program.UpdateLocale();

            // Refresh tab to update language-related stuff
            if (CurrentTab != null) CurrentTab.Open(this);

            // Inform user he/she needs to close the Main Form to refresh all locales
            if (Msg.YesNo(this, Resources.CloseModForLangChange, MsgSeverity.Info, Resources.CloseModForLangChangeYes, Resources.CloseModForLangChangeNo))
                Restart();
        }
        #endregion

        #region Toolbar
        private void toolUniverseEditor_Click(object sender, EventArgs e)
        {
            OpenFileTab("World/Maps", Universe.FileExt, (path, overwrite) => new MapEditor(path, overwrite));
        }

        private void toolUniverseEntityEditor_Click(object sender, EventArgs e)
        {
            ShowSingleInstanceTab<EntityEditor>();
        }

        private void toolUniverseItemEditor_Click(object sender, EventArgs e)
        {
            ShowSingleInstanceTab<ItemEditor>();
        }

        private void toolUniverseTerrainEditor_Click(object sender, EventArgs e)
        {
            ShowSingleInstanceTab<TerrainEditor>();
        }
        #endregion
    }
}
