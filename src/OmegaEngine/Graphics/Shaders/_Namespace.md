---
uid: OmegaEngine.Graphics.Shaders
summary: *content
---
Shaders are small pieces of code executed directly on the graphics card. They govern how vertexes are transformed and how each individual pixel color is calculated.

The engine stores shaders in the High Level Shader Format (HLSL) and supports the [DirectX Standard Annotations and Semantics](#semantics). It can dynamically generate shaders at runtime and apply pixel shaders to the entire scene.

## Shader Types

OmegaEngine uses two main categories of shaders:

### Surface shaders

<xref:OmegaEngine.Graphics.Shaders.SurfaceShader>s control the appearance of individual renderable objects' surfaces. They determine how materials, textures, and lighting interact to produce the final look of a model or terrain.

Surface shaders are assigned to <xref:OmegaEngine.Graphics.Renderables.PositionableRenderable>s via the <xref:OmegaEngine.Graphics.Renderables.PositionableRenderable.SurfaceShader> property.

**Examples:**

- **<xref:OmegaEngine.Graphics.Shaders.GeneralShader>** - Standard Phong lighting with diffuse, specular, and ambient components. Supports multiple light sources. normal maps, specular maps and emissive maps.  
  This is the default shader used if no other shader is specified.

- **<xref:OmegaEngine.Graphics.Shaders.WaterShader>** - Animated water surface with reflections and refractions.  
  This is used automatically by <xref:OmegaEngine.Graphics.Renderables.Water>.

Surface shaders receive per-object data like world transformation matrices, material properties, and effective light sources for the object's position.

### Post shaders

<xref:OmegaEngine.Graphics.Shaders.PostShader>s are applied to the entire rendered scene after all objects have been drawn. They perform screen-space effects that affect the complete image.

Post shaders are added to a <xref:OmegaEngine.Graphics.View>'s <xref:OmegaEngine.Graphics.View.PostShaders> collection.

**Examples:**

- **<xref:OmegaEngine.Graphics.Shaders.PostBlurShader>** - Blurs the entire scene, useful for depth-of-field or motion blur effects.
  ```csharp
  view.PostShaders.Add(new PostBlurShader
  {
      BlurStrength = 2.0
  });
  ```

- **<xref:OmegaEngine.Graphics.Shaders.PostColorCorrectionShader>** - Adjusts brightness, contrast, and saturation of the rendered scene.
  ```csharp
  view.PostShaders.Add(new PostColorCorrectionShader
  {
      Brightness = 1.1,
      Contrast = 1.2,
      Saturation = 0.9
  });
  ```

- **<xref:OmegaEngine.Graphics.Shaders.PostSepiaShader>** - Applies a sepia tone effect to create an old photograph look.
  ```csharp
  view.PostShaders.Add(new PostSepiaShader());
  ```

Post shaders can be chained together, with each shader processing the output of the previous one.

## Semantics

The [DirectX Standard Annotations and Semantics (DXSAS)](https://docs.microsoft.com/en-us/windows/desktop/direct3d9/dx9-graphics-reference-effects-dxsas) provide a way to add metadata to HLSL code that automates the integration between shader code and the engine. By specifying things such as value bindings for transform matrices shaders can more easily be exchanged between tools such as [FX Composer](http://developer.nvidia.com/fx-composer) and different graphics engines.

The engine supports a subset of DXSAS 0.8.
