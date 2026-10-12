![OmegaEngine](https://github.com/omegaengine/omegaengine/raw/master/logo.png)

[![Build](https://github.com/omegaengine/omegaengine/actions/workflows/build.yml/badge.svg)](https://github.com/omegaengine/omegaengine/actions/workflows/build.yml)

OmegaEngine is a 3D graphics engine for .NET that you install as a NuGet package and drive entirely from C#. A few lines of code give you a 3D view, in its own window or embedded in your WinForms application.

**[Documentation](https://docs.omegaengine.de/)**

## Why OmegaEngine?

- **A library, not an IDE.** Reference a package, write code, press F5. Your project stays a normal C# project.
- **Embeds in your application.** [RenderPanel](https://docs.omegaengine.de/hosting.html#renderpanel) is a regular WinForms control, so a 3D view sits next to your other controls in editors, viewers and visualization tools.
- **From millimeters to planets.** Positions are stored with [64-bit precision](https://docs.omegaengine.de/api/OmegaEngine.Graphics.Cameras.html#floating-coordinate-system) and [distance compression](https://docs.omegaengine.de/scenes.html#distance-compression) keeps far-away objects in view, so one scene can hold both a spaceship and the planet it orbits.
- **Take only what you need.** Use the engine on its own, or add the complementary OmegaGUI, AlphaFramework and AlphaEditor to build a complete game with its own editor.
- **Moddable by default.** An [overlay filesystem](https://docs.omegaengine.de/api/OmegaEngine.Foundation.Storage.html) lets mods add or replace any content file without touching the original.
- **Small and undemanding.** About 8 MB including all dependencies, and it runs on every GPU with a Windows 10/11 driver.

See [Features](https://docs.omegaengine.de/features.html) for the full list and screenshots.

## First steps

The quickest start is one of the **[project templates](https://www.nuget.org/packages/OmegaEngine.Templates#readme-body-tab)**:

```shell
dotnet new install OmegaEngine.Templates
dotnet new omegaengine-winforms -o MyApp
```

To add the engine to a project of your own instead, create a WinForms project targeting .NET 10 (`net10.0-windows`).  
Add a reference to the NuGet package `OmegaEngine`. Then add the following code to render a textured sphere:

```csharp
var engine = new Engine(this, new EngineConfig { TargetSize = ClientSize });
var scene = new Scene
{
    Positionables = { Model.Sphere(engine, XTexture.Get(engine, "flag.png")) }
};
var view = new View(scene, new ArcballCamera()) { BackgroundColor = Color.CornflowerBlue };
engine.Views.Add(view);

Paint += delegate { engine.Render(); };
```

You additionally need to ensure this native dependency is installed or bundled with your application:

- [DirectX June 2010 Runtime](https://www.microsoft.com/en-us/download/details.aspx?id=8109) (provides `d3dx9_43.dll`)

The engine requires a graphics card with Shader Model 3.0 support. Every GPU with a Windows 10/11 driver meets this.

## NuGet packages

| Package                                                                                    | Description                                                         |
| ------------------------------------------------------------------------------------------ | ------------------------------------------------------------------- |
| [OmegaEngine](https://www.nuget.org/packages/OmegaEngine/)                                 | 3D graphics rendering based on DirectX 9.                           |
| [OmegaEngine.Foundation](https://www.nuget.org/packages/OmegaEngine.Foundation/)           | Rendering-agnostic infrastructure like storage and data structures. |
| [OmegaGUI](https://www.nuget.org/packages/OmegaGUI/)                                       | GUI toolkit with XML file format and Lua scripting.                 |
| [AlphaFramework.World](https://www.nuget.org/packages/AlphaFramework.World/)               | Basis for engine-agnostic models of game worlds.                    |
| [AlphaFramework.Presentation](https://www.nuget.org/packages/AlphaFramework.Presentation/) | Basis for presenters that visualize game worlds using the engine.   |
| [AlphaEditor](https://www.nuget.org/packages/AlphaEditor/)                                 | Toolkit for creating editors for games based on AlphaFramework.     |

## Sample game

**[Frame of Reference](https://github.com/omegaengine/omegaengine/tree/master/src/FrameOfReference)** is the official sample game for OmegaEngine. It is included in OmegaEngine source code but is not a part of the released library binaries.

## Source structure

| Path                        | Description                                                          |
| --------------------------- | -------------------------------------------------------------------- |
| `\build.ps1`                | A script that compiles the entire project                            |
| `\src\`                     | The actual source code in a Visual Studio project                    |
| `\shaders\`                 | Source code for HLSL shaders                                         |
| `\templates\`               | Source code for project templates                                    |
| `\doc\`                     | Files for creating source code documentation                         |
| `\content\`                 | Game content files (.X files, PNGs, ...)                             |
| `\artifacts\Debug\`         | The compiled debug binaries (created by `\src\build.ps1 Debug`)      |
| `\artifacts\Release\`       | The compiled release binaries (created by `\src\build.ps1 Release`)  |
| `\artifacts\Templates\`     | The packaged templates (created by `\templates\build.ps1`)           |
| `\artifacts\Documentation\` | The compiled source code documentation (created by `\doc\build.ps1`) |
