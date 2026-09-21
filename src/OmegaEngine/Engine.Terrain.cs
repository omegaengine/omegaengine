/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using OmegaEngine.Graphics.Shaders;
using SlimDX;
using SlimDX.Direct3D9;

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
    /// <returns>One shader per entry in <paramref name="textureMasks"/>, in the same order; <c>null</c> for masks no usable shader could be built for.</returns>
    internal TerrainShader?[] GetTerrainShaders(bool lighting, ushort[] textureMasks)
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
    /// <returns>The newly generated or previously cached shader; <c>null</c> if no usable shader could be built.</returns>
    /// <remarks>
    /// This method is thread-safe.
    /// Shader Model 2.0 hardware runs out of arithmetic instruction slots from 9 textures on when lighting is enabled.
    /// Such a mask falls back to an unlit shader, which keeps the terrain correctly textured but flatly lit.
    /// </remarks>
    private TerrainShader? GetTerrainShader(bool lighting, int textureMask)
    {
        var terrainShaders = lighting ? _terrainShadersLighting : _terrainShadersNoLighting;
        if (terrainShaders[textureMask] is {} cached)
            return cached;

        // TerrainShader constructor only generates shader source/bytecode (CPU work) and is therefore thread-safe
        TerrainShader shader;
        try
        {
            shader = new TerrainShader(lighting, Capabilities, textureMask);
        }
        catch (ShaderCompileException ex)
        {
            Log.Error($"Failed to compile terrain shader for {BitCount(textureMask)} textures with lighting {(lighting ? "enabled" : "disabled")} on Shader Model {Capabilities.MaxShaderModel}", ex);
            return Fallback();
        }

        // Assigning the Engine creates the effect on the device and is not thread-safe
        lock (_terrainShaderLock)
        {
            if (terrainShaders[textureMask] is {} raced)
            {
                // Another thread got there first, so this one is surplus
                shader.Dispose();
                return raced;
            }

            try
            {
                RegisterChild(shader);
            }
            catch (Exception ex) when (ex is CompilationException or Direct3D9Exception)
            {
                Log.Error($"Failed to create terrain shader for {BitCount(textureMask)} textures with lighting {(lighting ? "enabled" : "disabled")} on the device", ex);
                UnregisterChild(shader);
                shader.Dispose();
                return Fallback();
            }

            return terrainShaders[textureMask] = shader;
        }

        // Rendering the terrain unlit is preferable to not rendering it at all.
        // The result is cached under the requested mask too, so the failed attempt is not repeated for every subset.
        TerrainShader? Fallback()
            => lighting
                ? terrainShaders[textureMask] = GetTerrainShader(lighting: false, textureMask)
                : null;
    }

    /// <summary>
    /// Counts the number of textures a bitmask enables.
    /// </summary>
    private static int BitCount(int value)
    {
        int count = 0;
        for (; value != 0; value >>= 1) count += value & 1;
        return count;
    }
}
