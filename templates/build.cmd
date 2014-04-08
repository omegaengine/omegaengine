@echo off
::Packages Visual Studio templates into a VSIX extension. Assumes "..\nuget\build.cmd" has already been executed.

set TargetDir=%~dp0..\build\Templates

rem Use bundled zip.exe
path %~dp0;%path%



if exist "%TargetDir%\ProjectTemplates" del "%TargetDir%\ProjectTemplates\*.zip" 2> NUL
if not exist "%TargetDir%\ProjectTemplates" mkdir "%TargetDir%\ProjectTemplates"

echo Packaging WinForms template...
cd /d "%~dp0WinForms"
zip -q -9 -r "%TargetDir%\ProjectTemplates\WinForms.zip" . --exclude obj bin *.suo
if errorlevel 1 pause

echo Packaging Fullscreen template...
cd /d "%~dp0Fullscreen"
zip -q -9 -r "%TargetDir%\ProjectTemplates\Fullscreen.zip" . --exclude obj bin *.suo
if errorlevel 1 pause

echo Packaging AlphaFramework template...
cd /d "%~dp0AlphaFramework"
zip -q -9 -r "%TargetDir%\ProjectTemplates\AlphaFramework.zip" . --exclude obj bin *.suo
if errorlevel 1 pause



if exist "%TargetDir%\omegaengine-templates.vsix" del "%TargetDir%\omegaengine-templates.vsix"

echo Creating VSIX extension...
cd /d "%~dp0vsix"
zip -q -9 -r "%TargetDir%\omegaengine-templates.vsix" .
if errorlevel 1 pause

echo Adding templates to extension...
cd /d "%TargetDir%"
zip -q -9 -r "%TargetDir%\omegaengine-templates.vsix" ProjectTemplates
if errorlevel 1 pause

echo Copying external NuGet packages...
copy "%~dp0..\src\packages\LinqBridge.1.3.0\LinqBridge.1.3.0.nupkg" "%~dp0..\build\Packages\LinqBridge.1.3.0.nupkg" > NUL
copy "%~dp0..\src\packages\SlimDX.4.0.13.44\SlimDX.4.0.13.44.nupkg" "%~dp0..\build\Packages\SlimDX.4.0.13.44.nupkg" > NUL

echo Adding NuGet packages to extension...
cd /d "%~dp0..\build"
zip -q -9 -r "%TargetDir%\omegaengine-templates.vsix" Packages
if errorlevel 1 pause
