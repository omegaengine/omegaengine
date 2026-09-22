$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

if (-not $env:DXSDK_DIR) { throw "DirectX SDK must be installed!" }

foreach ($shader in Get-ChildItem -Filter *.fx) {
    $outputFile = Join-Path "..\src\OmegaEngine\Shaders" ($shader.BaseName + ".fxo")

    # The post-screen shaders (half-typed globals in include\Quad.fxh) and Water.fx (ps_1_x techniques)
    # are only accepted by the legacy D3DX compiler, while General.fx only compiles with the current one.
    [string[]]$legacy = @(if ($shader.Name -eq "General.fx") {} else {"/LD"})

    . "$env:DXSDK_DIR\Utilities\bin\x64\fxc.exe" /nologo @legacy @args /Tfx_2_0 /Fo"$outputFile" $shader.Name
}

popd
