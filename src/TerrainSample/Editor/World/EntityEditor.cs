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
using System.IO;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;
using AlphaEditor.Properties;
using Common;
using Common.Controls;
using Common.Values;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Input;
using Presentation;
using World;
using World.EntityComponents;
using World.Positionables;
using World.Templates;
using World.Terrains;
using Mesh = World.EntityComponents.Mesh;

namespace AlphaEditor.World
{
    /// <summary>
    /// Allows the user to edit <see cref="EntityTemplate"/>s
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "The designer-shim class doesn't add any complexity, it only prevents the WinForms designer from getting confused")]
    public partial class EntityEditor : EntityEditorDesignerShim
    {
        #region Variables
        private EditorPresenter _presenter;
        private Universe _universe;

        private Dialogs.AddRenderControlTool _addRenderControlTool;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new entity editor.
        /// </summary>
        public EntityEditor()
        {
            InitializeComponent();

            // Hard-coded filename
            FilePath = TemplateManager.EntityFileName;

            // Ugly hack to make mouse wheel work
            splitRender.Panel2.MouseMove += delegate { splitRender.Panel2.Focus(); };

            // Close dialogs when the owning tab closes
            TabClosed += delegate { if (_addRenderControlTool != null) _addRenderControlTool.Close(); };
        }
        #endregion

        //--------------------//

        #region Handlers
        /// <inheritdoc />
        protected override void OnInitialize()
        {
            // Load template lists before creating testing universe
            TemplateManager.LoadLists();

            // Create an empty testing universe with a plain terrain
            _universe = new Universe(new Terrain(new TerrainSize(27, 27, 30, 30))) {LightPhase = 2};

            base.OnInitialize();

            // Initialize engine
            renderPanel.Setup();

            // Activate some effects
            renderPanel.Engine.Anisotropic = true;
            renderPanel.Engine.Effects.PerPixelLighting = true;
            renderPanel.Engine.Effects.NormalMapping = checkNormalMapping.Checked;
            renderPanel.Engine.Effects.PostScreenEffects = true;
            renderPanel.Engine.Effects.WaterEffects = WaterEffectsType.ReflectAll;

            propertyGridUniverse.SelectedObject = _universe;
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            // Backup the previously selected class
            var selectedClass = TemplateList.SelectedEntry;

            // Setup the presentator on startup or when it was lost/reset
            if (_presenter == null)
            {
                _presenter = new EditorPresenter(renderPanel.Engine, _universe, true);
                _presenter.Initialize();
                _presenter.HookIn();

                renderPanel.AddInputReceiver(_presenter); // Update the presenter based on user input
                renderPanel.AddInputReceiver(new UpdateReceiver(() => renderPanel.Engine.Render())); // Force immediate redraw to give responsive feel
            }

            // Remove all previous entites from rendering universe
            _universe.Positionables.Clear();

            if (selectedClass == null)
            { // No class selected

                #region Render Control
                comboRender.Items.Clear();
                buttonAddRender.Enabled = buttonRemoveRender.Enabled = buttonBrowseRender.Enabled = false;
                propertyGridRender.SelectedObject = null;
                #endregion

                #region Collision Control
                buttonAddCollision.Enabled = buttonRemoveCollision.Enabled = false;
                propertyGridCollision.SelectedObject = null;
                labelCollision.Text = "";
                #endregion

                #region Movement Control
                buttonAddMovement.Enabled = buttonRemoveMovement.Enabled = false;
                propertyGridMovement.SelectedObject = null;
                labelMovement.Text = "";
                #endregion
            }
            else
            { // Class is selected

                #region Render Control
                buttonAddRender.Enabled = true;

                // Backup currently selected render controller before clearing list
                var prevRender = comboRender.SelectedItem;
                comboRender.Items.Clear();

                // List render controllers in drop-down combo-box
                foreach (var renderControl in selectedClass.RenderControls)
                    comboRender.Items.Add(renderControl);

                if (comboRender.Items.Count > 0)
                {
                    // Select previous render controller or first one in list
                    comboRender.SelectedItem = (prevRender != null && comboRender.Items.Contains(prevRender)) ?
                        prevRender : comboRender.Items[0];
                    buttonRemoveRender.Enabled = buttonBrowseRender.Enabled = true;
                }
                else
                { // No render controllers in the list
                    propertyGridRender.SelectedObject = null;
                    buttonRemoveRender.Enabled = buttonBrowseRender.Enabled = false;
                }
                #endregion

                #region Collision Control
                buttonAddCollision.Enabled = (selectedClass.CollisionControl == null);
                buttonRemoveCollision.Enabled = !buttonAddCollision.Enabled;
                propertyGridCollision.SelectedObject = selectedClass.CollisionControl;
                labelCollision.Text = (selectedClass.CollisionControl == null) ? "None" : selectedClass.CollisionControl.ToString();
                #endregion

                #region Movement Control
                buttonAddMovement.Enabled = (selectedClass.MovementControl == null);
                buttonRemoveMovement.Enabled = !buttonAddMovement.Enabled;
                propertyGridMovement.SelectedObject = selectedClass.MovementControl;
                labelMovement.Text = (selectedClass.MovementControl == null) ? "None" :
                    selectedClass.MovementControl.ToString();
                #endregion

                #region Setup sample rendering
                // Make sure device is ready before trying to setup rendering
                if (renderPanel.Engine.NeedsReset) return;

                // Add new Entity to Universe (Presenter will auto-update engine)
                try
                {
                    _universe.Positionables.Add(new Entity2D
                    {
                        Name = "Entity",
                        Position = _universe.Terrain.Center,
                        // Clone the class, so that in case it is changed the old version is still available for cleanup operations
                        TemplateData = TemplateList.SelectedEntry.Clone()
                    });
                }
                    #region Error handling
                catch (NotSupportedException)
                {
                    Msg.Inform(this, Resources.InvalidFilePath, MsgSeverity.Warn);
                }
                catch (FileNotFoundException ex)
                {
                    Msg.Inform(this, Resources.FileNotFound + "\n" + ex.FileName, MsgSeverity.Warn);
                }
                catch (IOException ex)
                {
                    Msg.Inform(this, Resources.FileNotLoadable + "\n" + ex.Message, MsgSeverity.Warn);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Msg.Inform(this, Resources.FileNotLoadable + "\n" + ex.Message, MsgSeverity.Warn);
                }
                catch (InvalidDataException ex)
                {
                    Msg.Inform(this, Resources.FileDamaged + "\n" + ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
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
            if (Templates.Contains(newName))
            {
                Msg.Inform(this, Resources.NameInUse, MsgSeverity.Warn);
                return;
            }
            #endregion

            var template = new EntityTemplate {Name = newName};
            Templates.Add(template);
            OnChange();
            OnUpdate();

            // Select the new entry
            TemplateList.SelectedEntry = template;
        }
        #endregion

        //--------------------//

        #region Dialogs
        /// <summary>
        /// Helper function for configuring the <see cref="Dialogs.AddRenderControlTool"/> form with event hooks.
        /// </summary>
        private void SetupAddRenderControlTool()
        {
            // Keep existing dialog instance
            if (_addRenderControlTool != null) return;

            _addRenderControlTool = new Dialogs.AddRenderControlTool();
            _addRenderControlTool.NewRenderControl += delegate(RenderControl renderControl)
            { // Callback when the "Add" button is clicked
                TemplateList.SelectedEntry.RenderControls.Add(renderControl);
                OnChange();

                // Select the newly added render control
                OnUpdate();
                comboRender.SelectedItem = renderControl;
            };

            // Clear the reference when the dialog is disposed
            _addRenderControlTool.Disposed += delegate { _addRenderControlTool = null; };
        }
        #endregion

        #region Class components

        #region Render Control
        private void buttonBrowseRender_Click(object sender, EventArgs e)
        {
            #region Test sphere
            var testSphere = comboRender.SelectedItem as TestSphere;
            if (testSphere != null)
            {
                // Get a particle system preset file path
                string path;
                if (!FileSelectorDialog.TryGetPath("Textures", ".*", out path)) return;

                // Apply the new texture file
                testSphere.Texture = path;
            }
                #endregion

            else
            {
                #region Mesh
                var mesh = comboRender.SelectedItem as Mesh;
                if (mesh != null)
                {
                    // Get a particle system preset file path
                    string path;
                    if (!FileSelectorDialog.TryGetPath("Meshes", ".x", out path)) return;

                    // Apply the new mesh file
                    mesh.Filename = path;
                }
                    #endregion

                else
                {
                    #region Particle system
                    var particleSystem = comboRender.SelectedItem as ParticleSystem;
                    if (particleSystem == null) return;

                    // Get a particle system preset file path
                    string path;
                    if (!FileSelectorDialog.TryGetPath("Graphics" + Path.DirectorySeparatorChar + particleSystem.GetType().Name, ".xml", out path)) return;

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
            // Setup a dialog for selecting the type of render control to add
            SetupAddRenderControlTool();

            // Display non-modal dialog (set this tab as owner on first show)
            if (_addRenderControlTool.Visible) _addRenderControlTool.Show();
            else _addRenderControlTool.Show(this);
        }

        private void buttonRemoveRender_Click(object sender, EventArgs e)
        {
            var renderControl = comboRender.SelectedItem as RenderControl;
            if (renderControl == null) return;

            TemplateList.SelectedEntry.RenderControls.Remove(renderControl);
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
                    TemplateList.SelectedEntry.CollisionControl = _presenter.GetCollisionCircle();
                    break;
                case DialogResult.No:
                    TemplateList.SelectedEntry.CollisionControl = _presenter.GetCollisionBox();
                    break;
                case DialogResult.Cancel:
                    return;
            }
            OnChange();

            OnUpdate();
        }

        private void buttonRemoveCollision_Click(object sender, EventArgs e)
        {
            TemplateList.SelectedEntry.CollisionControl = null;
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
            TemplateList.SelectedEntry.MovementControl = new MovementControl();
            OnChange();

            OnUpdate();
        }

        private void buttonRemoveMovement_Click(object sender, EventArgs e)
        {
            TemplateList.SelectedEntry.MovementControl = null;
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
            renderPanel.Engine.Debug();
        }

        private void checkBoxNormalMap_CheckedChanged(object sender, EventArgs e)
        {
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
            _presenter.View.SwingCameraTo(_presenter.CreateCamera(null), 1);
        }

        private void buttonOrthographicView_Click(object sender, EventArgs e)
        {
            _presenter.View.SwingCameraTo(new TrackCamera(50, 2000)
            {
                Name = "Orthographic",
                Target = _universe.Terrain.ToEngineCoords(_universe.Terrain.Center).Position + new DoubleVector3(0, 100, 0),
                Radius = 500
            }, 1);
        }
        #endregion
    }
}
