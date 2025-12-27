# Assets

An asset is a content file loaded by the engine at runtime. This includes textures, models, sounds, etc.. These files are located using a [Virtual File System](#vfs).

Asset files are often referenced many times during an application's runtime. To prevent repeated load delays the engine keeps loaded and parsed content in an in-memory cache that can be flushed, e.g. after switching maps.

## VFS

The Virtual File System (VFS) combines multiple directory structures into a single view of the filesystem used to load assets.

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

## Mods

A game modification (mod) is a set of changes based on an existing game used to modify existing gameplay, add additional content or create an entirely new game.

The engine supports mods via a [virtual file system](#vfs) and modding support in the AlphaEditor.

## Loading Assets

Many asset types provide static `FromContent()` methods for easy loading from the content directories:

```csharp
// Load a 3D model
var model = Model.FromContent(engine, "Models/Character.x");

// Load a texture
var texture = XTexture.FromContent(engine, "Textures/Concrete.png");

// Load a particle system preset
var particles = CpuParticlePreset.FromContent("Fire");
```

The `FromContent()` method:

1. Searches for the file in the [VFS](#vfs) content directories
2. Loads and parses the file
3. Adds the asset to the <xref:OmegaEngine.Assets.CacheManager> automatically
4. Returns the asset ready for use

### Cache Manager

The <xref:OmegaEngine.Assets.CacheManager> (accessible via <xref:OmegaEngine.Engine.Cache>) automatically caches loaded assets to improve performance through reference counting.

When you load an asset:

1. The cache is checked for an existing instance with that name
2. If found, the reference count is incremented and the cached instance is returned
3. If not found, the asset is loaded, added to the cache with reference count of 1

When you dispose an asset:

1. The reference count is decremented
2. If the count reaches zero, the asset remains in cache but is marked for potential cleanup
3. Calling <xref:OmegaEngine.Assets.CacheManager.Clean> removes all assets with zero references

For scenarios like level transitions:

```csharp
// Load level 1 assets
var level1Assets = LoadLevel1Assets(engine);

// ... play level 1 ...

// Dispose level 1 assets
foreach (var asset in level1Assets)
    asset.Dispose();

// Clean cache to free memory before loading level 2
engine.Cache.Clean();

// Load level 2 assets
var level2Assets = LoadLevel2Assets(engine);
```
