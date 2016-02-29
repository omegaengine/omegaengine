Param ([Parameter(Mandatory=$True)] [string]$NewVersion)
#Sets a new version number in all relevant locations
$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

[System.IO.File]::WriteAllText("$ScriptDir\VERSION", $NewVersion)

(Get-Content "$ScriptDir\src\GlobalAssemblyInfo.cs" -Encoding UTF8) `
  -replace 'AssemblyVersion\(".*"\)', ('AssemblyVersion("' + $NewVersion + '")') |
  Set-Content "$ScriptDir\src\GlobalAssemblyInfo.cs" -Encoding UTF8
[System.IO.File]::WriteAllText("$ScriptDir\VERSION", $NewVersion)
(Get-Content "$ScriptDir\src\AlphaFramework\GlobalAssemblyInfo.cs" -Encoding UTF8) `
  -replace 'AssemblyVersion\(".*"\)', ('AssemblyVersion("' + $NewVersion + '")') |
  Set-Content "$ScriptDir\src\AlphaFramework\GlobalAssemblyInfo.cs" -Encoding UTF8
[System.IO.File]::WriteAllText("$ScriptDir\VERSION", $NewVersion)
(Get-Content "$ScriptDir\src\FrameOfReference\GlobalAssemblyInfo.cs" -Encoding UTF8) `
  -replace 'AssemblyVersion\(".*"\)', ('AssemblyVersion("' + $NewVersion + '")') |
  Set-Content "$ScriptDir\src\FrameOfReference\GlobalAssemblyInfo.cs" -Encoding UTF8

#\templates\*\MyTemplate.vstemplate
(Get-Content "$ScriptDir\templates\vsix\extension.vsixmanifest" -Encoding UTF8) `
  -replace '<Version>.*</Version>', "<Version>$NewVersion</Version>" |
  Set-Content "$ScriptDir\templates\vsix\extension.vsixmanifest" -Encoding UTF8

(Get-Content "$ScriptDir\doc\OmegaEngine.Doxyfile" -Encoding UTF8) `
  -replace 'PROJECT_NUMBER = ".*"', ('PROJECT_NUMBER = "' + $NewVersion + '"') |
  Set-Content "$ScriptDir\doc\OmegaEngine.Doxyfile" -Encoding UTF8
(Get-Content "$ScriptDir\doc\AlphaFramework.Doxyfile" -Encoding UTF8) `
  -replace 'PROJECT_NUMBER = ".*"', ('PROJECT_NUMBER = "' + $NewVersion + '"') |
  Set-Content "$ScriptDir\doc\AlphaFramework.Doxyfile" -Encoding UTF8
(Get-Content "$ScriptDir\doc\FrameOfReference.Doxyfile" -Encoding UTF8) `
  -replace 'PROJECT_NUMBER = ".*"', ('PROJECT_NUMBER = "' + $NewVersion + '"') |
  Set-Content "$ScriptDir\doc\FrameOfReference.Doxyfile" -Encoding UTF8
