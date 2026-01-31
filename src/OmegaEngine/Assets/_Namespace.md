---
uid: OmegaEngine.Assets
summary: Assets are content files loaded by the engine at runtime. This includes textures, models, sounds, etc..
---
Asset files are often referenced many times during an application's runtime. To prevent repeated load delays the engine keeps loaded and parsed content in an in-memory [cache](#cache) that can be flushed, e.g. after switching maps.

TODO: review

See <xref:OmegaEngine.Foundation.Storage> for details on the filesystem and content loading system.

## Loading assets

Many asset types provide static `Get()` methods for easy loading from the content directories:

```csharp
// Load a 3D model
var model = XMesh.Get(engine, "Character.x");

// Load a texture
var texture = XTexture.Get(engine, "Concrete.png");
```

The `Get()` method:

1. Searches for the file in the [filesystem](xref:OmegaEngine.Foundation.Storage#filesystem)
2. Loads and parses the file
3. Adds the asset to the [cache](#cache)
4. Returns the asset ready for use

### Texture naming convention

When loading meshes from .X files, <xref:OmegaEngine.Assets.XMesh> automatically looks for associated textures. For a mesh file named `Character.x`, the system will search for textures in this order:

1. Texture names embedded in the .X file (if present)
2. `Meshes/Character.png` - texture with same base name as mesh
3. Fallback to default texture handling

This convention allows mesh textures to be automatically discovered without explicit configuration.

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

## API
