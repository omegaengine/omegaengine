/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Storage.SlimDX;
using NanoByte.Common.Values;
using OmegaEngine;
using OmegaGUI;
using OmegaGUI.Model;
using Control = OmegaGUI.Model.Control;
using GroupBox = OmegaGUI.Model.GroupBox;
using ListBox = OmegaGUI.Model.ListBox;
using View = OmegaEngine.Graphics.View;

namespace AlphaFramework.Editor.Gui
{
    /// <summary>
    /// Allows a user to design a GUI dialog
    /// </summary>
    public partial class GuiEditor : UndoCloneTab
    {
        #region Variables
        private GuiManager _guiManager;
        private DialogRenderer _dialogRenderer;
        private Dialogs.AddControlTool _addControlTool;

        /// <summary>Wrapper to save you the trouble of casting all the time</summary>
        private Dialog DialogModel { get { return (Dialog)Content; } set { Content = value; } }

        /// <summary>The mouse coordinates where the picking started (i.e. where the mouse was first pressed)</summary>
        private Point _pickStart;

        /// <summary>The location of the mouse pointer the last time <see cref="renderPanel_MouseMove"/> was called</summary>
        private Point _lastLocation;

        /// <summary>The selection rectangle pulled by the mouse to be drawn by the <see cref="Engine"/>.</summary>
        private Rectangle _pickRectangle;

        /// <summary>The control currently being dragged/moved with the mouse</summary>
        private Control _dragControl;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new GUI editor.
        /// </summary>
        /// <param name="filePath">The path to the file to be edited.</param>
        /// <param name="overwrite"><c>true</c> if an existing file supposed to be overwritten when <see cref="Tab.SaveFile"/> is called.</param>
        public GuiEditor(string filePath, bool overwrite)
        {
            InitializeComponent();

            FilePath = filePath;
            _overwrite = overwrite;

            // Close dialogs when the owning tab closes
            TabClosed += delegate { if (_addControlTool != null) _addControlTool.Close(); };
        }
        #endregion

        //--------------------//

        #region Handlers
        /// <inheritdoc/>
        protected override void OnInitialize()
        {
            #region File handling
            if (Path.IsPathRooted(FilePath))
            {
                _fullPath = FilePath;
                if (!_overwrite && File.Exists(_fullPath))
                { // Load existing file
                    Log.Info("Load file: " + _fullPath);
                    DialogModel = XmlStorage.LoadXml<Dialog>(_fullPath);
                }
                else
                { // Create new file
                    Log.Info("Create file: " + _fullPath);
                    DialogModel = new Dialog();
                    DialogModel.SaveXml(_fullPath);
                }
            }
            else
            { // File name only? Might not save to same dir loaded from!
                Log.Info("Load file: " + FilePath);
                DialogModel = Dialog.FromContent(FilePath);
                _fullPath = ContentManager.CreateFilePath("GUI", FilePath);
            }
            #endregion

            // Initialize engine
            var engine = renderPanel.Setup();

            // Blue background
            engine.Views.Add(new View(Color.CornflowerBlue));

            // Render GUI
            engine.ExtraRender += DialogRender;
            _guiManager = new GuiManager(engine);

            base.OnInitialize();
        }

        /// <inheritdoc/>
        protected override void OnSaveFile()
        {
            Log.Info("Save file: " + _fullPath);
            string directory = Path.GetDirectoryName(_fullPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            DialogModel.SaveXml(_fullPath);

            base.OnSaveFile();
        }

        /// <inheritdoc/>
        protected override void OnDelete()
        {
            // Since this might be triggered by a hotkey instead of the actual button, we must check
            if (buttonRemove.Enabled)
            {
                foreach (object item in listBox.SelectedItems)
                    DialogModel.Controls.Remove((Control)item);
                OnChange();

                propertyGrid.SelectedObjects = null;
                OnUpdate();
            }
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            // After an update nothing is selected, so disable all buttons
            buttonRemove.Enabled = buttonCopy.Enabled = buttonMoveUp.Enabled = buttonMoveDown.Enabled = false;

            // Backup the previously selected item
            var item = listBox.SelectedItem;

            #region Fill ListBox with controls
            listBox.BeginUpdate();

            // Start with a blank slate
            propertyGrid.SelectedObjects = null;
            listBox.Items.Clear();

            // List the dialog itself as a control aswell
            listBox.Items.Add(DialogModel);

            foreach (Control control in DialogModel.Controls)
            {
                listBox.Items.Add(control);

                // Detect changes even where the PropertyGrid doesn't report back
                var controlListBox = control as ListBox;
                if (controlListBox != null) controlListBox.Items.Changed += OnChange;
                var controlComboBox = control as DropdownList;
                if (controlComboBox != null) controlComboBox.Items.Changed += OnChange;
            }

            listBox.EndUpdate();
            #endregion

            // Restore the previously selected item if possible
            if (item != null && listBox.Items.Contains(item))
                listBox.SelectedItem = item;

            // Reset the GUI rendering system
            if (_dialogRenderer != null) _dialogRenderer.Dispose();
            _dialogRenderer = new DialogRenderer(_guiManager, DialogModel);
            renderPanel.Engine.Render();

            base.OnUpdate();
        }

        /// <inheritdoc/>
        protected override void OnClose()
        {
            // Shutdown GUI system
            if (_dialogRenderer != null) _dialogRenderer.Dispose();
            if (_guiManager != null) _guiManager.Dispose();

            base.OnClose();
        }
        #endregion

        #region Render control
        /// <summary>
        /// Performs GUI related rendering in the <see cref="Engine"/>
        /// </summary>
        private void DialogRender()
        {
            // Call the GUI render handler
            if (_dialogRenderer != null) _dialogRenderer.DialogRender.OnRender(1);

            // Display picking rectangles being pulled up by the mouse
            if (_pickRectangle != Rectangle.Empty)
                renderPanel.Engine.DrawRectangleOutline(_pickRectangle, Color.Black);

            // Display selection rectangles around controls
            foreach (object obj in propertyGrid.SelectedObjects)
            {
                var control = obj as Control;
                if (control != null) renderPanel.Engine.DrawRectangleOutline(control.DrawBox, Color.Red);
            }
        }
        #endregion

        //--------------------//

        #region Dialogs
        /// <summary>
        /// Helper function for configuring the <see cref="Dialogs.AddControlTool"/> form with event hooks.
        /// </summary>
        private void SetupAddControlTool()
        {
            // Keep existing dialog instance
            if (_addControlTool != null) return;

            _addControlTool = new Dialogs.AddControlTool();
            _addControlTool.NewControl += (control =>
            { // Callback when the "Add" button is clicked
                DialogModel.Controls.Add(control);
                OnChange();

                // Select the newly added control
                OnUpdate();
                listBox.SelectedItems.Clear();
                listBox.SelectedItem = control;
            });

            // Clear the reference when the dialog is disposed
            _addControlTool.Disposed += delegate { _addControlTool = null; };
        }
        #endregion

        #region Buttons
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            // Setup a dialog for selecting the type of control to add
            SetupAddControlTool();

            // Display non-modal dialog (set this tab as owner on first show)
            if (_addControlTool.Visible) _addControlTool.Show();
            else _addControlTool.Show(this);
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void buttonMoveUp_Click(object sender, EventArgs e)
        {
            DialogModel.Controls.Remove((Control)listBox.SelectedItem);

            int insertIndex = listBox.SelectedIndex - 2;
            if (insertIndex < 0) insertIndex = DialogModel.Controls.Count;
            DialogModel.Controls.Insert(insertIndex, (Control)listBox.SelectedItem);
            OnChange();

            OnUpdate();
            listBox.SelectedIndex = insertIndex + 1;
        }

        private void buttonMoveDown_Click(object sender, EventArgs e)
        {
            DialogModel.Controls.Remove((Control)listBox.SelectedItem);

            int insertIndex = listBox.SelectedIndex;
            if (insertIndex > DialogModel.Controls.Count) insertIndex = 0;
            DialogModel.Controls.Insert(insertIndex, (Control)listBox.SelectedItem);
            OnChange();

            OnUpdate();
            listBox.SelectedIndex = insertIndex + 1;
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            foreach (object item in listBox.SelectedItems)
            {
                Control clonedControl = ((Control)item).Clone();
                clonedControl.Location = new Point();
                DialogModel.Controls.Add(clonedControl);
            }
            OnChange();

            OnUpdate();
        }
        #endregion

        #region Lists
        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonRemove.Enabled = buttonCopy.Enabled = buttonMoveUp.Enabled = buttonMoveDown.Enabled =
                (listBox.SelectedItem is Control);

            var selectedControls = new object[listBox.SelectedItems.Count];
            listBox.SelectedItems.CopyTo(selectedControls, 0);
            propertyGrid.SelectedObjects = selectedControls;

            renderPanel.Engine.Render();
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            OnChange();

            if (DialogModel.NeedsUpdate || e.ChangedItem.Label == "Name") OnUpdate();
            else renderPanel.Engine.Render();

            propertyGrid.Refresh();
        }
        #endregion

        #region Mouse

        #region MouseDown
        private void renderPanel_MouseDown(object sender, MouseEventArgs e)
        {
            _pickStart = _lastLocation = e.Location;

            bool accumulate = ModifierKeys.HasFlag(Keys.Control);

            // Switch to control dragging instead of selection if the mouse is pressed while above a control
            if (DialogModel != null && DialogModel.EffectiveScale == 1)
            {
                // Loop through all controls from back to front
                for (int i = DialogModel.Controls.Count - 1; i >= 0; i--)
                {
                    if (DialogModel.Controls[i].DrawBox.Contains(_pickStart))
                    {
                        _dragControl = DialogModel.Controls[i];

                        // Remove all previous selections unless the user wants to accumulate selections
                        if (!accumulate) listBox.ClearSelected();

                        // Toggle entries when accumulating
                        if (accumulate && listBox.SelectedItems.Contains(_dragControl)) listBox.SelectedItems.Remove(_dragControl);
                        else listBox.SelectedItems.Add(_dragControl);

                        // Only select top-most control
                        return;
                    }
                }
            }
        }
        #endregion

        #region MouseMove
        private void renderPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseButtons != MouseButtons.None && renderPanel.Engine != null)
            {
                if (_dragControl == null)
                {
                    // Update the selection rectangle while dragging
                    _pickRectangle = Rectangle.FromLTRB(
                        _pickStart.X < e.X ? _pickStart.X : e.X,
                        _pickStart.Y < e.Y ? _pickStart.Y : e.Y,
                        _pickStart.X > e.X ? _pickStart.X : e.X,
                        _pickStart.Y > e.Y ? _pickStart.Y : e.Y);
                }
                else
                {
                    // Determine how much the mouse has moved
                    var locationDelta = new Point(e.X - _lastLocation.X, e.Y - _lastLocation.Y);
                    if (_dragControl.AlignHorizontal == HorizontalMode.FromRight) locationDelta.X = -locationDelta.X;
                    if (_dragControl.AlignVertical == VerticalMode.FromBottom) locationDelta.Y = -locationDelta.Y;

                    // Only move in 2 pixel steps
                    if (Math.Abs(locationDelta.X) > 2 || Math.Abs(locationDelta.Y) > 2)
                    {
                        _lastLocation = e.Location;

                        // Move the control accordingly
                        _dragControl.Location = new Point(
                            _dragControl.Location.X + locationDelta.X,
                            _dragControl.Location.Y + locationDelta.Y);

                        // Select the moved control
                        listBox.ClearSelected();
                        listBox.SelectedItem = _dragControl;
                    }
                }

                renderPanel.Engine.Render();
            }
        }
        #endregion

        #region MouseUp
        private void renderPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (renderPanel.Engine == null) return;

            if (_dragControl == null)
            {
                // Select all controls within the rectangle
                var selectedControls = DialogModel.PickControls(Rectangle.FromLTRB(
                    _pickStart.X < e.X ? _pickStart.X : e.X,
                    _pickStart.Y < e.Y ? _pickStart.Y : e.Y,
                    _pickStart.X > e.X ? _pickStart.X : e.X,
                    _pickStart.Y > e.Y ? _pickStart.Y : e.Y));

                bool accumulate = ModifierKeys.HasFlag(Keys.Control);

                // Remove all previous selections unless the user wants to accumulate selections
                if (!accumulate) listBox.ClearSelected();

                // Transfer selection to the list box
                if (selectedControls.Count == 0)
                {
                    // Select dialog if no controls were selected
                    listBox.SelectedIndex = 0;
                }
                else
                {
                    foreach (var control in selectedControls)
                    {
                        // Don't select GroupBoxes in multi-selections
                        if (!(selectedControls.Count > 1 && control is GroupBox))
                        {
                            // Toggle entries when accumulating
                            if (accumulate && listBox.SelectedItems.Contains(control)) listBox.SelectedItems.Remove(control);
                            else listBox.SelectedItems.Add(control);
                        }
                    }
                }

                _pickRectangle = Rectangle.Empty;
            }
            else
            {
                // Check if the control was actually moved away from its initial position
                if (_pickStart != e.Location) OnChange();

                _dragControl = null;
            }

            renderPanel.Engine.Render();
        }
        #endregion

        #endregion
    }
}
