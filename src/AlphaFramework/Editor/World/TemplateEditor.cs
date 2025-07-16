/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AlphaFramework.Editor.Properties;
using AlphaFramework.World.Templates;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Controls;
using OmegaEngine.Storage;

namespace AlphaFramework.Editor.World
{
    /// <summary>
    /// Abstract base tab for editing <see cref="Template{T}"/>es
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Template{T}"/>es to edit</typeparam>
    public abstract partial class TemplateEditor<T> : UndoCloneTab<NamedCollection<T>> where T : Template<T>
    {
        #region Variables
        // Don't use WinForms designer for this, since it doesn't understand generics
        /// <summary>The filtered tree view listing all <see cref="Template{T}"/>s.</summary>
        protected readonly FilteredTreeView<T> TemplateList = new()
        {
            CheckBoxes = true,
            Location = new(6, 44),
            Size = new(128, 306),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            TabIndex = 1
        };
        #endregion

        #region Constructor
        /// <inheritdoc/>
        protected TemplateEditor()
        {
            InitializeComponent();

            groupTemplate.Controls.Add(TemplateList);
            TemplateList.SelectedEntryChanged += delegate { OnUpdate(); };
            TemplateList.CheckedEntriesChanged += delegate { UpdateHelper(); };
            propertyGridTemplate.PropertyValueChanged += delegate { OnChange(); };
        }
        #endregion

        //--------------------//

        #region Handlers
        /// <summary>
        /// Called on startup to load the content for this tab from a file
        /// </summary>
        protected override void OnInitialize()
        {
            #region File handling
            if (Path.IsPathRooted(FilePath))
            {
                _fullPath = FilePath;
                if (!_overwrite && File.Exists(_fullPath))
                { // Load existing file
                    Log.Info("Load file: " + _fullPath);
                    Content = XmlStorage.LoadXml<NamedCollection<T>>(_fullPath);
                }
                else
                { // Create new file
                    Log.Info("Create file: " + _fullPath);
                    Content = [];
                    Content.SaveXml(_fullPath);
                }
            }
            else
            { // File name only? Might not save to same dir loaded from!
                Log.Info("Load file: " + FilePath);
                _fullPath = ContentManager.CreateFilePath("World", FilePath);
                if (ContentManager.FileExists("World", FilePath))
                { // Load existing file, might be from an Archive and not fullPath
                    string id = FilePath;
                    using (var stream = ContentManager.GetFileStream("World", id))
                        Content = XmlStorage.LoadXml<NamedCollection<T>>(stream);
                }
                else
                { // Create new file
                    Log.Info("Create file: " + _fullPath);
                    Content = [];
                    Content.SaveXml(_fullPath);
                }
            }
            #endregion

            TemplateList.Nodes = Content;

            base.OnInitialize();
        }

        /// <summary>
        /// Called when the content of this tab is to be saved to a file - no error-handling!
        /// </summary>
        protected override void OnSaveFile()
        {
            Log.Info("Save file: " + _fullPath);
            string directory = Path.GetDirectoryName(_fullPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            Content.SaveXml(_fullPath);

            base.OnSaveFile();
        }

        /// <summary>
        /// Called to delete the currently selected object in this tab
        /// </summary>
        protected override void OnDelete()
        {
            if (TemplateList.CheckedEntries.Count != 0)
            {
                if (!Msg.YesNo(this, string.Format(Resources.DeleteCheckedEntries, TemplateList.CheckedEntries.Count), MsgSeverity.Warn, Resources.YesDelete, Resources.NoKeep))
                    return;

                foreach (var entry in TemplateList.CheckedEntries)
                    Content.Remove(entry);
            }
            else if (TemplateList.SelectedEntry != null)
            {
                if (!Msg.YesNo(this, string.Format(Resources.DeleteSelectedEntry, TemplateList.SelectedEntry), MsgSeverity.Warn, Resources.YesDelete, Resources.NoKeep))
                    return;

                Content.Remove(TemplateList.SelectedEntry);
            }
            else return;

            TemplateList.SelectedEntry = null;
            OnChange();
            OnUpdate();
        }

        /// <summary>
        /// Called on startup, content updates and tab switch to refresh any on-screen displays
        /// </summary>
        protected override void OnUpdate()
        {
            UpdateHelper();

            base.OnUpdate();
        }

        /// <summary>
        /// Updates the controls (toolbars, PropertyGrid, etc.) based on the currently selected and currently checked templates.
        /// </summary>
        private void UpdateHelper()
        {
            buttonRename.Enabled = buttonCopy.Enabled = (TemplateList.SelectedEntry != null);
            buttonRemove.Enabled = buttonExport.Enabled = (TemplateList.SelectedEntry != null || TemplateList.CheckedEntries.Count > 0);

            // Set up PropertyGrid
            if (TemplateList.CheckedEntries.Count == 0) propertyGridTemplate.SelectedObject = TemplateList.SelectedEntry;
            else propertyGridTemplate.SelectedObjects = TemplateList.CheckedEntries.ToArray();
        }

        /// <summary>
        /// Hook to be called when the user wants a new <see cref="Template{T}"/> to be added.
        /// </summary>
        protected virtual void OnNewTemplate()
        {}

        /// <inheritdoc/>
        public override void Undo()
        {
            base.Undo();

            TemplateList.Nodes = Content;

            // Reasign the selection if the class still exists after the undo-action
            if (TemplateList.SelectedEntry == null) return;
            string className = TemplateList.SelectedEntry.Name;
            TemplateList.SelectedEntry = Content.Contains(className) ? Content[className] : null;
        }

        /// <inheritdoc/>
        public override void Redo()
        {
            base.Redo();

            TemplateList.Nodes = Content;

            // Reasign the selection if the class still exists after the redo-action
            if (TemplateList.SelectedEntry == null) return;
            string className = TemplateList.SelectedEntry.Name;
            TemplateList.SelectedEntry = Content.Contains(className) ? Content[className] : null;
        }
        #endregion

        //--------------------//

        #region Buttons
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            OnNewTemplate();
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            string newName = InputBox.Show(this, "Template name", Resources.EnterTemplateName, TemplateList.SelectedEntry.Name + " 2");
            if (string.IsNullOrEmpty(newName)) return;

            #region Error handling
            if (newName == TemplateList.SelectedEntry.Name || Content.Contains(newName))
            {
                Msg.Inform(this, Resources.NameInUse, MsgSeverity.Warn);
                return;
            }
            #endregion

            // Create a copy with a new name
            T clonedTemplate = TemplateList.SelectedEntry.Clone();
            clonedTemplate.Name = newName;
            Content.Add(clonedTemplate);

            OnChange();
            OnUpdate();

            // Select the new entry
            TemplateList.SelectedEntry = clonedTemplate;
        }

        private void buttonRename_Click(object sender, EventArgs e)
        {
            string newName = InputBox.Show(this, "Template name", Resources.EnterTemplateName, TemplateList.SelectedEntry.Name);
            if (string.IsNullOrEmpty(newName) || newName == TemplateList.SelectedEntry.Name) return;

            try
            {
                Content.Rename(TemplateList.SelectedEntry, newName);
            }
                #region Error handling
            catch (InvalidOperationException)
            {
                Msg.Inform(this, Resources.NameInUse, MsgSeverity.Warn);
                return;
            }
            #endregion

            OnChange();
            OnUpdate();
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            if (TemplateList.CheckedEntries.Count != 0)
            {
                if (!Msg.YesNo(this, string.Format(Resources.ExportCheckedEntries, TemplateList.CheckedEntries.Count), MsgSeverity.Warn, Resources.OK, Resources.Cancel))
                    return;
            }
            else if (TemplateList.SelectedEntry != null)
            {
                if (!Msg.YesNo(this, string.Format(Resources.ExportSelectedEntry, TemplateList.SelectedEntry), MsgSeverity.Warn, Resources.OK, Resources.Cancel))
                    return;
            }
            else return;

            dialogExportXml.FileName = "";
            dialogExportXml.ShowDialog(this);
        }
        #endregion

        #region Export
        private void dialogExportXml_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (TemplateList.CheckedEntries.Count != 0)
                TemplateList.CheckedEntries.ToList().SaveXml(dialogExportXml.FileName);
            else if (TemplateList.SelectedEntry != null)
                TemplateList.SelectedEntry.SaveXml(dialogExportXml.FileName);

            ToastProvider.ShowToast(Resources.SavedFile);
        }
        #endregion
    }
}
