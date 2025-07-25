$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

dotnet tool restore
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

echo "Build API docs"
dotnet docfx --logLevel=warning --warningsAsErrors docfx.json
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

popd
