The OmegaGame uses the .NET Framework 2.0 (with a C# 4.0 compiler). The recommended IDE is Microsoft Visual Studio 2010.
To compile the included shader code the DirectX SDK (http://msdn.microsoft.com/directx/) needs to be installed.
The setup is build using Inno Setup 5.4.1 or newer (http://files.jrsoftware.org/ispack/isdl.htm).

\build.cmd - A script that automatically compiles the source code and then creates an installer (use the command-line argument "+doc" to compile source code documentation)
\cleanup.cmd - A cleanup script that removes compiled binaries, deletes temporary "obj" directories, resets Visual Studio settings and so on
\src\ - The actual source code in a Visual Studio project
\setup\ - Files for creating the installer
\doc\ - Files for creating source code documentation
\data\ - Game content files (.X files, PNGs, ...) 
\build\Debug\ - The compiled debug binaries
\build\Release\ - The compiled release binaries
\build\ReleaseSDK\ - The compiled SDK release binaries (for use in external projects)
\build\Setup\ - The executable installer
\build\Documentation\ - The compiled source code documentation

Das "build.cmd" script assumes that Visual Studio 2010 is installed and that Inno Setup 5 is installed at its default location.

The engine requires shader files to be located in a subdirectory of the installation path named "Shaders".

The TerrainSample and SpaceSample projects place "_portable" and "*.Settings.xml" files in the build directories which cause the game content files to be loaded from \data\.
When releasing the binaries as standalone applications these files are not present and game content files are instead expected be located in a subdirectory of the installation path named "Base".
This naming scheme is defined by the sample projects and not intrinsic to the OmegaEngine.

To open the Debug Console when running one of the sample projects press Ctrl + Alt + Shift + D.

Command-line arguments for the sample projects:
/map MapName		Loads MapName in normal game mode
/modify MapName		Loads MapName in modification mode	
/benchmark			Executes the automatic benchmark
/menu MapName		Loads MapName as the background map for the main menu