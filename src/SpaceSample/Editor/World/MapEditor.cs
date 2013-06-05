/*
 * Copyright 2006-2013 Bastian Eicher
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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Utils;
using Common.Storage;
using Common.Undo;
using ICSharpCode.SharpZipLib.Zip;
using OmegaEngine.Input;
using Presentation;
using World;
using Resources = AlphaEditor.Properties.Resources;

namespace AlphaEditor.World
{
    /// <summary>
    /// Allows the user to edit game maps
    /// </summary>
    public partial class MapEditor : UndoCommandTab
    {
        #region Variables
        private EditorPresenter _presenter;
        private UpdateReceiver _updateReceiver;
        private Universe _universe;
        private Dialogs.MapPropertiesTool _mapPropertiesTool;

        /// <summary>Don't handle <see cref="FilteredTreeView{T}.SelectedEntryChanged"/> event when <see langword="true"/>.</summary>
        private bool _dontSetEntityTemplate;

        /// <summary>Don't handle <see cref="ListBox.SelectedIndexChanged"/> event when <see langword="true"/>.</summary>
        private bool _dontUpdatePositionableSelection;

        /// <summary>Contains backups of <see cref="Positionable"/> property values for the undo system.</summary>
        private readonly MultiPropertyTracker _propertyGridPositionableTracker;

        // Don't use WinForms designer for this, since it doesn't understand generics
        private readonly FilteredTreeView<EntityTemplate> _entityTemplateList = new FilteredTreeView<EntityTemplate>
        {
            Dock = DockStyle.Fill,
            Enabled = false
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new map editor.
        /// </summary>
        /// <param name="filePath">The path to the file to be edited.</param>
        /// <param name="overwrite"><see langword="true"/> if an existing file supposed to be overwritten when <see cref="Tab.SaveFile"/> is called.</param>
        public MapEditor(string filePath, bool overwrite)
        {
            InitializeComponent();

            _propertyGridPositionableTracker = new MultiPropertyTracker(propertyGridPositionable);

            tabPageTemplate.Controls.Add(_entityTemplateList);
            _entityTemplateList.SelectedEntryChanged += SelectedEntityTemplateChanged;

            FilePath = filePath;
            _overwrite = overwrite;

            // Close dialogs when the owning tab closes
            TabClosed += delegate { if (_mapPropertiesTool != null) _mapPropertiesTool.Close(); };

            // Ugly hack to make mouse wheel work
            splitRender.Panel2.MouseMove += delegate { splitRender.Panel2.Focus(); };
        }
        #endregion

        //--------------------//

        #region Handlers
        /// <inheritdoc />
        protected override void OnInitialize()
        {
            // Load template lists before touching any map files
            TemplateManager.LoadLists();

            #region File handling
            if (Path.IsPathRooted(FilePath))
            {
                _fullPath = FilePath;
                if (!_overwrite && File.Exists(_fullPath))
                { // Load existing file
                    Log.Info("Load file: " + _fullPath);
                    _universe = Universe.Load(_fullPath);
                }
                else
                { // Create new file
                    Log.Info("Create file: " + _fullPath);
                    _universe = new Universe();
                    _universe.Save(_fullPath);
                }
            }
            else
            { // File name only? Might not save to same dir loaded from!
                Log.Info("Load file: " + FilePath);
                try
                {
                    _universe = Universe.FromContent(FilePath);
                }
                    #region Error handling
                catch (ZipException ex)
                {
                    // Wrap exception since only certain exception types are allowed
                    throw new InvalidDataException(ex.Message, ex);
                }
                #endregion

                _fullPath = ContentManager.CreateFilePath("World/Maps", FilePath);
            }
            #endregion

            // Initialize engine
            renderPanel.Setup();

            base.OnInitialize();
        }

        /// <inheritdoc />
        protected override void OnSaveFile()
        {
            Log.Info("Save file: " + _fullPath);
            string directory = Path.GetDirectoryName(_fullPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            _universe.Save(_fullPath);

            base.OnSaveFile();
        }

        /// <inheritdoc />
        protected override void OnDelete()
        {
            // Since this might be triggered by a hotkey instead of the actual button, we must check
            if (!buttonRemove.Enabled) return;

            ExecuteCommand(new Commands.RemovePositionables(_universe, _presenter.SelectedPositionables));
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            // Setup the presentator on startup or when it was lost/reset
            if (_presenter == null)
            {
                _presenter = new EditorPresenter(renderPanel.Engine, _universe, false) {WireframeEntities = checkWireframe.Checked};
                _presenter.Initialize();
                _presenter.HookIn();

                renderPanel.AddInputReceiver(_presenter); // Update the presenter based on user input
                renderPanel.AddInputReceiver(_updateReceiver = new UpdateReceiver(() => renderPanel.Engine.Render())); // Force immediate redraw to give responsive feel

                #region Monitoring events
                _presenter.SelectedPositionables.Added += SelectedPositionables_Added;
                _presenter.SelectedPositionables.Removing += SelectedPositionables_Removing;
                _presenter.SelectedPositionables.Removed += SelectedPositionables_Removed;
                #endregion
            }

            UpdatePositionablesListBox();
            UpdateSelectionControls();
            if (_mapPropertiesTool != null) _mapPropertiesTool.UpdateUniverse(_universe);

            base.OnUpdate();
        }
        #endregion

        #region Render control
        /// <summary>Destroys the current <see cref="Presenter"/> and calls <see cref="OnUpdate"/> to have a new one created.</summary>
        private void ResetPresenter()
        {
            // Keep the current Camera angle
            var cameraBackup = _presenter.View.Camera;

            // Stop input handling (needs to be reinitiliazed later)
            renderPanel.RemoveInputReceiver(_updateReceiver);
            renderPanel.RemoveInputReceiver(_presenter);

            // Clear everything
            _presenter.HookOut();
            _presenter.Dispose();
            _presenter = null;
            propertyGridPositionable.SelectedObjects = null;

            // And recreate it
            OnUpdate();

            // Restore the Camera angle
            if (_presenter != null) _presenter.View.Camera = cameraBackup;
        }
        #endregion

        //--------------------//

        #region Dialogs
        /// <summary>
        /// Helper function for configuring the <see cref="Dialogs.MapPropertiesTool"/> form with event hooks.
        /// </summary>
        private void SetupMapPropertiesTool()
        {
            // Keep existing dialog instance
            if (_mapPropertiesTool != null) return;

            _mapPropertiesTool = new Dialogs.MapPropertiesTool(_universe);
            _mapPropertiesTool.ExecuteCommand += ExecuteCommand;

            // Clear the reference when the dialog is disposed
            _mapPropertiesTool.Disposed += delegate { _mapPropertiesTool = null; };
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Determines the <see cref="EntityTemplate"/> of the currently selected <see cref="Entity"/>s
        /// </summary>
        /// <returns>The <see cref="EntityTemplate"/> if all currently selected <see cref="Entity"/>s have the same; <see langword="null"/> otherwise.</returns>
        private EntityTemplate GetCurrentEntityTemplate()
        {
            EntityTemplate currentTemplate = null;
            foreach (Entity entity in _presenter.SelectedPositionables.Entities)
            {
                // First class
                if (currentTemplate == null) currentTemplate = entity.TemplateData;
                    // Check if classes are still the same
                else if (entity.TemplateData.Name != currentTemplate.Name) // Only compare name to handle cloned entries
                    return null;
            }

            // All classes were the same
            return currentTemplate;
        }
        #endregion

        #region Toolbar

        #region Test
        private void buttonTestInGame_Click(object sender, EventArgs e)
        {
            #region Pre-launch chekcs
            if (Changed)
            { // Pending changes
                if (!Msg.OkCancel(this, Resources.TestInGameMap + "\n" + Resources.AutoSavePendingChanges, MsgSeverity.Warn, Resources.TestInGameContinueSave, null))
                    return;

                if (!SaveFile()) return;
            }
            else
            { // All up-to-date
                if (!Msg.OkCancel(this, Resources.TestInGameMap, MsgSeverity.Info, Resources.TestInGameContinue, null))
                    return;
            }
            #endregion

            // Trim away the file extension, if the file is located within the regular game content directory
            string mapFile = Path.IsPathRooted(FilePath) ? FilePath : FilePath.Replace(Universe.FileExt, "");

            // Close the tab in case the map gets changed inside the game
            ForceClose();

            try
            {
                Program.LaunchGame("/modify \"" + mapFile + "\"");
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
        #endregion

        #region Import/Export
        private void buttonImportXml_Click(object sender, EventArgs e)
        {
            renderPanel.Engine.DebugClose(); // Always-on-top window might mess with common dialog

            // Use the filter stored in the tag as an identifier
            dialogImportXml.FileName = "";
            dialogImportXml.InitialDirectory = ContentManager.CreateDirPath("World/Maps");
            dialogImportXml.ShowDialog(this);
        }

        private void buttonExportXml_Click(object sender, EventArgs e)
        {
            renderPanel.Engine.DebugClose(); // Always-on-top window might mess with common dialog

            // Use the filter stored in the tag as an identifier
            dialogExportXml.FileName = "";
            dialogExportXml.InitialDirectory = ContentManager.CreateDirPath("World/Maps");
            dialogExportXml.ShowDialog(this);
        }
        #endregion

        #region New
        private void buttonNewEntity_Click(object sender, EventArgs e)
        {
            // Ask the user which entity template to use as a basis for the new entity
            var selectTemplate = new Dialogs.SelectTemplateDialog<EntityTemplate>(TemplateManager.EntityTemplates);
            if (selectTemplate.ShowDialog(this) != DialogResult.OK) return;

            // Create a new entity from the selected template
            var newEntity = new Entity {TemplateName = selectTemplate.SelectedTemplate};

            // Add the new Entity to the Universe
            ExecuteCommand(new Commands.AddPositionables(_universe, new[] {newEntity}));

            // Select the newly added entity
            _presenter.SelectedPositionables.Clear();
            _presenter.SelectedPositionables.Add(newEntity);
        }

        private void buttonNewBenchmarkPoint_Click(object sender, EventArgs e)
        {
            // Create a benchmark point based on the current camera settings
            var cameraState = _presenter.CameraState;
            if (cameraState == null) return;
            var newEntity = new BenchmarkPoint {Position = cameraState.Position, Rotation = cameraState.Rotation, Radius = cameraState.Radius};

            // Add the new entity to the Universe
            ExecuteCommand(new Commands.AddPositionables(_universe, new[] {newEntity}));

            // Select the newly added entity
            _presenter.SelectedPositionables.Clear();
            _presenter.SelectedPositionables.Add(newEntity);

            // Switch to the Entity Properties tab, to allow the user to customize the entity
            tabControl.SelectedTab = tabPagePositionables;
            tabEntities.SelectedTab = tabPageProperties;
        }
        #endregion

        #region Edit
        private void buttonCopy_Click(object sender, EventArgs e)
        {
            // Clone the selected entities
            var clonedEntities = new Positionable[_presenter.SelectedPositionables.Count];
            int i = 0;
            foreach (Positionable positionable in _presenter.SelectedPositionables)
            {
                clonedEntities[i] = positionable.Clone();
                i++;
            }

            // Add the new entities to the Universe
            ExecuteCommand(new Commands.AddPositionables(_universe, clonedEntities));

            // Select the newly added entities
            _presenter.SelectedPositionables.Clear();
            foreach (Positionable positionable in clonedEntities)
                _presenter.SelectedPositionables.Add(positionable);

            // Switch to the Entity tab, to allow the user to customize the entities
            tabControl.SelectedTab = tabPagePositionables;
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            Delete();
        }
        #endregion

        #region Map
        private void buttonMapProperties_ButtonClick(object sender, EventArgs e)
        {
            // Setup a dialog for selecting the type of render control to add
            SetupMapPropertiesTool();

            // Display non-modal dialog (set this tab as owner on first show)
            if (_mapPropertiesTool.Visible) _mapPropertiesTool.Show();
            else _mapPropertiesTool.Show(this);
        }
        #endregion

        #endregion

        #region Import/Export

        #region XML
        private void dialogImportXml_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                // Load the universe from the XML file but keep the old terrain data
                ExecuteCommand(new Commands.ImportXml(
                    () => _universe, value =>
                    {
                        _universe = value;
                        if (_mapPropertiesTool != null) _mapPropertiesTool.UpdateUniverse(_universe);
                    }, // Universe access delegates
                    dialogImportXml.FileName, ResetPresenter));
            }
                #region Error handling
            catch (FileNotFoundException ex)
            {
                Msg.Inform(this, Resources.FileNotFound + "\n" + ex.FileName, MsgSeverity.Warn);
                return;
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
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, Resources.FileDamaged + "\n" + ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
                return;
            }
            #endregion

            ToastProvider.ShowToast(Resources.DataImported);
        }

        private void dialogExportXml_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                // Save the map stats, entities, etc. to an XML file
                _universe.SaveXml(dialogExportXml.FileName);
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, Resources.FileNotSavable + "\n" + ex.Message, MsgSeverity.Warn);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, Resources.FileNotSavable + "\n" + ex.Message, MsgSeverity.Warn);
                return;
            }
            #endregion

            ToastProvider.ShowToast(Resources.SavedFile);
        }
        #endregion

        #endregion

        //--------------------//

        #region Positionable list
        private void PositionablesFilterEvent(object sender, EventArgs e)
        {
            UpdatePositionablesListBox();
        }

        /// <summary>
        /// Updates the content of <see cref="listBoxPositionables"/>.
        /// </summary>
        private void UpdatePositionablesListBox()
        {
            // Prevent unexpected loops, performance wasting, etc.
            _dontUpdatePositionableSelection = true;
            listBoxPositionables.BeginUpdate();

            // Update list entries
            listBoxPositionables.Items.Clear();
            foreach (var positionable in _universe.Positionables)
            {
                // Show an item in the ListBox if it is either already selected or passes the filter checks
                if (_presenter.SelectedPositionables.Contains(positionable) || IsPositionableListed(positionable))
                    listBoxPositionables.Items.Add(positionable);
            }

            // Update selection
            listBoxPositionables.ClearSelected();
            foreach (Positionable positionable in _presenter.SelectedPositionables)
                listBoxPositionables.SelectedItems.Add(positionable);

            // Restore deactivated stuff
            listBoxPositionables.EndUpdate();
            _dontUpdatePositionableSelection = false;
        }

        /// <summary>
        /// Determines whether a <see cref="Positionable"/> is to be displayed in the <see cref="listBoxPositionables"/> based on the current filter criteria.
        /// </summary>
        private bool IsPositionableListed(Positionable positionable)
        {
            if (!(positionable.Name ?? "").ContainsIgnoreCase(textSearch.Text)) return false;

            return (checkEntity.Checked && positionable is Entity) ||
                (checkBenchmarkPoint.Checked && positionable is BenchmarkPoint) ||
                (checkMemo.Checked && positionable is Memo);
        }
        #endregion

        #region Positionable selection
        /// <summary>
        /// Updates all UI controls related to selected <see cref="Positionable"/>s
        /// </summary>
        /// <remarks>Used by <see cref="OnUpdate"/>.</remarks>
        private void UpdateSelectionControls()
        {
            // Enable/disable buttons
            buttonRemove.Enabled = buttonCopy.Enabled = (_presenter.SelectedPositionables.Count > 0);

            // Enable/disable EntityTemplate selection
            _entityTemplateList.Enabled = (_presenter.SelectedPositionables.Entities.Count > 0);

            // Refresh PropertyGrid
            propertyGridPositionable.SelectedObjects = propertyGridPositionable.SelectedObjects;

            // Refresh the entity template selection tree list
            _dontSetEntityTemplate = true;
            _entityTemplateList.Nodes = (_entityTemplateList.Enabled ? TemplateManager.EntityTemplates : null);
            _entityTemplateList.SelectedEntry = GetCurrentEntityTemplate();
            _dontSetEntityTemplate = false;
        }

        private void listBoxPositionables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dontUpdatePositionableSelection) return;

            // Copy the currently selected array to a collection
            // ToDo: Make copying superflous
            var selectedEntities = new List<Positionable>(listBoxPositionables.SelectedItems.Count);
            selectedEntities.AddRange(listBoxPositionables.SelectedItems.Cast<Positionable>());

            // Overwrite the presenter's selection list with the new one
            _presenter.SelectedPositionables.SetMany(selectedEntities);
        }

        private void SelectedPositionables_Added(Positionable positionable)
        {
            // Update the list box without causing unexpected loops
            _dontUpdatePositionableSelection = true;
            listBoxPositionables.SelectedItems.Add(positionable);
            _dontUpdatePositionableSelection = false;

            // Copy the currently selected array to a collection and then add the new entity
            var selectedEntities = new List<object>(propertyGridPositionable.SelectedObjects) {positionable};

            // Convert the collection back to an array and update the PropertyGrid
            propertyGridPositionable.SelectedObjects = selectedEntities.ToArray();

            UpdatePositionablesListBox();
            UpdateSelectionControls();
        }

        private void SelectedPositionables_Removing(Positionable positionable)
        {
            // Update the list box without causing unexpected loops
            _dontUpdatePositionableSelection = true;
            listBoxPositionables.SelectedItems.Remove(positionable);
            _dontUpdatePositionableSelection = false;

            // Copy the currently selected array to a collection and then remove the entity
            var selectedEntities = new List<object>(propertyGridPositionable.SelectedObjects);
            selectedEntities.Remove(positionable);

            // Convert the collection back to an array and update the PropertyGrid
            propertyGridPositionable.SelectedObjects = selectedEntities.ToArray();
        }

        private void SelectedPositionables_Removed(Positionable positionable)
        {
            UpdatePositionablesListBox();
            UpdateSelectionControls();
        }
        #endregion

        #region Positionable modification
        private void propertyGridPositionable_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            // Add undo-entry for changed property
            ExecuteCommand(_propertyGridPositionableTracker.GetCommand(e.ChangedItem));
        }

        private void SelectedEntityTemplateChanged(object sender, EventArgs e)
        {
            if (_dontSetEntityTemplate || _entityTemplateList.SelectedEntry == null) return;

            // Set the new entity template for all currently selected bodies
            ExecuteCommand(new Commands.ChangeEntityTemplates(
                _presenter.SelectedPositionables.Entities, _entityTemplateList.SelectedEntry.Name));
        }
        #endregion

        //--------------------//

        #region Debug
        private void checkWireframe_CheckedChanged(object sender, EventArgs e)
        {
            _presenter.WireframeEntities = checkWireframe.Checked;
        }

        private void buttonDebug_Click(object sender, EventArgs e)
        {
            renderPanel.Engine.Debug();
        }
        #endregion
    }
}
