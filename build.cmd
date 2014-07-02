@echo off
::Compiles the source code and then creates redistributables.
::Use command-line argument "+doc" to additionally create source code documentation.
if "%1"=="+doc" set BUILD_DOC=TRUE
if "%2"=="+doc" set BUILD_DOC=TRUE
if "%3"=="+doc" set BUILD_DOC=TRUE
if "%4"=="+doc" set BUILD_DOC=TRUE

echo.
call "%~dp0src\build.cmd" Release
echo.
call "%~dp0nuget\build.cmd" %*
echo.
call "%~dp0templates\build.cmd" %*

::Optionally create debug build and documentation
if "%BUILD_DOC%"=="TRUE" (
  echo.
  call "%~dp0src\build.cmd" Debug
  echo.
  call "%~dp0doc\build.cmd"
)