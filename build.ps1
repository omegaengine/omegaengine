$ErrorActionPreference = "Stop"
pushd $(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

src\build.ps1
nuget\build.ps1
templates\build.ps1
doc\build.ps1

popd
