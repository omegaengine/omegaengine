$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

if (-not $env:DXSDK_DIR) { throw "DirectX SDK must be installed!" }

foreach ($shader in Get-ChildItem -Filter *.fx) {
    $outputFile = Join-Path "..\src\OmegaEngine\Shaders" ($shader.BaseName + ".fxo")
    . "$env:DXSDK_DIR\Utilities\bin\x64\fxc.exe" /nologo @args /Tfx_2_0 /Fo"$outputFile" $shader.Name
}

popd
