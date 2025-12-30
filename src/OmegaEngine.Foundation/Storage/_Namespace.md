---
uid: OmegaEngine.Foundation.Storage
summary: Storage subsystem with overlay filesystem and serialization.
---
TODO: introduction

```csharp
// Load a particle system preset
var particles = CpuParticlePreset.FromContent("Fire");
```

### Directory names

Asset loading methods automatically prepend directory names based on the asset type.

TODO: Convert into table

```csharp
// Loads from "Textures/stone.png"
var texture = XTexture.Get(engine, "stone.png");

// Loads from "Meshes/mesh.x"
var meshTexture = XMesh.Get(engine, "mesh.x");

// Loads from "Meshes/mesh.png" (texture associated with a mesh)
var meshTexture = XTexture.Get(engine, "mesh.png", meshTexture: true);

// Loads from "Graphics/CpuParticleSystem/fire.xml"
var particles = CpuParticlePreset.FromContent("fire");
```

## Filesystem

The filesystem combines multiple directory structures into a single view used to load assets.

Search order:

1. Mod
    - Directory specified via the `/mod` command-line argument  
      (only if implemented by the game)
    - Directories specified in the `OMEGAENGINE_CONTENT_MOD` environment variable  
      (only if the `/mod` command-line argument was not used)
2. Base
    - Directory specified in game settings  
      (only if implemented by the game)
    - Directories specified in the `OMEGAENGINE_CONTENT` environment variable  
      (only if not overriden by game settings)
    - The `content` directory next to the game's executable  
      (only if not overriden by the `OMEGAENGINE_CONTENT` environment variable or game settings)

### Base directory

The base directory is usually located in the directory of the application EXE and named `base`. This location can be overridden in the engine configuration.

### Mods

A game modification (mod) is a set of changes based on an existing game used to modify existing gameplay, add additional content or create an entirely new game.

## API
