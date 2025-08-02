$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

echo "Build docs"
if (Test-Path api) { rm api -Recurse -Force }
dotnet tool restore
dotnet docfx --logLevel=warning --warningsAsErrors docfx.json
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

popd
