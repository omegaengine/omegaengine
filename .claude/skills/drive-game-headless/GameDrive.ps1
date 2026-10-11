<#
.SYNOPSIS
Helpers for driving the Frame of Reference game window from PowerShell.

.DESCRIPTION
Dot-source this file, then use the functions below:

    . .\.claude\skills\drive-game-headless\GameDrive.ps1

    Backup-GameSaves                          # ALWAYS before launching
    Start-Game -Savegame MyScenario           # returns once loading is done
    Send-GameKey Escape
    Send-GameClick 400 300
    Save-GameScreenshot shot.png
    Stop-Game
    $outcome = Read-GameOutcome               # the Resume this run wrote, if it wrote one
    Restore-GameSaves                         # ALWAYS after

Input is posted with PostMessage, so the window does not need focus to receive it
-- but screenshots capture the screen region the window occupies, so
Save-GameScreenshot brings the window to the front first (see Show-GameWindow).
#>

Add-Type -AssemblyName System.Drawing

# Match Constants.SavegameFileExt and .MapFileExt in src\FrameOfReference\World\Constants.cs.
$SavegameExt = '.FrameOfReferenceSave'
$MapExt = '.FrameOfReferenceMap'

Add-Type @'
using System;
using System.Runtime.InteropServices;

public static class GameWin {
    [DllImport("user32.dll")]
    public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")]
    public static extern bool GetClientRect(IntPtr hWnd, out RECT rect);
    [DllImport("user32.dll")]
    public static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    public static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extraInfo);
    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);
    [DllImport("kernel32.dll")]
    public static extern uint GetCurrentThreadId();
    [DllImport("user32.dll")]
    public static extern bool AttachThreadInput(uint attach, uint attachTo, bool doAttach);
    [DllImport("user32.dll")]
    public static extern bool BringWindowToTop(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern bool IsIconic(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

    public const int SW_RESTORE = 9;

    public const byte VK_MENU = 0x12;
    public const uint KEYEVENTF_KEYUP = 0x0002;

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT { public int X, Y; }

    public const uint WM_KEYDOWN = 0x0100, WM_KEYUP = 0x0101;
    public const uint WM_MOUSEMOVE = 0x0200;
    public const uint WM_LBUTTONDOWN = 0x0201, WM_LBUTTONUP = 0x0202;
    public const uint WM_RBUTTONDOWN = 0x0204, WM_RBUTTONUP = 0x0205;
}
'@ -ErrorAction SilentlyContinue

$GameRepoDir = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))

# The Debug build: what Start-Game launches and where its portable-mode saves live
$GameBinDir = Join-Path $GameRepoDir 'artifacts\Debug\net472'

function Get-GameProcess {
    <#
    .SYNOPSIS Returns the game process Start-Game launched in this PowerShell process, or nothing if there is none or it has exited.
    .DESCRIPTION Never any other FrameOfReference process: that may be the user's own game.
    #>
    if ($script:GameProcess -and -not $script:GameProcess.HasExited) { $script:GameProcess }
}

function Get-GameWindow {
    <#.SYNOPSIS Returns the game's main window handle, or throws if it is not running.#>
    $proc = Get-GameProcess
    if (-not $proc) { throw "No game launched by Start-Game in this PowerShell process is running." }
    $proc.Refresh()
    $handle = $proc.MainWindowHandle
    if ($handle -eq [IntPtr]::Zero) { throw "FrameOfReference has no main window yet -- wait for it to finish loading." }
    $handle
}

# The slots the game writes by itself, which a test run therefore overwrites
$GameOwnSlots = @('Resume')

# Exists only between Backup-GameSaves and Restore-GameSaves
$GameSaveBackupDir = Join-Path $env:TEMP 'frame-of-reference-saves-backup'

function Backup-GameSaves {
    <#
    .SYNOPSIS Copies aside the slot the game writes by itself (Resume). Run before every test launch. Other slots are not covered: never overwrite them.
    .DESCRIPTION
    Refuses while an earlier backup is still waiting to be restored: the slots would hold that run's output by now,
    and backing them up would lose the user's own.
    #>
    if (Test-Path $GameSaveBackupDir) {
        throw "An earlier backup in $GameSaveBackupDir was never restored. Put it back with Restore-GameSaves first (or delete it, once you have checked the slots hold the user's saves)."
    }

    New-Item -ItemType Directory $GameSaveBackupDir | Out-Null
    foreach ($name in $GameOwnSlots) {
        $file = Join-Path (Get-GameDataDir) "$name$SavegameExt"
        if (Test-Path $file) { Copy-Item $file $GameSaveBackupDir }
    }
    Write-Host "Saves backed up to $GameSaveBackupDir"
}

function Restore-GameSaves {
    <#
    .SYNOPSIS Puts the user's save slots back as they were at Backup-GameSaves, and removes the backup. Run after every test launch.
    .DESCRIPTION A slot that did not exist at the backup is deleted, so a run leaves no Resume behind that the user never had.
    #>
    if (-not (Test-Path $GameSaveBackupDir)) { throw "No backup at $GameSaveBackupDir." }

    foreach ($name in $GameOwnSlots) {
        $backup = Join-Path $GameSaveBackupDir "$name$SavegameExt"
        $file = Join-Path (Get-GameDataDir) "$name$SavegameExt"
        if (Test-Path $backup) { Copy-Item $backup $file -Force }
        elseif (Test-Path $file) { Remove-Item $file -Force }
    }
    Remove-Item $GameSaveBackupDir -Recurse -Force
    Write-Host "Saves restored from $GameSaveBackupDir"
}

function Get-GameDataDir {
    <#.SYNOPSIS Portable-mode save directory of the Debug build (NOT the user profile).#>
    Join-Path $GameBinDir 'data'
}

function Resolve-GameMod {
    <#
    .SYNOPSIS Returns the full path of a mod directory: a path to one, or the name of one in the Debug build's Mods folder. Throws if there is none.
    .DESCRIPTION
    Checked here rather than left to the game, which reports a missing mod in a message box that blocks until
    someone closes it.
    #>
    param([Parameter(Mandatory)][string]$Mod)

    foreach ($candidate in @($Mod, (Join-Path $GameBinDir "Mods\$Mod"))) {
        if (Test-Path $candidate -PathType Container) { return (Resolve-Path $candidate).Path.TrimEnd('\') }
    }
    throw "No mod directory at '$Mod' or '$(Join-Path $GameBinDir "Mods\$Mod")'."
}

function Assert-GameMap {
    <#.SYNOPSIS Throws unless the game would find a map passed with /map: a name in the content's or the mod's World\Maps, or a map file.#>
    param(
        [Parameter(Mandatory)][string]$Map,
        [string]$ModDir  # resolved, as from Resolve-GameMod
    )

    # As in Game.GetMap: a file name is relative to the install base, anything else a name in World\Maps
    $candidates = if ($Map.EndsWith($MapExt, [StringComparison]::OrdinalIgnoreCase)) {
        @([IO.Path]::Combine($GameBinDir, $Map))
    } else {
        @($ModDir, (Join-Path $GameRepoDir 'content')) | Where-Object { $_ } | ForEach-Object { Join-Path $_ "World\Maps\$Map$MapExt" }
    }
    if (-not ($candidates | Where-Object { Test-Path $_ -PathType Leaf })) {
        throw "No map '$Map' at $($candidates -join ' or ')."
    }
}

function Start-Game {
    <#
    .SYNOPSIS Launches the Debug build of the game, optionally into a savegame or a fresh game on a map, and waits until it has finished loading.
    .DESCRIPTION Refuses while FrameOfReference is already running: that may be the user's own game, and Stop-Game would close it.
    #>
    param(
        [string]$Savegame,
        [string]$Map,  # a map in content\World\Maps (or the mod's World\Maps), e.g. Mountains
        [string]$Mod,  # a mod directory, overlaid on content\: a path, or a name in the Debug build's Mods folder
        [int]$TimeoutSeconds = 90
    )

    if ($Savegame -and $Map) { throw "Pass -Savegame or -Map, not both." }

    $exe = Join-Path $GameBinDir 'FrameOfReference.exe'
    if (-not (Test-Path $exe)) { throw "$exe not found -- build first with: dotnet build src\OmegaEngine.slnx" }
    $running = @(Get-Process FrameOfReference -ErrorAction SilentlyContinue)
    if ($running) { throw "FrameOfReference is already running (ID $($running.Id -join ', ')). It may be the user's: ask them to close it rather than stopping it." }

    # Checked here rather than left to the game, which reports a missing savegame or map in a message box that blocks
    # until someone closes it
    $modDir = if ($Mod) { Resolve-GameMod $Mod }
    if ($Savegame -and -not (Test-Path (Join-Path (Get-GameDataDir) "$Savegame$SavegameExt"))) {
        throw "No savegame '$Savegame' in $(Get-GameDataDir)."
    }
    if ($Map) { Assert-GameMap $Map -ModDir $modDir }

    # Windows PowerShell rejects an empty -ArgumentList, so only pass it when there is something to pass
    $arguments = @()
    # The game combines /mod with its Mods folder, which leaves an absolute path as it is
    if ($modDir) { $arguments += '/mod', $modDir }
    if ($Savegame) { $arguments += '/savegame', $Savegame }
    elseif ($Map) { $arguments += '/map', $Map }
    $startArgs = @{FilePath = $exe; PassThru = $true}
    # Start-Process joins the arguments with spaces, so quote any that contain one (e.g. a mod path)
    if ($arguments) { $startArgs.ArgumentList = $arguments | ForEach-Object { if ($_ -match '\s') { "`"$_`"" } else { $_ } } }
    $script:GameStartedAt = Get-Date  # tells a Resume this run wrote from one left over from before
    $script:GameLogMark = 0  # earlier runs are told apart by process ID
    $proc = Start-Process @startArgs
    $script:GameProcess = $proc

    # The window opens before the game is usable: the map and its terrain load behind a splash, and input posted
    # meanwhile is queued and fires once it is done.
    # The log says when startup is over, whether that ends in the main menu or in the save.
    Wait-GameLog 'Game startup' -TimeoutSeconds $TimeoutSeconds | Out-Null
    # Startup logs what it did (e.g. 'Enter InGame state') before 'Game startup', so leave all of it to later waits
    $script:GameLogMark = 0
    Start-Sleep -Seconds 1  # let the first frames render
    $proc
}

function Get-GameLogPath {
    <#.SYNOPSIS The game's log file. Every run appends to it, under a header naming its process ID.#>
    Join-Path $env:TEMP "FrameOfReference $env:USERNAME Log.txt"
}

function Read-GameLogLines {
    <#.SYNOPSIS The complete lines of the game's log file so far (a line still being written is left out).#>
    $log = Get-GameLogPath
    if (-not (Test-Path $log)) { return }
    # Opened shared, since the game holds the file open for writing
    $stream = [IO.File]::Open($log, 'Open', 'Read', 'ReadWrite')
    try { $text = (New-Object IO.StreamReader($stream)).ReadToEnd() } finally { $stream.Dispose() }
    $lines = $text -split "`r?`n"
    $lines | Select-Object -First ($lines.Count - 1)
}

function Set-GameLogMark {
    <#
    .SYNOPSIS Makes Wait-GameLog ignore everything logged so far.
    .DESCRIPTION Send-GameKey and Send-GameClick call this themselves. Call it before anything else that is to be waited for, e.g. before letting game time pass.
    #>
    $script:GameLogMark = @(Read-GameLogLines).Count
}

function Wait-GameLog {
    <#
    .SYNOPSIS Waits until the running game logs a line matching a pattern, and returns that line.
    .DESCRIPTION
    Only lines from the running process count, so earlier runs in the same log file never match. Of those, only
    lines logged since the last Start-Game, Send-GameKey, Send-GameClick or Set-GameLogMark count, and each
    Wait-GameLog moves past the line it returns, so waiting for the same pattern twice waits for two occurrences.
    Useful markers: 'Game startup' (done loading, at startup), 'Enter InGame state' (a game has been entered,
    e.g. after pressing N in the main menu), 'Loading GUI dialog: PauseMenu' (a dialog has opened),
    'Recorded arrival time for .* at ' (an entity has reached a waypoint).
    #>
    param(
        [Parameter(Mandatory)][string]$Pattern,
        [int]$TimeoutSeconds = 60
    )

    $proc = Get-GameProcess
    if (-not $proc) { throw "No game launched by Start-Game in this PowerShell process is running." }
    $prefix = "^\[[^\]]+\] $($proc.Id) \w+: "

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        $proc.Refresh()
        if ($proc.HasExited) { throw "FrameOfReference exited (code $($proc.ExitCode)) before logging '$Pattern'." }
        $lines = @(Read-GameLogLines)
        for ($i = [int]$script:GameLogMark; $i -lt $lines.Count; $i++) {
            if ($lines[$i] -match $prefix -and $lines[$i] -match $Pattern) {
                $script:GameLogMark = $i + 1
                return $lines[$i]
            }
        }
        Start-Sleep -Milliseconds 500
    }
    throw "FrameOfReference did not log '$Pattern' within $TimeoutSeconds s."
}

function Read-GameSave {
    <#
    .SYNOPSIS Returns the XML of a savegame, e.g. (Read-GameSave TestScenario).Session.Player.Money
    .DESCRIPTION
    Reads through System.IO.Compression: Windows PowerShell's Expand-Archive refuses any extension but .zip.
    For what a run did, use Read-GameOutcome instead: the Resume slot may still hold the user's own save.
    #>
    param(
        [Parameter(Mandatory)][string]$Name  # a slot name, or a path to a .FrameOfReferenceSave
    )

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $path = if (Test-Path $Name) { (Resolve-Path $Name).Path } else { Join-Path (Get-GameDataDir) "$Name$SavegameExt" }
    $zip = [IO.Compression.ZipFile]::OpenRead($path)
    try {
        $reader = New-Object IO.StreamReader($zip.GetEntry('data.xml').Open())
        try { [xml]$reader.ReadToEnd() } finally { $reader.Dispose() }
    } finally { $zip.Dispose() }
}

function Get-GameRunResume {
    <#.SYNOPSIS The path of the Resume slot if the last Start-Game run wrote it, or nothing if it is older (the user's own, or none at all).#>
    $file = Join-Path (Get-GameDataDir) "Resume$SavegameExt"
    if ($script:GameStartedAt -and (Test-Path $file) -and (Get-Item $file).LastWriteTime -ge $script:GameStartedAt) { $file }
}

function Read-GameOutcome {
    <#
    .SYNOPSIS Returns the XML of the Resume slot the last run left, e.g. (Read-GameOutcome).Session.Player.Money. Call after Stop-Game, before Restore-GameSaves.
    .DESCRIPTION
    Returns nothing, with a warning, if the run did not write Resume: it was killed or never entered a game. Reading
    the slot then would give the user's own save, not what the run did. Never throws, so it is safe in a finally
    block ahead of Restore-GameSaves.
    #>
    if (-not $script:GameStartedAt) {
        Write-Warning "No Start-Game in this PowerShell process, so there is no telling whether Resume is this run's. Run Start-Game and Read-GameOutcome in one command."
        return
    }
    $file = Get-GameRunResume
    if (-not $file) {
        Write-Warning "The run did not write Resume (killed, or never entered a game), so there is no outcome to read."
        return
    }
    try { Read-GameSave $file }
    catch { Write-Warning "Could not read $($file): $_" }
}

function New-GameBaseSave {
    <#
    .SYNOPSIS Starts a new game, exits cleanly and copies the Resume slot it leaves to a file, as a base for Invoke-Scenario.ps1.
    .DESCRIPTION
    A new game is played on the Tutorial map, as from the main menu, unless -Map names another.
    -Mod launches the game with that mod, as for Start-Game.
    Overwrites the Resume slot, so call it between Backup-GameSaves and Restore-GameSaves.
    #>
    param(
        [Parameter(Mandatory)][string]$Path,
        [string]$Map,
        [string]$Mod
    )

    Start-Game -Map $Map -Mod $Mod | Out-Null
    try {
        if (-not $Map) {
            Send-GameKey N
            Wait-GameLog 'Enter InGame state' | Out-Null
        }
        Start-Sleep -Seconds 2
    } finally {
        Stop-Game
    }
    $resume = Get-GameRunResume
    if (-not $resume) { throw "The game did not write Resume on exit, so there is no new game to copy (the slot holds the user's own save)." }
    Copy-Item $resume $Path -Force
    $Path
}

function Remove-GameSave {
    <#.SYNOPSIS Deletes a scenario slot written for a test run. Refuses Resume, which Restore-GameSaves puts back instead.#>
    param([Parameter(Mandatory)][string[]]$Name)

    foreach ($slot in $Name) {
        if ($slot -in $GameOwnSlots) { throw "'$slot' is the user's; restore it with Restore-GameSaves instead." }
        Remove-Item (Join-Path (Get-GameDataDir) "$slot$SavegameExt") -ErrorAction SilentlyContinue
    }
}

function Stop-Game {
    <#.SYNOPSIS Closes the game. Prefer this over Stop-Process so it exits cleanly.#>
    $proc = Get-GameProcess
    if (-not $proc) { return }
    $proc.CloseMainWindow() | Out-Null
    if (-not $proc.WaitForExit(10000)) {
        Write-Warning "FrameOfReference did not close within 10 s and was killed; it wrote no Resume."
        try { $proc.Kill() } catch [InvalidOperationException] {}  # exited in the meantime
    }
}

function Send-GameKey {
    <#.SYNOPSIS Posts a key press. Key is a [Windows.Forms.Keys] name, e.g. Escape, F1, D1, Space.#>
    param(
        [Parameter(Mandatory)][string]$Key,
        [int]$HoldMilliseconds = 50
    )

    Add-Type -AssemblyName System.Windows.Forms
    # Parsed rather than looked up as a static member, which would turn a misspelt name into key code 0
    $code = try { [int][Enum]::Parse([System.Windows.Forms.Keys], $Key, $true) }
            catch { throw "'$Key' is not a System.Windows.Forms.Keys name, e.g. Escape, F1, D1, Space." }
    $handle = Get-GameWindow
    Set-GameLogMark
    [GameWin]::PostMessage($handle, [GameWin]::WM_KEYDOWN, [IntPtr]$code, [IntPtr]0) | Out-Null
    Start-Sleep -Milliseconds $HoldMilliseconds
    [GameWin]::PostMessage($handle, [GameWin]::WM_KEYUP, [IntPtr]$code, [IntPtr]0) | Out-Null
}

function Send-GameClick {
    <#.SYNOPSIS Posts a click at client-area coordinates (as seen in Save-GameScreenshot output).#>
    param(
        [Parameter(Mandatory)][int]$X,
        [Parameter(Mandatory)][int]$Y,
        [switch]$Right
    )

    $handle = Get-GameWindow
    $lParam = [IntPtr](($Y -shl 16) -bor ($X -band 0xFFFF))
    $down = if ($Right) { [GameWin]::WM_RBUTTONDOWN } else { [GameWin]::WM_LBUTTONDOWN }
    $up = if ($Right) { [GameWin]::WM_RBUTTONUP } else { [GameWin]::WM_LBUTTONUP }

    Set-GameLogMark
    # Move first: OmegaGUI tracks hover state, and controls only react where the cursor is.
    [GameWin]::PostMessage($handle, [GameWin]::WM_MOUSEMOVE, [IntPtr]0, $lParam) | Out-Null
    Start-Sleep -Milliseconds 100
    [GameWin]::PostMessage($handle, $down, [IntPtr]1, $lParam) | Out-Null
    Start-Sleep -Milliseconds 60
    [GameWin]::PostMessage($handle, $up, [IntPtr]0, $lParam) | Out-Null
}

function Show-GameWindow {
    <#.SYNOPSIS Brings the game window to the front and waits until it has rendered again. Throws if Windows refuses.#>
    param([int]$Attempts = 5)

    $handle = Get-GameWindow
    if ([GameWin]::GetForegroundWindow() -eq $handle) { return }
    if ([GameWin]::IsIconic($handle)) { [GameWin]::ShowWindow($handle, [GameWin]::SW_RESTORE) | Out-Null }

    # Windows' foreground lock refuses SetForegroundWindow while the user is active in another app.
    # Two ways around it, tried in turn: a synthetic Alt press (counts as input from this process),
    # and attaching to the input queue of the thread that currently owns the foreground.
    for ($i = 0; $i -lt $Attempts -and [GameWin]::GetForegroundWindow() -ne $handle; $i++) {
        # Released only after the switch: a complete Alt tap in the user's app would open its menu bar
        [GameWin]::keybd_event([GameWin]::VK_MENU, 0, 0, [UIntPtr]::Zero)
        try { [GameWin]::SetForegroundWindow($handle) | Out-Null }
        finally { [GameWin]::keybd_event([GameWin]::VK_MENU, 0, [GameWin]::KEYEVENTF_KEYUP, [UIntPtr]::Zero) }
        Start-Sleep -Milliseconds 200
        if ([GameWin]::GetForegroundWindow() -eq $handle) { break }

        $foregroundThread = [GameWin]::GetWindowThreadProcessId([GameWin]::GetForegroundWindow(), [IntPtr]::Zero)
        $ownThread = [GameWin]::GetCurrentThreadId()
        $attached = $foregroundThread -ne 0 -and $foregroundThread -ne $ownThread -and [GameWin]::AttachThreadInput($ownThread, $foregroundThread, $true)
        try {
            [GameWin]::BringWindowToTop($handle) | Out-Null
            [GameWin]::SetForegroundWindow($handle) | Out-Null
        } finally {
            if ($attached) { [GameWin]::AttachThreadInput($ownThread, $foregroundThread, $false) | Out-Null }
        }
        Start-Sleep -Milliseconds 300
    }

    if ([GameWin]::GetForegroundWindow() -ne $handle) {
        throw "Could not bring the FrameOfReference window to the front, so a screenshot would show another window."
    }

    # The game stops rendering while it is in the background (dimmed picture, "FPS: 0.00") and needs a moment to catch up
    Start-Sleep -Milliseconds 2500
}

function Save-GameScreenshot {
    <#.SYNOPSIS Captures the game's client area to a PNG, bringing the window to the front first.#>
    param([Parameter(Mandatory)][string]$Path)

    $handle = Get-GameWindow
    Show-GameWindow

    $rect = New-Object GameWin+RECT
    [GameWin]::GetClientRect($handle, [ref]$rect) | Out-Null
    $origin = New-Object GameWin+POINT
    [GameWin]::ClientToScreen($handle, [ref]$origin) | Out-Null

    $width = $rect.Right - $rect.Left
    $height = $rect.Bottom - $rect.Top
    $bmp = New-Object Drawing.Bitmap $width, $height
    $gfx = [Drawing.Graphics]::FromImage($bmp)
    try {
        $gfx.CopyFromScreen($origin.X, $origin.Y, 0, 0, (New-Object Drawing.Size $width, $height))
        $bmp.Save($Path, [Drawing.Imaging.ImageFormat]::Png)
    } finally {
        $gfx.Dispose()
        $bmp.Dispose()
    }
    Write-Host "Screenshot ($width x $height) saved to $Path"
    $Path
}
