---
uid: AlphaFramework.Presentation.Config
summary: Configuration system for game and editor settings.
---
AlphaFramework's configuration system provides a structured way to manage game settings with automatic change notifications and persistence.

## Usage

Derive from <xref:AlphaFramework.Presentation.Config.SettingsBase> to create your application's settings class:

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

## Current settings

Applications typically use a `Settings.Current` static property to provide global access to the current settings instance. This pattern allows settings to be accessed from anywhere in the application:

```csharp
public class Settings : SettingsBase
{
    public static Settings Current { get; set; } = new();
}

// Access from anywhere
if (Settings.Current.Graphics.Anisotropic)
{
    // Enable anisotropic filtering
}
```

This singleton-like pattern ensures consistent settings across the application and simplifies configuration management.

## API
