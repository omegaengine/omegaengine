/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using NanoByte.Common.Controls;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.LightSources;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Graphics.Shaders;
using View = OmegaEngine.Graphics.View;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine;

/// <summary>
/// Provides a debug interface for manipulating views, scenes, bodies and lights in the Engine in real-time
/// </summary>
internal partial class DebugForm : Form
{
    #region Variables
    private readonly Engine _engine;
    private readonly MonitoredCollection<PostShader> _shaders = [];
    private readonly MonitoredCollection<LightSource> _lights = [];

    /// <summary>
    /// Shows the render hierarchy. Created in code rather than by the WinForms Designer, which cannot handle generic controls.
    /// </summary>
    private readonly FilteredTreeView<RenderableEntry> _renderableTreeView = new()
    {
        Dock = DockStyle.Fill
    };

    /// <summary>
    /// The renderables currently shown in <see cref="_renderableTreeView"/>, in the order they are shown in.
    /// </summary>
    private NamedCollection<RenderableEntry> _renderableEntries = [];

    /// <summary>
    /// Wraps a <see cref="Renderable"/> for display in <see cref="_renderableTreeView"/>.
    /// </summary>
    /// <param name="name">The path of the renderable in the render hierarchy, with levels separated by <see cref="Named.TreeSeparator"/>.</param>
    /// <param name="renderable">The wrapped renderable.</param>
    private sealed class RenderableEntry(string name, Renderable renderable) : INamed
    {
        public string Name { get; set; } = name;

        public Renderable Renderable { get; } = renderable;
    }
    #endregion

    #region Properties
    /// <summary>The currently selected <see cref="View"/>; <c>null</c> if none.</summary>
    private View? CurrentView => viewListBox.SelectedItem as View;
    #endregion

    #region Constructor
    /// <summary>
    /// Makes <paramref name="listBox"/> mirror the contents of <paramref name="collection"/>.
    /// </summary>
    private static void ConnectCollectionToListBox<T>(MonitoredCollection<T> collection, ListBox listBox)
    {
        collection.Added += entry => listBox.Items.Add(entry);
        collection.Removing += entry => listBox.Items.Remove(entry);
    }

    /// <summary>
    /// Initializes the debug interface for a specific instance of the <see cref="Engine"/>.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> instance to debug.</param>
    public DebugForm(Engine engine)
    {
        InitializeComponent();
        renderableTreePanel.Controls.Add(_renderableTreeView);
        _renderableTreeView.SelectedEntryChanged += renderableTreeView_SelectedEntryChanged;

        _engine = engine;

        // Show the initial selection options
        UpdateViews();

        // Keep the collections and GUI in sync
        ConnectCollectionToListBox(_shaders, shaderListBox);
        ConnectCollectionToListBox(_lights, lightListBox);
    }
    #endregion

    //--------------------//

    #region Event hanlders
    private void viewListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        SelectView(CurrentView);
    }

    private void viewPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
    {
        UpdateViews();
    }

    private void shaderListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        HandleSelection(shaderListBox, shaderPropertyGrid);
    }

    private void renderablePropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
    {
        UpdateRenderables();
    }

    private void renderableTreeView_SelectedEntryChanged(object sender, EventArgs e)
    {
        ShowSelectedRenderable();
    }

    private void lightPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
    {
        UpdateLights();
    }

    private void lightListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        HandleSelection(lightListBox, lightPropertyGrid);
    }

    private void updateTimer_Tick(object sender, EventArgs e)
    {
        UpdateViews();
        UpdateShaders();
        UpdateRenderables();
        UpdateLights();
    }
    #endregion

    #region Update

    #region Views
    private void UpdateViews()
    {
        // Store currently selected view
        var lastView = CurrentView;

        // Fill the list box with the current list of views
        viewListBox.Items.Clear();
        foreach (var view in _engine.Views)
        {
            foreach (var childView in view.ChildViews)
                viewListBox.Items.Add(childView);
            viewListBox.Items.Add(view);
        }

        // Reselect the last view if it is still available
        if (lastView != null && viewListBox.Items.Contains(lastView))
        {
            viewListBox.SelectedItem = lastView;
            SelectView(lastView);

            if (cameraPropertyGrid.SelectedObject != lastView.Camera)
            {
                cameraPropertyGrid.SelectedObject = lastView.Camera;
                cameraLabel.Text = lastView.Camera.ToString();
            }
        }
        else SelectView(null);
    }
    #endregion

    #region Shaders
    private void UpdateShaders()
    {
        var currentView = CurrentView;
        if (currentView == null) _shaders.Clear();
        else
        {
            // Transfer the list en bloc, thereby updating the list box without loosing the current selection
            _shaders.SetMany(currentView.PostShaders);
        }
    }
    #endregion

    #region Renderables
    /// <summary>
    /// Rebuilds the render hierarchy and transfers it to <see cref="_renderableTreeView"/> if anything changed.
    /// </summary>
    private void UpdateRenderables()
    {
        var currentView = CurrentView;
        var entries = new NamedCollection<RenderableEntry>();
        if (currentView != null)
        {
            // Overlays and the skybox are not part of the hierarchy, so they stay at the top level
            var rootLabels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            AddRenderableEntries(currentView.FloatingModels, parentPath: null, rootLabels, entries);
            if (currentView.Scene.Skybox != null)
                AddRenderableEntries([currentView.Scene.Skybox], parentPath: null, rootLabels, entries);
            AddRenderableEntries(currentView.Scene.Positionables, parentPath: null, rootLabels, entries);
        }

        if (entries.Select(x => (x.Name, x.Renderable)).SequenceEqual(_renderableEntries.Select(x => (x.Name, x.Renderable)))) return;

        var selected = _renderableTreeView.SelectedEntry?.Renderable;
        _renderableEntries = entries;
        _renderableTreeView.Nodes = entries;

        // The tree view tracks the selection by path, which changes when a renderable is renamed or reparented
        var selectedEntry = entries.FirstOrDefault(x => x.Renderable == selected);
        if (_renderableTreeView.SelectedEntry != selectedEntry) _renderableTreeView.SelectedEntry = selectedEntry;
        ShowSelectedRenderable();
    }

    /// <summary>
    /// Adds entries for <paramref name="renderables"/> and all their children to <paramref name="entries"/>.
    /// </summary>
    /// <param name="renderables">The renderables to add.</param>
    /// <param name="parentPath">The path of the parent entry; <c>null</c> for the top level.</param>
    /// <param name="siblingLabels">The labels already used on this level of the hierarchy.</param>
    /// <param name="entries">The collection to add the entries to.</param>
    private static void AddRenderableEntries(IEnumerable<Renderable> renderables, string? parentPath, ISet<string> siblingLabels, NamedCollection<RenderableEntry> entries)
    {
        foreach (var renderable in renderables)
        {
            string label = GetUniqueLabel(renderable, siblingLabels);
            string path = parentPath == null ? label : parentPath + Named.TreeSeparator + label;
            entries.Add(new(path, renderable));

            if (renderable is PositionableRenderable positionable)
                AddRenderableEntries(positionable.Children, path, new HashSet<string>(StringComparer.OrdinalIgnoreCase), entries);
        }
    }

    /// <summary>
    /// Gets a label for <paramref name="renderable"/> that is not in <paramref name="siblingLabels"/> yet and adds it there.
    /// </summary>
    private static string GetUniqueLabel(Renderable renderable, ISet<string> siblingLabels)
    {
        // The separator would split the label into additional hierarchy levels
        string baseLabel = renderable.ToString().Replace(Named.TreeSeparator, '/');

        string label = baseLabel;
        for (int i = 2; !siblingLabels.Add(label); i++)
            label = $"{baseLabel} ({i})";
        return label;
    }

    /// <summary>
    /// Transfers the renderable selected in <see cref="_renderableTreeView"/> to <see cref="renderablePropertyGrid"/>.
    /// </summary>
    private void ShowSelectedRenderable()
    {
        var renderable = _renderableTreeView.SelectedEntry?.Renderable;
        if (renderablePropertyGrid.SelectedObject != renderable) renderablePropertyGrid.SelectedObject = renderable;
    }
    #endregion

    #region Lights
    private void UpdateLights()
    {
        var currentView = CurrentView;
        if (currentView == null) _lights.Clear();
        else
        {
            // Transfer the list en bloc, thereby updating the list box without loosing the current selection
            _lights.SetMany(currentView.Scene.Lights);
        }
    }
    #endregion

    #endregion

    #region Select

    #region View
    private void SelectView(View view)
    {
        // ReSharper disable RedundantComparisonWithNull
        dumpViewButton.Enabled = view is TextureView;
        // ReSharper restore RedundantComparisonWithNull

        if (viewPropertyGrid.SelectedObject != view)
        {
            viewPropertyGrid.SelectedObject = view;
            if (view == null)
            {
                cameraPropertyGrid.SelectedObject = null;
                cameraLabel.Text = "Camera:";
            }
            else
            {
                cameraPropertyGrid.SelectedObject = view.Camera;
                cameraLabel.Text = view.Camera.ToString();
            }
        }

        UpdateShaders();
        UpdateRenderables();
        UpdateLights();
    }
    #endregion

    #region Generic
    /// <summary>
    /// Transfers the selected objects from <paramref name="listBox"/> to <paramref name="propertyGrid"/>.
    /// </summary>
    private static void HandleSelection(ListBox listBox, PropertyGrid propertyGrid)
    {
        if (listBox.SelectedItems.Count == 0)
        {
            propertyGrid.SelectedObjects = null;
            return;
        }

        // Copy selected items to an array
        var array = new object[listBox.SelectedItems.Count];
        listBox.SelectedItems.CopyTo(array, 0);

        // Transfer the array to the property grid
        propertyGrid.SelectedObjects = array;
    }
    #endregion

    #endregion

    //--------------------//

    #region Buttons

    #region Frame log
    private void logFrameButton_Click(object sender, EventArgs e)
    {
        logFrameSaveFileDialog.ShowDialog();
    }

    private void logFrameSaveFileDialog_FileOk(object sender, CancelEventArgs e)
    {
        _engine.Performance.LogFrame(logFrameSaveFileDialog.FileName);
        logFrameSaveFileDialog.FileName = "";
    }
    #endregion

    #region Dump view
    private void dumpViewButton_Click(object sender, EventArgs e)
    {
        var view = CurrentView as TextureView;
        if (view?.GetRenderTarget() != null)
        {
            // Select target file
            dumpViewSaveFileDialog.ShowDialog();
        }
    }

    private void dumpViewSaveFileDialog_FileOk(object sender, CancelEventArgs e)
    {
        try
        {
            // Copy the render target surface to the file
            if (CurrentView is not TextureView view)
            {
                Msg.Inform(this, Resources.DumpFail, MsgSeverity.Warn);
                return;
            }

            Surface.ToFile(view.GetRenderTarget().Surface, dumpViewSaveFileDialog.FileName, ImageFileFormat.Png);
        }
        catch (Direct3D9Exception)
        {
            Msg.Inform(this, Resources.DumpFail, MsgSeverity.Warn);
        }

        dumpViewSaveFileDialog.FileName = "";
    }
    #endregion

    #region Post Screen Shaders
    private void addButton_Click(object sender, EventArgs e)
    {
        var currentView = CurrentView;
        if (currentView != null)
        {
            switch (Msg.YesNoCancel(this, "Make light source directional?", MsgSeverity.Info,
                        "Directional\nCreate a new directional light source",
                        "Point\nCreate a new point light source"))
            {
                case DialogResult.Yes:
                    currentView.Scene.Lights.Add(new DirectionalLight());
                    UpdateLights();
                    break;
                case DialogResult.No:
                    currentView.Scene.Lights.Add(new PointLight());
                    UpdateLights();
                    break;
            }
        }
    }

    private void removeButton_Click(object sender, EventArgs e)
    {
        var currentView = CurrentView;
        if (currentView == null) return;

        foreach (LightSource light in lightListBox.SelectedItems)
            currentView.Scene.Lights.Remove(light);
        UpdateLights();
    }
    #endregion

    #endregion
}
