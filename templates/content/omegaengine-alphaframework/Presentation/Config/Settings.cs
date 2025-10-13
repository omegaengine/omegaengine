using System.Xml.Serialization;
using AlphaFramework.Presentation.Config;
using LuaInterface;
using NanoByte.Common;
using NanoByte.Common.Storage;

namespace Template.AlphaFramework.Presentation.Config;

/// <summary>
/// Stores settings for the application
/// </summary>
[XmlRoot("Settings")] // Suppress XMLSchema declarations (no inheritance used for properties)
public sealed class Settings : SettingsBase
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
            string settingsPath = Locations.GetSaveConfigPath("Template.AlphaFramework", true, "Settings.xml");
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
            string settingsPath = Locations.GetSaveConfigPath("Template.AlphaFramework", true, "Settings.xml");
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
        => Current.AddChangeHandler(SaveCurrent);
}
