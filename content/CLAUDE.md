# OmegaEngine - Content

Assets are loaded at runtime; the Editor and game both read from this directory. The parts marked *(NuGet)* are bundled in the `OmegaEngine` NuGet package (see `src/OmegaEngine/OmegaEngine.csproj`), so changing them changes what every game built on the engine gets. The rest is for Frame of Reference.

## File layout

- `World/`: serialized simulation data.
  - `Maps/*.FrameOfReferenceMap`: ZIP files with an alternate file ending, one per map. `Menu` is the main-menu background scene; `Tutorial` is what a new game starts.
    - `data.xml`: serialized `Universe`.
    - `height.png`: terrain height map.
    - `texture.png`: terrain texture map.
    - `occlusion.png`: terrain self-shadowing occlusion angles.
  - `*Templates.xml`: config for classes of terrain, entities, etc. (`EntityTemplates.xml`, `TerrainTemplates.xml`).
- `GUI/`: OmegaGUI dialog definitions and scripts (see `GUI/CLAUDE.md`).
- `Graphics/` *(NuGet)*:
  - `CpuParticleSystem/*.xml`: `CpuParticlePreset` definitions for particle effects (e.g. `Fire.xml`, `Swarm.xml`).
  - `Shaders/*.fx`: HLSL effects compiled at runtime in variants selected by preprocessor defines (see `DynamicShader`). They cannot `#include` from `shaders/include/`.
- `Meshes/`: 3D mesh assets and paired textures.
  - `Engine/` *(NuGet)*: meshes the engine itself draws (axis arrows, waypoint markers, ...).
- `Textures/`: shared textures.
  - `Skybox/<name>/`: six-face cubemap (`ft`, `bk`, `lf`, `rt`, `up`, `dn`). The name matches the `<Skybox>` element in the map XML.
  - `Particles/` *(NuGet)*: particle textures referenced from `CpuParticlePreset` XML.
  - `Terrain/`: terrain surface textures.
  - `Water/` *(NuGet)*: water surface textures.
  - `Shaders/` *(NuGet)*: textures sampled by shaders.
