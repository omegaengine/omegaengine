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
using NanoByte.Common.Undo;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Foundation.Storage;

namespace AlphaFramework.Editor.Graphics;

/// <summary>
/// Allows a user to tweak the parameters for a <see cref="CpuParticleSystem"/>
/// </summary>
public partial class CpuParticleSystemEditor : ParticleSystemEditor
{
    #region Variables
    private CpuParticlePreset _preset;
    private CpuParticleSystem _particleSystem;
    private Scene _scene;
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new CPU particle system editor.
    /// </summary>
    /// <param name="filePath">The path to the file to be edited.</param>
    /// <param name="overwrite"><c>true</c> if an existing file supposed to be overwritten when <see cref="Tab.SaveFile"/> is called.</param>
    public CpuParticleSystemEditor(string filePath, bool overwrite)
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
                _preset = XmlStorage.LoadXml<CpuParticlePreset>(_fullPath);
            }
            else
            { // Create new file
                Log.Info("Create file: " + _fullPath);
                _preset = new();
                _preset.SaveXml(_fullPath);
            }
        }
        else
        { // File name only? Might not save to same dir loaded from!
            Log.Info("Load file: " + FilePath);
            _preset = CpuParticlePreset.FromContent(FilePath);
            _fullPath = ContentManager.CreateFilePath("Graphics/CpuParticleSystem", FilePath);
        }
        #endregion

        // Initialize engine
        renderPanel.Setup();

        // Setup scene
        _particleSystem = new(_preset);
        _scene = new() {Positionables = {_particleSystem}};
        renderPanel.Engine.Views.Add(new(_scene, Camera) {BackgroundColor = Color.Black});

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
        // Set up PropertyGrids
        propertyGridSystem.SelectedObject = _preset;
        propertyGridLower1.SelectedObject = _preset.LowerParameters1;
        propertyGridUpper1.SelectedObject = _preset.UpperParameters1;
        propertyGridLower2.SelectedObject = _preset.LowerParameters2;
        propertyGridUpper2.SelectedObject = _preset.UpperParameters2;

        // Update the engine particle system configuration
        _particleSystem.Preset = _preset;

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

    #region Reset
    private void buttonReset_Click(object sender, EventArgs e)
    {
        // Remove the old particle system...
        _scene.Positionables.Remove(_particleSystem);
        _particleSystem.Dispose();

        // ... and replace it with a new one using the same _preset
        _particleSystem = new(_preset);
        _scene.Positionables.Add(_particleSystem);
    }
    #endregion
}
