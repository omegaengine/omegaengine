---
uid: OmegaEngine.Graphics.LightSources
summary: Light sources illuminate <xref:OmegaEngine.Graphics.Renderables.PositionableRenderable>s in a <xref:OmegaEngine.Graphics.Scene>.
---
> [!TIP]
> To enable lighting in a <xref:OmegaEngine.Graphics.View>, set the <xref:OmegaEngine.Graphics.View.Lighting> property to `true` and add light sources to the scene's <xref:OmegaEngine.Graphics.Scene.Lights> collection.
>
> ```csharp
> var view = new View(scene, camera) { Lighting = true };
> scene.Lights.Add(new DirectionalLight
> {
>     Direction = new(-1, -1, 1),
>     Diffuse = Color.White
> });
> ```
