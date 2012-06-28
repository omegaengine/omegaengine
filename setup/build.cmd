@echo off
::Creates a ZIP archive, a Visual Studio extension and an Inno Setup installer. Assumes "..\nuget\build.cmd" and "..\templates\build.cmd" have already been executed.
cd /d "%~dp0"

rem Project settings
set TargetDir=%~dp0..\build\Setup

echo Building library archive...
if not exist "%TargetDir%" mkdir "%TargetDir%"
if exist "%TargetDir%\omegaengine.zip" del "%TargetDir%\omegaengine.zip"
cd /d "%~dp0..\build\ReleaseSDK"
if exist content rmdir /s /q content
mkdir content\Graphics\Shaders
copy ..\..\content\Graphics\Shaders content\Graphics\Shaders\ > NUL
mkdir content\Meshes\Engine
copy ..\..\content\Meshes\Engine content\Meshes\Engine\ > NUL
mkdir content\Textures\Water
copy ..\..\content\Textures\flag.png content\Textures\flag.png > NUL
copy ..\..\content\Textures\Water content\Textures\Water\ > NUL
mkdir content\GUI\Textures
copy ..\..\content\GUI\Textures content\GUI\Textures\ > NUL
zip -q -9 -r "%TargetDir%\omegaengine.zip" .
if errorlevel 1 pause
cd /d "%~dp0"

echo Building Visual Studio extension...
if exist "%TargetDir%\omegaengine_sdk.vsix" del "%TargetDir%\omegaengine_sdk.vsix"
cd /d "%~dp0vsix"
zip -q -9 -r "%TargetDir%\omegaengine_sdk.vsix" .
if errorlevel 1 pause
cd /d "%~dp0..\build"
zip -q -9 -r "%TargetDir%\omegaengine_sdk.vsix" ProjectTemplates
if errorlevel 1 pause
zip -q -9 -r "%TargetDir%\omegaengine_sdk.vsix" Packages
if errorlevel 1 pause
cd /d "%~dp0"


rem Handle WOW
if %PROCESSOR_ARCHITECTURE%==x86 set ProgramFiles_temp=%ProgramFiles%
if not %PROCESSOR_ARCHITECTURE%==x86 set ProgramFiles_temp=%ProgramFiles(x86)%

rem Check for Inno Setup 5 installation (32-bit)
if not exist "%ProgramFiles_temp%\Inno Setup 5" (
  echo ERROR: No Inno Setup 5 installation found. >&2
  pause
  goto end
)

echo Building Samples installer...
"%ProgramFiles_temp%\Inno Setup 5\iscc.exe" /Q samples.iss
if errorlevel 1 pause

:end
