/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Storage;
using OmegaEngine;
using OmegaEngine.Foundation.Storage;
using OmegaGUI;
using OmegaGUI.Model;
using Control = OmegaGUI.Model.Control;
using GroupBox = OmegaGUI.Model.GroupBox;
using ListBox = OmegaGUI.Model.ListBox;

namespace AlphaFramework.Editor.Gui;

/// <summary>
/// Allows a user to design a GUI dialog
/// </summary>
public partial class GuiEditor : UndoCloneTab<Dialog>
{
    #region Variables
    private GuiManager _guiManager = null!;
    private DialogRenderer? _dialogRenderer;
    private Dialogs.AddControlTool? _addControlTool;

    /// <summary>The mouse coordinates where the picking started (i.e. where the mouse was first pressed)</summary>
    private Point _pickStart;

    /// <summary>The location of the mouse pointer the last time <see cref="renderPanel_MouseMove"/> was called</summary>
    private Point _lastLocation;

    /// <summary>The selection rectangle pulled by the mouse to be drawn by the <see cref="Engine"/>.</summary>
    private Rectangle _pickRectangle;

    /// <summary>The control currently being dragged/moved with the mouse</summary>
    private Control? _dragControl;
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
        TabClosed += delegate { _addControlTool?.Close(); };
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
                Log.Info($"Load file: {_fullPath}");
                Content = XmlStorage.LoadXml<Dialog>(_fullPath);
            }
            else
            { // Create new file
                Log.Info($"Create file: {_fullPath}");
                Content = new();
                Content.SaveXml(_fullPath);
            }
        }
        else
        { // File name only? Might not save to same dir loaded from!
            Log.Info($"Load file: {FilePath}");
            Content = Dialog.FromContent(FilePath);
            _fullPath = ContentManager.CreateFilePath("GUI", FilePath);
        }
        #endregion

        // Initialize engine
        var engine = renderPanel.Setup();

        // Blue background
        engine.Views.Add(new(Color.CornflowerBlue));

        // Render GUI
        engine.ExtraRender += DialogRender;
        _guiManager = new(engine);

        base.OnInitialize();
    }

    /// <inheritdoc/>
    protected override void OnSaveFile()
    {
        Log.Info($"Save file: {_fullPath}");
        string directory = Path.GetDirectoryName(_fullPath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        Content.SaveXml(_fullPath);

        base.OnSaveFile();
    }

    /// <inheritdoc/>
    protected override void OnDelete()
    {
        // Since this might be triggered by a hotkey instead of the actual button, we must check
        if (buttonRemove.Enabled)
        {
            foreach (object item in listBox.SelectedItems)
                Content.Controls.Remove((Control)item);
            OnChange();

            propertyGrid.SelectedObjects = null;
            OnUpdate();
        }
    }

    /// <inheritdoc/>
    protected override void OnUpdate()
    {
        Debug.Assert(renderPanel.Engine != null);

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
        listBox.Items.Add(Content);

        foreach (Control control in Content.Controls)
        {
            listBox.Items.Add(control);

            // Detect changes even where the PropertyGrid doesn't report back
            if (control is ListBox controlListBox) controlListBox.Items.Changed += OnChange;
            if (control is DropdownList controlComboBox) controlComboBox.Items.Changed += OnChange;
        }

        listBox.EndUpdate();
        #endregion

        // Restore the previously selected item if possible
        if (item != null && listBox.Items.Contains(item))
            listBox.SelectedItem = item;

        // Reset the GUI rendering system
        _dialogRenderer?.Dispose();
        _dialogRenderer = new(_guiManager, Content);
        renderPanel.Engine.Render();

        base.OnUpdate();
    }

    /// <inheritdoc/>
    protected override void OnClose()
    {
        // Shutdown GUI system
        _dialogRenderer?.Dispose();
        _guiManager.Dispose();

        base.OnClose();
    }
    #endregion

    #region Render control
    /// <summary>
    /// Performs GUI related rendering in the <see cref="Engine"/>
    /// </summary>
    private void DialogRender()
    {
        Debug.Assert(renderPanel.Engine != null);

        // Call the GUI render handler
        _dialogRenderer?.DialogRender.OnRender(1);

        // Display picking rectangles being pulled up by the mouse
        if (_pickRectangle != Rectangle.Empty)
            renderPanel.Engine.DrawRectangleOutline(_pickRectangle, Color.Black);

        // Display selection rectangles around controls
        foreach (var control in propertyGrid.SelectedObjects.OfType<Control>())
            renderPanel.Engine.DrawRectangleOutline(control.DrawBox, Color.Red);
    }
    #endregion

    //--------------------//

    #region Dialogs
    /// <summary>
    /// Helper function for configuring the <see cref="Dialogs.AddControlTool"/> form with event hooks.
    /// </summary>
    [MemberNotNull(nameof(_addControlTool))]
    private void SetupAddControlTool()
    {
        // Keep existing dialog instance
        if (_addControlTool != null) return;

        _addControlTool = new();
        _addControlTool.NewControl += (control =>
        { // Callback when the "Add" button is clicked
            Content.Controls.Add(control);
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
        Content.Controls.Remove((Control)listBox.SelectedItem);

        int insertIndex = listBox.SelectedIndex - 2;
        if (insertIndex < 0) insertIndex = Content.Controls.Count;
        Content.Controls.Insert(insertIndex, (Control)listBox.SelectedItem);
        OnChange();

        OnUpdate();
        listBox.SelectedIndex = insertIndex + 1;
    }

    private void buttonMoveDown_Click(object sender, EventArgs e)
    {
        Content.Controls.Remove((Control)listBox.SelectedItem);

        int insertIndex = listBox.SelectedIndex;
        if (insertIndex > Content.Controls.Count) insertIndex = 0;
        Content.Controls.Insert(insertIndex, (Control)listBox.SelectedItem);
        OnChange();

        OnUpdate();
        listBox.SelectedIndex = insertIndex + 1;
    }

    private void buttonCopy_Click(object sender, EventArgs e)
    {
        foreach (Control item in listBox.SelectedItems)
        {
            var clonedControl = item.Clone();
            clonedControl.Location = new();
            Content.Controls.Add(clonedControl);
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

        renderPanel.Engine?.Render();
    }

    private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
    {
        OnChange();

        if (Content.NeedsUpdate || e.ChangedItem.Label == "Name") OnUpdate();
        else renderPanel.Engine?.Render();

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
        if (Content is { EffectiveScale: 1 })
        {
            // Loop through all controls from back to front
            for (int i = Content.Controls.Count - 1; i >= 0; i--)
            {
                if (Content.Controls[i].DrawBox.Contains(_pickStart))
                {
                    _dragControl = Content.Controls[i];

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
                    _dragControl.Location = new(
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
            var selectedControls = Content.PickControls(Rectangle.FromLTRB(
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
