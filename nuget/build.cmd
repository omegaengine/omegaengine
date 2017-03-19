@echo off
::Creates NuGet packages. Assumes "..\src\build.cmd Release" has already been executed.

set /p version= < "%~dp0..\VERSION"
set TargetDir=%~dp0..\build\Packages

rem Prepare clean output directory
if not exist "%TargetDir%" mkdir "%TargetDir%"
del /q "%TargetDir%\*"


echo Building NuGet packages...
FOR %%A IN ("%~dp0*.symbols.nuspec") DO (
  nuget pack "%%A" -Symbols -Version "%version%" -Properties VersionSuffix="" -OutputDirectory "%TargetDir%"
  if errorlevel 1 exit /b %errorlevel%
)
FOR %%A IN ("%~dp0*.nosymbols.nuspec") DO (
  nuget pack "%%A" -Version "%version%" -Properties VersionSuffix="" -OutputDirectory "%TargetDir%"
  if errorlevel 1 exit /b %errorlevel%
)