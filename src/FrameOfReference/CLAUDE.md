# Frame of Reference

"Frame of Reference" is the official sample game for [OmegaEngine](../../CLAUDE.md). It is a real-time strategy / exploration game that doubles as a worked example of how to build a game with OmegaEngine, OmegaGUI, AlphaFramework and AlphaEditor. It is included in the engine source but is **not** part of the released library binaries.

## Tech stack

- C# 13 on .NET Framework 4.7.2, `x86` only. Unlike `AlphaFramework.World`, the sample game's `World` project targets only .NET Framework 4.7.2.
- built on OmegaEngine for 3D graphics (DirectX 9 / SlimDX).
- uses the [AlphaFramework](../AlphaFramework/_Namespace.md) game framework to map the game world onto OmegaEngine rendering constructs.
- UI uses [OmegaGUI](../OmegaGUI/_Namespace.md) XML dialogs with embedded Lua. Content lives under the repo-level `content/` directory.

## Solution layout (`src/FrameOfReference/`)

Layered the same way as AlphaFramework — `World` → `Presentation` → `Game`, with `Editor` alongside.

- `World/`: pure simulation, no rendering. `Universe` (partials `Universe.Lighting`/`Universe.Pathfinding`/`Universe.Storage`) is the serialized world root; `Session` (with `Session.TimeTravel`) wraps a Universe for savegames and advances game time. Sub-areas:
  - `Positionables/`: entities placed in the world.
  - `Components/`: composable entity behavior/data.
  - `Templates/`: data-driven templates loaded at startup to instantiate entities.
- `Presentation/`: maps simulation entities to the OmegaEngine scene graph. `Presenter` (partials `.Entities`/`.Lighting`) is the base; concrete presenters specialize it — `InteractivePresenter`, `InGamePresenter`, `EditorPresenter`, `MenuPresenter`, `BenchmarkPresenter`.
- `Game/`: WinForms host, game loop, savegame management.
  - `States/`: game-state machine.
- `Editor/`: separate WinForms map editor built on [AlphaEditor](../AlphaFramework/Editor/). Has a build dependency on `Game` (see `OmegaEngine.slnx`).
- `UnitTests/`: xUnit tests.

## Conventions

- Follow `.editorconfig` strictly (see the engine-level [CLAUDE.md](../../CLAUDE.md) for the style summary).
- File-scoped namespaces, primary constructors, and collection expressions are used throughout: match the surrounding style.
- `World/` must stay free of rendering/UI dependencies. Anything that touches the OmegaEngine scene graph belongs in `Presentation/` or above.
- Doc comments on public APIs are the norm (XML doc warning 1591 is suppressed but most public types have them).
