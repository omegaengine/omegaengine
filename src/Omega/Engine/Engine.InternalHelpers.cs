/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Drawing;
using Common.Utils;
using SlimDX;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics.Shaders;
using OmegaEngine.Graphics.VertexDecl;

namespace OmegaEngine
{
    // This file contains helper methods that are called from inside the Engine
    partial class Engine
    {
        #region Render quad with shader
        /// <summary>
        /// Draw a textured quad filling the current <see cref="Viewport"/> using <see cref="PositionTextured"/> - useful for <see cref="PostShader"/>
        /// </summary>
        internal void DrawQuadShader()
        {
            using (new ProfilerEvent("Rendering textured quad for post-screen shaders"))
            {
                // Prepare render states
                FillMode = FillMode.Solid;
                CullMode = Cull.Counterclockwise;
                FfpLighting = false;
                Fog = false;

                var vertexes = new[]
                {
                    new PositionTextured(-1, 1, 0, 0, 0), new PositionTextured(-1, -1, 0, 0, 1),
                    new PositionTextured(1, 1, 0, 1, 0), new PositionTextured(1, -1, 0, 1, 1)
                };

                Device.VertexFormat = PositionTextured.Format;
                Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, vertexes);
            }
        }
        #endregion

        #region Render textured quad
        /// <summary>
        /// Draw a textured quad filling the current <see cref="Viewport"/> using <see cref="TransformedTextured"/>
        /// </summary>
        internal void DrawQuadTextured()
        {
            using (new ProfilerEvent("Rendering textured quad"))
            {
                // Prepare render states
                FillMode = FillMode.Solid;
                CullMode = Cull.Counterclockwise;
                FfpLighting = false;
                Fog = false;

                // Calculate texture coordinates in the centers of the corner texels
                var vertexes = new[]
                {
                    new TransformedTextured(Device.Viewport.X, Device.Viewport.Y + Device.Viewport.Height - 1, 0, 1, 0, 1),
                    new TransformedTextured(Device.Viewport.X, Device.Viewport.Y, 0, 1, 0, 0),
                    new TransformedTextured(Device.Viewport.X + Device.Viewport.Width - 1, Device.Viewport.Y + Device.Viewport.Height - 1, 0, 1, 1, 1),
                    new TransformedTextured(Device.Viewport.X + Device.Viewport.Width - 1, Device.Viewport.Y, 0, 1, 1, 0)
                };

                // Disable texture filtering
                Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.None);
                Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.None);

                Device.VertexFormat = TransformedTextured.Format;
                Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, vertexes);

                // Restore texture filtering
                SetupTextureFiltering();
            }
        }
        #endregion

        #region Render colored quad
        /// <summary>
        /// Draws a colored quad filling the current <see cref="Viewport"/> using <see cref="TransformedColored"/> - useful for fading
        /// </summary>
        /// <param name="color">The color of the quad</param>
        internal void DrawQuadColored(Color color)
        {
            using (new ProfilerEvent("Rendering colored quad"))
            {
                int colVal = color.ToArgb();

                // Prepare render states
                FillMode = FillMode.Solid;
                CullMode = Cull.Counterclockwise;
                FfpLighting = false;
                Fog = false;

                var vertexes = new[]
                {
                    new TransformedColored(Device.Viewport.X, Device.Viewport.Y + Device.Viewport.Height - 1, 0, 1, colVal),
                    new TransformedColored(Device.Viewport.X, Device.Viewport.Y, 0, 1, colVal),
                    new TransformedColored(Device.Viewport.X + Device.Viewport.Width - 1, Device.Viewport.Y + Device.Viewport.Height - 1, 0, 1, colVal),
                    new TransformedColored(Device.Viewport.X + Device.Viewport.Width - 1, Device.Viewport.Y, 0, 1, colVal)
                };

                SetTexture(null);
                Device.VertexFormat = TransformedColored.Format;
                Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, vertexes);
            }
        }
        #endregion

        //--------------------//

        #region Draw bounding sphere
        /// <summary>
        /// Display a <see cref="BoundingSphere"/> as a wireframe model
        /// </summary>
        /// <param name="sphere">The sphere to be display</param>
        internal void DrawBoundingSphere(BoundingSphere sphere)
        {
            using (new ProfilerEvent("Draw bounding sphere"))
            {
                // Backup current render states
                Matrix lastWorldTransform = WorldTransform;
                Cull lastCullMode = CullMode;
                FillMode lastFillMode = FillMode;
                bool lastLighting = FfpLighting;

                // Set new states
                WorldTransform = Matrix.Scaling(new Vector3(sphere.Radius)) * Matrix.Translation(sphere.Center);
                CullMode = Cull.None;
                FillMode = FillMode.Wireframe;
                FfpLighting = false;

                // Render the sphere
                SetTexture(null);
                _simpleSphere.DrawSubset(0);

                // Restore the old states
                WorldTransform = lastWorldTransform;
                CullMode = lastCullMode;
                FillMode = lastFillMode;
                FfpLighting = lastLighting;
            }
        }
        #endregion

        #region Draw bounding box
        /// <summary>
        /// Display a <see cref="BoundingBox"/> as a wireframe model
        /// </summary>
        /// <param name="box">The box to be display</param>
        internal void DrawBoundingBox(BoundingBox box)
        {
            using (new ProfilerEvent("Draw bounding box"))
            {
                // Backup current render states
                Matrix lastWorldTransform = WorldTransform;
                Cull lastCullMode = CullMode;
                FillMode lastFillMode = FillMode;
                bool lastLighting = FfpLighting;

                // Set new states
                Vector3 boxCenter = box.Minimum + (box.Maximum - box.Minimum) * 0.5f;
                WorldTransform = Matrix.Scaling(box.Maximum - box.Minimum) * Matrix.Translation(boxCenter);
                CullMode = Cull.None;
                FillMode = FillMode.Wireframe;
                FfpLighting = false;

                // Render the box
                SetTexture(null);
                _simpleBox.DrawSubset(0);

                // Restore the old states
                WorldTransform = lastWorldTransform;
                CullMode = lastCullMode;
                FillMode = lastFillMode;
                FfpLighting = lastLighting;
            }
        }
        #endregion

        //--------------------//

        #region Terrain shader cache
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
                terrainShaders[textureMask] = new TerrainShader(this, lighting, controllers);
            }
            return terrainShaders[textureMask];
        }
        #endregion
    }
}
