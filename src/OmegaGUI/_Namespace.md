---
uid: OmegaGUI
summary: *content
---
OmegaGUI is a GUI toolkit for OmegaEngine.

**NuGet package:** [OmegaGUI](https://www.nuget.org/packages/OmegaGUI/)

## Architecture

OmegaGUI separates GUI definitions from rendering:

- <xref:OmegaGUI.Model> contains classes for XML-serializable GUI descriptions. These define the structure and properties of dialogs and controls independent of rendering.
- <xref:OmegaGUI.Render> contains classes that perform the actual rendering of GUI elements defined in the Model namespace.

<xref:AlphaFramework.Editor> provides a WYSIWYG editor for creating and editing GUI dialogs visually.

## Scripting

OmegaGUI uses Lua scripting for event handling, allowing you to define interactive behavior without recompiling your application.
