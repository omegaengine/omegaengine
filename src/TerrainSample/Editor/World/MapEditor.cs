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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AlphaFramework.Editor;
using AlphaFramework.Editor.World.Commands;
using AlphaFramework.Editor.World.Dialogs;
using AlphaFramework.Editor.World.TerrainModifiers;
using AlphaFramework.World.Positionables;
using AlphaFramework.World.Templates;
using AlphaFramework.World.Terrains;
using Common;
using Common.Controls;
using Common.Storage;
using Common.Undo;
using Common.Utils;
using ICSharpCode.SharpZipLib.Zip;
using OmegaEngine.Input;
using SlimDX;
using TerrainSample.Presentation;
using TerrainSample.World;
using TerrainSample.World.Positionables;
using TerrainSample.World.Templates;
using Resources = AlphaFramework.Editor.Properties.Resources;

namespace TerrainSample.Editor.World
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
        private MapPropertiesTool _mapPropertiesTool;

        /// <summary>Don't handle <see cref="FilteredTreeView{T}.SelectedEntryChanged"/> event when <see langword="true"/>.</summary>
        private bool _dontSetEntityTemplate;

        /// <summary>Don't handle <see cref="ListBox.SelectedIndexChanged"/> event when <see langword="true"/>.</summary>
        private bool _dontUpdatePositionableSelection;

        /// <summary>Contains backups of <see cref="Positionable{TCoordinates}"/> property values for the undo system.</summary>
        private readonly MultiPropertyTracker _propertyGridPositionableTracker;

        // Don't use WinForms designer for this, since it doesn't understand generics
        private readonly FilteredTreeView<EntityTemplate> _entityTemplateList = new FilteredTreeView<EntityTemplate>
        {
            Dock = DockStyle.Fill,
            Enabled = false
        };

        // Don't use WinForms designer for this, since it doesn't understand arrays
        private readonly RadioButton[] _textureRadios = new RadioButton[16];
        private readonly Button[] _textureButtons = new Button[16];
        #endregion

        #region Properties
        /// <summary>
        /// The index of the currently selected <see cref="_textureRadios"/>.
        /// </summary>
        private byte SelectedTextureRadioIndex
        {
            get
            {
                for (byte i = 0; i < _textureRadios.Length; i++)
                    if (_textureRadios[i].Checked) return i;
                return 0;
            }
        }
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
            InitializeTextureComponents();

            _propertyGridPositionableTracker = new MultiPropertyTracker(propertyGridPositionable);

            tabPageTemplate.Controls.Add(_entityTemplateList);
            _entityTemplateList.SelectedEntryChanged += SelectedEntityTemplateChanged;

            FilePath = filePath;
            _overwrite = overwrite;

            // Close dialogs when the owning tab closes
            TabClosed += delegate { if (_mapPropertiesTool != null) _mapPropertiesTool.Close(); };

            // Ugly hack to make mouse wheel work
            splitVertical.Panel2.MouseMove += delegate { splitVertical.Panel2.Focus(); };
        }

        /// <summary>
        /// Generates the components for <see cref="tabPageTexture"/>.
        /// </summary>
        private void InitializeTextureComponents()
        {
            for (int i = 0; i < 16; i++)
            {
                _textureRadios[i] = new RadioButton {UseVisualStyleBackColor = true, AutoSize = true, Left = 6, Top = 6 + 16 * i};
                tabPageTexture.Controls.Add(_textureRadios[i]);

                _textureButtons[i] = new Button {UseVisualStyleBackColor = true, Size = new Size(28, 16), Left = 119, Top = 3 + 16 * i, Anchor = AnchorStyles.Top | AnchorStyles.Right, Text = @"..."};
                int templateIndex = i; // Copy to local variable to prevent modification by loop outside of closure
                _textureButtons[i].Click += delegate
                {
                    var dialog = new SelectTemplateDialog<TerrainTemplate>(Template<TerrainTemplate>.All);
                    if (dialog.ShowDialog() == DialogResult.OK)
                        ExecuteCommandSafe(new ChangeTerrainTemplate<TerrainTemplate>(_universe.Terrain, templateIndex, dialog.SelectedTemplate, _presenter.RebuildTerrain));
                };
                tabPageTexture.Controls.Add(_textureButtons[i]);
            }
            _textureRadios[0].Checked = true;
        }
        #endregion

        //--------------------//

        #region Handlers
        /// <inheritdoc />
        protected override void OnInitialize()
        {
            // Load template lists before touching any map files
            EntityTemplate.LoadAll();
            ItemTemplate.LoadAll();
            TerrainTemplate.LoadAll();

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
                    _universe = new Universe(new Terrain<TerrainTemplate>(TerrainSizeDialog.Create()));
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
            // Automatically update outdated shadow maps
            if (_universe.Terrain.LightAngleMapsOutdated || _universe.Terrain.LightRiseAngleMap == null || _universe.Terrain.LightSetAngleMap == null)
            {
                try
                {
                    var generator = LightAngleMapGenerator.FromTerrain(_universe.Terrain);
                    TrackingDialog.Run(this, generator);

                    _universe.Terrain.LightRiseAngleMap = generator.LightRiseAngleMap;
                    _universe.Terrain.LightSetAngleMap = generator.LightSetAngleMap;
                    _universe.Terrain.LightAngleMapsOutdated = false;
                }
                catch (OperationCanceledException)
                {}
            }

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

            ExecuteCommand(new RemovePositionables<Vector2>(_universe, _presenter.SelectedPositionables));
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            // Setup the presentator on startup or when it was lost/reset
            if (_presenter == null)
            {
                _presenter = new EditorPresenter(renderPanel.Engine, _universe, false)
                {
                    WireframeTerrain = checkWireframe.Checked
                };
                _presenter.Initialize();
                _presenter.HookIn();

                renderPanel.AddInputReceiver(_presenter); // Update the presenter based on user input
                renderPanel.AddInputReceiver(_updateReceiver = new UpdateReceiver(() => renderPanel.Engine.Render())); // Force immediate redraw to give responsive feel

                UpdatePaintingStatus(null, EventArgs.Empty);

                #region Monitoring events
                _presenter.SelectedPositionables.Added += SelectedPositionables_Added;
                _presenter.SelectedPositionables.Removing += SelectedPositionables_Removing;
                _presenter.SelectedPositionables.Removed += SelectedPositionables_Removed;
                _presenter.PostionableMove += presenter_PositionableMove;
                _presenter.TerrainPaint += presenter_TerrainPaint;
                #endregion
            }

            UpdatePositionablesListBox();
            UpdateSelectionControls();
            UpdateTextureControls();
            if (_mapPropertiesTool != null) _mapPropertiesTool.UpdateUniverse(_universe);

            UpdateXml();

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
        /// Helper function for configuring the <see cref="MapPropertiesTool"/> form with event hooks.
        /// </summary>
        private void SetupMapPropertiesTool()
        {
            // Keep existing dialog instance
            if (_mapPropertiesTool != null) return;

            _mapPropertiesTool = new MapPropertiesTool(_universe);
            _mapPropertiesTool.ExecuteCommand += ExecuteCommandSafe;

            // Clear the reference when the dialog is disposed
            _mapPropertiesTool.Disposed += delegate { _mapPropertiesTool = null; };
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Determines the <see cref="EntityTemplate"/> of the currently selected <see cref="EntityBase{TCoordinates,TTemplate}"/>s
        /// </summary>
        /// <returns>The <see cref="EntityTemplate"/> if all currently selected <see cref="EntityBase{TCoordinates,TTemplate}"/>s have the same; <see langword="null"/> otherwise.</returns>
        private EntityTemplate GetCurrentEntityTemplate()
        {
            EntityTemplate currentTemplate = null;
            foreach (var entity in _presenter.SelectedPositionables.OfType<Entity>())
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

        /// <summary>
        /// Determines the point on the <see cref="Terrain{TTemplate}"/> that is currently at the center of the screen
        /// </summary>
        /// <returns>The world coordinates of the <see cref="Terrain{TTemplate}"/> point currently at the center of the screen</returns>
        private Vector2 GetScreenTerrainCenter()
        {
            // ToDo: Place at center of screen
            //_presenter.View.Pick(_presenter.View.AreaCenter);

            return _universe.Terrain.Center;
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
        private void buttonImportHeightMap_Click(object sender, EventArgs e)
        {
            renderPanel.Engine.DebugClose(); // Always-on-top window might mess with common dialog

            // Use the filter stored in the tag as an identifier
            dialogImportHeightMap.FileName = "";
            dialogImportHeightMap.InitialDirectory = ContentManager.CreateDirPath("World/Maps");
            dialogImportHeightMap.ShowDialog(this);
        }

        private void buttonExportHeightMap_Click(object sender, EventArgs e)
        {
            renderPanel.Engine.DebugClose(); // Always-on-top window might mess with common dialog

            // Use the filter stored in the tag as an identifier
            dialogExportHeightMap.FileName = "";
            dialogExportHeightMap.InitialDirectory = ContentManager.CreateDirPath("World/Maps");
            dialogExportHeightMap.ShowDialog(this);
        }

        private void buttonImportTextureMap_Click(object sender, EventArgs e)
        {
            renderPanel.Engine.DebugClose(); // Always-on-top window might mess with common dialog

            // Use the filter stored in the tag as an identifier
            dialogImportTextureMap.FileName = "";
            dialogImportTextureMap.InitialDirectory = ContentManager.CreateDirPath("World/Maps");
            dialogImportTextureMap.ShowDialog(this);
        }

        private void buttonExportTextureMap_Click(object sender, EventArgs e)
        {
            renderPanel.Engine.DebugClose(); // Always-on-top window might mess with common dialog

            // Use the filter stored in the tag as an identifier
            dialogExportTextureMap.FileName = "";
            dialogExportTextureMap.InitialDirectory = ContentManager.CreateDirPath("World/Maps");
            dialogExportTextureMap.ShowDialog(this);
        }
        #endregion

        #region New
        private void buttonNewEntity_Click(object sender, EventArgs e)
        {
            // Ask the user which entity template to use as a basis for the new entity
            var selectTemplate = new SelectTemplateDialog<EntityTemplate>(Template<EntityTemplate>.All);
            if (selectTemplate.ShowDialog(this) != DialogResult.OK) return;

            // Create a new entity from the selected template
            var newEntity = new Entity {TemplateName = selectTemplate.SelectedTemplate, Position = GetScreenTerrainCenter()};

            // Add the new Entity to the Universe
            ExecuteCommandSafe(new AddPositionables<Vector2>(_universe, new[] { newEntity }));

            // Select the newly added entity
            _presenter.SelectedPositionables.Clear();
            _presenter.SelectedPositionables.Add(newEntity);
            tabControl.SelectedTab = tabPagePositionables;
        }

        private void buttonNewBenchmarkPoint_Click(object sender, EventArgs e)
        {
            // Create a benchmark point based on the current camera settings
            var cameraState = _presenter.CameraState;
            if (cameraState == null) return;
            var newEntity = new BenchmarkPoint<Vector2> {Position = cameraState.Position, Rotation = cameraState.Rotation, Radius = cameraState.Radius};

            // Add the new entity to the Universe
            ExecuteCommandSafe(new AddPositionables<Vector2>(_universe, new[] { newEntity }));

            // Select the newly added entity
            _presenter.SelectedPositionables.Clear();
            _presenter.SelectedPositionables.Add(newEntity);

            // Switch to the Entity Properties tab, to allow the user to customize the entity
            tabControl.SelectedTab = tabPagePositionables;
            tabControlEntities.SelectedTab = tabPageProperties;
        }
        #endregion

        #region Edit
        private void buttonCopy_Click(object sender, EventArgs e)
        {
            // Clone the selected entities
            var clonedEntities = new Positionable<Vector2>[_presenter.SelectedPositionables.Count];
            int i = 0;
            foreach (var positionable in _presenter.SelectedPositionables)
            {
                clonedEntities[i] = positionable.Clone();
                clonedEntities[i].Position = GetScreenTerrainCenter();
                i++;
            }

            // Add the new entities to the Universe
            ExecuteCommandSafe(new AddPositionables<Vector2>(_universe, clonedEntities));

            // Select the newly added entities
            _presenter.SelectedPositionables.Clear();
            foreach (var positionable in clonedEntities)
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
            // Setup a dialog for selecting the type of render component to add
            SetupMapPropertiesTool();

            // Display non-modal dialog (set this tab as owner on first show)
            if (_mapPropertiesTool.Visible) _mapPropertiesTool.Show();
            else _mapPropertiesTool.Show(this);
        }

        private void buttonCameraStatupPerspective_Click(object sender, EventArgs e)
        {
            if (!Msg.OkCancel(this, Resources.StoreCameraPerspective, MsgSeverity.Info, Resources.StoreCameraPerspectiveOK, Resources.StoreCameraPerspectiveCancel)) return;

            // ToDo: Simplify with static reflection in .NET 4.0
            var cameraStatePointer = new PropertyPointer<CameraState<Vector2>>(() => _universe.Camera, camera => _universe.Camera = camera);
            ExecuteCommand(new SetValueCommand<CameraState<Vector2>>(cameraStatePointer, _presenter.CameraState));
        }
        #endregion

        #endregion

        #region Import/Export

        #region Height-map
        private void dialogImportHeightMap_FileOk(object sender, CancelEventArgs e)
        {
            int undoCount = UndoBackups.Count;

            // Load the height-map from the PNG file but keep everything else
            ExecuteCommandSafe(new ImportHeightMap(
                _universe.Terrain, dialogImportHeightMap.FileName, _presenter.RebuildTerrain));

            if (UndoBackups.Count > undoCount)
                ToastProvider.ShowToast(Resources.DataImported);
        }

        private void dialogExportHeightMap_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                // Save the height-map to a PNG file
                _universe.Terrain.SaveHeightMap(dialogExportHeightMap.FileName);
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

        #region Texture-map
        private void dialogImportTextureMap_FileOk(object sender, CancelEventArgs e)
        {
            int undoCount = UndoBackups.Count;

            // Load the texture-map from the PNG file but keep everything else
            ExecuteCommandSafe(new ImportTextureMap(
                _universe.Terrain, dialogImportTextureMap.FileName, _presenter.RebuildTerrain));

            if (UndoBackups.Count > undoCount)
                ToastProvider.ShowToast(Resources.DataImported);
        }

        private void dialogExportTextureMap_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                // Save the texture-map to a PNG file
                _universe.Terrain.SaveTextureMap(dialogExportTextureMap.FileName);
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
            foreach (var positionable in _presenter.SelectedPositionables)
                listBoxPositionables.SelectedItems.Add(positionable);

            // Restore deactivated stuff
            listBoxPositionables.EndUpdate();
            _dontUpdatePositionableSelection = false;
        }

        /// <summary>
        /// Determines whether a <see cref="Positionable{TCoordinates}"/> is to be displayed in the <see cref="listBoxPositionables"/> based on the current filter criteria.
        /// </summary>
        private bool IsPositionableListed(Positionable<Vector2> positionable)
        {
            if (!(positionable.Name ?? "").ContainsIgnoreCase(textSearch.Text)) return false;

            return (checkEntity.Checked && positionable is Entity) ||
                   (checkWater.Checked && positionable is Water) ||
                   (checkWaypoint.Checked && positionable is Waypoint) ||
                   (checkBenchmarkPoint.Checked && positionable is BenchmarkPoint<Vector2>) ||
                   (checkMemo.Checked && positionable is Memo<Vector2>);
        }
        #endregion

        #region Positionable selection
        /// <summary>
        /// Updates all UI controls related to selected <see cref="Positionable{TCoordinates}"/>s
        /// </summary>
        /// <remarks>Used by <see cref="OnUpdate"/>.</remarks>
        private void UpdateSelectionControls()
        {
            // Enable/disable buttons
            buttonRemove.Enabled = buttonCopy.Enabled = (_presenter.SelectedPositionables.Count > 0);

            // Enable/disable EntityTemplate selection
            _entityTemplateList.Enabled = _presenter.SelectedPositionables.OfType<Entity>().Any();

            // Refresh PropertyGrid
            propertyGridPositionable.SelectedObjects = propertyGridPositionable.SelectedObjects;

            // Refresh the entity template selection tree list
            _dontSetEntityTemplate = true;
            _entityTemplateList.Nodes = (_entityTemplateList.Enabled ? Template<EntityTemplate>.All : null);
            _entityTemplateList.SelectedEntry = GetCurrentEntityTemplate();
            _dontSetEntityTemplate = false;
        }

        private void listBoxPositionables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dontUpdatePositionableSelection) return;

            // Copy the currently selected array to a collection
            var selectedEntities = new List<Positionable<Vector2>>(listBoxPositionables.SelectedItems.Count);
            selectedEntities.AddRange(listBoxPositionables.SelectedItems.Cast<Positionable<Vector2>>());

            // Overwrite the presenter's selection list with the new one
            _presenter.SelectedPositionables.SetMany(selectedEntities);
        }

        private void SelectedPositionables_Added(Positionable<Vector2> positionable)
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

        private void SelectedPositionables_Removing(Positionable<Vector2> positionable)
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

        private void SelectedPositionables_Removed(Positionable<Vector2> positionable)
        {
            UpdatePositionablesListBox();
            UpdateSelectionControls();
        }
        #endregion

        #region Positionable modification
        /// <summary>
        /// To be called when an <see cref="Positionable{TCoordinates}"/> is to be moved
        /// </summary>
        /// <param name="positionables">The <see cref="Positionable{TCoordinates}"/>s to be moved.</param>
        /// <param name="target">The terrain position to move the <paramref name="positionables"/> to</param>
        private void presenter_PositionableMove(IEnumerable<Positionable<Vector2>> positionables, Vector2 target)
        {
            ExecuteCommand(new MovePositionables(positionables, target));
        }

        private void propertyGridPositionable_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            // Add undo-entry for changed property
            ExecuteCommand(_propertyGridPositionableTracker.GetCommand(e.ChangedItem));
        }

        private void SelectedEntityTemplateChanged(object sender, EventArgs e)
        {
            if (_dontSetEntityTemplate || _entityTemplateList.SelectedEntry == null) return;

            // Set the new entity template for all currently selected bodies
            ExecuteCommandSafe(new ChangeEntityTemplates(
                _presenter.SelectedPositionables.OfType<ITemplated>(), _entityTemplateList.SelectedEntry.Name));
        }
        #endregion

        #region Terrain modification
        /// <summary>
        /// Updates the <see cref="_textureRadios"/> based on the <see cref="Terrain{TTemplate}.Templates"/>.
        /// </summary>
        private void UpdateTextureControls()
        {
            for (int i = 0; i < _textureRadios.Length; i++)
            {
                _textureRadios[i].Text = (i < 10 ? @"&" : "") + i + @" " +
                                         ((_universe.Terrain.Templates[i] == null) ? "(empty)" : _universe.Terrain.Templates[i].Name);
            }
        }

        /// <summary>
        /// Updates the <see cref="EditorPresenter.TerrainBrush"/> based on the current UI selections.
        /// </summary>
        private void UpdatePaintingStatus(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabPageHeight)
                _presenter.TerrainBrush = new TerrainBrush((int)upDownHeightSize.Value, radioHeightShapeCircle.Checked);
            else if (tabControl.SelectedTab == tabPageTexture)
                _presenter.TerrainBrush = new TerrainBrush((int)upDownTextureSize.Value, radioTextureShapeCircle.Checked);
            else _presenter.TerrainBrush = null;
        }

        private Base _mapModifier;

        /// <summary>
        /// Handles terrain painting (changing the height- or texture-map).
        /// </summary>
        /// <param name="terrainCoords">The terrain coordinates in world space. Use negative coordinates to indicate null-operations.</param>
        /// <param name="done">True when the user has finished his painting (i.e., released the mouse).</param>
        private void presenter_TerrainPaint(Vector2 terrainCoords, bool done)
        {
            if (_mapModifier == null)
            { // Begin new painting operation
                if (tabControl.SelectedTab == tabPageHeight)
                {
                    // Delay presenter reset until after the input event handling has finished,
                    // otherwise there would be an exception since resetting the presenter removes all input handlers
                    Action invokeResetPresenter = () => BeginInvoke((Action)ResetPresenter);

                    if (radioHeightRaise.Checked)
                        _mapModifier = new HeightShift(_universe.Terrain, _presenter.Terrain, invokeResetPresenter, (short)upDownHeightStrength.Value);
                    else if (radioHeightLower.Checked)
                        _mapModifier = new HeightShift(_universe.Terrain, _presenter.Terrain, invokeResetPresenter, (short)-upDownHeightStrength.Value);
                    else if (radioHeightSmooth.Checked)
                        _mapModifier = new HeightSmooth(_universe.Terrain, _presenter.Terrain, invokeResetPresenter, (double)upDownHeightStrength.Value / 6);
                    else if (radioHeightNoise.Checked)
                        _mapModifier = new HeightNoise(_universe.Terrain, _presenter.Terrain, invokeResetPresenter, (double)upDownHeightStrength.Value, (double)upDownHeightNoiseFrequency.Value);
                    else if (radioHeightPlateau.Checked)
                        _mapModifier = new HeightPlateau(_universe.Terrain, _presenter.Terrain, invokeResetPresenter);
                    else return;
                }
                else if (tabControl.SelectedTab == tabPageTexture)
                    _mapModifier = new Texture(_universe.Terrain, () => _presenter.RebuildTerrain(), SelectedTextureRadioIndex);
                else return;
            }

            // Negative coordinates indicate null operations
            if (terrainCoords.X >= 0 && terrainCoords.Y >= 0 && _presenter.TerrainBrush.HasValue)
                _mapModifier.Apply(terrainCoords, _presenter.TerrainBrush.Value);

            if (done)
            { // Finish painting operation
                ExecuteCommand(_mapModifier.GetCommand());
                _mapModifier = null;
            }
        }
        #endregion

        #region XML Editor
        private bool _suppressXmlUpdate;

        private void UpdateXml()
        {
            if (_suppressXmlUpdate) return;

            using (Entity.MaskTemplateData())
                xmlEditor.SetContent(_universe.ToXmlString(), "XML");
        }

        private void xmlEditor_ContentChanged(string text)
        {
            _suppressXmlUpdate = true;
            try
            {
                ExecuteCommand(new Commands.ImportXml(
                    getUniverse: () => _universe,
                    setUniverse: value =>
                    {
                        _universe = value;
                        if (_mapPropertiesTool != null) _mapPropertiesTool.UpdateUniverse(_universe);
                    },
                    xmlData: text,
                    refreshHandler: ResetPresenter));
                xmlEditor.TextEditor.Document.UndoStack.ClearAll();
            }
            finally
            {
                _suppressXmlUpdate = false;
            }
        }

        /// <inheritdoc />
        public override void Undo()
        {
            if (xmlEditor.TextEditor.EnableUndo) xmlEditor.TextEditor.Undo();
            else base.Undo();
        }

        /// <inheritdoc />
        public override void Redo()
        {
            if (xmlEditor.TextEditor.EnableRedo) xmlEditor.TextEditor.Redo();
            else base.Redo();
        }
        #endregion

        //--------------------//

        #region Debug
        private void checkWireframe_CheckedChanged(object sender, EventArgs e)
        {
            _presenter.WireframeTerrain = checkWireframe.Checked;
        }

        private void buttonDebug_Click(object sender, EventArgs e)
        {
            renderPanel.Engine.Debug();
        }
        #endregion
    }
}
