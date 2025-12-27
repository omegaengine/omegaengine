---
uid: OmegaEngine.Graphics.Renderables
summary: *content
---
Objects rendered in 3D space.

All renderables inherit from <xref:OmegaEngine.Graphics.Renderables.Renderable>, which provides the base rendering interface.

## <xref:OmegaEngine.Graphics.Renderables.PositionableRenderable>

Renderables that have a position and orientation in 3D space. These are objects that exist within the scene at specific locations.

**Key properties:**
- **Position** - Location in 3D space (64-bit precision)
- **Rotation** - Orientation as a quaternion
- **SurfaceShader** - Controls the appearance of the object's surface

**Examples:**
- <xref:OmegaEngine.Graphics.Renderables.Model> - 3D models loaded from files
- <xref:OmegaEngine.Graphics.Renderables.Terrain> - Heightmap-based terrain
- <xref:OmegaEngine.Graphics.Renderables.CpuParticleSystem> - CPU-simulated particle effects

## <xref:OmegaEngine.Graphics.Renderables.Skybox>

Special renderable that create a background for the scene. Unlike <xref:OmegaEngine.Graphics.Renderables.PositionableRenderable>s, skyboxes "follow" the camera position to create the illusion of infinite distance.
