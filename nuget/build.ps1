$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

$Version = Get-Content ..\VERSION

if (!(Test-Path ..\artifacts\Packages)) { mkdir ..\artifacts\Packages | Out-Null }
rm -Recurse -Force ..\artifacts\Packages\*

echo "Building NuGet packages..."
nuget pack OmegaEngine.nuspec -Version $Version -Properties VersionSuffix="" -OutputDirectory ..\artifacts\Packages # No symbols in OmegaEngine package
foreach ($x in ls *.nuspec -Exclude OmegaEngine.nuspec) {
  nuget pack $x.FullName -Symbols -Version $Version -Properties VersionSuffix="" -OutputDirectory ..\artifacts\Packages
}

popd
