Param ([Parameter(Mandatory=$True)] [string]$NewVersion)
#Sets a new version number in all relevant locations
$ErrorActionPreference = "Stop"

[System.IO.File]::WriteAllText("$PSScriptRoot\VERSION", $NewVersion)

(Get-Content "$PSScriptRoot\src\GlobalAssemblyInfo.cs" -Encoding UTF8) `
  -replace 'AssemblyVersion\(".*"\)', ('AssemblyVersion("' + $NewVersion + '")') |
  Set-Content "$PSScriptRoot\src\GlobalAssemblyInfo.cs" -Encoding UTF8
[System.IO.File]::WriteAllText("$PSScriptRoot\VERSION", $NewVersion)
(Get-Content "$PSScriptRoot\src\AlphaFramework\GlobalAssemblyInfo.cs" -Encoding UTF8) `
  -replace 'AssemblyVersion\(".*"\)', ('AssemblyVersion("' + $NewVersion + '")') |
  Set-Content "$PSScriptRoot\src\AlphaFramework\GlobalAssemblyInfo.cs" -Encoding UTF8
[System.IO.File]::WriteAllText("$PSScriptRoot\VERSION", $NewVersion)
(Get-Content "$PSScriptRoot\src\FrameOfReference\GlobalAssemblyInfo.cs" -Encoding UTF8) `
  -replace 'AssemblyVersion\(".*"\)', ('AssemblyVersion("' + $NewVersion + '")') |
  Set-Content "$PSScriptRoot\src\FrameOfReference\GlobalAssemblyInfo.cs" -Encoding UTF8

#\templates\*\MyTemplate.vstemplate
(Get-Content "$PSScriptRoot\templates\vsix\extension.vsixmanifest" -Encoding UTF8) `
  -replace '<Version>.*</Version>', "<Version>$NewVersion</Version>" |
  Set-Content "$PSScriptRoot\templates\vsix\extension.vsixmanifest" -Encoding UTF8

(Get-Content "$PSScriptRoot\doc\OmegaEngine.Doxyfile" -Encoding UTF8) `
  -replace 'PROJECT_NUMBER = ".*"', ('PROJECT_NUMBER = "' + $NewVersion + '"') |
  Set-Content "$PSScriptRoot\doc\OmegaEngine.Doxyfile" -Encoding UTF8
(Get-Content "$PSScriptRoot\doc\AlphaFramework.Doxyfile" -Encoding UTF8) `
  -replace 'PROJECT_NUMBER = ".*"', ('PROJECT_NUMBER = "' + $NewVersion + '"') |
  Set-Content "$PSScriptRoot\doc\AlphaFramework.Doxyfile" -Encoding UTF8
(Get-Content "$PSScriptRoot\doc\FrameOfReference.Doxyfile" -Encoding UTF8) `
  -replace 'PROJECT_NUMBER = ".*"', ('PROJECT_NUMBER = "' + $NewVersion + '"') |
  Set-Content "$PSScriptRoot\doc\FrameOfReference.Doxyfile" -Encoding UTF8
