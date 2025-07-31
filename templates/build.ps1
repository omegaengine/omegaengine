Param ($Version = "1.0.0-dev")
$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

echo "Testing templates against local build"
dotnet build --verbosity quiet --property OmegaEngineVersion=$Version
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

echo "Generating Directory.Build.props"
if (!(Test-Path generated)) { mkdir generated | Out-Null }
Set-Content generated\Directory.Build.props -Encoding UTF8 -Value @"
<Project>

  <PropertyGroup>
    <OmegaEngineVersion>$Version</OmegaEngineVersion>
  </PropertyGroup>

</Project>
"@

echo "Packaging templates"
nuget pack -Verbosity quiet -Version $Version -OutputDirectory ..\artifacts\Release

popd
