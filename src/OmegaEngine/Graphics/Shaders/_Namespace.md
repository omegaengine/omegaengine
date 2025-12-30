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

## API
