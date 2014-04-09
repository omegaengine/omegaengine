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
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Collections;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Graphics.Shaders;
using View = OmegaEngine.Graphics.View;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine
{
    /// <summary>
    /// Provides a debug interface for manipulating views, scenes, bodies and lights in the Engine in real-time
    /// </summary>
    internal partial class DebugForm : Form
    {
        #region Variables
        private readonly Engine _engine;
        private readonly MonitoredCollection<View> _views = new MonitoredCollection<View>();
        private readonly MonitoredCollection<PostShader> _shaders = new MonitoredCollection<PostShader>();
        private readonly MonitoredCollection<Renderable> _renderables = new MonitoredCollection<Renderable>();
        private readonly MonitoredCollection<LightSource> _lights = new MonitoredCollection<LightSource>();
        #endregion

        #region Properties
        /// <summary>The currently selected <see cref="View"/>; <see langword="null"/> if none.</summary>
        private View CurrentView { get { return viewListBox.SelectedItem as View; } }
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

            _engine = engine;

            // Show the initial selection options
            UpdateViews();

            // Keep the collections and GUI in sync
            ConnectCollectionToListBox(_views, viewListBox);
            ConnectCollectionToListBox(_shaders, shaderListBox);
            ConnectCollectionToListBox(_renderables, renderableListBox);
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

        private void renderableListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            HandleSelection(renderableListBox, renderablePropertyGrid);
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
        private void UpdateRenderables()
        {
            var currentView = CurrentView;
            var tempList = new LinkedList<Renderable>();
            if (currentView != null)
            {
                // Fill a temporary list with the current list of renderables
                foreach (var model in currentView.FloatingModels)
                    tempList.AddLast(model);
                if (currentView.Scene.Skybox != null)
                    tempList.AddLast(currentView.Scene.Skybox);
                foreach (var positionable in currentView.Scene.Positionables)
                    tempList.AddLast(positionable);
            }

            // Transfer the new list en bloc, thereby updating the list box without loosing the current selection
            _renderables.SetMany(tempList);
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
            dumpViewButton.Enabled = view != null && view is TextureView;
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
            if (view != null && view.GetRenderTarget() != null)
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
                var view = CurrentView as TextureView;
                if (view == null)
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
}
