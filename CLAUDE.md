# OmegaEngine

OmegaEngine is a general-purpose, light-weight, modular, gameplay-agnostic 3D graphics engine for .NET and DirectX 9. It is suitable for games as well as visualization projects.

This repository also bundles **Frame of Reference** (`src/FrameOfReference`), the official sample game built on the engine. See [src/FrameOfReference/CLAUDE.md](src/FrameOfReference/CLAUDE.md) for it.

## Tech stack

- C# 13 on **.NET Framework 4.7.2**, `x86` only. `EnableWindowsTargeting` is on so it builds from non-Windows hosts too. The graphics-agnostic parts (`OmegaEngine.Foundation`, the `World` layers) multi-target .NET Framework 4.7.2 and .NET 10.
- DirectX 9 via SlimDX.

## Layered architecture

The projects build up in layers, from low-level rendering to a full game framework. Lower layers must not depend on higher ones.

```
OmegaEngine.Foundation   ← rendering-agnostic infrastructure (storage, collections, geometry, math)
        │
OmegaEngine              ← DirectX 9 rendering, audio, input
   │        │
OmegaGUI   AlphaFramework.World        ← engine-agnostic game-world models (Universe + Session)
   │        │
   │   AlphaFramework.Presentation     ← presenters that visualize a World using the engine
   │        │
   │   AlphaFramework.Editor (AlphaEditor)  ← toolkit for building game/map editors
   │        │
   └──→ Frame of Reference (sample game, src/FrameOfReference)
```

The engine, GUI, and AlphaFramework layers ship as NuGet packages; Frame of Reference does not.

## Solution layout (`src/`)

- `OmegaEngine.Foundation/`: rendering-agnostic infrastructure shared across layers.
  - `Storage/` (content/config loading), `Collections/`, `Geometry/`, `Light/`, `Design/`, `Shims/` (polyfills bridging the two target frameworks).
- `OmegaEngine/`: the core engine. `Engine` (split across `Engine.*.cs` partials: `.Render`, `.Audio`, `.Terrain`, `.Debug`) is the central object; `RenderHost`/`RenderPanel` host it in WinForms. Sub-areas:
  - `Graphics/`: `Cameras/`, `LightSources/`, `Renderables/` (scene-graph objects), `Shaders/`, `VertexDecl/`.
  - `Audio/`, `Input/`, `Assets/` (cached GPU/asset resources).
  - `DebugConsole`/`DebugForm` provide the in-engine debug overlay.
- `UnitTests/`: xUnit tests for `OmegaEngine` and `OmegaEngine.Foundation`.
- `OmegaGUI/`: XML-based GUI toolkit with embedded Lua scripting.
  - `Model/` is the serializable dialog model.
  - `Render/` draws it through the engine.
- `AlphaFramework/`: high-level game framework on top of the engine (see `AlphaFramework/_Namespace.md`).
  - `World/`: engine-agnostic world models. `UniverseBase` holds static world data; `Session` tracks dynamic game state. Sub-areas: `Positionables/`, `Components/`, `Properties/`, `Templates/` (data-driven entity templates), `Terrains/`, `Paths/`.
  - `Presentation/`: `PresenterBase`/`GameBase` translate a World into renderable engine objects and drive the app lifecycle.
  - `Editor/`: AlphaEditor, reusable WinForms base (`MainFormBase`, undo `Tab`s, mod packaging) for building game editors.
  - `UnitTests/`: xUnit tests for AlphaFramework projects.

## Build & test

- `src/build.ps1`: creates a Release build of the engine and Frame of Reference. Only works on Windows.
- `src/build.sh`: creates a Release build of the engine and Frame of Reference. Runs on Linux but creates build output for Windows.
- `src/test.ps1`: runs unit tests. Requires a prior build. Only works on Windows.

## Conventions

- Follow `.editorconfig` strictly: it pins extensive C# style rules (expression-bodied members preferred, `var` only when the type is apparent, braces required only for multiline blocks, no qualification for fields/properties, etc.).
- File-scoped namespaces, primary constructors, and collection expressions are used throughout: match the surrounding style.
- Respect the layering: `OmegaEngine.Foundation` stays rendering-agnostic; `World` layers stay free of rendering/UI dependencies; engine code must not depend on AlphaFramework or the sample game.
- Code that multi-targets net472 + net10 may need `Shims/` polyfills rather than newer BCL APIs; check what the other target framework supports.
- Doc comments on public APIs are the norm (XML doc warning 1591 is suppressed but most public types have them).

## Content layout (`content/`)

Runtime assets. Some of them are bundled in the `OmegaEngine` NuGet (see `src\OmegaEngine\OmegaEngine.csproj`) while the rest is for [Frame of Reference](src/FrameOfReference/).

- `World/`: serialized simulation data.
  - `Maps/*.FrameOfReferenceMap`: ZIP files with alternate file ending
    - `data.xml`: Serialized `Universe`
    - `height.png`: Terrain height map
    - `texture.png`: Terrain texture map
    - `occlusion.png`: Terrain self-shadowing occlusion angles
  - `*Templates.xml`: config for classes of terrain, entities, etc..
- `GUI/`: OmegaGUI dialog definitions and scripts.
  - `*.xml`: top-level screens (`MainMenu`, `PauseMenu`, etc.).
  - `InGame/`: in-game HUD.
  - `MsgBox/`: reusable message-box dialogs.
  - `Utils.lua`: shared Lua helpers.
  - `Language/*.locale`: localization strings (referenced in `.xml` files via `[LocalizationKey]`).
  - `Textures/`: GUI-specific textures (`base.png` skin sheet, `logo.png`).
- `Graphics/CpuParticleSystem/*.xml`: `CpuParticlePreset` definitions for particle effects (e.g. `FusionDrive.xml`, `Explosion.xml`).
- `Graphics/Shaders/*.fxd`: Templates for dynamic shader templates that are evaluated and compiled at runtime.
- `Meshes/`: 3D mesh assets and paired textures.
- `Textures/`: shared textures.
  - `Skybox/<name>/`: six-face cubemap (`ft`, `bk`, `lf`, `rt`, `up`, `dn`). The name matches the `<Skybox>` element in the map XML.
  - `Particles/`: particle textures referenced from `CpuParticlePreset` XML.
  - `Terrain/`: terrain surface textures.
  - `Water/`: water surface textures.
  - `Shaders/`: textures sampled by shaders.

## Other directories

- `content/`: runtime assets for the sample game (`Meshes/`, `Textures/`, `GUI/`, `World/`, `Music/`, `Sounds/`, `Graphics/`).
- `shaders/`: HLSL shader source.
- `templates/`: `OmegaEngine.Templates` project templates for scaffolding new engine-based projects.
- `doc/`: source for the documentation site at <https://docs.omegaengine.de/>.
- `artifacts/`: build output (`Debug/`, `Release/`).

## File encoding

This repo stores most files with CRLF line endings and uses the Git option `core.autocrlf=false`. Some `.cs` files carry a UTF-8 BOM. This should be preserved.

Running `sed -i` for bulk replacements rewrites whole files as LF. Instead prefer using the Edit tool or PowerShell `[IO.File]::ReadAllText/WriteAllText` with `UTF8Encoding($false)` plus explicit CRLF preservation, and re-prepend the BOM if the file originally had one.
