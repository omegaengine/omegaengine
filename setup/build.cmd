@echo off
::Creates an Inno Setup installer. Assumes "..\src\build.cmd Release" has already been executed.
cd /d "%~dp0"

rem Project settings
set TargetDir=%~dp0..\build\Setup

rem Handle WOW
if %PROCESSOR_ARCHITECTURE%==x86 set ProgramFiles_temp=%ProgramFiles%
if not %PROCESSOR_ARCHITECTURE%==x86 set ProgramFiles_temp=%ProgramFiles(x86)%

rem Check for Inno Setup 5 installation (32-bit)
if not exist "%ProgramFiles_temp%\Inno Setup 5" (
  echo ERROR: No Inno Setup 5 installation found. >&2
  pause
  goto end
)
path %ProgramFiles_temp%\Inno Setup 5;%path%

echo Building SDK archive...
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
zip -9 -r "%TargetDir%\omegaengine.zip" . > NUL
if errorlevel 1 pause
cd /d "%~dp0"

echo Building SDK installer...
iscc /Q sdk.iss
if errorlevel 1 pause

echo Building Samples installer...
iscc /Q samples.iss
if errorlevel 1 pause

:end
