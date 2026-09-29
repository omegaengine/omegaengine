$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

if (-not $env:DXSDK_DIR) { throw "DirectX SDK must be installed!" }

foreach ($shader in Get-ChildItem -Filter *.fx) {
    $outputFile = Join-Path "..\src\OmegaEngine\Shaders" ($shader.BaseName + ".fxo")

    # QUAD_FLOAT makes include\Quad.fxh declare its globals as float, since Shader Model 3.0 does not allow half-typed globals
    . "$env:DXSDK_DIR\Utilities\bin\x64\fxc.exe" /nologo /DQUAD_FLOAT @args /Tfx_2_0 /Fo"$outputFile" $shader.Name
    if ($LASTEXITCODE -ne 0) { throw "Failed to compile $($shader.Name)" }
}

popd
