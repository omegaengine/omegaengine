OmegaEngine
===========

The OmegaEngine uses the .NET Framework 2.0 (with a C# 4.0 compiler). The recommended IDE is Microsoft Visual Studio 2010 or newer.
To compile the included shader code the DirectX SDK (http://msdn.microsoft.com/directx/) needs to be installed.

\build.cmd - A script that automatically compiles the source code (use the command-line argument "+doc" to compile source code documentation)
\cleanup.cmd - A cleanup script that removes compiled binaries, deletes temporary "obj" directories, resets Visual Studio settings and so on
\src\ - The actual source code in a Visual Studio project
\lib\ - Pre-compiled 3rd party libraries which are not available via NuGet
\nuget\ - Specification files for building NuGet packages
\templates\ - Source code for Visual Studio templates
\doc\ - Files for creating source code documentation
\content\ - Game content files (.X files, PNGs, ...) 
\build\Debug\ - The compiled debug binaries (created by \src\build.cmd Debug)
\build\Release\ - The compiled release binaries (created by \src\build.cmd Release)
\build\Packages\ - The compiled NuGet packages (created by \nuget\build.cmd)
\build\Templates\ - The packaged Visual Studio templates (created by \templates\build.cmd)
\build\Documentation\ - The compiled source code documentation (created by \doc\build.cmd)

"VERSION" contains the version numbers used by build scripts.
Use "Set-Version.ps1 X.Y.Z" to change the version number. This ensures that the version also gets set in other locations (e.g. AssemblyInfo).

The "build.cmd" script assumes that Visual Studio 2010/2012/2013/2015 is installed.

The engine requires shader files to be located in a subdirectory of the installation path named "Shaders".

"Frame of Reference" is the official sample game for the OmegaEngine. It is included in the OmegaEngine source code but is not a part of the released library binaries.
The FrameOfReference\Game project places the files "_portable" and "config\Settings.xml" in the build directories which together cause the game content files to be loaded from \content\.
When releasing the binaries as standalone applications these files are not present and game content files are instead expected be located in a subdirectory of the installation path named "content".

To open the Debug Console when running one of the sample projects press Ctrl + Alt + Shift + D.

Command-line arguments for the sample projects:
/map MapName		Loads MapName in normal game mode
/modify MapName		Loads MapName in modification mode	
/benchmark			Executes the automatic benchmark
/menu MapName		Loads MapName as the background map for the main menu