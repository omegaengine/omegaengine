$ErrorActionPreference = "Stop"
pushd $(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

$Version = Get-Content ..\VERSION

if (!(Test-Path ..\build\Packages)) { mkdir ..\build\Packages | Out-Null }
rm -Recurse -Force ..\build\Packages\*

echo "Building NuGet packages..."
nuget pack OmegaEngine.nuspec -Version $Version -Properties VersionSuffix="" -OutputDirectory ..\build\Packages # No symbols in OmegaEngine package
foreach ($x in ls *.nuspec -Exclude OmegaEngine.nuspec) {
  nuget pack $x.FullName -Symbols -Version $Version -Properties VersionSuffix="" -OutputDirectory ..\build\Packages
}

popd
