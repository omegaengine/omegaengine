@echo off
::Creates a ZIP archive, a Visual Studio extension and an Inno Setup installer. Assumes "..\nuget\build.cmd" and "..\templates\build.cmd" have already been executed.
call "%~dp0..\version.cmd"

rem Project settings
set TargetDir=%~dp0..\build\Setup

rem Prepare clean output directory
del /q "%TargetDir%\*"
if not exist "%TargetDir%" mkdir "%TargetDir%"

rem Use bundled utility EXEs
path %~dp0utils;%path%


echo ##teamcity[progressMessage 'Building library archive']
if not exist "%TargetDir%" mkdir "%TargetDir%"
if exist "%TargetDir%\omegaengine-%version%.zip" del "%TargetDir%\omegaengine-%version%.zip"
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
zip -q -9 -r "%TargetDir%\omegaengine-%version%.zip" .
if errorlevel 1 pause
cd /d "%~dp0"
echo ##teamcity[publishArtifacts 'build/Setup/omegaengine-%version%.zip']

echo ##teamcity[progressMessage 'Building Visual Studio extension']
if exist "%TargetDir%\omegaengine-templates.vsix" del "%TargetDir%\omegaengine-templates.vsix"
cd /d "%~dp0templates"
zip -q -9 -r "%TargetDir%\omegaengine-templates.vsix" .
if errorlevel 1 pause
cd /d "%~dp0..\build"
zip -q -9 -r "%TargetDir%\omegaengine-templates.vsix" ProjectTemplates
if errorlevel 1 pause

:: Copy external dependency NuGet packages
copy "%~dp0..\src\packages\LinqBridge.1.3.0\LinqBridge.1.3.0.nupkg" "Packages\LinqBridge.1.3.0.nupkg" > NUL

zip -q -9 -r "%TargetDir%\omegaengine-templates.vsix" Packages
if errorlevel 1 pause
cd /d "%~dp0"
echo ##teamcity[publishArtifacts 'build/Setup/omegaengine-templates.vsix']

rem Handle WOW
if %PROCESSOR_ARCHITECTURE%==x86 set ProgramFiles_temp=%ProgramFiles%
if not %PROCESSOR_ARCHITECTURE%==x86 set ProgramFiles_temp=%ProgramFiles(x86)%

rem Check for Inno Setup 5 installation (32-bit)
if not exist "%ProgramFiles_temp%\Inno Setup 5" (
  echo ERROR: No Inno Setup 5 installation found. >&2
  pause
  goto end
)

echo ##teamcity[progressMessage 'Building Samples installer']
"%ProgramFiles_temp%\Inno Setup 5\iscc.exe" /q "/dVersion=%version%" samples.iss
if errorlevel 1 pause
echo ##teamcity[publishArtifacts 'build/Setup/omegaengine-samples-%version%.exe']

:end
