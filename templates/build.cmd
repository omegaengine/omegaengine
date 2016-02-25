@echo off
::Packages Visual Studio templates into a VSIX extension. Assumes "..\nuget\build.cmd" has already been executed.
pushd "%~dp0"
set TargetDir=%~dp0..\build\Templates

rem Use bundled zip.exe
path %~dp0;%path%



if exist "%TargetDir%\ProjectTemplates" del "%TargetDir%\ProjectTemplates\*.zip" 2> NUL
if not exist "%TargetDir%\ProjectTemplates" mkdir "%TargetDir%\ProjectTemplates"

echo Packaging WinForms template...
cd /d "%~dp0WinForms"
zip -q -9 -r "%TargetDir%\ProjectTemplates\WinForms.zip" . --exclude obj bin
if errorlevel 1 exit /b %errorlevel%

echo Packaging Fullscreen template...
cd /d "%~dp0Fullscreen"
zip -q -9 -r "%TargetDir%\ProjectTemplates\Fullscreen.zip" . --exclude obj bin
if errorlevel 1 exit /b %errorlevel%

echo Packaging AlphaFramework template...
cd /d "%~dp0AlphaFramework"
zip -q -9 -r "%TargetDir%\ProjectTemplates\AlphaFramework.zip" . --exclude obj bin
if errorlevel 1 exit /b %errorlevel%



if exist "%TargetDir%\omegaengine-templates.vsix" del "%TargetDir%\omegaengine-templates.vsix"

echo Creating VSIX extension...
cd /d "%~dp0vsix"
zip -q -9 -r "%TargetDir%\omegaengine-templates.vsix" .
if errorlevel 1 exit /b %errorlevel%

echo Adding templates to extension...
cd /d "%TargetDir%"
zip -q -9 -r "%TargetDir%\omegaengine-templates.vsix" ProjectTemplates
if errorlevel 1 exit /b %errorlevel%

echo Copying external NuGet packages...
FOR /d %%A in ("%~dp0..\src\packages\ICSharpCode.SharpZipLib.Patched.*") DO xcopy /y "%%A\*.nupkg" "%~dp0..\build\Packages\*" > NUL
FOR /d %%A in ("%~dp0..\src\packages\LinqBridge.*") DO xcopy /y "%%A\*.nupkg" "%~dp0..\build\Packages\*" > NUL
FOR /d %%A in ("%~dp0..\src\packages\NanoByte.Common.*") DO xcopy /y "%%A\*.nupkg" "%~dp0..\build\Packages\*" > NUL
FOR /d %%A in ("%~dp0..\src\packages\NanoByte.Common.WinForms.*") DO xcopy /y "%%A\*.nupkg" "%~dp0..\build\Packages\*" > NUL
FOR /d %%A in ("%~dp0..\src\packages\NanoByte.Common.SlimDX.*") DO xcopy /y "%%A\*.nupkg" "%~dp0..\build\Packages\*" > NUL
FOR /d %%A in ("%~dp0..\src\packages\SlimDX.*") DO xcopy /y "%%A\*.nupkg" "%~dp0..\build\Packages\*" > NUL

echo Adding NuGet packages to extension...
cd /d "%~dp0..\build"
zip -q -9 -r "%TargetDir%\omegaengine-templates.vsix" Packages
if errorlevel 1 exit /b %errorlevel%

popd