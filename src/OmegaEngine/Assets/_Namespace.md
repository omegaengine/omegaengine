---
uid: OmegaEngine.Assets
summary: Assets are content files loaded by the engine at runtime. This includes textures, models, sounds, etc..
---
Asset files are often referenced many times during an application's runtime. To prevent repeated load delays the engine keeps loaded and parsed content in an in-memory [cache](#cache) that can be flushed, e.g. after switching maps.

## Cache

The <xref:OmegaEngine.Assets.CacheManager> (accessible via the [Engine.Cache](xref:OmegaEngine.Engine.Cache) property) automatically caches loaded assets to improve performance through reference counting.

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
