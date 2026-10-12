<#
.SYNOPSIS
Helpers for driving the Frame of Reference game window from PowerShell.

.DESCRIPTION
Dot-source this file, then use the functions below:

    . .\.claude\skills\drive-game-headless\GameDrive.ps1

    $outcome = Invoke-GameRun -Savegame MyScenario -RemoveSlots MyScenario {
        Send-GameKey Escape
        Send-GameClick 400 300
        Save-GameScreenshot shot.png
    }

Input is posted with PostMessage, so the window does not need focus to receive it
-- but screenshots capture the screen region the window occupies, so
Save-GameScreenshot brings the window to the front first (see Show-GameWindow).
#>

Add-Type -AssemblyName System.Drawing

# Matches Constants.SavegameFileExt and .MapFileExt in src\FrameOfReference\World\Constants.cs.
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
    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

    public const int SW_RESTORE = 9;
    public const uint SWP_NOSIZE = 0x0001, SWP_NOZORDER = 0x0004;

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
    public const uint WM_MOUSEWHEEL = 0x020A;
}
'@ -ErrorAction SilentlyContinue

# The Debug build: what Start-Game launches and where its portable-mode saves live
$GameRepoDir = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$GameBinDir = Join-Path $GameRepoDir 'artifacts\Debug\net10.0-windows'

# Portable-mode save directory of the Debug build (NOT the user profile)
$GameDataDir = Join-Path $GameBinDir 'data'

# The Debug build's portable-mode settings, which the game and editor also write (e.g. the window size after a resize)
$GameSettingsPath = Join-Path $GameBinDir 'config\Settings.xml'

# The slots the game writes by itself, which a test run therefore overwrites
$GameOwnSlots = @('Resume')

# What Backup-GameSaves and Restore-GameSaves put back: those slots and the settings Start-Game rewrites
$GameOwnFiles = @($GameOwnSlots | ForEach-Object { Join-Path $GameDataDir "$_$SavegameExt" }) + $GameSettingsPath

# Exists only between Backup-GameSaves and Restore-GameSaves
$GameSaveBackupDir = Join-Path $env:TEMP 'frame-of-reference-saves-backup'

function Get-GameProcess {
    <#
    .SYNOPSIS Returns the game process Start-Game launched in this PowerShell process, or nothing if there is none or it has exited.
    .DESCRIPTION Never any other FrameOfReference process: that may be the user's own game. -Required throws instead of returning nothing.
    #>
    param([switch]$Required)

    if ($script:GameProcess -and -not $script:GameProcess.HasExited) { $script:GameProcess }
    elseif ($Required) { throw "No game launched by Start-Game in this PowerShell process is running." }
}

function Get-GameWindow {
    <#.SYNOPSIS Returns the game's main window handle, or throws if it is not running.#>
    $proc = Get-GameProcess -Required
    $proc.Refresh()
    $handle = $proc.MainWindowHandle
    if ($handle -eq [IntPtr]::Zero) { throw "FrameOfReference has no main window yet -- wait for it to finish loading." }
    $handle
}

function Backup-GameSaves {
    <#
    .SYNOPSIS Copies aside the slot the game writes by itself (Resume) and its settings (which Start-Game changes). Invoke-GameRun calls this. Other slots are not covered: never overwrite them.
    .DESCRIPTION
    Refuses while an earlier backup is still waiting to be restored: the slots would hold that run's output by now,
    and backing them up would lose the user's own.
    #>
    if (Test-Path $GameSaveBackupDir) {
        throw "An earlier backup in $GameSaveBackupDir was never restored. Put it back with Restore-GameSaves first (or delete it, once you have checked the slots hold the user's saves)."
    }

    New-Item -ItemType Directory $GameSaveBackupDir | Out-Null
    foreach ($file in $GameOwnFiles) {
        if (Test-Path $file) { Copy-Item $file $GameSaveBackupDir }
    }
    Write-Host "Saves backed up to $GameSaveBackupDir"
}

function Restore-GameSaves {
    <#
    .SYNOPSIS Puts the user's save slot and settings back as they were at Backup-GameSaves, and removes the backup. Invoke-GameRun calls this.
    .DESCRIPTION A file that did not exist at the backup is deleted, so a run leaves no Resume or Settings.xml behind that the user never had.
    #>
    if (-not (Test-Path $GameSaveBackupDir)) { throw "No backup at $GameSaveBackupDir." }

    foreach ($file in $GameOwnFiles) {
        $backup = Join-Path $GameSaveBackupDir (Split-Path -Leaf $file)
        if (Test-Path $backup) { Copy-Item $backup $file -Force }
        elseif (Test-Path $file) { Remove-Item $file -Force }
    }
    Remove-Item $GameSaveBackupDir -Recurse -Force
    Write-Host "Saves restored from $GameSaveBackupDir"
}

function Set-GameWindowSettings {
    <#
    .SYNOPSIS Edits the Debug build's Settings.xml so the game starts windowed, with a client area of the given size.
    .DESCRIPTION Starts from src\FrameOfReference\Game\config\Settings.xml if there is none, since the built-in defaults are fullscreen and lack the ContentDir.
    #>
    param(
        [Parameter(Mandatory)][int]$Width,
        [Parameter(Mandatory)][int]$Height,
        [int]$AntiAliasing = 8
    )

    $source = if (Test-Path $GameSettingsPath) { $GameSettingsPath } else { Join-Path $GameRepoDir 'src\FrameOfReference\Game\config\Settings.xml' }
    $xml = New-Object Xml.XmlDocument
    $xml.Load($source)

    $display = $xml.DocumentElement.SelectSingleNode('Display')
    if (-not $display) { $display = $xml.DocumentElement.AppendChild($xml.CreateElement('Display')) }
    $fullscreen = $display.SelectSingleNode('Fullscreen')
    if (-not $fullscreen) { $fullscreen = $display.AppendChild($xml.CreateElement('Fullscreen')) }
    $fullscreen.InnerText = 'false'
    $windowSize = $display.SelectSingleNode('WindowSize')
    if (-not $windowSize) { $windowSize = $display.AppendChild($xml.CreateElement('WindowSize')) }
    # XmlSerializer writes a Size as attributes
    $windowSize.SetAttribute('Width', $Width)
    $windowSize.SetAttribute('Height', $Height)
    $antiAliasingNode = $display.SelectSingleNode('AntiAliasing')
    if (-not $antiAliasingNode) { $antiAliasingNode = $display.AppendChild($xml.CreateElement('AntiAliasing')) }
    $antiAliasingNode.InnerText = $AntiAliasing

    New-Item -ItemType Directory -Force (Split-Path -Parent $GameSettingsPath) | Out-Null
    $xml.Save($GameSettingsPath)
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
    .SYNOPSIS Launches the Debug build of the game in a window, optionally into a savegame or a fresh game on a map, and waits until it has finished loading. Invoke-GameRun calls this.
    .DESCRIPTION
    Width and Height are the window's client area. They are written to the game's settings, so Start-Game refuses to
    run outside Backup-GameSaves/Restore-GameSaves, which put the user's back. Also refuses while FrameOfReference is
    already running: that may be the user's own game, and Stop-Game would close it.
    #>
    param(
        [string]$Savegame,
        [string]$Map,  # a map in content\World\Maps (or the mod's World\Maps), e.g. Mountains
        [string]$Mod,  # a mod directory, overlaid on content\: a path, or a name in the Debug build's Mods folder
        [int]$Width = 1600,
        [int]$Height = 900,
        [int]$Screen = 0,
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
    if ($Savegame -and -not (Test-Path (Join-Path $GameDataDir "$Savegame$SavegameExt"))) {
        throw "No savegame '$Savegame' in $GameDataDir."
    }
    if ($Map) { Assert-GameMap $Map -ModDir $modDir }

    if (-not (Test-Path $GameSaveBackupDir)) { throw "Start the game through Invoke-GameRun: Start-Game changes the game's settings, and only Restore-GameSaves puts the user's back." }
    Set-GameWindowSettings $Width $Height

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
    Move-GameWindow -Screen $Screen
    Start-Sleep -Seconds 1  # let the first frames render
    $proc
}

function Get-GameScreens {
    <#.SYNOPSIS The monitors, primary first, then the others from left to right: what -Screen indexes.#>
    Add-Type -AssemblyName System.Windows.Forms
    @([System.Windows.Forms.Screen]::AllScreens | Sort-Object @{Expression = {-not $_.Primary}}, @{Expression = {$_.Bounds.X}})
}

function Move-GameWindow {
    <#
    .SYNOPSIS Puts the game window in the top left corner of a monitor's working area. Start-Game calls this.
    .DESCRIPTION
    Windows cascades each new window a little further down and right than the last, so after a few launches the bottom
    of the client area slips under the taskbar, which then shows in every screenshot. Screen 0 is the primary monitor,
    where Windows' toast notifications pop up over the bottom right corner; another monitor (Get-GameScreens) is clear of them.
    #>
    param([int]$Screen = 0)

    $screens = Get-GameScreens
    if ($Screen -ge $screens.Count) { throw "There is no screen $Screen; this machine has $($screens.Count)." }
    $area = $screens[$Screen].WorkingArea
    [GameWin]::SetWindowPos((Get-GameWindow), [IntPtr]::Zero, $area.X, $area.Y, 0, 0, [GameWin]::SWP_NOSIZE -bor [GameWin]::SWP_NOZORDER) | Out-Null
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
    .DESCRIPTION Send-GameKey, Send-GameClick, Send-GameWheel and Set-GameSlider call this themselves. Call it before anything else that is to be waited for, e.g. before letting game time pass.
    #>
    $script:GameLogMark = @(Read-GameLogLines).Count
}

function Wait-GameLog {
    <#
    .SYNOPSIS Waits until the running game logs a line matching a pattern, and returns that line.
    .DESCRIPTION
    Only lines from the running process count, so earlier runs in the same log file never match. Of those, only
    lines logged since the last Start-Game, input sent (Send-GameKey, Send-GameClick, Send-GameWheel, Set-GameSlider) or Set-GameLogMark count, and each
    Wait-GameLog moves past the line it returns, so waiting for the same pattern twice waits for two occurrences.
    Useful markers: 'Game startup' (done loading, at startup), 'Enter InGame state' (a game has been entered,
    e.g. after pressing N in the main menu), 'Loading GUI dialog: PauseMenu' (a dialog has opened),
    'Recorded arrival time for .* at ' (an entity has reached a waypoint).
    #>
    param(
        [Parameter(Mandatory)][string]$Pattern,
        [int]$TimeoutSeconds = 60
    )

    $proc = Get-GameProcess -Required
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
    .SYNOPSIS Returns the XML of a savegame, e.g. (Read-GameSave TestScenario).Session.Universe.LightPhase
    .DESCRIPTION
    Reads through System.IO.Compression: Windows PowerShell's Expand-Archive refuses any extension but .zip.
    For what a run did, use the outcome Invoke-GameRun returns instead: the Resume slot may still hold the user's own save.
    #>
    param(
        [Parameter(Mandatory)][string]$Name  # a slot name, or a path to a .FrameOfReferenceSave
    )

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $path = if (Test-Path $Name) { (Resolve-Path $Name).Path } else { Join-Path $GameDataDir "$Name$SavegameExt" }
    $zip = [IO.Compression.ZipFile]::OpenRead($path)
    try {
        $reader = New-Object IO.StreamReader($zip.GetEntry('data.xml').Open())
        try { [xml]$reader.ReadToEnd() } finally { $reader.Dispose() }
    } finally { $zip.Dispose() }
}

function Remove-GameSave {
    <#.SYNOPSIS Deletes a scenario slot written for a test run. Refuses Resume, which Restore-GameSaves puts back instead.#>
    param([Parameter(Mandatory)][string[]]$Name)

    foreach ($slot in $Name) {
        if ($slot -in $GameOwnSlots) { throw "'$slot' is the user's; restore it with Restore-GameSaves instead." }
        Remove-Item (Join-Path $GameDataDir "$slot$SavegameExt") -ErrorAction SilentlyContinue
    }
}

function Stop-Game {
    <#.SYNOPSIS Closes the game cleanly, which writes Resume. Invoke-GameRun calls this.#>
    $proc = Get-GameProcess
    if (-not $proc) { return }
    $proc.CloseMainWindow() | Out-Null
    if (-not $proc.WaitForExit(10000)) {
        Write-Warning "FrameOfReference did not close within 10 s and was killed; it wrote no Resume."
        try { $proc.Kill() } catch [InvalidOperationException] {}  # exited in the meantime
    }
}

function Invoke-GameRun {
    <#
    .SYNOPSIS Runs a script block against the game: backs up the user's saves, starts the game, runs the block, closes the game, puts the saves back, and returns the XML of the Resume the run wrote.
    .DESCRIPTION
    What the block outputs is shown, not returned. Variables it sets do not outlive it, so use them inside it.
    Returns nothing, with a warning, if the run did not write Resume: it was killed or never entered a game. Reading
    the slot then would give the user's own save, not what the run did.
    RemoveSlots are deleted afterwards, typically the scenario slot the run started from.
    #>
    param(
        [Parameter(Mandatory, Position = 0)][scriptblock]$Script,
        [string]$Savegame,
        [string]$Map,
        [string]$Mod,
        [string[]]$RemoveSlots,
        [int]$Width = 1600,
        [int]$Height = 900,
        [int]$Screen = 0
    )

    Backup-GameSaves
    try {
        try {
            Start-Game -Savegame $Savegame -Map $Map -Mod $Mod -Width $Width -Height $Height -Screen $Screen | Out-Null
            & $Script | Out-Host
        } finally {
            Stop-Game
        }

        # Read before the restore puts the user's Resume back; only a Resume newer than the launch is this run's
        $resume = Join-Path $GameDataDir "Resume$SavegameExt"
        if ($script:GameStartedAt -and (Test-Path $resume) -and (Get-Item $resume).LastWriteTime -ge $script:GameStartedAt) {
            try { Read-GameSave $resume }
            catch { Write-Warning "Could not read $($resume): $_" }
        } else {
            Write-Warning "The run did not write Resume (killed, or never entered a game), so there is no outcome to read."
        }
    } finally {
        try { Restore-GameSaves }
        finally { if ($RemoveSlots) { Remove-GameSave $RemoveSlots } }
    }
}

function Send-GameKey {
    <#
    .SYNOPSIS Posts a key press. Key is a [Windows.Forms.Keys] name, e.g. Escape, F1, D1, Space.
    .DESCRIPTION
    A longer -HoldMilliseconds keeps the key down, repeating WM_KEYDOWN as a real keyboard's auto-repeat would, e.g. to
    move the camera, which goes on for as long as its key is held.
    #>
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
    $release = (Get-Date).AddMilliseconds($HoldMilliseconds)
    Start-Sleep -Milliseconds ([Math]::Min($HoldMilliseconds, 50))
    while ((Get-Date) -lt $release) {
        # Bit 30 of lParam marks an auto-repeat: the key was already down
        [GameWin]::PostMessage($handle, [GameWin]::WM_KEYDOWN, [IntPtr]$code, [IntPtr]0x40000000) | Out-Null
        Start-Sleep -Milliseconds 30
    }
    [GameWin]::PostMessage($handle, [GameWin]::WM_KEYUP, [IntPtr]$code, [IntPtr]0) | Out-Null
}

function Send-GameWheel {
    <#.SYNOPSIS Posts mouse wheel notches over a point of the client area: positive away from the user (zooms in), negative towards.#>
    param(
        [Parameter(Mandatory)][int]$Notches,
        [int]$X = 800,
        [int]$Y = 450
    )

    $handle = Get-GameWindow
    # Unlike other mouse messages, WM_MOUSEWHEEL carries screen coordinates
    $point = New-Object GameWin+POINT
    $point.X = $X; $point.Y = $Y
    [GameWin]::ClientToScreen($handle, [ref]$point) | Out-Null
    $lParam = [IntPtr](($point.Y -shl 16) -bor ($point.X -band 0xFFFF))
    $delta = if ($Notches -gt 0) { 120 } else { -120 }
    Set-GameLogMark
    for ($i = 0; $i -lt [Math]::Abs($Notches); $i++) {
        [GameWin]::PostMessage($handle, [GameWin]::WM_MOUSEWHEEL, [IntPtr](($delta -band 0xFFFF) -shl 16), $lParam) | Out-Null
        Start-Sleep -Milliseconds 60
    }
}

function Send-GameMouse {
    <#.SYNOPSIS Posts a mouse message at client-area coordinates. Buttons is the wParam, 1 while a button is held.#>
    param(
        [Parameter(Mandatory)][uint32]$Message,
        [Parameter(Mandatory)][int]$X,
        [Parameter(Mandatory)][int]$Y,
        [int]$Buttons = 0
    )

    $lParam = [IntPtr](($Y -shl 16) -bor ($X -band 0xFFFF))
    [GameWin]::PostMessage((Get-GameWindow), $Message, [IntPtr]$Buttons, $lParam) | Out-Null
}

function Send-GameClick {
    <#
    .SYNOPSIS Posts a click at client-area coordinates (as seen in Save-GameScreenshot output).
    .DESCRIPTION
    The pointer stays where the click left it, so a control's tooltip or hover state comes up a moment later and shows
    in screenshots. -Park moves it to the right edge of the client area afterwards (Hide-GamePointer).
    #>
    param(
        [Parameter(Mandatory)][int]$X,
        [Parameter(Mandatory)][int]$Y,
        [switch]$Right,
        [switch]$Park
    )

    $down = if ($Right) { [GameWin]::WM_RBUTTONDOWN } else { [GameWin]::WM_LBUTTONDOWN }
    $up = if ($Right) { [GameWin]::WM_RBUTTONUP } else { [GameWin]::WM_LBUTTONUP }

    Set-GameLogMark
    # Move first: OmegaGUI tracks hover state, and controls only react where the cursor is.
    Send-GameMouse ([GameWin]::WM_MOUSEMOVE) $X $Y
    Start-Sleep -Milliseconds 100
    Send-GameMouse $down $X $Y -Buttons 1
    Start-Sleep -Milliseconds 60
    Send-GameMouse $up $X $Y
    if ($Park) { Hide-GamePointer }
}

function Hide-GamePointer {
    <#.SYNOPSIS Moves the (posted) pointer to the right edge of the client area, away from anything with a tooltip or hover state.#>
    $rect = New-Object GameWin+RECT
    [GameWin]::GetClientRect((Get-GameWindow), [ref]$rect) | Out-Null
    Start-Sleep -Milliseconds 150
    Send-GameMouse ([GameWin]::WM_MOUSEMOVE) ($rect.Right - 5) ([int]($rect.Bottom * 0.7))
}

function Set-GameSlider {
    <#
    .SYNOPSIS Moves an OmegaGUI slider by keyboard: presses the mouse button on its thumb, posts the keys, and lets go.
    .DESCRIPTION
    Posted input cannot drag a slider: it only follows the mouse while Control.MouseButtons reports the physical left
    button down. A click on its track moves it by a single step, and letting go of the thumb drops its keyboard focus.
    Keys sent while the button is held still reach it: End (maximum), Home (minimum), Right/Left (one step each).
    #>
    param(
        [Parameter(Mandatory)][int]$X,  # the thumb, in client coordinates
        [Parameter(Mandatory)][int]$Y,
        [string]$Key = 'End',
        [int]$Repeat = 1
    )

    Send-GameMouse ([GameWin]::WM_MOUSEMOVE) $X $Y
    Start-Sleep -Milliseconds 100
    Send-GameMouse ([GameWin]::WM_LBUTTONDOWN) $X $Y -Buttons 1
    Start-Sleep -Milliseconds 100
    try {
        for ($i = 0; $i -lt $Repeat; $i++) { Send-GameKey $Key }
    } finally {
        Start-Sleep -Milliseconds 100
        Send-GameMouse ([GameWin]::WM_LBUTTONUP) $X $Y
    }
}

function Save-GameScreenshotSeries {
    <#
    .SYNOPSIS Captures a numbered series of screenshots (00.png, 01.png, ...) into a directory, e.g. to follow an animation.
    .DESCRIPTION
    Only the first capture waits for the window to come to the front; the rest follow each other in about
    IntervalMilliseconds plus ~100 ms. Real time, not game time: the session's time warp factor still applies.
    #>
    param(
        [Parameter(Mandatory)][string]$Directory,
        [int]$Count = 30,
        [int]$IntervalMilliseconds = 300
    )

    New-Item -ItemType Directory -Force $Directory | Out-Null
    for ($i = 0; $i -lt $Count; $i++) {
        Save-GameScreenshot (Join-Path $Directory ('{0:00}.png' -f $i)) | Out-Null
        Start-Sleep -Milliseconds $IntervalMilliseconds
    }
}

function Save-GameContactSheet {
    <#
    .SYNOPSIS Tiles the same region of a series of screenshots into one labelled image, so a whole animation can be reviewed in a single Read.
    .DESCRIPTION
    Takes the files named like 00.png from Save-GameScreenshotSeries, or the given -Files. -Zoom above 1 enlarges each
    tile (nearest neighbour), for small details such as whether two models touch. -Zoom below 1 shrinks it (smoothly),
    e.g. -Zoom 0.4 -Columns 3 over whole 1600x900 screenshots to review a scouting run's steps in one Read.
    #>
    param(
        [string]$Directory,
        [string[]]$Files,
        [Parameter(Mandatory)][int]$X,
        [Parameter(Mandatory)][int]$Y,
        [Parameter(Mandatory)][int]$Width,
        [Parameter(Mandatory)][int]$Height,
        [Parameter(Mandatory)][string]$Path,
        [int]$Columns = 6,
        [double]$Zoom = 1
    )

    if (-not $Files) { $Files = @(Get-ChildItem $Directory -Filter *.png | Where-Object Name -match '^\d+\.png$' | Sort-Object Name | ForEach-Object FullName) }
    if (-not $Files) { throw "No screenshots to tile." }

    $tileWidth = [int]($Width * $Zoom); $tileHeight = [int]($Height * $Zoom)
    $sheet = New-Object Drawing.Bitmap ($tileWidth * [Math]::Min($Columns, $Files.Count)), ($tileHeight * [Math]::Ceiling($Files.Count / $Columns))
    $gfx = [Drawing.Graphics]::FromImage($sheet)
    $font = New-Object Drawing.Font 'Arial', 12
    try {
        $gfx.InterpolationMode = if ($Zoom -lt 1) { 'HighQualityBicubic' } else { 'NearestNeighbor' }
        for ($i = 0; $i -lt $Files.Count; $i++) {
            $tile = New-Object Drawing.Rectangle (($i % $Columns) * $tileWidth), ([Math]::Floor($i / $Columns) * $tileHeight), $tileWidth, $tileHeight
            $image = [Drawing.Image]::FromFile($Files[$i])
            try { $gfx.DrawImage($image, $tile, (New-Object Drawing.Rectangle $X, $Y, $Width, $Height), [Drawing.GraphicsUnit]::Pixel) }
            finally { $image.Dispose() }
            $gfx.DrawString([IO.Path]::GetFileNameWithoutExtension($Files[$i]), $font, [Drawing.Brushes]::Yellow, $tile.X + 2, $tile.Y + 2)
        }
        $sheet.Save($Path, [Drawing.Imaging.ImageFormat]::Png)
    } finally {
        $font.Dispose()
        $gfx.Dispose()
        $sheet.Dispose()
    }
    $Path
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
