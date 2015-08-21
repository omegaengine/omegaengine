#Sets a new version number in all relevant locations
$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

if ($args.Length -lt 1) { Write-Error "Missing argument" }
$NewVersion = $args[0]

[System.IO.File]::WriteAllText("$ScriptDir\VERSION", $NewVersion)
(Get-Content "$ScriptDir\src\AssemblyInfo.Global.cs" -Encoding UTF8) `
  -replace 'AssemblyVersion\(".*"\)', ('AssemblyVersion("' + $NewVersion + '")') |
  Set-Content "$ScriptDir\src\AssemblyInfo.Global.cs" -Encoding UTF8
[System.IO.File]::WriteAllText("$ScriptDir\VERSION", $NewVersion)
(Get-Content "$ScriptDir\src\AlphaFramework\AssemblyInfo.Global.cs" -Encoding UTF8) `
  -replace 'AssemblyVersion\(".*"\)', ('AssemblyVersion("' + $NewVersion + '")') |
  Set-Content "$ScriptDir\src\AlphaFramework\AssemblyInfo.Global.cs" -Encoding UTF8
[System.IO.File]::WriteAllText("$ScriptDir\VERSION", $NewVersion)
(Get-Content "$ScriptDir\src\FrameOfReference\AssemblyInfo.Global.cs" -Encoding UTF8) `
  -replace 'AssemblyVersion\(".*"\)', ('AssemblyVersion("' + $NewVersion + '")') |
  Set-Content "$ScriptDir\src\FrameOfReference\AssemblyInfo.Global.cs" -Encoding UTF8
#\templates\*\MyTemplate.vstemplate
(Get-Content "$ScriptDir\templates\vsix\extension.vsixmanifest" -Encoding UTF8) `
  -replace '<Version>.*</Version>', "<Version>$NewVersion</Version>" |
  Set-Content "$ScriptDir\templates\vsix\extension.vsixmanifest" -Encoding UTF8