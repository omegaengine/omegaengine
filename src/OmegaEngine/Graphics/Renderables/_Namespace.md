---
uid: OmegaEngine.Graphics.Renderables
summary: Objects rendered by OmegaEngine.
---
All renderables inherit from <xref:OmegaEngine.Graphics.Renderables.Renderable>, which provides the base rendering interface.

<xref:OmegaEngine.Graphics.Renderables.PositionableRenderable> are renderables that have a position and orientation in 3D space. These are objects that exist within a <xref:OmegaEngine.Graphics.Scene> at specific locations. Examples include <xref:OmegaEngine.Graphics.Renderables.Model>, <xref:OmegaEngine.Graphics.Renderables.Terrain> and <xref:OmegaEngine.Graphics.Renderables.CpuParticleSystem>

<xref:OmegaEngine.Graphics.Renderables.Skybox> is non-positionable renderable. Skyboxes "follow" the camera position to create the illusion of infinite distance.
