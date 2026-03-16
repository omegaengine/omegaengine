# Lighting

OmegaEngine uses a realtime lighting system that supports multiple light types and shadowing techniques.

## Lighting model

The default surface shader (<xref:OmegaEngine.Graphics.Shaders.GeneralShader>) implements **Blinn-Phong** lighting, which provides:

- **Diffuse lighting** - Light scattered equally in all directions from a surface
- **Specular highlights** - Bright spots that appear on shiny surfaces  
- **Ambient lighting** - Base illumination level to prevent completely dark areas
- **Emissive lighting** - Light emitted by surfaces without any light source shining on them (not itself treated as a light source illuminating other surfaces)

See <xref:OmegaEngine.Graphics.LightSources> for details on the types of light sources.

## Shadowing

OmegaEngine supports two distinct shadow casting methods for different purposes.

### Bounding sphere shadows

<xref:OmegaEngine.Graphics.Renderables.PositionableRenderable>s can cast realtime shadows on each other using a simple bounding sphere approximation. This provides easy shadowing with low performance impact, but with low fidelity: when an object is only partially within a shadow, it gets darkened in its entirety, rather than showing a shadow "line" across its surface.

This technique:

- projects cones starting at light sources and passing through shadow casters, matching their bounding sphere's radius
- tests whether shadow receivers are partially, fully or not at all within those cones

Each <xref:OmegaEngine.Graphics.Renderables.PositionableRenderable> can be a shadow caster, a shadow receiver, both or neither.

### Terrain self-shadowing

Terrain rendering supports high-fidelity self-shadowing using pre-calculated occlusion interval maps (see <xref:AlphaFramework.World.Terrains.ITerrain.OcclusionIntervalMap>).

This technique:

- calculates which angles each terrain point is visible from
- stores the data in occlusion interval maps
- uses this data during rendering to determine if terrain points are in shadow
- provides accurate terrain shadows without real-time calculations
