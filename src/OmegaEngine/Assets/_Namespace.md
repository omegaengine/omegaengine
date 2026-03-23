---
uid: OmegaEngine.Assets
summary: Assets are content files loaded by the engine at runtime. This includes textures, models, sounds, etc..
---
When loading an asset the <xref:OmegaEngine.Foundation.Storage> subsystem is used to locate the file and then the [asset cache](#cache) is used to store it in-memory for reuse.

## Get methods

Most asset types provide static `Get()` methods. You pass in the asset's ID (which is its filename) and you'll either get an already cached instance or the contents is loaded from disk and cached.

Examples:

```csharp
// Load a 3D model
var model = XMesh.Get(engine, "Character.x");

// Load a texture
var texture = XTexture.Get(engine, "Grass.png");
```

### Directories

These methods automatically choose [content subdirectories](xref:OmegaEngine.Foundation.Storage) based on the asset type:

| Asset Type    | Method                                                          | Content Subdirectory |
| ------------- | --------------------------------------------------------------- | -------------------- |
| Textures      | `XTexture.Get(engine, "Grass.png")`                             | `Textures`           |
| Meshes        | `XMesh.Get(engine, "Character.x")`                              | `Meshes`             |
| Mesh textures | `XTexture.Get(engine, "Character_Skin.png", meshTexture: true)` | `Meshes`             |
| Sound         | `XSound.Get(engine, "Bird.wav")`                                | `Sounds`             |

Examples:

```csharp
// Loads from "Textures/texture.png"
var texture = XTexture.Get(engine, "texture.png");

// Loads from "Meshes/mesh.x"
var mesh = XMesh.Get(engine, "mesh.x");

// Loads from "Meshes/texture.png" (texture associated with a mesh)
var meshTexture = XTexture.Get(engine, "texture.png", meshTexture: true);

// Loads from "Graphics/CpuParticleSystem/fire.xml"
var particles = CpuParticlePreset.FromContent("fire");
```

## Cache

Asset files are often referenced many times during an application's runtime. To prevent repeated load delays the engine keeps loaded and parsed content in an in-memory cache that can be flushed, e.g. after switching maps. The <xref:OmegaEngine.Assets.CacheManager> (accessible via the [Engine.Cache](xref:OmegaEngine.Engine.Cache) property) implements this through reference counting.

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
List<IDisposable> level1Assets = LoadLevel1Assets(engine);

// ... play level 1 ...

// Dispose level 1 assets
foreach (var asset in level1Assets)
    asset.Dispose();

// Clean cache to free memory before loading level 2
engine.Cache.Clean();

// Load level 2 assets
List<IDisposable> level2Assets = LoadLevel2Assets(engine);
```

## Texture naming convention

When loading meshes from .X files, <xref:OmegaEngine.Assets.XMesh> automatically looks for associated textures using filename suffixes:

| Suffix      | Map Type     | Property                                          | Purpose                         |
| ----------- | ------------ | ------------------------------------------------- | ------------------------------- |
| `_normal`   | Normal map   | <xref:OmegaEngine.Graphics.XMaterial.NormalMap>   | Surface detail and bump mapping |
| `_height`   | Height map   | <xref:OmegaEngine.Graphics.XMaterial.HeightMap>   | Parallax occlusion mapping      |
| `_specular` | Specular map | <xref:OmegaEngine.Graphics.XMaterial.SpecularMap> | Specular highlight intensity    |
| `_emissive` | Emissive map | <xref:OmegaEngine.Graphics.XMaterial.EmissiveMap> | Self-illumination               |
| `_glow`     | Glow map     | <xref:OmegaEngine.Graphics.XMaterial.GlowMap>     | Bloom/glow effects              |

For example, if a mesh references `Character.png`, the engine will automatically search for and load if present:

- `Character_normal.png` for normal mapping
- `Character_height.png` for parallax effects
- `Character_specular.png` for specular highlights
- `Character_emissive.png` for emissive surfaces
- `Character_glow.png` for glow effects

## API
