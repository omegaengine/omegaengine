<#
.SYNOPSIS
Builds and runs the scenario harness, which writes a savegame slot for a test run. Arguments are forwarded to it.

.EXAMPLE
./Invoke-Scenario.ps1 --help

.EXAMPLE
./Invoke-Scenario.ps1 base.FrameOfReferenceSave TestDwarf --light-phase 1 --script add-entity --set template=Test.Dwarf --set name=Hero --set position=1000,1000 --player Hero
#>
[CmdletBinding()]
param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [object[]] $Arguments
)

$ErrorActionPreference = "Stop"

$Arguments = @($Arguments | ForEach-Object { if ($_ -is [array]) { $_ -join ',' } else { "$_" } })

$project = Join-Path $PSScriptRoot "harness\Scenario.csproj"
$buildOutput = dotnet build $project -v q --nologo
if ($LASTEXITCODE -ne 0) {
    $buildOutput | Write-Host
    throw "Building the harness failed."
}

& (Join-Path $PSScriptRoot "harness\bin\Debug\net10.0\Scenario.exe") @Arguments
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }
