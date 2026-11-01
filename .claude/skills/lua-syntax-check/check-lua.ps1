<#
.SYNOPSIS
Parse-checks the Lua embedded in OmegaGUI dialog XML files.

.DESCRIPTION
Each <On*> element in a dialog is its own Lua chunk at runtime, so each is checked
as a separate chunk. Loading the file with [xml] un-escapes &lt; / &amp; for us.
Standalone .lua files are checked as a whole.

All chunks are checked in one `luac -p` call; only if that fails does the script
re-check chunk by chunk to attribute the error to a dialog and control.

.EXAMPLE
.\check-lua.ps1
.EXAMPLE
.\check-lua.ps1 -Path content\GUI\InGame
#>
[CmdletBinding()]
param(
    # File or directory to check. Defaults to content/GUI relative to the repo root.
    [string]$Path
)

$ErrorActionPreference = 'Stop'

$luac = (Get-Command luac -ErrorAction SilentlyContinue).Source
if (-not $luac) {
    throw "luac is not on the PATH, so the Lua syntax check cannot run. Report it as skipped, not as passed."
}

if (-not $Path) {
    $repoRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
    $Path = Join-Path $repoRoot 'content\GUI'
}

$files = if (Test-Path $Path -PathType Container) {
    Get-ChildItem -Path $Path -Recurse -Include *.xml, *.lua
} else {
    Get-Item $Path
}

$workDir = Join-Path ([IO.Path]::GetTempPath()) ("lua-check-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $workDir | Out-Null
$errFile = Join-Path $workDir 'stderr.txt'

try {
    # Extract every chunk to its own file, remembering where it came from.
    $chunks = @()
    foreach ($file in $files) {
        if ($file.Extension -eq '.lua') {
            $chunks += [pscustomobject]@{ Label = $file.FullName; Code = (Get-Content -LiteralPath $file.FullName -Raw) }
            continue
        }

        [xml]$xml = Get-Content -LiteralPath $file.FullName -Raw
        foreach ($node in $xml.SelectNodes('//*[starts-with(name(), "On")]')) {
            if ([string]::IsNullOrWhiteSpace($node.InnerText)) { continue }
            # Name the enclosing control so failures are locatable in the dialog.
            $owner = $node.ParentNode.Attributes['Name']
            $label = "$($file.FullName) :: $($node.Name)"
            if ($owner) { $label += " on '$($owner.Value)'" }
            $chunks += [pscustomobject]@{ Label = $label; Code = $node.InnerText }
        }
    }

    if ($chunks.Count -eq 0) {
        Write-Host "No Lua chunks found under $Path."
        return
    }

    $paths = @()
    for ($i = 0; $i -lt $chunks.Count; $i++) {
        $chunkPath = Join-Path $workDir ("{0:D4}.lua" -f $i)
        Set-Content -LiteralPath $chunkPath -Value $chunks[$i].Code -Encoding utf8
        $paths += $chunkPath
    }

    # Fast path: one luac invocation for everything. luac aborts on the first error,
    # so a failure only tells us that something is wrong, not what.
    $proc = Start-Process -FilePath $luac -ArgumentList (@('-p') + $paths) `
        -NoNewWindow -Wait -PassThru -RedirectStandardError $errFile
    if ($proc.ExitCode -eq 0) {
        Write-Host "OK: $($chunks.Count) chunk(s) parsed cleanly." -ForegroundColor Green
        return
    }

    # Slow path: attribute each failure to its dialog and control.
    $errors = 0
    for ($i = 0; $i -lt $chunks.Count; $i++) {
        # Start-Process, not `2>`, because PowerShell 5.1 turns a native command's
        # redirected stderr into error records.
        $proc = Start-Process -FilePath $luac -ArgumentList '-p', "`"$($paths[$i])`"" `
            -NoNewWindow -Wait -PassThru -RedirectStandardError $errFile
        if ($proc.ExitCode -eq 0) { continue }

        $errors++
        Write-Host "FAIL $($chunks[$i].Label)" -ForegroundColor Red
        # luac prefixes its own path and the chunk file; strip both, keep line:message.
        Get-Content -LiteralPath $errFile | ForEach-Object {
            Write-Host "     line $($_ -replace '^.*\\\d{4}\.lua:', '')"
        }
    }

    Write-Host ""
    Write-Host "$errors of $($chunks.Count) chunk(s) failed to parse." -ForegroundColor Red
    exit 1
} finally {
    Remove-Item -LiteralPath $workDir -Recurse -Force -ErrorAction SilentlyContinue
}
