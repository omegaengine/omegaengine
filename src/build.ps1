Param ($Version = "1.0.0-pre")
$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

echo "Build binaries"
if ($env:CI) { $ci = "/p:ContinuousIntegrationBuild=True" }
dotnet msbuild /v:Quiet /t:Restore /t:Build /p:Configuration=Release /p:Version=$Version $ci
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

popd
