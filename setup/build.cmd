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
if exist Base rmdir /s /q Base
mkdir Base\Graphics\Shaders
copy ..\data\Graphics\Shaders Base\Graphics\Shaders\ > NUL
mkdir Base\Meshes\Engine
copy ..\data\Meshes\Engine Base\Meshes\Engine\ > NUL
mkdir Base\Textures\Water
copy ..\data\Textures\flag.png Base\Textures\flag.png > NUL
copy ..\data\Textures\Water Base\Textures\Water\ > NUL
mkdir Base\GUI\Textures
copy ..\data\GUI\Textures Base\GUI\Textures\ > NUL
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
