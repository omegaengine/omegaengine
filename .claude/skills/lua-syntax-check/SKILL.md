---
name: lua-syntax-check
description: Parse-check the Lua embedded in OmegaGUI dialogs (content/GUI/**/*.xml `<OnShow>`, `<OnUpdate>`, `<OnClick>`, ... and content/GUI/**/*.lua) with real Lua, and PCall stubbed chunks to diff behaviour of refactored dialog helpers. Use after editing any file under content/GUI (or a mod's GUI folder), before handing the edit over.
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

Exit code 0 means every chunk parsed. On failure it names the dialog file, the
event node, and the enclosing control, with the Lua error and its line *within
that chunk*.

The syntax check needs `luac` on the `PATH`; the behaviour check below needs
`lua` as well. If either is absent, say so and skip that check rather than
pretending it passed — don't try to install a Lua toolchain.

## Why chunks, not files

Each `<On*>` element is loaded as its own chunk at runtime, so they must be
checked separately: a local declared in `<OnShow>` is not in scope in
`<OnUpdate>`, and a stray `end` in one chunk must not be masked by the next.

Reading the dialog with `[xml]` (as the script does) un-escapes the `&lt;` and
`&amp;` that the XML text content requires, so the checker sees the same source
the game does. When writing Lua into a dialog, remember the escaping goes the
other way: `a &lt;= b` in the XML is `a <= b` at runtime.

## Checking behaviour, not just syntax

The same harness can `PCall` a chunk against stubbed globals. This is how to
diff old-vs-new behaviour of a refactored dialog helper without launching the
game: stub the GUI controls and C# objects the chunk touches as tables, run both
versions, compare what they wrote.

```powershell
$lua = @'
-- stubs for the controls/objects the chunk reads and writes
Settings = { Audio = { MusicVolume = 0.5 } }
MusicVolume = { Value = 7 }
MusicVolumeValueLabel = { Text = "" }
'@ + $chunkSource + @'
print(MusicVolumeValueLabel.Text)
'@
$lua | Set-Content $tmp -Encoding utf8
& lua $tmp
```

Wrap the chunk in `pcall` when the point is to see whether it errors rather than
to trust its output.
