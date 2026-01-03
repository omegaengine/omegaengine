using System;

namespace AlphaFramework.Presentation.Config;

/// <summary>
/// Common settings used by most AlphaFramework applications.
/// </summary>
public abstract class SettingsBase
{
    /// <summary>Stores general game settings (UI language, difficulty level, etc.).</summary>
    public GeneralSettings General { get; set; } = new();

    /// <summary>Stores settings for the user controls (mouse, keyboard, etc.).</summary>
    public ControlsSettings Controls { get; set; } = new();

    /// <summary>Stores display settings (resolution, etc.). Changes here require the engine to be reset.</summary>
    public DisplaySettings Display { get; set; } = new();

    /// <summary>Stores graphics settings (effect details, etc.). Changes here don't require the engine to be reset.</summary>
    public GraphicsSettings Graphics { get; set; } = new();

    /// <summary>Stores settings for the game's editor.</summary>
    public EditorSettings Editor { get; set; } = new();

    /// <summary>
    /// Registers a handler to be invoked when any of the settings have been changed.
    /// </summary>
    protected virtual void AddChangeHandler(Action handler)
    {
        General.Changed += handler;
        Display.Changed += handler;
        Graphics.Changed += handler;
        Editor.Changed += handler;
    }
}
