$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

echo "Downloading references to other documentation..."
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]'Tls11,Tls12'
Invoke-WebRequest https://common.nano-byte.net/nanobyte-common.tag -OutFile nanobyte-common.tag

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
