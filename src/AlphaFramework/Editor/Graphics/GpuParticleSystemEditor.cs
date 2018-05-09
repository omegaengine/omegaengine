/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Undo;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Storage;
using View = OmegaEngine.Graphics.View;

namespace AlphaFramework.Editor.Graphics
{
    /// <summary>
    /// Allows a user to tweak the parameters for a <see cref="GpuParticleSystem"/>
    /// </summary>
    public partial class GpuParticleSystemEditor : ParticleSystemEditor
    {
        #region Variables
        private GpuParticlePreset _preset;
        private GpuParticleSystem _particleSystem;
        private Scene _scene;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new GPU particle system editor.
        /// </summary>
        /// <param name="filePath">The path to the file to be edited.</param>
        /// <param name="overwrite"><c>true</c> if an existing file supposed to be overwritten when <see cref="Tab.SaveFile"/> is called.</param>
        public GpuParticleSystemEditor(string filePath, bool overwrite)
        {
            InitializeComponent();

            FilePath = filePath;
            _overwrite = overwrite;
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
                    _preset = XmlStorage.LoadXml<GpuParticlePreset>(_fullPath);
                }
                else
                { // Create new file
                    Log.Info("Create file: " + _fullPath);
                    _preset = new GpuParticlePreset();
                    _preset.SaveXml(_fullPath);
                }
            }
            else
            { // File name only? Might not save to same dir loaded from!
                Log.Info("Load file: " + FilePath);
                _preset = GpuParticlePreset.FromContent(FilePath);
                _fullPath = ContentManager.CreateFilePath("Graphics/GpuParticleSystem", FilePath);
            }
            #endregion

            // Initialize engine
            renderPanel.Setup();

            // Setup scene
            _particleSystem = new GpuParticleSystem(_preset);
            _scene = new Scene {Positionables = {_particleSystem}};
            renderPanel.Engine.Views.Add(new View(_scene, Camera) {BackgroundColor = Color.Black});

            base.OnInitialize();
        }

        /// <inheritdoc/>
        protected override void OnSaveFile()
        {
            Log.Info("Save file: " + _fullPath);
            string directory = Path.GetDirectoryName(_fullPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            _preset.SaveXml(_fullPath);

            base.OnSaveFile();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            // Set up PropertyGrid
            propertyGridSystem.SelectedObject = _preset;

            // Update the engine particle system configuration
            if (_particleSystem != null) _particleSystem.Preset = _preset;

            base.OnUpdate();
        }
        #endregion

        //--------------------//

        #region Lists
        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            // Add undo-entry for changed property
            ExecuteCommandSafe(new PropertyChangedCommand(((PropertyGrid)s).SelectedObject, e));
        }
        #endregion
    }
}
