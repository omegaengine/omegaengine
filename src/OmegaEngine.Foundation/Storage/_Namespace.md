---
uid: OmegaEngine.Foundation.Storage
summary: The storage subsystem provides a unified way to locate, read, and write content files.
---
<xref:OmegaEngine.Foundation.Storage.ContentManager> lets you request files using a **type** and an **ID**.

* **Type**: determines name of subdirectory to look in
* **ID**: determines filename

The actual physical locations of these directories are abstracted.

**Directory structure example**

```text
content/
  Textures/
    Grass.png
  Meshes/
    Character.x
  Sounds/
    Bird.wav
```

## Base content

By default, content is loaded from a `content` directory next to the game executable.

### Environment variable

You can override the location using the environment variable `OMEGAENGINE_CONTENT`.

Multiple directories can be specified using `;` as a separator. Directories are checked in the order they are specified.

### Settings

You can override the location using [application settings](xref:AlphaFramework.Presentation.Config), but you need to explicitly wire this up in your `Program.cs`:

```csharp
if (Settings.Current.General.ContentDir is {} contentDir)
    ContentManager.BaseDir = new(Path.Combine(Locations.InstallBase, contentDir));
```

## Mods

Mods act as an override layer. Files in mod directories takes precedence over files with the same path in base content.

### Environment variable

You can specify the mod directory using the environment variable `OMEGAENGINE_CONTENT_MOD`.

Multiple directories can be specified using `;` as a separator. Directories are checked in the order they are specified.

### Command-line argument

You can specify the mod directory using command-line arguments, but you need to explicitly wire this up in your `Program.cs`:

```csharp
if (Arguments.GetOption("mod") is {} mod)
    ContentManager.ModDir = new(Path.Combine(Locations.InstallBase, "Mods", mod));
```

## Lookup order

When resolving a file, the system searches in this order:

1. Mod directories (in specified order)
2. Base content directories (in specified order)

The first match wins.

## API
