OmegaEngine
===========
The OmegaEngine is a general-purpose 3D graphics engine written in C# using the .NET Framework 2.0 and DirectX 9 via [SlimDX](http://slimdx.org/). The engine is designed to be light-weight, modular and gameplay-agnostic.
The complementary OmegaGUI, AlphaFramework and AlphaEditor help you build games using the OmegaEngine.

**[Documentation](http://omegaengine.de/)**

**[NuGet packages](https://github.com/omegaengine/omegaengine/wiki/Download#nuget-packages)** (for .NET Framework 2.0+):  
[![OmegaEngine](https://img.shields.io/nuget/v/OmegaEngine.svg?label=OmegaEngine)](https://www.nuget.org/packages/OmegaEngine/)
[![OmegaEngine.Backend](https://img.shields.io/nuget/v/OmegaEngine.Backend.svg?label=OmegaEngine.Backend)](https://www.nuget.org/packages/OmegaEngine.Backend/)
[![OmegaGUI](https://img.shields.io/nuget/v/OmegaGUI.svg?label=OmegaGUI)](https://www.nuget.org/packages/OmegaGUI/)  
[![AlphaFramework.World](https://img.shields.io/nuget/v/AlphaFramework.World.svg?label=AlphaFramework.World)](https://www.nuget.org/packages/AlphaFramework.World/)
[![AlphaFramework.Presentation](https://img.shields.io/nuget/v/AlphaFramework.Presentation.svg?label=AlphaFramework.Presentation)](https://www.nuget.org/packages/AlphaFramework.Presentation/)
[![AlphaEditor](https://img.shields.io/nuget/v/AlphaEditor.svg?label=AlphaEditor)](https://www.nuget.org/packages/AlphaEditor/)

[![Build status](https://img.shields.io/appveyor/ci/omegaengine/omegaengine.svg)](https://ci.appveyor.com/project/omegaengine/omegaengine)


Source directory structure
--------------------------
- `\build.ps1` - A script that compiles the entire project
- `\src\` - The actual source code in a Visual Studio project
- `\lib\` - Pre-compiled 3rd party libraries which are not available via NuGet
- `\nuget\` - Specification files for building NuGet packages
- `\templates\` - Source code for Visual Studio templates
- `\doc\` - Files for creating source code documentation
- `\content\` - Game content files (.X files, PNGs, ...) 
- `\build\Debug\` - The compiled debug binaries (created by \src\build.ps1 Debug)
- `\build\Release\` - The compiled release binaries (created by \src\build.ps1 Release)
- `\build\Packages\` - The compiled NuGet packages (created by \nuget\build.ps1)
- `\build\Templates\` - The packaged Visual Studio templates (created by \templates\build.ps1)
- `\build\Documentation\` - The compiled source code documentation (created by \doc\build.ps1)

`VERSION` contains the version numbers used by build scripts.
Use `.\Set-Version.ps1 "X.Y.Z"` in PowerShall to change the version number. This ensures that the version also gets set in other locations (e.g. `AssemblyInfo`).

The `build.ps1` script assumes that Visual Studio 2017 is installed.
To compile the included shader code the DirectX SDK (http://msdn.microsoft.com/directx/) needs to be installed.

The engine requires shader files to be located in a subdirectory of the installation path named `Shaders`.

"Frame of Reference" is the official sample game for the OmegaEngine. It is included in the OmegaEngine source code but is not a part of the released library binaries.
The `FrameOfReference\Game` project places the files `_portable` and `config\Settings.xml` in the build directories which together cause the game content files to be loaded from `\content\`.
When releasing the binaries as standalone applications these files are not present and game content files are instead expected be located in a subdirectory of the installation path named `content`.

To open the Debug Console when running one of the sample projects press Ctrl + Alt + Shift + D.

Command-line arguments for the sample projects:
| `/map *MapName*`    | Loads *MapName* in normal game mode                     |
| `/modify *MapName*` | Loads *MapName* in modification mode	                |
| `/benchmark`        | Executes the automatic benchmark                        |
| `/menu *MapName*`   | Loads *MapName* as the background map for the main menu |
