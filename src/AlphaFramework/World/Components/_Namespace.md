---
uid: AlphaFramework.World.Components
summary: Component system for describing rendering aspects, collision bodies, gameplay behavior, etc..
---
TODO: review

AlphaFramework's component system allows you to describe various aspects of game entities in an engine-agnostic way. Components define how entities should be rendered, how they interact physically, and what gameplay behavior they exhibit, without depending on the rendering engine.

## Component types

AlphaFramework provides several component types:

- **<xref:AlphaFramework.World.Components.Mesh>** - Visual representation using 3D models
- **<xref:AlphaFramework.World.Components.AnimatedMesh>** - 3D models with skeletal animation
- **<xref:AlphaFramework.World.Components.Collision`1>** - Physical collision boundaries
- **<xref:AlphaFramework.World.Components.LightSource>** - Light emission properties
- **<xref:AlphaFramework.World.Components.CpuParticleSystem>** - Particle effect definitions

## Usage

Components are serializable and describe properties independent of the rendering engine. When rendering, these components are converted to their OmegaEngine equivalents by presenters.

For building reusable entities from components, see <xref:AlphaFramework.World.Templates>.

## API
