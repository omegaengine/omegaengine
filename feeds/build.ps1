<#
.PARAMETER Version
    Version number for OmegaEngine and Frame of Reference.
.PARAMETER ContentVersion
    Version number of an already existing release of the Frame of Reference content feed to use.
    Leave unset to build a new release with the same version as the game.
#>
Param ($Version = "1.0.0-pre", [string]$ContentVersion = $null)
$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

.\0template.ps1 frame-of-reference.xml.template version=$Version content-version=$(if ($ContentVersion) { $ContentVersion } else { $Version })

if (-not $ContentVersion) {
    .\0template.ps1 frame-of-reference-content.xml.template version=$Version
}

# Move output to local 0repo instance, if present
if (Test-Path ..\..\publishing\incoming) {
    mv *.xml ..\..\publishing\incoming\
    mv *.tar.zst ..\..\publishing\incoming\
}

popd
