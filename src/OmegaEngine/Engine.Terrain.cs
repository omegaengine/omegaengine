/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Linq;
using OmegaEngine.Graphics.Shaders;

namespace OmegaEngine;

// This file contains helper methods for Terrain rendering
partial class Engine
{
    /// <summary>
    /// A cache for generated <see cref="TerrainShader"/>s with lighting enabled. The array index is used as a bitmask that indicates which textures are enabled.
    /// </summary>
    private readonly TerrainShader?[] _terrainShadersLighting = new TerrainShader?[65536];

    /// <summary>
    /// A cache for generated <see cref="TerrainShader"/>s with lighting disabled. The array index is used as a bitmask that indicates which textures are enabled.
    /// </summary>
    private readonly TerrainShader?[] _terrainShadersNoLighting = new TerrainShader?[65536];

    private readonly object _terrainShaderLock = new();

    /// <summary>
    /// Returns a shader for each entry in <paramref name="textureMasks"/>, compiling any not yet cached.
    /// </summary>
    /// <param name="lighting">Get shaders with lighting enabled?</param>
    /// <param name="textureMasks">The bitmasks indicating which textures are enabled per subset.</param>
    /// <returns>One shader per entry in <paramref name="textureMasks"/>, in the same order.</returns>
    internal TerrainShader[] GetTerrainShaders(bool lighting, ushort[] textureMasks)
    {
        var compiled = textureMasks.Distinct()
            .AsParallel()
            .ToDictionary(mask => mask, mask => GetTerrainShader(lighting, mask));
        return textureMasks.Select(mask => compiled[mask]).ToArray();
    }

    /// <summary>
    /// Generates a shader for a specific set of enabled textures. Results are cached.
    /// </summary>
    /// <param name="lighting">Get a shader with lighting enabled?</param>
    /// <param name="textureMask">A bitmask that indicates which textures are enabled.</param>
    /// <returns>The newly generated or previously cached shader.</returns>
    /// <remarks>This method is thread-safe.</remarks>
    private TerrainShader GetTerrainShader(bool lighting, int textureMask)
    {
        var terrainShaders = lighting ? _terrainShadersLighting : _terrainShadersNoLighting;
        if (terrainShaders[textureMask] is {} shader)
            return shader;

        // TerrainShader constructor only generates shader source/bytecode (CPU work) and is therefore thread-safe
        shader = new TerrainShader(lighting, Capabilities, textureMask);

        // RegisterChild triggers Effect.FromStream(Device, ...) and is not thread-safe
        lock (_terrainShaderLock)
        {
            if (terrainShaders[textureMask] == null)
                RegisterChild(terrainShaders[textureMask] = shader);
        }

        return terrainShaders[textureMask];
    }
}
