/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using SlimDX.Direct3D9;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.VertexDecl;

namespace OmegaEngine.Graphics.Renderables
{
    /// <summary>
    /// A cubic skybox existing of 4 or 6 non-animated planes.
    /// </summary>
    public class SimpleSkybox : Skybox
    {
        #region Variables
        private readonly VertexBuffer _vb;
        private readonly IndexBuffer _ib;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new skybox using texture-files
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
        /// <param name="textures">An array of the 6 textures to be uses (right, left, top, bottom, front, back)</param>
        /// <exception cref="ArgumentException">Thrown if there are not exactly 6 textures</exception>
        protected SimpleSkybox(Engine engine, XTexture[] textures) : base(engine, textures)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            #endregion

            #region Vertexes
            var vertexes = new[]
            {
                // Right
                new PositionTextured(1.0f, -1.0f, 1.0f, 0.0f, 1.0f),
                new PositionTextured(1.0f, 1.0f, 1.0f, 0.0f, 0.0f),
                new PositionTextured(1.0f, 1.0f, -1.0f, 1.0f, 0.0f),
                new PositionTextured(1.0f, -1.0f, -1.0f, 1.0f, 1.0f),
                // Left
                new PositionTextured(-1.0f, -1.0f, -1.0f, 0.0f, 1.0f),
                new PositionTextured(-1.0f, 1.0f, -1.0f, 0.0f, 0.0f),
                new PositionTextured(-1.0f, 1.0f, 1.0f, 1.0f, 0.0f),
                new PositionTextured(-1.0f, -1.0f, 1.0f, 1.0f, 1.0f),
                // Top
                new PositionTextured(-1.0f, 1.0f, 1.0f, 0.0f, 1.0f),
                new PositionTextured(-1.0f, 1.0f, -1.0f, 0.0f, 0.0f),
                new PositionTextured(1.0f, 1.0f, -1.0f, 1.0f, 0.0f),
                new PositionTextured(1.0f, 1.0f, 1.0f, 1.0f, 1.0f),
                // Bottom
                new PositionTextured(-1.0f, -1.0f, -1.0f, 0.0f, 1.0f),
                new PositionTextured(-1.0f, -1.0f, 1.0f, 0.0f, 0.0f),
                new PositionTextured(1.0f, -1.0f, 1.0f, 1.0f, 0.0f),
                new PositionTextured(1.0f, -1.0f, -1.0f, 1.0f, 1.0f),
                // Front
                new PositionTextured(-1.0f, -1.0f, 1.0f, 0.0f, 1.0f),
                new PositionTextured(-1.0f, 1.0f, 1.0f, 0.0f, 0.0f),
                new PositionTextured(1.0f, 1.0f, 1.0f, 1.0f, 0.0f),
                new PositionTextured(1.0f, -1.0f, 1.0f, 1.0f, 1.0f),
                // Back
                new PositionTextured(1.0f, -1.0f, -1.0f, 0.0f, 1.0f),
                new PositionTextured(1.0f, 1.0f, -1.0f, 0.0f, 0.0f),
                new PositionTextured(-1.0f, 1.0f, -1.0f, 1.0f, 0.0f),
                new PositionTextured(-1.0f, -1.0f, -1.0f, 1.0f, 1.0f)
            };

            _vb = BufferHelper.CreateVertexBuffer(engine.Device, vertexes, PositionTextured.Format);
            #endregion

            #region Indexes
            _ib = BufferHelper.CreateIndexBuffer(engine.Device, new short[]
            {
                0, 2, 3, 0, 1, 2, // Right
                4, 5, 6, 4, 6, 7, // Left
                8, 9, 10, 8, 10, 11, // Top
                12, 13, 14, 12, 14, 15, // Bottom
                16, 18, 19, 16, 17, 18, // Front
                20, 21, 22, 20, 22, 23 // Back
            });
            #endregion
        }
        #endregion

        #region Static access
        /// <summary>
        /// Creates a new skybox using a cached <see cref="XTexture"/>s (loading new ones if they aren't cached).
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> providing the cache and rendering capabilities.</param>
        /// <param name="rt">The ID of the "right" texture.</param>
        /// <param name="lf">The ID of the "left" texture</param>
        /// <param name="up">The ID of the "up" texture.</param>
        /// <param name="dn">The ID of the "down" texture.</param>
        /// <param name="ft">The ID of the "front" texture.</param>
        /// <param name="bk">The ID of the "back" texture.</param>
        /// <returns>The skybox that was created.</returns>
        /// <exception cref="FileNotFoundException">Thrown if on of the specified texture files could not be found.</exception>
        /// <exception cref="IOException">Thrown if there was an error reading one of the texture files.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to one of the texture files is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the texture files does not contain a valid texture.</exception>
        public static SimpleSkybox FromAsset(Engine engine, string rt, string lf, string up, string dn, string ft, string bk)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            #endregion

            var textures = new[]
            {
                XTexture.Get(engine, rt, false), XTexture.Get(engine, lf, false), XTexture.Get(engine, up, false),
                XTexture.Get(engine, dn, false), XTexture.Get(engine, ft, false), XTexture.Get(engine, bk, false)
            };
            return new SimpleSkybox(engine, textures);
        }
        #endregion

        //--------------------//

        #region Render
        /// <inheritdoc />
        internal override void Render(Camera camera, GetLights lights)
        {
            base.Render(camera, lights);

            Engine.SetVertexBuffer(_vb);
            Engine.Device.Indices = _ib;

            // Turn off texture wrapping at edges to prevent seams
            Engine.Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
            Engine.Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

            // Draw all sides of the cube
            for (int i = 0; i < 6; ++i)
            {
                // Skip any "empty" sides without errors
                if (Textures[i] == null) continue;

                Engine.SetTexture(Textures[i]);
                Engine.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, i * 4, 4, i * 6, 2);
            }

            // Restore normal texture handling
            Engine.Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Wrap);
            Engine.Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Wrap);
        }
        #endregion

        //--------------------//

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (Disposed || Engine == null || Engine.Disposed) return; // Don't try to dispose more than once

            try
            {
                if (disposing)
                { // This block will only be executed on manual disposal, not by Garbage Collection
                    if (_vb != null) _vb.Dispose();
                    if (_ib != null) _ib.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion
    }
}
