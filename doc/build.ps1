﻿$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

if (Test-Path ..\artifacts\Documentation) {rm -Recurse -Force ..\artifacts\Documentation}
mkdir ..\artifacts\Documentation | Out-Null

function Run-Doxygen($Doxyfile) {
    .\_0install.ps1 run --batch http://repo.roscidus.com/devel/doxygen $Doxyfile
    if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
}

Run-Doxygen OmegaEngine.Doxyfile
Run-Doxygen AlphaFramework.Doxyfile
Run-Doxygen FrameOfReference.Doxyfile
cp index.html ..\artifacts\Documentation\

popd
