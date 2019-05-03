![OmegaEngine](logo.png)

[![Build status](https://img.shields.io/appveyor/ci/omegaengine/omegaengine.svg)](https://ci.appveyor.com/project/omegaengine/omegaengine)  
The OmegaEngine is a general-purpose 3D graphics engine written in C# using the .NET Framework 2.0 and DirectX 9 via [SlimDX](http://slimdx.org/). The engine is designed to be light-weight, modular and gameplay-agnostic.
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

Prerequisites:
- [DirectX June 2010 Runtime](http://omegaengine.de/support/directx-jun2010-minimal.exe) (run-time dependency)
- [Visual Studio Community 2017 or newer](http://www.visualstudio.com/downloads/download-visual-studio-vs) (build-time dependency)
- [DirectX SDK](https://www.microsoft.com/en-us/download/details.aspx?id=23549) (build-time dependency)

### OmegaEngine NuGet packages

[![API documentation](https://img.shields.io/badge/api-docs-orange.svg)](https://api.omegaengine.de/omegaengine/)

- **[OmegaEngine](http://www.nuget.org/packages/OmegaEngine/)** is the main package. If you are unsure where to start, this is a safe bet.
- **[OmegaEngine.Backend](http://www.nuget.org/packages/OmegaEngine.Backend/)** contains the actual library binaries for the OmegaEngine without the default assets (content and shader files). It is automatically included by the main package. Use this package directly if another project in your solution with the same build output directory already references the main package to avoid duplicating the assets.
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

[![API documentation](https://img.shields.io/badge/api-docs-orange.svg)](https://api.omegaengine.de/frame-of-reference/)

"Frame of Reference" is the official sample game for the OmegaEngine. It is [included in the OmegaEngine source code](src/FrameOfReference/) but is not a part of the released library binaries.  
The `FrameOfReference\Game` project places the files `_portable` and `config\Settings.xml` in the build directories which together cause the game content files to be loaded from `\content\`.  
When releasing the binaries as standalone applications these files are not present and game content files are instead expected be located in a subdirectory of the installation path named `content`.

To open the Debug Console when running the sample project press `Ctrl + Alt + Shift + D`.

Command-line arguments for the sample project:

| Usage             | Description                                             |
| ----------------- | ------------------------------------------------------- |
| `/map MapName`    | Loads *MapName* in normal game mode                     |
| `/modify MapName` | Loads *MapName* in modification mode                    |
| `/benchmark`      | Executes the automatic benchmark                        |
| `/menu MapName`   | Loads *MapName* as the background map for the main menu |

## Source structure

| Path                        | Description                                                              |
| --------------------------- | ------------------------------------------------------------------------ |
| `\build.ps1`                | A script that compiles the entire project                                |
| `\src\`                     | The actual source code in a Visual Studio project                        |
| `\lib\`                     | Pre-compiled 3rd party libraries which are not available via NuGet       |
| `\nuget\`                   | Specification files for building NuGet packages                          |
| `\templates\`               | Source code for Visual Studio templates                                  |
| `\doc\`                     | Files for creating source code documentation                             |
| `\content\`                 | Game content files (.X files, PNGs, ...)                                 |
| `\artifacts\Debug\`         | The compiled debug binaries (created by `\src\build.ps1 Debug`)          |
| `\artifacts\Release\`       | The compiled release binaries (created by `\src\build.ps1 Release`)      |
| `\artifacts\Packages\`      | The compiled NuGet packages (created by `\nuget\build.ps1`)              |
| `\artifacts\Templates\`     | The packaged Visual Studio templates (created by `\templates\build.ps1`) |
| `\artifacts\Documentation\` | The compiled source code documentation (created by `\doc\build.ps1`)     |

`VERSION` contains the version numbers used by build scripts. Use `.\Set-Version.ps1 "X.Y.Z"` in PowerShall to change the version number. This ensures that the version also gets set in other locations (e.g. [`GlobalAssemblyInfo.cs`](src/GlobalAssemblyInfo.cs)).

The `build.ps1` script assumes that Visual Studio 2017 or newer is installed. To compile the included shader code the DirectX SDK needs to be installed.

The engine requires shader files to be located in a subdirectory of the installation path named `Shaders`.
