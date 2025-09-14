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
using System.IO;
using System.Xml.Serialization;
using LuaInterface;
using NanoByte.Common;
using OmegaEngine.Foundation.Storage;
using Locations = NanoByte.Common.Storage.Locations;

namespace FrameOfReference.World.Config;

/// <summary>
/// Stores settings for the application
/// </summary>
[XmlRoot("Settings")] // Suppress XMLSchema declarations (no inheritance used for properties)
public sealed class Settings
{
    private static Settings? _current;

    /// <summary>
    /// The currently active set of settings
    /// </summary>
    public static Settings Current => _current ?? LoadCurrent();

    // Dummy constructor to prevent external instancing of this class
    private Settings()
    {}

    /// <summary>
    /// Loads the current settings from an automatically located XML file
    /// </summary>
    /// <remarks>Any errors are logged and then ignored.</remarks>
    [LuaGlobal(Name = "LoadSettings", Description = "Loads the current settings from an automatically located XML file")]
    public static Settings LoadCurrent()
    {
        try
        {
            string settingsPath = Locations.GetSaveConfigPath(GeneralSettings.AppName, true, "Settings.xml");
            if (File.Exists(settingsPath))
            {
                _current = XmlStorage.LoadXml<Settings>(settingsPath);
                Log.Info($"Loaded settings from {settingsPath}");
                return _current;
            }
        }
        #region Error handling
        catch (IOException ex)
        {
            Log.Warn($"Failed to load settings: {ex.Message}\nReverting to defaults");
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Warn($"Insufficient rights to load settings: {ex.Message}\nReverting to defaults");
        }
        catch (InvalidDataException ex)
        {
            Log.Warn($"Settings file damaged: {ex.Message}\nReverting to defaults");
        }
        #endregion

        Log.Info("Loaded default settings");
        return _current = new();
    }

    /// <summary>
    /// Saves the current settings to an automatically located XML file
    /// </summary>
    /// <remarks>Any errors are logged and then ignored.</remarks>
    [LuaGlobal(Name = "SaveSettings", Description = "Saves the current settings to an automatically located XML file")]
    public static void SaveCurrent()
    {
        if (_current == null) return;

        try
        {
            string settingsPath = Locations.GetSaveConfigPath(GeneralSettings.AppName, true, "Settings.xml");
            _current.SaveXml(settingsPath);
            Log.Info($"Saved settings to {settingsPath}");
        }
        #region Error handling
        catch (IOException ex)
        {
            Log.Warn($"Failed to save settings: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Warn($"Insufficient rights to save settings: {ex.Message}");
        }
        #endregion
    }

    /// <summary>
    /// Configures <see cref="Current"/> settings to be automatically saved when they are changed.
    /// </summary>
    public static void EnableAutoSave()
    {
        Current.General.Changed += SaveCurrent;
        Current.Controls.Changed += SaveCurrent;
        Current.Display.Changed += SaveCurrent;
        Current.Graphics.Changed += SaveCurrent;
    }

    // ReSharper disable FieldCanBeMadeReadOnly.Global
    /// <summary>Stores general game settings (UI language, difficulty level, etc.).</summary>
    public GeneralSettings General = new();

    /// <summary>Stores settings for the user controls (mouse, keyboard, etc.).</summary>
    public ControlsSettings Controls = new();

    /// <summary>Stores display settings (resolution, etc.). Changes here require the engine to be reset.</summary>
    public DisplaySettings Display = new();

    /// <summary>Stores graphics settings (effect details, etc.). Changes here don't require the engine to be reset.</summary>
    public GraphicsSettings Graphics = new();

    /// <summary>Stores settings for the game's editor.</summary>
    public EditorSettings Editor = new();

    // ReSharper restore FieldCanBeMadeReadOnly.Global

    /// <summary>Contains a reference to the <see cref="ConfigForm"/> while it is open</summary>
    private ConfigForm? _configForm;

    /// <summary>
    /// Displays a configuration interface for the settings, allowing easy manipulation of values
    /// </summary>
    public void Config()
    {
        // Only create a new form if there isn't already one open
        if (_configForm == null)
        {
            _configForm = new(this);

            // Remove the reference as soon the form is closed
            _configForm.FormClosed += delegate { _configForm = null; };
        }

        _configForm.Show();
    }
}
