/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using AlphaFramework.Editor.Properties;
using NanoByte.Common.Controls;
using OmegaEngine.Foundation.Storage;

namespace AlphaFramework.Editor;

/// <summary>
/// Allows the user to select a file for the Mod (stored in an Archive or a real file)
/// </summary>
public partial class FileSelectorDialog : Form
{
    #region Variables
    // Don't use WinForms designer for this, since it doesn't understand generics
    private readonly FilteredTreeView<FileEntry> _fileList = new()
    {
        Name = "FileList",
        Location = new(12, 57),
        Size = new(240, 196),
        Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        TabIndex = 1,
        Separator = Path.DirectorySeparatorChar
    };

    private string _type, _extension;
    private string _path;
    private bool _overwrite;
    #endregion

    #region Constructor
    /// <inheritdoc/>
    public FileSelectorDialog()
    {
        InitializeComponent();

        _fileList.SelectionConfirmed += fileList_SelectionConfirmed;
        Controls.Add(_fileList);
    }
    #endregion

    #region Get path
    /// <summary>
    /// Gets the file path for a game-content file
    /// </summary>
    /// <param name="type">The type of file you want (e.g. Textures, Sounds, ...)</param>
    /// <param name="extension">The file extension of the file type with a dot, but without an asterisk (e.g. .xml, .png, ...)</param>
    /// <param name="path">The absolute path to the requested content file</param>
    /// <param name="overwrite">Returns whether the user wants an existing file to be overwritten</param>
    /// <param name="allowNew">Allow the user to create a new file instead of opening an existing one?</param>
    /// <returns><c>true</c> if a file was selected, <c>false</c> if none was selected</returns>
    /// <exception cref="InvalidOperationException">The user didn't select a file.</exception>
    private static bool TryGetPath(string type, string extension, out string path, out bool overwrite, bool allowNew)
    {
        var selector = new FileSelectorDialog {_type = type, _extension = extension, buttonNew = {Enabled = allowNew}};

        // Display a nice text for the filetype
        string filetype = Path.GetFileName(type);
        if (filetype == "Meshes") filetype = "Mesh"; // Handle unusual plural form of Mesh
        else if (filetype.EndsWith("s", StringComparison.Ordinal)) filetype = filetype.Remove(filetype.Length - 1, 1); // Remove plural "s"
        selector.selectLabel.Text = string.Format(Resources.Select, filetype);

        // Create containing directory
        string directory;
        try
        {
            directory = ContentManager.CreateDirPath(type);
        }
        #region Error handling
        catch (IOException ex)
        {
            Msg.Inform(selector, $"{Resources.InvalidDirectoryPath}\n{ex.Message}", MsgSeverity.Warn);
            path = null;
            overwrite = false;
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            Msg.Inform(selector, $"{Resources.InvalidDirectoryPath}\n{ex.Message}", MsgSeverity.Warn);
            path = null;
            overwrite = false;
            return false;
        }
        #endregion

        // Setup file dialogs
        selector.openFileDialog.InitialDirectory = selector.saveFileDialog.InitialDirectory = directory;
        selector.openFileDialog.Filter = selector.saveFileDialog.Filter = string.Format("{0} (*{1})|*{1}", type, extension);

        // Display form
        selector.UpdateFileList();
        selector.ShowDialog();

        if (string.IsNullOrEmpty(selector._path))
        { // User canceled
            path = null;
            overwrite = false;
            return false;
        }

        // User has selected
        path = selector._path;
        overwrite = selector._overwrite;
        return true;
    }

    /// <summary>
    /// Gets the file path for a game-content file (allowing the user to create a new file)
    /// </summary>
    /// <param name="type">The type of file you want (e.g. Textures, Sounds, ...)</param>
    /// <param name="extension">The file extension of the file type with a dot, but without an asterisk (e.g. .xml, .png, ...)</param>
    /// <param name="path">The absolute path to the requested content file</param>
    /// <param name="overwrite">Returns whether the user wants an existing file to be overwritten</param>
    /// <returns><c>true</c> if a file was selected, <c>false</c> if none was selected</returns>
    /// <exception cref="InvalidOperationException">The user didn't select a file.</exception>
    public static bool TryGetPath(string type, string extension, out string path, out bool overwrite)
        => TryGetPath(type, extension, out path, out overwrite, allowNew: true);

    /// <summary>
    /// Gets the file path for a game-content file (not allowing the user to create a new file)
    /// </summary>
    /// <param name="type">The type of file you want (e.g. Textures, Sounds, ...)</param>
    /// <param name="extension">The file extension of the file type with a dot, but without an asterisk (e.g. .xml, .png, ...)</param>
    /// <param name="path">The absolute path to the requested content file</param>
    /// <returns><c>true</c> if a file was selected, <c>false</c> if none was selected</returns>
    /// <exception cref="InvalidOperationException">The user didn't select a file.</exception>
    public static bool TryGetPath(string type, string extension, out string path)
        => TryGetPath(type, extension, out path, out bool _, allowNew: false);
    #endregion

    //--------------------//

    #region File list
    private void UpdateFileList()
    {
        _fileList.Nodes = ContentManager.GetFileList(_type, _extension);
    }
    #endregion

    #region File dialogs
    private void buttonOpen_Click(object sender, EventArgs e)
    {
        openFileDialog.ShowDialog(this);
        UpdateFileList();
    }

    private void buttonNew_Click(object sender, EventArgs e)
    {
        saveFileDialog.ShowDialog(this);
        UpdateFileList();
    }

    private void openFileDialog_FileOk(object sender, CancelEventArgs e)
    {
        _path = openFileDialog.FileName;
        Close();
    }

    private void saveFileDialog_FileOk(object sender, CancelEventArgs e)
    {
        _path = saveFileDialog.FileName;
        if (File.Exists(_path)) _overwrite = true;
        Close();
    }
    #endregion

    #region File list
    private void fileList_SelectionConfirmed(object sender, EventArgs e)
    {
        var entry = _fileList.SelectedEntry;
        if (entry != null)
        {
            _path = entry.Name;
            Close();
        }
    }
    #endregion
}
