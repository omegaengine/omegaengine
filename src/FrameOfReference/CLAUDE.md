# Frame of Reference

"Frame of Reference" is the official sample game for [OmegaEngine](../../CLAUDE.md). It is a real-time strategy / exploration game that doubles as a worked example of how to build a game with OmegaEngine, OmegaGUI, AlphaFramework and AlphaEditor. It is included in the engine source but is **not** part of the released library binaries.

## Tech stack

The tech stack and conventions of the [root `CLAUDE.md`](../../CLAUDE.md) apply here too. On top of the engine, the game:

- uses the [AlphaFramework](../AlphaFramework/_Namespace.md) game framework to simplify mapping game world to OmegaEngine rendering constructors.
- builds its UI from [OmegaGUI](../OmegaGUI/_Namespace.md) XML dialogs with embedded Lua, loaded from `content/GUI/`.

The game references the engine projects in this repository directly rather than through NuGet packages, so engine changes take effect in it without a release.

## Solution layout (`src/FrameOfReference/`)

- `World/`: pure simulation, no rendering (see `World/CLAUDE.md`).
- `Presentation/`: maps simulation entities to OmegaEngine scene graph. `Presenter` (partials `.Entities`/`.Lighting`/`.ChildViewThumbnails`) is the base; concrete presenters specialize it: `InteractivePresenter`, `InGamePresenter`, `EditorPresenter`, `MenuPresenter`, `BenchmarkPresenter`.
- `Game/`: WinForms host, game loop, savegame management.
  - `States/`: game-state machine.
- `Editor/`: separate WinForms tool for editing maps; build using [AlphaEditor](../AlphaFramework/Editor/). Has a build dependency on `Game` (see `OmegaEngine.slnx`).
- `UnitTests/`: xUnit tests. Multi-targets `net10.0-windows` and `net10.0` so simulation code is also tested without the Windows-only parts.

## Build & test

- The game is built and tested together with the engine by the scripts in the root `CLAUDE.md`.
- `.claude/skills/drive-game-headless/`: launches the game, posts input to it, takes screenshots, and builds savegame scenarios, for confirming changes in the running game without a human at the keyboard. Only works on Windows.

## Conventions

- Anything that touches the OmegaEngine scene graph belongs in `Presentation/` or above, never in `World/`.
- Only the states derived from `SessionStateBase` (`InGame`, `Modify`, `Benchmark`) advance the simulation. While the pause menu is open, the simulation is frozen.
