/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using Common.Utils;
using OmegaEngine.Graphics.Shaders;

namespace OmegaEngine
{
    // This file contains helper methods for Terrain rendering
    partial class Engine
    {
        /// <summary>
        /// A cache for generated <see cref="TerrainShader"/>s with lighting enabled. The array index is used as a bitmask that indicates which textures are enabled.
        /// </summary>
        private readonly TerrainShader[] _terrainShadersLighting = new TerrainShader[65536];

        /// <summary>
        /// A cache for generated <see cref="TerrainShader"/>s with lighting disabled. The array index is used as a bitmask that indicates which textures are enabled.
        /// </summary>
        private readonly TerrainShader[] _terrainShadersNoLighting = new TerrainShader[65536];

        /// <summary>
        /// Generates a shader for a specific set of enabled textures. Results are cached internally.
        /// </summary>
        /// <param name="lighting">Get a shader with lighting enabled?</param>
        /// <param name="textureMask">A bitmask that indicates which textures are enabled.</param>
        internal TerrainShader GetTerrainShader(bool lighting, ushort textureMask)
        {
            var terrainShaders = lighting ? _terrainShadersLighting : _terrainShadersNoLighting;
            if (terrainShaders[textureMask] == null)
            {
                var texturesList = new LinkedList<int>();
                for (int i = 0; i < 16; i++)
                    if (MathUtils.CheckFlag(textureMask, 1 << i)) texturesList.AddLast(i + 1);
                var controllers = new Dictionary<string, IEnumerable<int>>(1) {{"textures", texturesList}};
                terrainShaders[textureMask] = new TerrainShader(lighting, controllers) {Engine = this};
            }
            return terrainShaders[textureMask];
        }
    }
}
