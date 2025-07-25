![OmegaEngine](logo.png)

[![Build](https://github.com/omegaengine/omegaengine/workflows/Build/badge.svg?branch=master)](https://github.com/omegaengine/omegaengine/actions?query=workflow%3ABuild)  
The OmegaEngine is a general-purpose 3D graphics engine written in C# using the .NET Framework 4.7.2 and DirectX 9. The engine is designed to be light-weight, modular and gameplay-agnostic.
The complementary OmegaGUI, AlphaFramework and AlphaEditor help you build games using the OmegaEngine.

**[Documentation](http://omegaengine.de/)**

## Code sample
Renders a textured sphere:
```csharp
var engine = new Engine(...);
var scene = new Scene
{
    Positionables = {Model.Sphere(engine, XTexture.Get(engine, "flag.png"))}
};
var view = new View(scene, new TrackCamera());
engine.Views.Add(view);
```

## Downloads

## Prerequisites

- [Visual C++ 2010 Redistributable x86](https://www.microsoft.com/en-us/download/details.aspx?id=26999)
- [DirectX June 2010 Runtime](https://www.microsoft.com/en-us/download/details.aspx?id=8109)
- [Visual Studio 2022 v17.13 or newer](https://www.visualstudio.com/downloads/)
- [DirectX SDK](https://www.microsoft.com/en-us/download/details.aspx?id=6812) (optional)

### OmegaEngine NuGet packages

[![API documentation](https://img.shields.io/badge/api-docs-orange.svg)](https://api.omegaengine.de/omegaengine/)

- **[OmegaEngine](http://www.nuget.org/packages/OmegaEngine/)** is the main package. If you are unsure where to start, this is a safe bet.
- **[OmegaGUI](http://www.nuget.org/packages/OmegaGUI/)** is a skinable GUI toolkit for the OmegaEngine with an XML file format und Lua scripting. The AlphaEditor contains a WYSIWYG editor for the toolkit.

### AlphaFramework NuGet packages

[![API documentation](https://img.shields.io/badge/api-docs-orange.svg)](https://api.omegaengine.de/alphaframework/)

The AlphaFramework is a Model-View-Presenter framework for creating game worlds based on OmegaEngine.
- **[AlphaFramework.World](http://www.nuget.org/packages/AlphaFramework.World/)** is used to build Models.
- The OmegaEngine acts as a View.
- **[AlphaFramework.Presentation](http://www.nuget.org/packages/AlphaFramework.Presentation/)** is used to build Presenters that bind Models to Views.
- **[AlphaEditor](http://www.nuget.org/packages/AlphaEditor/)** is an IDE-like editor for AlphaFramework games. You can use it to create GUI screens, maps, particle systems, etc..

### Visual Studio templates

The **[OmegaEngine Templates](https://visualstudiogallery.msdn.microsoft.com/65016a18-e699-47e8-ad91-114faf038d05)** Visual Studio extension can help you to quickly set up a suitable structure for an OmegaEngine project.

## Frame of Reference

**[Frame of Reference](https://github.com/omegaengine/omegaengine/tree/master/src/FrameOfReference)** is the official sample game for the OmegaEngine. It is included in the OmegaEngine source code but is not a part of the released library binaries.

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
