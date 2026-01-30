---
uid: OmegaEngine.Graphics.Shaders
summary: Shaders are small pieces of code executed directly on the graphics card. They govern how vertexes are transformed and how each individual pixel color is calculated.
---
## Surface shaders

<xref:OmegaEngine.Graphics.Shaders.SurfaceShader>s control the appearance of individual renderable objects' surfaces. They determine how materials, textures, and lighting interact to produce the final look of a model or terrain.

Surface shaders are assigned via the [PositionableRenderable.SurfaceShader](xref:OmegaEngine.Graphics.Renderables.PositionableRenderable.SurfaceShader) property.  They receive per-object data like world transformation matrices, material properties, and effective light sources for the object's position.

### Examples

<xref:OmegaEngine.Graphics.Shaders.GeneralShader> provides standard Phong lighting with diffuse, specular, and ambient components. Supports multiple light sources. normal maps, specular maps and emissive maps.  
This is the default shader used if no other shader is specified.

<xref:OmegaEngine.Graphics.Shaders.WaterShader> renders an animated water surface with reflections and refractions.  
This is used automatically by <xref:OmegaEngine.Graphics.Renderables.Water>.

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

Shaders can be generated and compiled at runtime using a templating system. This allows the engine to optimize shaders for specific use cases without requiring pre-compiled variants for every combination of features.

These templates files use the file ending `.fxd`. They combine standard HLSL code with XML directives embedded in triple-slash (`///`) comments that control code generation. The <xref:OmegaEngine.Graphics.Shaders.DynamicShader> class processes `.fxd` files to generate final HLSL code that is then compiled.

### Counter

Defines a counter variable that can be substituted into generated code:

```
/// <Counter ID="main" Type="int" Min="1" Max="16" />
```

Counter types:
- `int` - Integer counter with min and max values
- `int-step` - Integer counter with fractional step increments (requires `Step` attribute)
- `char` - Character counter with explicit character list defined in `<Char>` child elements

### Code

Generates code blocks using counter values:

```
/// <Code Type="Repeat" Count="16"><![CDATA[texture Texture{main};
/// sampler2D texture{main}Sampler = sampler_state { texture = <Texture{main}>; };]]></Code>
```

Code types:
- `Repeat` - Repeats code block N times (requires `Count` attribute)
- `Sync` - Generates code synchronized with controller values (requires `Controller` and `Max` attributes)

### Filters

Conditionally include/exclude code based on capabilities:

```
/// <BeginFilter Target="PS2x" Lighting="true" />
struct outLight2x { /* ... */ };
/// <EndFilter />
```

Filter attributes:
- `Target` - Shader model requirement (`PS14`, `PS20`, `PS2x`, `PS2ab`, `PS2a`, `PS2b`)
- `Lighting` - Whether code is for lighting (`true`) or non-lighting (`false`) shaders

### Sample

The <xref:OmegaEngine.Graphics.Shaders.TerrainShader> class demonstrates dynamic shader generation. It creates shader code based on the number of textures, lighting requirements, and other terrain-specific parameters, then compiles the shader on-demand.

This code from `Terrain.fxd` generates texture sampling code for up to 16 textures, accessing texture ights from `texWeights1.x`, `texWeights1.y`, etc.:

```
/// <Counter ID="main" Type="int" Min="1" Max="16" />
/// <Counter ID="group" Type="int-step" Min="1" Max="4" Step="0.25" />
/// <Counter ID="component" Type="char">
///   <Char>x</Char>
///   <Char>y</Char>
///   <Char>z</Char>
///   <Char>w</Char>
/// </Counter>
/// <Code Type="Sync" Controller="textures" Max="16"><![CDATA[
///   color += tex2D(texture{main}Sampler, texCoord) * texWeights{group}.{component};
/// ]]></Code>
```

## API
