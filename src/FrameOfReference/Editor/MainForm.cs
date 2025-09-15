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
using System.Linq;
using System.Windows.Forms;
using AlphaFramework.Editor;
using AlphaFramework.Editor.Properties;
using FrameOfReference.Editor.World;
using FrameOfReference.Presentation.Config;
using FrameOfReference.World;
using NanoByte.Common.Controls;

namespace FrameOfReference.Editor;

/// <summary>
/// The main window of the editor, container for the editor tabs.
/// </summary>
public sealed partial class MainForm : MainFormBase
{
    /// <inheritdoc/>
    public MainForm()
    {
        InitializeComponent();

        // Moved hotkeys out of WinForms designer / resource file to prevent problems with localized versions of Visual Studio
        toolUniverseEntityEditor.ShortcutKeys = Keys.Control | Keys.Shift | Keys.E;
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
        // Open files passed as command-line arguments
        foreach (string file in Environment.GetCommandLineArgs().Where(file => file.EndsWith(Universe.FileExt, StringComparison.OrdinalIgnoreCase)))
            AddTab(new MapEditor(file, false));
    }

    /// <inheritdoc/>
    protected override void Restart()
    {
        Program.Restart = true;
        Close();
    }

    /// <inheritdoc/>
    protected override void LaunchGame(params string[] arguments)
        => Program.LaunchGame(arguments);

    /// <inheritdoc/>
    protected override void ChangeLanguage(string language)
    {
        // Set language and propagate change
        Settings.Current.General.Language = language;
        Program.UpdateLocale();

        // Refresh tab to update language-related stuff
        CurrentTab?.Open(this);

        // Inform user he/she needs to close the Main Form to refresh all locales
        if (Msg.YesNo(this, Resources.CloseModForLangChange, MsgSeverity.Info, Resources.CloseModForLangChangeYes, Resources.CloseModForLangChangeNo))
            Restart();
    }

    private void toolUniverseEditor_Click(object sender, EventArgs e)
    {
        OpenFileTab("World/Maps", Universe.FileExt, (path, overwrite) => new MapEditor(path, overwrite));
    }

    private void toolUniverseEntityEditor_Click(object sender, EventArgs e)
    {
        ShowSingleInstanceTab<EntityEditor>();
    }

    private void toolUniverseTerrainEditor_Click(object sender, EventArgs e)
    {
        ShowSingleInstanceTab<TerrainEditor>();
    }
}
