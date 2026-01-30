# Lighting

TODO: review

OmegaEngine uses a flexible lighting system that supports multiple light types and advanced shading techniques.

## Lighting Model

The default shader (<xref:OmegaEngine.Graphics.Shaders.GeneralShader>) implements **Blinn-Phong** lighting, which provides:

- **Diffuse lighting** - Light scattered equally in all directions from a surface
- **Specular highlights** - Bright spots that appear on shiny surfaces  
- **Ambient lighting** - Base illumination level to prevent completely dark areas

The Blinn-Phong model is an efficient approximation of how light interacts with surfaces, suitable for real-time rendering.

## Shadowing Techniques

OmegaEngine provides multiple shadowing approaches:

### Real-time bounding sphere shadows

Light sources automatically cast shadows based on object bounding spheres. This provides fast, approximate shadows suitable for dynamic scenes.

### Pre-computed terrain self-shadowing

Terrain rendering supports detailed self-shadowing using pre-calculated occlusion interval maps (see <xref:AlphaFramework.World.Terrains.ITerrain.OcclusionIntervalMap>). This technique:

- Calculates which angles each terrain point is visible from
- Stores the data in occlusion interval maps
- Uses this data during rendering to determine if terrain points are in shadow
- Provides accurate terrain shadows without real-time calculations

See [Terrain](terrain.md#shadowing) for more details on terrain shadowing.
