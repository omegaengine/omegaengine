---
uid: AlphaFramework.Presentation.Config
summary: Configuration system for game and editor settings.
---
AlphaFramework's configuration system provides a structured way to manage game settings with automatic change notifications and persistence.

TODO: review

## Settings Structure

The configuration system is based on <xref:AlphaFramework.Presentation.Config.SettingsBase>, which organizes settings into logical groups:

- **<xref:AlphaFramework.Presentation.Config.GeneralSettings>** - General game settings (UI language, difficulty level, etc.)
- **<xref:AlphaFramework.Presentation.Config.ControlsSettings>** - User control bindings (mouse, keyboard, etc.)
- **<xref:AlphaFramework.Presentation.Config.DisplaySettings>** - Display configuration (resolution, fullscreen, etc.). Changes require engine reset.
- **<xref:AlphaFramework.Presentation.Config.GraphicsSettings>** - Graphics quality settings (anisotropic filtering, shadow quality, etc.). Changes don't require engine reset.
- **<xref:AlphaFramework.Presentation.Config.EditorSettings>** - Editor-specific settings

## Usage

Derive from `SettingsBase` to create your application's settings class:

```csharp
public class MyGameSettings : SettingsBase
{
    // Add game-specific settings groups here
}
```

Access and modify settings:

```csharp
var settings = new MyGameSettings();

// Modify graphics settings
settings.Graphics.Anisotropic = true;
settings.Graphics.ShadowQuality = ShadowQualityPreset.High;

// Modify display settings
settings.Display.Resolution = new Size(1920, 1080);
settings.Display.Fullscreen = true;
```

Settings automatically raise change notifications that can be used to update the engine configuration in real-time.

TODO: `Settings.Current`

## API
