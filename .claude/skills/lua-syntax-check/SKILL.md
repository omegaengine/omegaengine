---
name: lua-syntax-check
description: Parse-check the Lua embedded in OmegaGUI dialogs (content/GUI/**/*.xml `<OnShow>`, `<OnUpdate>`, `<OnClick>`, ... and content/GUI/**/*.lua) with real Lua, and export single chunks to run against stubbed globals, to diff the behaviour of a refactored dialog helper. Use after editing any dialog or script under content/GUI (or a mod's GUI folder), before handing the edit over.
---

# Lua syntax check for OmegaGUI dialogs

A syntax error in a dialog script is only discovered when that dialog is opened
in-game. Run this check before handing over any `content/GUI` edit.

## Run it

```powershell
& .\.claude\skills\lua-syntax-check\check-lua.ps1
```

Checks every dialog and `.lua` file under `content/GUI` (~1.5s). Pass `-Path` to
narrow it to one file or subdirectory, or to check a mod's dialogs, such as
those of the main mod:

```powershell
& .\.claude\skills\lua-syntax-check\check-lua.ps1 -Path content\GUI\InGame
& .\.claude\skills\lua-syntax-check\check-lua.ps1 -Path ..\frame-of-reference-extra\content\GUI
```

It prints `OK: <n> chunk(s) parsed cleanly.` when every chunk parsed. On failure
it exits with code 1 and names the dialog file, the event node, and the
enclosing control, with the Lua error and its line *within that chunk*.

The syntax check needs `luac` on the `PATH`; the behaviour check below needs
`lua`. If either is absent, say so and skip that check rather than pretending
it passed — don't try to install a Lua toolchain.

## Why chunks, not files

Each `<On*>` element is loaded as its own chunk at runtime, so they must be
checked separately: a local declared in `<OnShow>` is not in scope in
`<OnUpdate>`, and a stray `end` in one chunk must not be masked by the next.

Reading the dialog with `[xml]` (as the script does) un-escapes the `&lt;` and
`&amp;` that the XML text content requires, so the checker sees the same source
the game does. When writing Lua into a dialog, remember the escaping goes the
other way: `a &lt;= b` in the XML is `a <= b` at runtime.

## Checking behaviour, not just syntax

The script only parses. To see what a chunk *does* without launching the game,
e.g. to diff a refactored dialog helper against the old one, export the chunks
and run one with `lua` against stubbed globals (`$scratch` being the session's
scratchpad directory):

```powershell
& .\.claude\skills\lua-syntax-check\check-lua.ps1 -Path content\GUI\Settings.xml -ExportTo "$scratch\lua"
```

`-ExportTo` writes each chunk to a file of its own, un-escaped as the game
sees it, and lists them: `016-Settings-OnChanged-MusicVolume.lua  <-
...Settings.xml :: OnChanged on 'MusicVolume'`. It checks nothing.

Then write the stubs with the `Write` tool: plain tables for the GUI controls
and C# objects the chunk touches.

```lua
-- $scratch\stubs.lua
Settings = { Audio = { MusicVolume = 0.5 } }
MusicVolume = { Value = 7 }
MusicVolumeValueLabel = { Text = "" }
```

```powershell
& lua -e "dofile([[$scratch\stubs.lua]])" -e "dofile([[$scratch\lua\016-Settings-OnChanged-MusicVolume.lua]]); print(MusicVolumeValueLabel.Text)"
```

The `-e` options run in order, so the chunk sees the stubs as globals, and the
`print` after it shows what it wrote. To compare old and new behaviour, export
the dialog as it was (`git show HEAD:content/GUI/...` into a scratch file, then
`-Path` that file, `-ExportTo` another directory), run both chunks against the
same stubs, and compare the output. A chunk that calls helpers from
`content/GUI/init.lua` needs that loaded as well: add another `dofile` for it
after the stubs. Wrap the chunk's `dofile` in `pcall` when the point is to see
whether it errors rather than to trust its output.
