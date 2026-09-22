<#
.SYNOPSIS
    Builds the headless render harness and runs it against a build of the engine.

.DESCRIPTION
    Handles the two pieces of environment setup the engine needs and that are easy to get
    wrong: the harness executable has to sit next to the engine DLLs (because the engine
    resolves its compiled shaders relative to the entry assembly), and OMEGAENGINE_CONTENT
    has to point at the matching content directory.

    Pass -Repo to render a different checkout - typically a `git worktree` of an older
    commit - so that before/after images come from the same harness and the same camera.

.EXAMPLE
    .\capture.ps1 -Out C:\shots\after -HarnessArgs '--map','Mountains','--phase','0,1,2,3'

.EXAMPLE
    .\capture.ps1 -Repo C:\tmp\baseline -Out C:\shots\before -HarnessArgs '--water','--dump-render-targets'
#>
[CmdletBinding(PositionalBinding = $false)]
param(
    # Where to write the images.
    [Parameter(Mandatory = $true)][string]$Out,

    # Repository root to render. Defaults to the checkout this skill lives in.
    [string]$Repo,

    # Build configuration whose artifacts to use.
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug',

    # Skip building the engine (it is already up to date).
    [switch]$NoBuild,

    # Passed straight to the harness; an unrecognised option makes it print the full list.
    [Parameter(ValueFromRemainingArguments = $true)][string[]]$HarnessArgs
)

$ErrorActionPreference = 'Stop'

$skillRoot = Split-Path $PSScriptRoot -Parent
$harnessProject = Join-Path $skillRoot 'harness\Shots.csproj'
if (-not $Repo) { $Repo = (Resolve-Path (Join-Path $skillRoot '..\..\..')).Path }

$artifacts = Join-Path $Repo "artifacts\$Configuration\net472"
$content = Join-Path $Repo 'content'
if (-not (Test-Path $content)) { throw "No content directory in '$Repo'" }

if (-not $NoBuild) {
    Write-Host "Building engine in $Repo ($Configuration)"
    & dotnet build (Join-Path $Repo 'src\OmegaEngine.slnx') -c $Configuration -v q | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Engine build failed" }
}
if (-not (Test-Path $artifacts)) { throw "No build output at '$artifacts'" }

# Build the harness against this repo's DLLs so its references resolve, then drop the
# executable in beside them; the engine derives its shader directory from the entry assembly.
Write-Host "Building harness"
& dotnet build $harnessProject -c Debug -v q "-p:OmegaArtifacts=$artifacts" | Out-Null
if ($LASTEXITCODE -ne 0) { throw "Harness build failed" }

# The .pdb comes along so that an exception inside the harness reports line numbers
$copied = @('Shots.exe', 'Shots.pdb') | ForEach-Object {
    $source = Join-Path $skillRoot "harness\bin\$_"
    if (Test-Path $source) { Copy-Item $source $artifacts -Force; Join-Path $artifacts $_ }
}

New-Item -ItemType Directory -Force $Out | Out-Null

$previousContent = $env:OMEGAENGINE_CONTENT
$env:OMEGAENGINE_CONTENT = $content
try {
    Push-Location $artifacts
    try {
        $exe = Join-Path $artifacts 'Shots.exe'

        # The harness is a WinExe, so it has no console of its own: its output only
        # materializes if the handles are redirected to a file. Keep that file inside the
        # output directory rather than the system temp directory, which may not be writable.
        $log = Join-Path $Out 'shots.log'
        $quoted = @("`"$exe`"", '--out', "`"$Out`"") + ($HarnessArgs | ForEach-Object { "`"$_`"" })
        cmd /c "$($quoted -join ' ') > `"$log`" 2>&1"
        $exitCode = $LASTEXITCODE
        if (Test-Path $log) {
            Get-Content $log
            Remove-Item $log -Force
        }
        if ($exitCode -ne 0) { throw "Harness exited with code $exitCode (see output above)" }
    }
    finally { Pop-Location }
}
finally {
    $env:OMEGAENGINE_CONTENT = $previousContent
    $copied | ForEach-Object { Remove-Item $_ -Force -ErrorAction SilentlyContinue }
}

Write-Host "Wrote images to $Out"
