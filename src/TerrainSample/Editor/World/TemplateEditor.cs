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
using System.IO;
using System.Windows.Forms;
using AlphaEditor.Properties;
using Common;
using Common.Controls;
using Common.Storage;
using World;

namespace AlphaEditor.World
{
    /// <summary>
    /// Abstract base tab for editing <see cref="Template{T}"/>es
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Template{T}"/>es to edit</typeparam>
    /// <seealso cref="TemplateManager"/>
    public abstract partial class TemplateEditor<T> : UndoCloneTab where T : Template<T>
    {
        #region Variables
        // Don't use WinForms designer for this, since it doesn't understand generics
        /// <summary>The filtered tree view listing all <see cref="Template{T}"/>s.</summary>
        protected readonly FilteredTreeView<T> TemplateList = new FilteredTreeView<T>
        {
            CheckBoxes = true,
            Location = new System.Drawing.Point(6, 44),
            Size = new System.Drawing.Size(128, 306),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            TabIndex = 1
        };
        #endregion

        #region Properties
        /// <summary>
        /// The list of <see cref="Template{T}"/> to visualize
        /// </summary>
        protected TemplateCollection<T> Templates { get { return (TemplateCollection<T>)Content; } }
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
                    Content = TemplateCollection<T>.Load(_fullPath);
                }
                else
                { // Create new file
                    Log.Info("Create file: " + _fullPath);
                    Content = new TemplateCollection<T>();
                    Templates.Save(_fullPath);
                }
            }
            else
            { // File name only? Might not save to same dir loaded from!
                Log.Info("Load file: " + FilePath);
                _fullPath = ContentManager.CreateFilePath("World", FilePath);
                if (ContentManager.FileExists("World", FilePath, true))
                { // Load existing file, might be from an Archive and not fullPath
                    Content = TemplateCollection<T>.FromContent(FilePath);
                }
                else
                { // Create new file
                    Log.Info("Create file: " + _fullPath);
                    Content = new TemplateCollection<T>();
                    Templates.Save(_fullPath);
                }
            }
            #endregion

            TemplateList.Entries = Templates;

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
            Templates.Save(_fullPath);

            // Reload all class lists to update other Editor tabs
            TemplateManager.LoadLists();

            base.OnSaveFile();
        }

        /// <summary>
        /// Called to delete the currently selected object in this tab
        /// </summary>
        protected override void OnDelete()
        {
            if (TemplateList.CheckedEntries.Length != 0)
            {
                if (!Msg.YesNo(this, string.Format(Resources.DeleteCheckedEntries, TemplateList.CheckedEntries.Length), MsgSeverity.Warn, Resources.YesDelete, Resources.NoKeep))
                    return;

                foreach (var entry in TemplateList.CheckedEntries)
                    Templates.Remove(entry);
            }
            else if (TemplateList.SelectedEntry != null)
            {
                if (!Msg.YesNo(this, string.Format(Resources.DeleteSelectedEntry, TemplateList.SelectedEntry), MsgSeverity.Warn, Resources.YesDelete, Resources.NoKeep))
                    return;

                Templates.Remove(TemplateList.SelectedEntry);
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
            buttonRemove.Enabled = buttonExport.Enabled = (TemplateList.SelectedEntry != null || TemplateList.CheckedEntries.Length > 0);

            // Set up PropertyGrid
            if (TemplateList.CheckedEntries.Length == 0) propertyGridTemplate.SelectedObject = TemplateList.SelectedEntry;
            else propertyGridTemplate.SelectedObjects = TemplateList.CheckedEntries;
        }

        /// <summary>
        /// Hook to be called when the user wants a new <see cref="Template{T}"/> to be added.
        /// </summary>
        protected virtual void OnNewTemplate()
        {}

        /// <inheritdoc />
        public override void Undo()
        {
            base.Undo();

            TemplateList.Entries = Templates;

            // Reasign the selection if the class still exists after the undo-action
            if (TemplateList.SelectedEntry == null) return;
            string className = TemplateList.SelectedEntry.Name;
            TemplateList.SelectedEntry = Templates.Contains(className) ? Templates[className] : null;
        }

        /// <inheritdoc />
        public override void Redo()
        {
            base.Redo();

            TemplateList.Entries = Templates;

            // Reasign the selection if the class still exists after the redo-action
            if (TemplateList.SelectedEntry == null) return;
            string className = TemplateList.SelectedEntry.Name;
            TemplateList.SelectedEntry = Templates.Contains(className) ? Templates[className] : null;
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
            if (newName == TemplateList.SelectedEntry.Name || Templates.Contains(newName))
            {
                Msg.Inform(this, Resources.NameInUse, MsgSeverity.Warn);
                return;
            }
            #endregion

            // Create a copy with a new name
            T clonedTemplate = TemplateList.SelectedEntry.Clone();
            clonedTemplate.Name = newName;
            Templates.Add(clonedTemplate);

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
                Templates.Rename(TemplateList.SelectedEntry, newName);
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
            if (TemplateList.CheckedEntries.Length != 0)
            {
                if (!Msg.YesNo(this, string.Format(Resources.ExportCheckedEntries, TemplateList.CheckedEntries.Length), MsgSeverity.Warn, Resources.OK, Resources.Cancel))
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
            if (TemplateList.CheckedEntries.Length != 0)
                XmlStorage.Save(dialogExportXml.FileName, TemplateList.CheckedEntries);
            else if (TemplateList.SelectedEntry != null)
                XmlStorage.Save(dialogExportXml.FileName, TemplateList.SelectedEntry);

            ToastProvider.ShowToast(Resources.SavedFile);
        }
        #endregion
    }
}
