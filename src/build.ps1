$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

$vsDir = . "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath -format value
$msBuild = "$vsDir\MSBuild\15.0\Bin\amd64\MSBuild.exe"

nuget restore
. $msBuild -v:Quiet -t:Build -p:Configuration=Release

popd
