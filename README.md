![OmegaEngine](logo.png)

[![Build](https://github.com/omegaengine/omegaengine/actions/workflows/build.yml/badge.svg)](https://github.com/omegaengine/omegaengine/actions/workflows/build.yml)

OmegaEngine is a general-purpose 3D graphics for .NET Framework and DirectX 9. The engine is designed to be light-weight, modular and gameplay-agnostic. It is designed to be:

 * light-weight (compiled binaries with external libraries < 4MB),
 * modular (use only the parts you need for your project) and
 * gameplay-agnostic (also suitable for visualization projects, etc.).

The complementary OmegaGUI, AlphaFramework and AlphaEditor help you build games using OmegaEngine.

**[Documentation](https://omegaengine.de/)**

## Prerequisites

You must install these components before you can use OmegaEngine:

- [Visual C++ 2010 Redistributable x86](https://www.microsoft.com/en-us/download/details.aspx?id=26999)
- [DirectX June 2010 Runtime](https://www.microsoft.com/en-us/download/details.aspx?id=8109)
- [Visual Studio 2022 v17.13 or newer](https://www.visualstudio.com/downloads/)

## First steps

Create a WinForms project targetting .NET Framework 4.7.2 or newer with the platform set to `x86`.  
Add a reference to the NuGet package `OmegaEngine`. Then add the following code to render a textured sphere:

```csharp
var engine = new Engine(this, new EngineConfig { TargetSize = ClientSize });
var scene = new Scene
{
    Positionables = { Model.Sphere(engine, XTexture.Get(engine, "flag.png")) }
};
var view = new View(scene, new TrackCamera()) { BackgroundColor = Color.CornflowerBlue };
engine.Views.Add(view);

Paint += delegate { engine.Render(); };
```

## NuGet packages

| Package                                                                                    | Description                                                          |
| ------------------------------------------------------------------------------------------ | -------------------------------------------------------------------- |
| [OmegaEngine](https://www.nuget.org/packages/OmegaEngine/)                                 | 3D graphics rendering based on DirectX 9.                            |
| [OmegaEngine.Foundation](https://www.nuget.org/packages/OmegaEngine.Foundation/)           | Rendering-agnostic infrastructure like storage  and data structures. |
| [OmegaGUI](https://www.nuget.org/packages/OmegaGUI/)                                       | GUI toolkit with XML file format und Lua scripting.                  |
| [AlphaFramework.World](https://www.nuget.org/packages/AlphaFramework.World/)               | Basis for engine-agnostic models of game worlds.                     |
| [AlphaFramework.Presentation](https://www.nuget.org/packages/AlphaFramework.Presentation/) | Basis for presenters that visualize game worlds using the engine.    |
| [AlphaEditor](https://www.nuget.org/packages/AlphaEditor/)                                 | Toolkit for creating editors for games based on AlphaFramework.      |

## Project templates

The **[project templates](https://www.nuget.org/packages/OmegaEngine.Templates#readme-body-tab)** help you create C# projects that use OmegaEngine, OmegaGUI and AlphaFramework.

## Sample game

**[Frame of Reference](https://github.com/omegaengine/omegaengine/tree/master/src/FrameOfReference)** is the official sample game for OmegaEngine. It is included in OmegaEngine source code but is not a part of the released library binaries.

## Source structure

| Path                        | Description                                                          |
| --------------------------- | -------------------------------------------------------------------- |
| `\build.ps1`                | A script that compiles the entire project                            |
| `\src\`                     | The actual source code in a Visual Studio project                    |
| `\templates\`               | Source code for project templates                                    |
| `\doc\`                     | Files for creating source code documentation                         |
| `\content\`                 | Game content files (.X files, PNGs, ...)                             |
| `\artifacts\Debug\`         | The compiled debug binaries (created by `\src\build.ps1 Debug`)      |
| `\artifacts\Release\`       | The compiled release binaries (created by `\src\build.ps1 Release`)  |
| `\artifacts\Templates\`     | The packaged templates (created by `\templates\build.ps1`)           |
| `\artifacts\Documentation\` | The compiled source code documentation (created by `\doc\build.ps1`) |
