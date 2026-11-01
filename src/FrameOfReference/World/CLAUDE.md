# Frame of Reference - World layer

## File layout

- `Positionables/`: things placed in the world: `Entity`, the `Waypoint`s that drive automated movement, `Trigger`s that run Lua scripts on proximity or timers, and `BenchmarkPoint`s for the benchmark mode. `Water` and the base classes come from `AlphaFramework.World`.
- `Components/`: collision shapes (`Box`, `Circle`) used by entity templates, alongside the components in `AlphaFramework.World`.
- `Templates/`: `EntityTemplate` and `TerrainTemplate`, prototypes loaded at startup from `content/World/*Templates.xml`.
- `Universe.cs`: the serialized world root, one per map (partials `.Lighting`, `.Pathfinding`, `.Storage`). `Session` (with `.TimeTravel`) wraps a Universe for savegames and advances game time in fixed steps.
- `CameraState.cs`: a saved camera position, used for `Universe.CurrentCamera` and named camera markers.

## Maps and savegames

- A map (`*.FrameOfReferenceMap`) stores the Universe together with its terrain data (`height.png`, `texture.png`, `occlusion.png`). A savegame (`*.FrameOfReferenceSave`) stores the Session without terrain data, and names the map it came from in `MapSourceFile`.
- Terrain data is loaded from that map, through `ContentManager`, on first access to `Universe.Terrain` (on Windows only), and pathfinding is initialized with it.
- Story scripts run from `Trigger`s through `Session.Lua`, which only the game sets (and only on .NET Framework). Without it, `Session.Update` fires no triggers.

## Conventions

The root `CLAUDE.md`'s rules for `World` layers apply (`[Description]` on concrete properties, `[Cloneable]` deep copies, editor-only attributes behind `#if NETFRAMEWORK`).
