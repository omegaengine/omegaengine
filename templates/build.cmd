@echo off
::Packages Visual Studio templates.

if exist "%~dp0..\build\ProjectTemplates" del "%~dp0..\build\ProjectTemplates\*.zip" 2> NUL
if not exist "%~dp0..\build\ProjectTemplates" mkdir "%~dp0..\build\ProjectTemplates"

cd /d "%~dp0WinForms"
zip -q -9 -r "..\..\build\ProjectTemplates\WinForms.zip" . --exclude obj bin *.suo
if errorlevel 1 pause

cd /d "%~dp0Fullscreen"
zip -q -9 -r "..\..\build\ProjectTemplates\Fullscreen.zip" . --exclude obj bin *.suo
if errorlevel 1 pause

cd /d "%~dp0Editor"
zip -q -9 -r "..\..\build\ProjectTemplates\Editor.zip" . --exclude obj bin *.suo
if errorlevel 1 pause
