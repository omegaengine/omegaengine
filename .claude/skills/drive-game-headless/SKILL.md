---
name: drive-game-headless
description: Launch and drive the Frame of Reference game window from PowerShell without a human — post key and mouse input to it, screenshot the client area, build savegame scenarios through the World model, and read back what a run did from the save it leaves. Use when a change has to be confirmed in the running game or a screenshot of it is wanted, and no unit test can cover it.
---

# Driving Frame of Reference headlessly

## Ask first

Opening game windows steals focus and can disrupt whatever else the user is
doing, and a test run **overwrites their `Resume` slot and settings**. Ask for
approval before launching.

## Prefer a unit test

This route is slow and unreliable for anything timing-dependent — frame pacing,
animations, physics settling. If the behaviour lives in `src/FrameOfReference/World`
it almost certainly can, and should, be covered by an xUnit test in
`src/FrameOfReference/UnitTests` instead. Reach for the game window only for
things that genuinely need the real render loop or the real GUI: dialog layout,
HUD bindings, Lua wiring, visuals. For how a map *renders* from a fixed camera,
including before/after comparisons against another commit, use the
`headless-render` skill instead: it needs no game window and no saves.

## Helpers

```powershell
. .\.claude\skills\drive-game-headless\GameDrive.ps1
```

| Function | Purpose |
|---|---|
| `Invoke-GameRun { ... } [-Savegame <Name> \| -Map <Name>] [-Mod <Dir>] [-RemoveSlots <Name>] [-Width 1600] [-Height 900] [-Screen 0]` | The whole run: back up the user's `Resume` and `config\Settings.xml`; launch the Debug exe **windowed** at that client size, put it in the top left corner of monitor `-Screen` (`Move-GameWindow`) and wait until it has **finished loading**; run the block; close the game cleanly; put the backup back; delete `-RemoveSlots`. Returns the `[xml]` of the `Resume` the run wrote, or nothing with a warning if it wrote none. Refuses while the game is already running (it may be the user's), with a savegame, map or mod that does not exist (the game would block on a message box), and while an earlier backup was never restored |
| `Wait-GameLog <Pattern> [-TimeoutSeconds 60]` | Wait until the running game logs a matching line, e.g. `'Enter InGame state'`. Only counts lines since the game started, the last input sent (`Send-GameKey`, `Send-GameClick`, `Send-GameWheel`, `Set-GameSlider`) or the last `Set-GameLogMark`, and moves past the line it returns, so a second call waits for a second occurrence |
| `Set-GameLogMark` | Make `Wait-GameLog` ignore everything logged so far, e.g. before letting game time pass without sending input |
| `Send-GameKey <Key> [-HoldMilliseconds 50]` | Post `WM_KEYDOWN`/`WM_KEYUP` (key name from `System.Windows.Forms.Keys`); a longer hold auto-repeats like a real keyboard, e.g. to move the camera |
| `Send-GameClick <X> <Y> [-Right] [-Park]` | Post a move, then button down/up at client coordinates; `-Park` moves the pointer off the control afterwards (`Hide-GamePointer`), so no tooltip or hover state comes up |
| `Send-GameWheel <Notches> [-X] [-Y]` | Post mouse wheel notches; positive zooms in |
| `Get-GameScreens` / `Move-GameWindow [-Screen 0]` | The monitors (primary first, then left to right), and moving the window to one of them |
| `Set-GameSlider <X> <Y> [-Key End] [-Repeat 1]` | Set a slider: press on its thumb, post `End`/`Home`/`Right`/`Left` while held, let go (see below) |
| `Show-GameWindow` | Bring the window to the front and wait until it renders again; throws if Windows refuses |
| `Save-GameScreenshot <Path>` | Capture the client rect to PNG (calls `Show-GameWindow` first) |
| `Save-GameScreenshotSeries <Dir> [-Count 30] [-IntervalMilliseconds 300]` | Capture `00.png`, `01.png`, ... to follow an animation |
| `Save-GameContactSheet -Directory <Dir> -X -Y -Width -Height -Path <Png> [-Columns 6] [-Zoom 1]` | Tile one region of a series into a single labelled image, to review it in one `Read`; `-Files` picks frames, `-Zoom` above 1 enlarges, below 1 shrinks |
| `Read-GameSave <Name>` | The `[xml]` of a slot or save file, e.g. `(Read-GameSave TestScenario).Session.Universe.LightPhase` |

A run looks like (`$scratch` being the session's scratchpad directory):

```powershell
. .\.claude\skills\drive-game-headless\GameDrive.ps1
$outcome = Invoke-GameRun -Savegame TestScenario -RemoveSlots TestScenario {
    Send-GameKey Escape
    Save-GameScreenshot "$scratch\shot.png"
}
$outcome.Session.Universe.GameTime
($outcome.Session.Universe.Entity | Where-Object Name -eq Hero).Position
```

Entities are `<Entity Name="...">` elements directly under `Universe`, and
`Where-Object Name` matches that attribute, not the element name.

What the block outputs is shown, not returned, and variables set in it do not
outlive it, so use them inside it. Then `Read` the PNG to see the result, and
`SendUserFile` it if the user should see it too. `Start-Game`, `Stop-Game`,
`Backup-GameSaves` and `Restore-GameSaves` are what `Invoke-GameRun` is made
of; don't call them yourself.

**Check outcomes in the save, looks in the screenshot.** Whether something
*happened* (entity positions, waypoints reached, triggers fired, game time) is
better read from the outcome `Invoke-GameRun` returns than from pixels. Never
use `Read-GameSave Resume` for this: a run that was killed or never entered a
game leaves the user's own `Resume` there.

## Things that will bite you

- **Saves are clobbered.** Portable-mode slots live in
  `artifacts/Debug/net472/data/`, *not* the user profile, alongside many more
  of the user's saves that the backup does not cover. Give scenario slots names
  of their own (`Invoke-Scenario.ps1` refuses existing ones) and pass them
  to `-RemoveSlots`.
- **The window size is the run's, not the user's.** `Invoke-GameRun` rewrites
  `artifacts/Debug/net472/config/Settings.xml` to windowed mode at
  `-Width`/`-Height` before launching. Windows shrinks a window that does not
  fit the screen, so check the size `Save-GameScreenshot` prints. Smaller
  windows make cheaper screenshots; for GUI layout checks pick a typical
  resolution, since the GUI scales with the window.
- **Anything on top of the window ends up in screenshots**, since they copy
  the screen. Windows cascades each new window further down and right, which
  would soon push the bottom of the client area under the taskbar, so
  `Start-Game` moves it into the top left corner of the working area. Toast
  notifications (e.g. other Claude sessions asking for permission) pop up over
  the bottom right of the *primary* monitor and overlap a 1600-wide window
  there; with a second monitor, `-Screen 1` keeps the game clear of them. The
  user may also be working at the machine, so look at every screenshot for
  windows that are not the game's.
- **A backup left by a killed run blocks the next one.** `Backup-GameSaves`
  refuses while its backup directory (`$GameSaveBackupDir` in `GameDrive.ps1`)
  exists. If no FrameOfReference is running and the backup is from an earlier
  session (compare its files' times and contents with the slots), it holds the
  user's files: put them back with `Restore-GameSaves`, then run again.
- **Loading takes a while after the window appears.** The map and its terrain
  load behind a splash, and input posted meanwhile is queued and fires later,
  once the screenshot meant to show it has already been taken.
  `Invoke-GameRun` waits for the log's `Game startup` line before running the
  block; use `Wait-GameLog` for later moments rather than guessing at sleeps.
- **Only the pause menu (`Escape`) freezes the game.** Under any other dialog,
  entities keep moving and the light phase keeps advancing, so time captures
  with that in mind.
- **`Escape` goes to the top dialog, not always to the pause menu.** Dialogs
  bind it too: with the main mod, a new game opens the tutorial's welcome
  dialog, where `Escape` is "Skip tutorial" and loads the `Story` map, whose
  own welcome dialog binds it again. To reach the pause menu, close such a
  dialog first, or start from a save or a `-Map` that opens none.
- **The light phase advances with game time**, so the time of day in a
  screenshot drifts the longer a run goes on. Set it in the scenario
  (`Invoke-Scenario.ps1 --light-phase`; `Invoke-GameRun` has no such option)
  and screenshot soon after loading when it matters.
- **Resume is written whenever a game is left**, to the main menu as well as on
  exit, so a run that went back to the menu still has an outcome.
- **Screenshots capture the screen**, not the window buffer (`PrintWindow`
  returns black for the Direct3D window), so `Save-GameScreenshot` steals focus
  to front the window and throws if Windows refuses. Input, by contrast, is
  posted and needs no focus.
- **Wait for the frame.** A change only appears once the render loop has
  ticked, so a screenshot taken right after input can miss it. Wait for what
  the game logs where there is something (`'Loading GUI dialog: PauseMenu'`
  for a dialog opening, `'Recorded arrival time for .* at '` for an entity
  reaching a waypoint); a short sleep is only for changes that log nothing,
  such as a list selection or a slider moving.
- **Content is read from the live repo tree**, so edits under `content/` take
  effect without a rebuild. Code changes need a **Debug** build first, since
  that is what `Invoke-GameRun` launches: `dotnet build src\OmegaEngine.slnx`
  (or `dotnet test src\OmegaEngine.slnx`, which builds the same).
  `src\build.ps1` builds Release and leaves the Debug exe stale.

## Running with a mod

A mod is a folder laid out like `content/` that holds only the files it adds
or changes. While it is active, every content lookup checks it before
`content/`. To test content in a mod (maps, entity templates, GUI dialogs,
textures), pass it everywhere content is resolved:

- `Invoke-GameRun -Mod <Dir>` launches with `/mod`. `-Map` then also finds
  maps in the mod's `World/Maps`.
- `Invoke-Scenario.ps1 ... --mod <Dir>` loads the mod's templates and maps, so
  `--new` and `add-entity` can use them. Without it, a save that uses them
  fails to build.

`<Dir>` is a path to the mod folder, or the name of a folder in
`artifacts/Debug/net472/Mods/` (where the game itself looks for `/mod <name>`).
AlphaEditor keeps mods wherever the user created them, so you'll usually pass
a path. The main one is `..\frame-of-reference-extra\content`, a sibling repo
with most of the game's models, textures and maps, including the `Story` map.
The user's saves often need it. A missing mod is caught before launch, because
the game would report it in a message box that blocks until someone closes it.

Saves are not tied to a mod: slots live in the same data folder either way.
They still expect the mod's content, though, so load a save built with a mod
with the same mod. The main menu's map can also come from the mod, so the
menu screenshots differ too.

## Finding what to press

GUI hotkeys and button names are in `content/GUI/**/*.xml` — read the dialog
before guessing at keys or click coordinates. With a mod, search its `GUI/`
folder as well: maps open dialogs that only the mod has, such as the main
mod's `GUI/Tutorial/` and `GUI/Story/`. Control positions in the XML are
in dialog space, and the GUI is scaled to the window, so they do not match
screenshot pixels: locate click targets, and any region to scan for pixels, in
a screenshot.

Click coordinates are only known once you have seen the dialog, and shell state
does not survive between calls, so work in two runs: a **scouting run** that
opens the dialog and screenshots it, then the **real run** with the coordinates
read off that screenshot. Screenshot the dialog again just before confirming
(e.g. before `Return`), to see what was actually selected.

A scouting run that screenshots every step leaves many full-size images, and
reading them one by one is slow and costly. Tile them scaled down instead, nine
to a sheet, labelled with their file names:

```powershell
Save-GameContactSheet -Files $shots -X 0 -Y 0 -Width 1600 -Height 900 -Zoom 0.4 -Columns 3 -Path "$scratch\scout.png"
```

At that size, dialog layouts and list highlights are still legible;
`Read` a single screenshot for small text such as a slider's value.

## Working the GUI

- **Buttons:** prefer the hotkey from the dialog XML over a click. `Return`
  usually confirms and `Escape` closes (but see above for what `Escape` does
  under the mod's dialogs).
- **Sliders cannot be dragged** with posted input: OmegaGUI only follows a drag
  while the *physical* left button is down. A click on the track moves the
  value by a single step. Letting go of the thumb drops its keyboard focus, so
  `End` sent after a click does nothing. Instead, use `Set-GameSlider` at the
  thumb: it holds the button down while it posts `End` (maximum), `Home`
  (minimum) or `Right`/`Left` (one step each, `-Repeat` for more). The
  Settings dialog (`content/GUI/Settings.xml`) has several.
- **Tooltips and hover:** the posted pointer stays where a click left it, and
  the control's tooltip or hover state comes up shortly after. Click with
  `-Park` when it would spoil a screenshot.

## Checking animations

A single screenshot rarely shows an animation, and reading 30 full screenshots
one by one is slow and costly. Capture a series with
`Save-GameScreenshotSeries` right after the input that starts it, then tile the
region around the entity into one image with `Save-GameContactSheet` and `Read`
that. For fine detail (does the unit touch the building?) build a second
sheet of just the interesting frames with `-Files` and `-Zoom 2`.

Screenshots show where things are, not why. To read an internal value in the
real engine (a pathfinding result, a computed distance), add a temporary
`NanoByte.Common.Log.Info("TEMP ...")`, rebuild Debug, catch the line with
`Wait-GameLog 'TEMP'`, and remove the line afterwards.

## Building a scenario

Build scenarios with `Invoke-Scenario.ps1` rather than by editing save XML. It
starts from a new game on a map or an existing save through the World model,
changes it, and writes it with `Session.Save`, so entity templates are really
resolved and the result loads like any other save. Hand-written template data
and waypoints are easy to get subtly wrong.

1. **The scenario**, written as a slot in the Debug data folder. `--new <map>`
   starts from a new game on a map in `content/World/Maps` (or the mod's), as
   the main menu does with `Tutorial`; a save file or slot name in its place
   starts from that save instead:

   ```powershell
   & .\.claude\skills\drive-game-headless\Invoke-Scenario.ps1 --new Tutorial TestDwarf `
       --light-phase 1 `
       --script add-entity --set template=Test.Dwarf --set name=Hero --set position=1000,1000 `
       --player Hero
   ```

   Options cover the common set-ups (`--light-phase`, `--time-warp`, `--camera`,
   `--position`, `--player`); `--help` lists them. It prints the state it wrote,
   read back from the file. The entities in the stock maps have no names, so
   add a named one with `add-entity` for `--position` and `--player`. Leave out
   the slot name (`TestDwarf`) to write nothing and only see the result.

   A `--set` belongs to the `--script` it follows, so one script can run
   several times with different settings, e.g. two entities:
   `--set template=Test.Dwarf --script add-entity --set name=A --set position=1000,1000 --script add-entity --set name=B --set position=1100,1000`.
   A `--set` before the first `--script` is shared by all of them.
2. **Anything else** goes in `harness/Scripts.cs` as a named script taking its
   settings from `--set`; `--list-scripts` shows what is there. Keep scripts
   that are likely to be wanted again, such as `add-entity`.

The harness references `src/FrameOfReference/World`, so it always matches the
current model; it builds itself on first use. To inspect a save by hand,
`Read-GameSave` returns its XML: a serialized `Session` wrapping a `Universe`, so
the same element shapes as the `data.xml` inside `content/World/Maps/*.FrameOfReferenceMap`
apply. Map coordinates are the `Position` values found there.
