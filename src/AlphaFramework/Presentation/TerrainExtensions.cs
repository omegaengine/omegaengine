using System;
using System.IO;
using System.Linq;
using AlphaFramework.World.Templates;
using AlphaFramework.World.Terrains;
using OmegaEngine;
using RenderableTerrain = OmegaEngine.Graphics.Renderables.Terrain;

namespace AlphaFramework.Presentation;

/// <summary>
/// Contains extension methods for <see cref="Terrain{TTemplate}"/>.
/// </summary>
public static class TerrainExtensions
{
    /// <summary>
    /// Generate a <see cref="RenderableTerrain"/> from this terrain data.
    /// </summary>
    /// <param name="terrain">The terrain data.</param>
    /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
    /// <param name="lighting">Shall this mesh be prepared for lighting? (calculate normal vectors, make shaders support lighting, ...)</param>
    /// <param name="blockSize">How many points in X and Y direction shall one block for culling be?</param>
    /// <exception cref="FileNotFoundException">One of the specified texture files could not be found.</exception>
    /// <exception cref="IOException">There was an error reading one of the texture files.</exception>
    /// <exception cref="UnauthorizedAccessException">Read access to one of the texture files is not permitted.</exception>
    /// <exception cref="InvalidDataException">One of the texture files does not contain a valid texture.</exception>
    public static RenderableTerrain ToRenderable<TTemplate>(this Terrain<TTemplate> terrain, Engine engine, bool lighting, int blockSize = 32)
        where TTemplate : TerrainTemplateBase<TTemplate>
        => RenderableTerrain.Create(
            engine,
            new(terrain.Size.X, terrain.Size.Y),
            terrain.Size.StretchH,
            terrain.Size.StretchV,
            terrain.HeightMap ?? throw new InvalidOperationException("Terrain height map missing"),
            terrain.TextureMap ?? throw new InvalidOperationException("Terrain texture map missing"),
            GetTextures(terrain.Templates),
            terrain.OcclusionIntervalMap,
            lighting,
            blockSize);

    private static string[] GetTextures<TTemplate>(TTemplate?[] templates)
        where TTemplate : TerrainTemplateBase<TTemplate>
        => templates
          .Select(x => x is { Texture: { Length: > 0 } texture }
               ? Path.Combine("Terrain", texture)
               : null)
          .ToArray();
}
