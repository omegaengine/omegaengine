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
using OmegaEngine.Storage;
using Locations = NanoByte.Common.Storage.Locations;

namespace FrameOfReference.World.Config
{
    /// <summary>
    /// Stores settings for the application
    /// </summary>
    [XmlRoot("Settings")] // Supress XMLSchema declarations (no inheritance used for properties)
    public sealed class Settings
    {
        #region Properties
        /// <summary>
        /// The currently active set of settings
        /// </summary>
        public static Settings Current { get; private set; }

        /// <summary>
        /// Automatically save any changed settings?
        /// </summary>
        public static bool AutoSave { get; set; } = true;
        #endregion

        #region Constructor
        // Dummy constructor to prevent external instancing of this class
        private Settings()
        {}
        #endregion

        //--------------------//

        #region Storage

        #region Load
        /// <summary>
        /// Loads the current settings from an automatically located XML file
        /// </summary>
        /// <remarks>Any errors are logged and then ignored.</remarks>
        [LuaGlobal(Name = "LoadSettings", Description = "Loads the current settings from an automatically located XML file")]
        public static void LoadCurrent()
        {
            try
            {
                string settingsPath = Locations.GetSaveConfigPath(GeneralSettings.AppName, true, "Settings.xml");
                if (File.Exists(settingsPath))
                {
                    Current = XmlStorage.LoadXml<Settings>(settingsPath);
                    Log.Info("Loaded settings from " + settingsPath);
                    return;
                }
            }
                #region Error handling
            catch (IOException ex)
            {
                Log.Warn("Failed to load settings: " + ex.Message + "\nReverting to defaults");
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn("Insufficient rights to load settings: " + ex.Message + "\nReverting to defaults");
            }
            catch (InvalidDataException ex)
            {
                Log.Warn("Settings file damaged: " + ex.Message + "\nReverting to defaults");
            }
            #endregion

            Log.Info("Loaded default settings");
            Current = new();
        }
        #endregion

        #region Save
        /// <summary>
        /// Saves the current settings to an automatically located XML file
        /// </summary>
        /// <remarks>Any errors are logged and then ignored.</remarks>
        [LuaGlobal(Name = "SaveSettings", Description = "Saves the current settings to an automatically located XML file")]
        public static void SaveCurrent()
        {
            try
            {
                string settingsPath = Locations.GetSaveConfigPath(GeneralSettings.AppName, true, "Settings.xml");
                Current.SaveXml(settingsPath);
                Log.Info("Saved settings to " + settingsPath);
            }
                #region Error handling
            catch (IOException ex)
            {
                Log.Warn("Failed to save settings: " + ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn("Insufficient rights to save settings: " + ex.Message);
            }
            #endregion
        }
        #endregion

        #endregion

        #region Values
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        /// <summary>Stores general game settings (UI language, difficulty level, etc.).</summary>
        public GeneralSettings General = new();

        /// <summary>Stores settings for the user controls (mouse, keyboard, etc.).</summary>
        public ControlsSettings Controls = new();

        /// <summary>Stores display settings (resolution, etc.). Changes here require the engine to be reset.</summary>
        public DisplaySettings Display = new();

        /// <summary>Stores graphics settings (effect details, etc.). Changes here don't require the engine to be reset.</summary>
        public GraphicsSettings Graphics = new();

        /// <summary>Stores sound settings (turn music on or off, etc.).</summary>
        public SoundSettings Sound = new();

        /// <summary>Stores settings for the game's editor.</summary>
        public EditorSettings Editor = new();

        // ReSharper restore FieldCanBeMadeReadOnly.Global
        #endregion

        #region Config
        /// <summary>Contains a reference to the <see cref="ConfigForm"/> while it is open</summary>
        private ConfigForm _configForm;

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
        #endregion
    }
}
