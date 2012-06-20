@echo off
::Creates NuGet packages. Assumes "..\src\build.cmd Release" has already been executed.

echo Building NuGet packages...
if not exist "%~dp0..\build\Packages" mkdir "%~dp0..\build\Packages"
FOR %%A IN ("%~dp0*.nuspec") DO (
  nuget pack "%%A" -OutputDirectory "%~dp0..\build\Packages"
  if errorlevel 1 pause
)