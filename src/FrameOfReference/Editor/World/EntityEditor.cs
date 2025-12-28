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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;
using AlphaFramework.Editor;
using AlphaFramework.Editor.Properties;
using AlphaFramework.Editor.World.Dialogs;
using AlphaFramework.World.Components;
using AlphaFramework.World.Templates;
using FrameOfReference.Presentation;
using FrameOfReference.World;
using FrameOfReference.World.Positionables;
using FrameOfReference.World.Templates;
using NanoByte.Common.Controls;
using OmegaEngine;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Foundation.Light;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Input;

namespace FrameOfReference.Editor.World;

/// <summary>
/// Allows the user to edit <see cref="EntityTemplate"/>s
/// </summary>
public partial class EntityEditor : EntityEditorDesignerShim
{
    #region Variables
    private EditorPresenter _presenter = null!;
    private Universe _universe = null!;

    private AddRenderComponentTool? _addRenderComponentTool;
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new entity editor.
    /// </summary>
    public EntityEditor()
    {
        InitializeComponent();

        // Hard-coded filename
        FilePath = Template<EntityTemplate>.FileName;

        // Ugly hack to make mouse wheel work
        splitRender.Panel2.MouseMove += delegate { splitRender.Panel2.Focus(); };

        // Close dialogs when the owning tab closes
        TabClosed += delegate { _addRenderComponentTool?.Close(); };
    }
    #endregion

    //--------------------//

    #region Handlers
    /// <inheritdoc/>
    protected override void OnInitialize()
    {
        EntityTemplate.LoadAll();
        TerrainTemplate.LoadAll();

        // Create an empty testing universe with a plain terrain
        _universe = new(new(new(27, 27, 30, 30))) {LightPhase = 1};

        base.OnInitialize();

        // Initialize engine
        renderPanel.Setup();
        renderPanel.MouseInputProvider.Scheme = MouseInputScheme.Planar;

        // Activate some effects
        renderPanel.Engine.Anisotropic = true;
        renderPanel.Engine.Effects.PerPixelLighting = true;
        renderPanel.Engine.Effects.NormalMapping = checkNormalMapping.Checked;
        renderPanel.Engine.Effects.PostScreenEffects = true;
        renderPanel.Engine.Effects.WaterEffects = WaterEffectsType.ReflectAll;

        propertyGridUniverse.SelectedObject = _universe;
    }

    /// <inheritdoc/>
    protected override void OnUpdate()
    {
        Debug.Assert(renderPanel.Engine != null);

        // Backup the previously selected class
        var selectedClass = TemplateList.SelectedEntry;

        // Setup the presenter on startup or when it was lost/reset
        if (_presenter == null)
        {
            _presenter = new(renderPanel.Engine, _universe, lighting: true);
            _presenter.Initialize();
            _presenter.HookIn();

            renderPanel.AddInputReceiver(_presenter); // Update the presenter based on user input
            renderPanel.AddInputReceiver(new ActionReceiver(() => renderPanel.Engine.Render())); // Force immediate redraw to give responsive feel
        }

        // Remove all previous entities from rendering universe
        _universe.Positionables.Clear();

        if (selectedClass == null)
        { // No class selected

            #region Render component
            comboRender.Items.Clear();
            buttonAddRender.Enabled = buttonRemoveRender.Enabled = buttonBrowseRender.Enabled = false;
            propertyGridRender.SelectedObject = null;
            #endregion

            #region Collision component
            buttonAddCollision.Enabled = buttonRemoveCollision.Enabled = false;
            propertyGridCollision.SelectedObject = null;
            labelCollision.Text = "";
            #endregion

            #region Movement compnent
            buttonAddMovement.Enabled = buttonRemoveMovement.Enabled = false;
            propertyGridMovement.SelectedObject = null;
            labelMovement.Text = "";
            #endregion
        }
        else
        { // Class is selected

            #region Render component
            buttonAddRender.Enabled = true;

            // Backup currently selected component controller before clearing list
            var prevRender = comboRender.SelectedItem;
            comboRender.Items.Clear();

            // List render components in drop-down combo-box
            foreach (var render in selectedClass.Render)
                comboRender.Items.Add(render);

            if (comboRender.Items.Count > 0)
            {
                // Select previous render component or first one in list
                comboRender.SelectedItem = (prevRender != null && comboRender.Items.Contains(prevRender)) ?
                    prevRender : comboRender.Items[0];
                buttonRemoveRender.Enabled = buttonBrowseRender.Enabled = true;
            }
            else
            { // No render components in the list
                propertyGridRender.SelectedObject = null;
                buttonRemoveRender.Enabled = buttonBrowseRender.Enabled = false;
            }
            #endregion

            #region Collision Control
            buttonAddCollision.Enabled = (selectedClass.Collision == null);
            buttonRemoveCollision.Enabled = !buttonAddCollision.Enabled;
            propertyGridCollision.SelectedObject = selectedClass.Collision;
            labelCollision.Text = selectedClass.Collision?.ToString() ?? "None";
            #endregion

            #region Movement Control
            buttonAddMovement.Enabled = (selectedClass.Movement == null);
            buttonRemoveMovement.Enabled = !buttonAddMovement.Enabled;
            propertyGridMovement.SelectedObject = selectedClass.Movement;
            labelMovement.Text = selectedClass.Movement?.ToString() ?? "None";
            #endregion

            #region Setup sample rendering
            // Make sure device is ready before trying to setup rendering
            if (renderPanel.Engine.NeedsReset) return;

            // Add new Entity to Universe (Presenter will auto-update engine)
            try
            {
                _universe.Positionables.Add(new Entity
                {
                    Name = "Entity",
                    Position = _universe.Terrain.Center,
                    // Clone the class, so that in case it is changed the old version is still available for cleanup operations
                    TemplateData = TemplateList.SelectedEntry!.Clone()
                });
            }
            #region Error handling
            catch (NotSupportedException)
            {
                Msg.Inform(this, Resources.InvalidFilePath, MsgSeverity.Warn);
            }
            catch (FileNotFoundException ex)
            {
                Msg.Inform(this, $"{Resources.FileNotFound}\n{ex.FileName}", MsgSeverity.Warn);
            }
            catch (IOException ex)
            {
                Msg.Inform(this, $"{Resources.FileNotLoadable}\n{ex.Message}", MsgSeverity.Warn);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, $"{Resources.FileNotLoadable}\n{ex.Message}", MsgSeverity.Warn);
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, $"{Resources.FileDamaged}\n{ex.Message}{(ex.InnerException == null ? "" : $"\n{ex.InnerException.Message}")}", MsgSeverity.Error);
            }
            #endregion

            #endregion
        }

        renderPanel.Engine.Render();

        base.OnUpdate();
    }

    /// <summary>
    /// Called when the user wants a new <see cref="EntityTemplate"/> to be added.
    /// </summary>
    protected override void OnNewTemplate()
    {
        string newName = InputBox.Show(this, "Entity Type Name", Resources.EnterTemplateName);

        #region Error handling
        if (string.IsNullOrEmpty(newName)) return;
        if (Content.Contains(newName))
        {
            Msg.Inform(this, Resources.NameInUse, MsgSeverity.Warn);
            return;
        }
        #endregion

        var template = new EntityTemplate {Name = newName};
        Content.Add(template);
        OnChange();
        OnUpdate();

        // Select the new entry
        TemplateList.SelectedEntry = template;
    }
    #endregion

    //--------------------//

    #region Dialogs
    /// <summary>
    /// Helper function for configuring the <see cref="AddRenderComponentTool"/> form with event hooks.
    /// </summary>
    [MemberNotNull(nameof(_addRenderComponentTool))]
    private void SetupAddRenderComponentTool()
    {
        // Keep existing dialog instance
        if (_addRenderComponentTool != null) return;

        _addRenderComponentTool = new();
        _addRenderComponentTool.NewRenderComponent += delegate(Render render)
        { // Callback when the "Add" button is clicked
            TemplateList.SelectedEntry!.Render.Add(render);
            OnChange();

            // Select the newly added render component
            OnUpdate();
            comboRender.SelectedItem = render;
        };

        // Clear the reference when the dialog is disposed
        _addRenderComponentTool.Disposed += delegate { _addRenderComponentTool = null; };
    }
    #endregion

    #region Class components

    #region Render component
    private void buttonBrowseRender_Click(object sender, EventArgs e)
    {
        #region Test sphere
        if (comboRender.SelectedItem is TestSphere testSphere)
        {
            // Get a particle system preset file path
            if (!FileSelectorDialog.TryGetPath("Textures", ".*", out string path)) return;

            // Apply the new texture file
            testSphere.Texture = path;
        }
        #endregion

        else
        {
            #region Mesh
            if (comboRender.SelectedItem is Mesh mesh)
            {
                // Get a particle system preset file path
                if (!FileSelectorDialog.TryGetPath("Meshes", ".x", out string path)) return;

                // Apply the new mesh file
                mesh.Filename = path;
            }
            #endregion

            else
            {
                #region Particle system
                if (comboRender.SelectedItem is not ParticleSystem particleSystem) return;

                // Get a particle system preset file path
                if (!FileSelectorDialog.TryGetPath($"Graphics{Path.DirectorySeparatorChar}{particleSystem.GetType().Name}", ".xml", out string path)) return;

                // Apply the new preset file
                particleSystem.Filename = path;
                #endregion
            }
        }
        OnChange();
        OnUpdate();
    }

    private void buttonAddRender_Click(object sender, EventArgs e)
    {
        // Setup a dialog for selecting the type of render component to add
        SetupAddRenderComponentTool();

        // Display non-modal dialog (set this tab as owner on first show)
        if (_addRenderComponentTool.Visible) _addRenderComponentTool.Show();
        else _addRenderComponentTool.Show(this);
    }

    private void buttonRemoveRender_Click(object sender, EventArgs e)
    {
        if (comboRender.SelectedItem is not Render render) return;

        TemplateList.SelectedEntry.Render.Remove(render);
        OnChange();

        OnUpdate();
    }

    private void propertyGridRender_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
    {
        OnChange();

        // Render-relevant properties may have changed
        OnUpdate();
    }

    private void comboRender_SelectedIndexChanged(object sender, EventArgs e)
    {
        propertyGridRender.SelectedObject = comboRender.SelectedItem;
    }
    #endregion

    #region Collision Control
    private void buttonAddCollision_Click(object sender, EventArgs e)
    {
        switch (Msg.YesNoCancel(this, "Add a circle collision body?", MsgSeverity.Info,
                    "Circle\nCreate a new circle collision body",
                    "Box\nCreate a new box collision body"))
        {
            case DialogResult.Yes:
                TemplateList.SelectedEntry!.Collision = _presenter.GetCollisionCircle();
                break;
            case DialogResult.No:
                TemplateList.SelectedEntry!.Collision = _presenter.GetCollisionBox();
                break;
            case DialogResult.Cancel:
                return;
        }
        OnChange();

        OnUpdate();
    }

    private void buttonRemoveCollision_Click(object sender, EventArgs e)
    {
        TemplateList.SelectedEntry.Collision = null;
        OnChange();

        OnUpdate();
    }

    private void propertyGridCollision_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
    {
        OnChange();

        // Render-relevant properties may have changed
        OnUpdate();
    }
    #endregion

    #region Movement Control
    private void buttonAddMovement_Click(object sender, EventArgs e)
    {
        TemplateList.SelectedEntry.Movement = new();
        OnChange();

        OnUpdate();
    }

    private void buttonRemoveMovement_Click(object sender, EventArgs e)
    {
        TemplateList.SelectedEntry.Movement = null;
        OnChange();

        OnUpdate();
    }

    private void propertyGridMovement_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
    {
        OnChange();
    }
    #endregion

    #endregion

    #region Test settings
    private void buttonDebug_Click(object sender, EventArgs e)
    {
        Debug.Assert(renderPanel.Engine != null);
        renderPanel.Engine.Debug();
    }

    private void checkBoxNormalMap_CheckedChanged(object sender, EventArgs e)
    {
        Debug.Assert(renderPanel.Engine != null);
        renderPanel.Engine.Effects.NormalMapping = checkNormalMapping.Checked;
    }

    private void checkWireframe_CheckedChanged(object sender, EventArgs e)
    {
        _presenter.WireframeEntities = checkWireframe.Checked;
    }

    private void checkBoundingSphere_Click(object sender, EventArgs e)
    {
        _presenter.BoundingSphereEntities = checkBoundingSphere.Checked;
    }

    private void checkBoundingBox_Click(object sender, EventArgs e)
    {
        _presenter.BoundingBoxEntities = checkBoundingBox.Checked;
    }
    #endregion

    #region Camera mode
    private void buttonNormalView_Click(object sender, EventArgs e)
    {
        renderPanel.MouseInputProvider.Scheme = MouseInputScheme.Planar;
        _presenter.SwingCameraTo();
    }

    private void buttonOrthographicView_Click(object sender, EventArgs e)
    {
        renderPanel.MouseInputProvider.Scheme = MouseInputScheme.Scene;
        _presenter.View.TransitionCameraTo(new ArcballCamera()
        {
            Name = "Orthographic",
            Target = _universe.Terrain.ToEngineCoords(_universe.Terrain.Center) + new DoubleVector3(0, 100, 0),
            MinRadius = 50,
            MaxRadius = 2000,
            Radius = 500
        });
    }
    #endregion
}
