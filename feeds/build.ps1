Param ($Version = "1.0.0-pre")
$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

.\0install.ps1 run --batch https://apps.0install.net/0install/0template.xml frame-of-reference.xml.template version=$Version
.\0install.ps1 run --batch https://apps.0install.net/0install/0template.xml frame-of-reference-content.xml.template version=$Version

# Move output to local 0repo instance, if present
if (Test-Path ..\..\publishing\incoming) {
    mv *.xml ..\..\publishing\incoming\
    mv *.tar.zst ..\..\publishing\incoming\
}

popd
