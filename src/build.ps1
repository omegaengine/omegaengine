Param ($Version = "1.0.0-pre")
$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

echo "Build binaries"
if ($env:CI) { $ci = "/p:ContinuousIntegrationBuild=True" }
dotnet msbuild /v:Quiet /t:Restore /t:Build /p:Configuration=Release /p:Version=$Version $ci
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

echo "Prepare Frame of Reference for publishing"
dotnet msbuild /v:Quiet /t:Publish /p:NoBuild=True /p:BuildProjectReferences=False /p:Configuration=Release /p:TargetFramework=net472 /p:Version=$Version FrameOfReference\Game
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
dotnet msbuild /v:Quiet /t:Publish /p:NoBuild=True /p:BuildProjectReferences=False /p:Configuration=Release /p:TargetFramework=net472 /p:Version=$Version FrameOfReference\Editor
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

popd
