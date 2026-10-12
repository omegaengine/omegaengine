# OmegaEngine

OmegaEngine is a general-purpose, light-weight, modular, gameplay-agnostic 3D graphics engine for .NET and DirectX 9. It is suitable for games as well as visualization projects.

This repository also bundles **Frame of Reference** (`src/FrameOfReference`), the official sample game built on the engine. See [src/FrameOfReference/CLAUDE.md](src/FrameOfReference/CLAUDE.md) for it.

## Tech stack

- C# 13 on **.NET 10** (`net10.0-windows`). `EnableWindowsTargeting` is on so it builds from non-Windows hosts too. The graphics-agnostic parts (`OmegaEngine.Foundation`, the `World` layers) additionally target plain `net10.0`; WinForms design-time code (`Foundation/Design/`, `[Editor]` attributes) only exists in the `-windows` build, behind `#if WINDOWS`.
- Math uses `System.Numerics` (`Vector3`, `Matrix4x4`, `Quaternion`, `Plane`) plus `Vortice.Mathematics` (`Color4`, `BoundingBox`, `BoundingSphere`, `Ray`). Watch out for differences to D3DX conventions, see `VectorMath` (`TransformCoordinate` divides by W, `NormalizeOrZero` keeps zero vectors) and note that `System.Numerics` multiplies quaternions in the opposite order to D3DX (`a * b` applies `b` first).
- Direct3D 9Ex (`IDirect3D9Ex`/`IDirect3DDevice9Ex`) via [Vortice.Direct3D9](https://github.com/amerkoleci/Vortice.Windows).
  - Vortice wraps only core Direct3D 9, so `src/OmegaEngine/D3DX/` contains hand-written bindings for the parts of `d3dx9_43.dll` the engine uses (`Effect`, `EffectCompiler`, `Mesh`, `Font`, `Sprite`), calling the COM vtables directly. Vtable indices come from the DirectX SDK headers; `src/UnitTests/D3DX/` catches mistakes.
  - Texture loading/saving (`Graphics/TextureLoader`, `SurfaceUtils`), render-to-texture (`RenderTarget`) and simple shapes (`MeshPrimitives`) are implemented in managed code instead of D3DX.
  - `Graphics/Direct3D9Extensions` fills gaps and works around bugs in Vortice's bindings (missing `D3DERR_*` codes, `GetSamplerState`, display modes, device state, shader versions). Don't use Vortice's `Lock<T>()` on buffers: it sizes the span in bytes instead of elements.
  - Note the D3D9Ex rules: `Pool.Managed` is rejected, so all resources live in `Pool.Default` (see `BufferUtils.DefaultPool`/`ToDefaultPool()`).
  - Shader Model 3.0 is the minimum (`EngineCapabilities.MinShaderModel`, checked in the `Engine` constructor). All shaders compile as `vs_3_0`/`ps_3_0`; there are no lower-profile fallbacks.
  - `ps_3_0` ignores fixed-function fog, so every surface shader applies fog itself via the `Fog`/`FogColor` semantics (see `shaders/include/Fog.fxh`). A `vs_3_0` pass also always needs an explicit pixel shader.

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
  - `Storage/` (content/config loading), `Collections/`, `Geometry/`, `Light/`, `Design/` (WinForms design-time support, including `TypeConverter`s for `System.Numerics` types).
- `OmegaEngine/`: the core engine. `Engine` (split across `Engine.*.cs` partials: `.Render`, `.Audio`, `.Terrain`, `.Debug`) is the central object; `RenderHost`/`RenderPanel` host it in WinForms. Sub-areas:
  - `Graphics/`: `Cameras/`, `LightSources/`, `Renderables/` (scene-graph objects), `Shaders/`, `VertexDecl/`.
  - `D3DX/`: bindings for the used parts of D3DX 9.
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
- `FrameOfReference/`: the sample game (see `src/FrameOfReference/CLAUDE.md`).

## Build & test

- `src/build.ps1`: creates a Release build of the engine and Frame of Reference. Only works on Windows.
- `src/build.sh`: creates a Release build of the engine and Frame of Reference. Runs on Linux but creates build output for Windows.
- `src/test.ps1`: runs unit tests. Requires a prior build. Only works on Windows.
- `dotnet test` runs on Microsoft.Testing.Platform (`global.json` sets `test.runner`, `UnitTests.props` sets `UseMicrosoftTestingPlatformRunner`). **Do not pass `--nologo`.** MTP forwards unrecognised options to the test app, which then exits 5 with `Zero tests ran` instead of reporting a bad argument — this looks exactly like a broken test setup.
- `.claude/skills/headless-render/`: renders maps to image files without a human at the keyboard, for checking rendering changes visually and for before/after comparison against another commit. Only works on Windows.

## Conventions

- Follow `.editorconfig` strictly: it pins extensive C# style rules (expression-bodied members preferred, `var` only when the type is apparent, braces required only for multiline blocks, no qualification for fields/properties, etc.).
- File-scoped namespaces, primary constructors, and collection expressions are used throughout: match the surrounding style.
- Respect the layering: `OmegaEngine.Foundation` stays rendering-agnostic; `World` layers stay free of rendering/UI dependencies; engine code must not depend on AlphaFramework or the sample game.
- Code in the multi-targeted projects must compile for plain `net10.0` too, so put anything WinForms-specific behind `#if WINDOWS`.
- Doc comments on public APIs are the norm (XML doc warning 1591 is suppressed but most public types have them).
- `[Description("...")]` (matching the XML doc comment) belongs on every concrete property implementation in `AlphaFramework/World/` (not on the interface/abstract declaration).

## Other directories

- `content/`: runtime assets, partly bundled in the `OmegaEngine` NuGet, the rest for the sample game (see `content/CLAUDE.md`).
- `shaders/`: HLSL shader source.
- `templates/`: `OmegaEngine.Templates` project templates for scaffolding new engine-based projects.
- `doc/`: source for the documentation site at <https://docs.omegaengine.de/>.
- `artifacts/`: build output (`Debug/`, `Release/`).

## File encoding

This repo stores most files with CRLF line endings and uses the Git option `core.autocrlf=false`. Some `.cs` files carry a UTF-8 BOM. This should be preserved.

Running `sed -i` for bulk replacements rewrites whole files as LF. Instead prefer using the Edit tool or PowerShell `[IO.File]::ReadAllText/WriteAllText` with `UTF8Encoding($false)` plus explicit CRLF preservation, and re-prepend the BOM if the file originally had one.
