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

src\build.ps1 $Version
src\test.ps1
templates\build.ps1 $Version
doc\build.ps1
feeds\build.ps1 $Version $ContentVersion

popd
