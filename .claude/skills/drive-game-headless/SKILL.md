---
name: drive-game-headless
description: Launch and drive the Frame of Reference game window from PowerShell without a human — post key and mouse input to it, screenshot the client area, build savegame scenarios through the World model, and read back what a run did from the save it leaves. Use when a change has to be confirmed in the running game or a screenshot of it is wanted, and no unit test can cover it.
---

# Driving Frame of Reference headlessly

## Ask first

Opening game windows steals focus and can disrupt whatever else the user is
doing, and a test run **overwrites their `Resume` slot**. Ask for approval before
launching.

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
| `Backup-GameSaves` / `Restore-GameSaves` | Copy the `Resume` slot aside and put it back exactly as it was (deleting it if the user did not have one). Backup refuses while an earlier one was never restored |
| `Start-Game [-Savegame <Name> \| -Map <Name>] [-Mod <Dir>]` | Launch the Debug exe and return once it has **finished loading** (waits for the log). Refuses while the game is already running, since that may be the user's, and refuses a savegame, map or mod that does not exist |
| `Stop-Game` | Close the game `Start-Game` launched cleanly (`CloseMainWindow`, then kill with a warning on timeout); a clean exit from a game writes `Resume`. Like all the functions acting on the game, it never touches one this PowerShell process did not launch |
| `Read-GameOutcome` | The `[xml]` of the `Resume` this run wrote, or nothing with a warning if it wrote none |
| `Wait-GameLog <Pattern>` | Wait until the running game logs a matching line, e.g. `'Enter InGame state'`. Only counts lines since the last `Start-Game`, `Send-GameKey`, `Send-GameClick` or `Set-GameLogMark`, and moves past the line it returns, so a second call waits for a second occurrence |
| `Set-GameLogMark` | Make `Wait-GameLog` ignore everything logged so far, e.g. before letting game time pass without sending input |
| `Send-GameKey <Key>` | Post `WM_KEYDOWN`/`WM_KEYUP` (key name from `System.Windows.Forms.Keys`) |
| `Send-GameClick <X> <Y> [-Right]` | Post a move, then button down/up at client coordinates |
| `Show-GameWindow` | Bring the window to the front and wait until it renders again; throws if Windows refuses |
| `Save-GameScreenshot <Path>` | Capture the client rect to PNG (calls `Show-GameWindow` first) |
| `Read-GameSave <Name>` | The `[xml]` of a slot or save file, e.g. `(Read-GameSave TestScenario).Session.Universe.LightPhase` |
| `New-GameBaseSave <Path> [-Map <Name>] [-Mod <Dir>]` | Start a new game, exit, and copy the `Resume` it leaves to a file, as a base for scenarios |
| `Remove-GameSave <Name>` | Delete scenario slots written for the run (refuses `Resume`) |

A run looks like (`$scratch` being the session's scratchpad directory):

```powershell
. .\.claude\skills\drive-game-headless\GameDrive.ps1
Backup-GameSaves
try {
    Start-Game -Savegame TestScenario
    Send-GameKey Escape
    Save-GameScreenshot "$scratch\shot.png"
} finally {
    try {
        Stop-Game
        $outcome = Read-GameOutcome   # what the run did, before the restore puts the user's Resume back
    } finally {
        Restore-GameSaves              # in a finally of its own, so nothing above can skip it
        Remove-GameSave TestScenario
    }
}
```

Run the whole bracket in **one** PowerShell call: shell state does not survive
between calls, and `Read-GameOutcome` only knows the run started in its own
process. Then `Read` the PNG to see the result, and `SendUserFile` it if the
user should see it too.

**Check outcomes in the save, looks in the screenshot.** Whether something
*happened* (entity positions, waypoints reached, triggers fired, game time) is
better read from `Read-GameOutcome` than from pixels. Never use
`Read-GameSave Resume` for this: a run that was killed or never entered a game
leaves the user's own `Resume` there.

## Things that will bite you

- **Saves are clobbered.** Portable-mode slots live in
  `artifacts/Debug/net472/data/`, *not* the user profile, alongside many more
  of the user's saves that the backup does not cover. Give scenario slots names
  of their own (`Invoke-Scenario.ps1` refuses existing ones) and
  `Remove-GameSave` them afterwards.
- **Loading takes a while after the window appears.** The map and its terrain
  load behind a splash, and input posted meanwhile is queued and fires later,
  once the screenshot meant to show it has already been taken. `Start-Game`
  waits for the log's `Game startup` line; use `Wait-GameLog` for later moments
  rather than guessing at sleeps.
- **Only the pause menu (`Escape`) freezes the game.** Under any other dialog,
  entities keep moving and the light phase keeps advancing, so time captures
  with that in mind.
- **`Escape` goes to the top dialog, not always to the pause menu.** Dialogs
  bind it too: with the main mod, a new game opens the tutorial's welcome
  dialog, where `Escape` is "Skip tutorial" and loads the `Story` map, whose
  own welcome dialog binds it again. To reach the pause menu, close such a
  dialog first, or start from a save or a `-Map` that opens none.
- **The light phase advances with game time**, so the time of day in a
  screenshot drifts the longer a run goes on. Set it with `--light-phase` and
  screenshot soon after loading when it matters.
- **Resume is written whenever a game is left**, to the main menu as well as on
  exit, so a run that went back to the menu still has an outcome.
- **Screenshots capture the screen**, not the window buffer (`PrintWindow`
  returns black for the Direct3D window), so `Save-GameScreenshot` steals focus
  to front the window and throws if Windows refuses. Input, by contrast, is
  posted and needs no focus.
- **Wait for the frame.** After input, sleep before screenshotting; the change
  only appears once the render loop has ticked.
- **Content is read from the live repo tree**, so edits under `content/` take
  effect without a rebuild. Code changes need a **Debug** build first, since
  that is what `Start-Game` launches: `dotnet build src\OmegaEngine.slnx`.
  `src\build.ps1` builds Release and leaves the Debug exe stale.

## Running with a mod

A mod is a folder laid out like `content/` that holds only the files it adds
or changes. While it is active, every content lookup checks it before
`content/`. To test content in a mod (maps, entity templates, GUI dialogs,
textures), pass it everywhere content is resolved:

- `Start-Game -Mod <Dir>` and `New-GameBaseSave -Mod <Dir>` launch with
  `/mod`. `-Map` then also finds maps in the mod's `World/Maps`.
- `Invoke-Scenario.ps1 ... --mod <Dir>` loads the mod's templates, so
  `add-entity` can use them. Without it, a save that uses them fails to build.

`<Dir>` is a path to the mod folder, or the name of a folder in
`artifacts/Debug/net472/Mods/` (where the game itself looks for `/mod <name>`).
AlphaEditor keeps mods wherever the user created them, so you'll usually pass
a path. The main one is `..\frame-of-reference-extra\content`, a sibling repo
with most of the game's models, textures and maps, including the `Story` map.
The user's saves often need it. A missing mod is caught before launch, because the game would report
it in a message box that blocks until someone closes it.

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

## Building a scenario

Build scenarios with `Invoke-Scenario.ps1` rather than by editing save XML. It
loads a base save through the World model, changes it, and writes it with
`Session.Save`, so entity templates are really resolved and the result loads like
any other save. Hand-written template data and waypoints are easy to get subtly
wrong.

1. **A base save.** Any existing save works. For a fresh one, inside the
   backup/restore bracket:
   `New-GameBaseSave "$scratch\Base.FrameOfReferenceSave"` (new game: the
   `Tutorial` map, with no entities on it). `-Map Mountains` starts from another
   map in `content/World/Maps` instead. The entities in the stock maps have no
   names, so add a named one with `add-entity` for `--position` and `--player`.
2. **The scenario**, written as a slot in the Debug data folder:

   ```powershell
   & .\.claude\skills\drive-game-headless\Invoke-Scenario.ps1 "$scratch\Base.FrameOfReferenceSave" TestDwarf `
       --light-phase 1 `
       --script add-entity --set template=Test.Dwarf --set name=Hero --set position=1000,1000 `
       --player Hero
   ```

   Options cover the common set-ups (`--light-phase`, `--time-warp`, `--camera`,
   `--position`, `--player`); `--help` lists them. It prints the state it wrote,
   read back from the file.
3. **Anything else** goes in `harness/Scripts.cs` as a named script taking its
   settings from `--set`; `--list-scripts` shows what is there. Keep scripts
   that are likely to be wanted again, such as `add-entity`.

The harness references `src/FrameOfReference/World`, so it always matches the
current model; it builds itself on first use. To inspect a save by hand,
`Read-GameSave` returns its XML: a serialized `Session` wrapping a `Universe`, so
the same element shapes as the `data.xml` inside `content/World/Maps/*.FrameOfReferenceMap`
apply. Map coordinates are the `Position` values found there.
