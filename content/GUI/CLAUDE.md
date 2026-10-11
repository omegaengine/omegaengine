# Frame of Reference - GUI

## File layout

- `*.xml`: top-level screens (`MainMenu`, `PauseMenu`, etc.).
- `InGame/`: in-game HUDs, one per game state (`HUD`, `HUD_Modify`, `HUD_Benchmark`), and the dialogs they open.
- `Savegame/`: loading and saving dialogs.
- `MsgBox/`: reusable message-box dialogs.
- `init.lua`: Lua helpers loaded into every dialog.
- `Language/*.locale`: localization strings (referenced in `.xml` files via `[LocalizationKey]`).
- `Textures/`: GUI-specific textures (`base.png` skin sheet).

## Conventions

- Lua code in `content/GUI/` (both `.xml` and `.lua` files): Use `snake_case` for variables, function and parameters defined in Lua. GUI controls and C# objects that are merely accessed by Lua use `PascalCase`.
- Lua embedded in `.xml` files (`<OnShow>`, `<OnUpdate>`, `<OnClick>`, ...) is XML text content, so it must be XML-escaped: write `&lt;` for `<` (e.g. `a &lt;= b`, `a &lt; b`) and `&amp;` for `&`.
- Lua null checks: prefer `x and x.y` over `x ~= nil and x.y`. This is more concise and idiomatic in Lua, where `nil` and `false` are falsy.
- `dofile()` and `LoadDialog()` paths are relative to `GUI/` and are plain strings.
- Do not use the keys `W`, `A`, `S`, `D` for GUI hotkeys, because they are already bound to camera control.
