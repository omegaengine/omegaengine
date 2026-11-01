<#
.SYNOPSIS
Builds and runs the scenario harness, which writes a savegame slot for a test run. Arguments are forwarded to it.

.EXAMPLE
./Invoke-Scenario.ps1 --help

.EXAMPLE
./Invoke-Scenario.ps1 --new Tutorial TestDwarf --light-phase 1 --script add-entity --set template=Test.Dwarf --set name=Hero --set position=1000,1000 --player Hero
#>
[CmdletBinding()]
param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [object[]] $Arguments
)

$ErrorActionPreference = "Stop"

# PowerShell turns a,b into an array, which [string[]] would join with spaces
$Arguments = @($Arguments | ForEach-Object { if ($_ -is [array]) { $_ -join ',' } else { "$_" } })

$project = Join-Path $PSScriptRoot "harness/Scenario.csproj"
$build = dotnet build $project -v q --nologo
if ($LASTEXITCODE -ne 0) {
    $build | Write-Host
    throw "Building the harness failed."
}

dotnet (Join-Path $PSScriptRoot "harness/bin/Debug/net10.0/Scenario.dll") @Arguments
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }
