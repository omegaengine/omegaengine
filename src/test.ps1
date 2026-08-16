$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

echo "Run unit tests"
dotnet test --no-build --configuration Release
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

popd
