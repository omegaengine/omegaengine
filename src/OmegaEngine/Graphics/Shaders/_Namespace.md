---
uid: OmegaEngine.Graphics.Shaders
summary: Shaders are small pieces of code executed directly on the graphics card. They govern how vertexes are transformed and how each individual pixel color is calculated.
---
All shaders target Shader Model 3.0 (`vs_3_0`/`ps_3_0`), the minimum the engine requires (see <xref:OmegaEngine.EngineCapabilities.MinShaderModel>).

## Surface shaders

<xref:OmegaEngine.Graphics.Shaders.SurfaceShader>s control the appearance of individual renderable objects' surfaces. They determine how materials, textures, and lighting interact to produce the final look of a model or terrain.

Surface shaders are assigned via the [PositionableRenderable.SurfaceShader](xref:OmegaEngine.Graphics.Renderables.PositionableRenderable.SurfaceShader) property.  They receive per-object data like world transformation matrices, material properties, and effective light sources for the object's position.

### Examples

<xref:OmegaEngine.Graphics.Shaders.GeneralShader> provides standard Phong lighting with diffuse, specular, and ambient components. Supports multiple light sources. normal maps, specular maps and emissive maps.  
This is the default shader used if no other shader is specified.

<xref:OmegaEngine.Graphics.Shaders.WaterShader> renders an animated water surface with reflections and refractions.  
This is used automatically by <xref:OmegaEngine.Graphics.Renderables.Water>.

### Fog

Shader Model 3.0 pixel shaders bypass Direct3D's fixed-function fog, so surface shaders must apply fog themselves. <xref:OmegaEngine.Graphics.Shaders.SurfaceShader> fills parameters with these semantics from <xref:OmegaEngine.EngineState>:

- `Fog` (`float3`): start distance, end distance, and `1` if fog is enabled (`0` otherwise)
- `FogColor` (`float3`): the fog color in linear space

The fog distance is the view-space depth, i.e. the `w` component of the clip-space position. See `shaders/include/Fog.fxh` for a reference implementation.

## Post-screen shaders

<xref:OmegaEngine.Graphics.Shaders.PostShader>s are applied to the entire rendered scene after all objects have been drawn. They perform screen-space effects that affect the complete image.

Post-screen shaders are added to the [View.PostShaders](xref:OmegaEngine.Graphics.View.PostShaders) collection. Adding multiple shaders to the collection allows you to chain effects, with each shader processing the output of the previous one.

### Examples

<xref:OmegaEngine.Graphics.Shaders.PostBlurShader> blurs the entire scene.
```csharp
view.PostShaders.Add(new PostBlurShader
{
    BlurStrength = 2.0
});
```

<xref:OmegaEngine.Graphics.Shaders.PostColorCorrectionShader> adjusts brightness, contrast, and saturation of the rendered scene.
```csharp
view.PostShaders.Add(new PostColorCorrectionShader
{
    Brightness = 1.1,
    Contrast = 1.2,
    Saturation = 0.9
});
  ```

<xref:OmegaEngine.Graphics.Shaders.PostSepiaShader> applies a sepia tone effect to create an old photograph look.
```csharp
view.PostShaders.Add(new PostSepiaShader());
```

## Dynamic shaders

Shaders can be compiled at runtime in multiple variants. This allows the engine to optimize shaders for specific use cases without requiring pre-compiled variants for every combination of features.

These shader files use the file ending `.fx` and are loaded via the <xref:OmegaEngine.Foundation.Storage.ContentManager>. They contain HLSL that selects a variant based on preprocessor defines. <xref:OmegaEngine.Graphics.Shaders.DynamicShader.FromContent(System.String,System.Collections.Generic.IReadOnlyDictionary{System.String,System.String})> compiles such a file with a specific set of defines.

- Use `#if`/`#else`/`#endif` to include code only in certain variants.
- Use macros with token pasting (`##`) to generate repetitive code. Branches on compile-time constants derived from the defines are compiled out entirely.
- Do not use sampler arrays: the effect framework ignores the states (e.g. `sRGBTexture`) of their elements. Declare samplers individually instead.
- Shader Model 3.0 has no integer bit operations. Test bits of a mask with division and modulo instead (see below).
- Provide defaults with `#ifndef`, so that the file can also be compiled standalone with `fxc.exe` for testing.

Compiling takes a while, so the resulting bytecode is stored in a <xref:OmegaEngine.Graphics.Shaders.ShaderCache> on disk. The cache key includes the source code, the defines and the compiler version, so modified or modded `.fx` files are recompiled automatically. Set <xref:OmegaEngine.Graphics.Shaders.DynamicShader.Cache> to `null` to always compile.

### Sample

The <xref:OmegaEngine.Graphics.Shaders.TerrainShader> class compiles `Terrain.fx` once for each combination of textures a terrain subset uses, with the defines `LIGHTING` (`0` or `1`) and `TEXTURE_MASK` (a bitmask of the used textures).

This code blends up to 16 textures, reading the weight of the texture with index `i - 1` from component `(i - 1) % 4` of `texWeights[(i - 1) / 4]`. Textures not in `TEXTURE_MASK` are not sampled at all:

```hlsl
static const int textureBits[16] = {0x1, 0x2, 0x4, 0x8, /* ... */ 0x8000};
bool textureEnabled(int i)
{ return (TEXTURE_MASK / textureBits[i]) % 2 == 1; }

#define BLEND_TEXTURE(i) if (textureEnabled(i - 1)) color += tex2D(texture##i##Sampler, texCoord) * texWeights[(i - 1) / 4][(i - 1) % 4];

float4 color = 0;
BLEND_TEXTURE(1) BLEND_TEXTURE(2) /* ... */ BLEND_TEXTURE(16)
```

## API
