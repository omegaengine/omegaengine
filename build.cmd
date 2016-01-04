@echo off
::Compiles the source code and then creates redistributables.

echo.
call "%~dp0src\build.cmd" Release
if errorlevel 1 pause

echo.
call "%~dp0nuget\build.cmd" %*
if errorlevel 1 pause

echo.
call "%~dp0templates\build.cmd" %*
if errorlevel 1 pause

echo.
call "%~dp0doc\build.cmd"
if errorlevel 1 pause