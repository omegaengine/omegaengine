/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using Common;
using SlimDX;
using SlimDX.Direct3D9;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.VertexDecl;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Renderables
{
    /// <summary>
    /// A group of textured or colored vertexes that can be rendered by the engine
    /// </summary>
    public class VertexGroup : PositionableRenderable
    {
        #region Variables
        private readonly VertexBuffer _vb;
        private readonly int _vertexCount;

        private IndexBuffer _ib;
        private readonly XMaterial _material = XMaterial.DefaultMaterial;
        private readonly PrimitiveType _primitiveType;
        private int _primitiveCount;
        #endregion

        #region Constructor

        #region Colored
        /// <summary>
        /// Creates a new colored vertex group
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
        /// <param name="primitiveType">The type of primitives to generate from the vertexes</param>
        /// <param name="vertexes">An array for vertexes with position and color information</param>
        /// <param name="indexes">An array of indexes for the index buffer; <see langword="null"/> for no indexes</param>
        public VertexGroup(Engine engine, PrimitiveType primitiveType, PositionColored[] vertexes,
            short[] indexes) : base(engine)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (vertexes == null) throw new ArgumentNullException("vertexes");
            #endregion

            _vertexCount = vertexes.Length;
            _vb = BufferHelper.CreateVertexBuffer(engine.Device, vertexes, PositionColored.Format);

            _primitiveType = primitiveType;
            Initialize(indexes);

            _material = XMaterial.DefaultMaterial;
        }

        /// <summary>
        /// Creates a new colored vertex group
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
        /// <param name="primitiveType">The type of primitives to generate from the vertexes</param>
        /// <param name="vertexes">An array for vertexes with position and color information</param>
        /// <param name="indexes">An array of indexes for the index buffer; <see langword="null"/> for no indexes</param>
        public VertexGroup(Engine engine, PrimitiveType primitiveType, PositionNormalColored[] vertexes,
            short[] indexes) : base(engine)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (vertexes == null) throw new ArgumentNullException("vertexes");
            #endregion

            _vertexCount = vertexes.Length;
            _vb = BufferHelper.CreateVertexBuffer(engine.Device, vertexes, PositionNormalColored.Format);

            _primitiveType = primitiveType;
            Initialize(indexes);
        }
        #endregion

        #region Textured
        /// <summary>
        /// Creates a new textured vertex group
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
        /// <param name="primitiveType">The type of primitives to generate from the vertexes</param>
        /// <param name="vertexes">An array for vertexes with position and texture information</param>
        /// <param name="indexes">An array of indexes for the index buffer; <see langword="null"/> for no indexes</param>
        /// <param name="material">The material to use for rendering</param>
        public VertexGroup(Engine engine, PrimitiveType primitiveType, PositionTextured[] vertexes,
            short[] indexes, XMaterial material) : base(engine)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (vertexes == null) throw new ArgumentNullException("vertexes");
            #endregion

            _vertexCount = vertexes.Length;
            _vb = BufferHelper.CreateVertexBuffer(engine.Device, vertexes, PositionTextured.Format);

            _primitiveType = primitiveType;
            Initialize(indexes);

            _material = material;
            // ReSharper disable ImpureMethodCallOnReadonlyValueField
            _material.HoldReference();
            // ReSharper restore ImpureMethodCallOnReadonlyValueField
        }

        /// <summary>
        /// Creates a new textured vertex group
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
        /// <param name="primitiveType">The type of primitives to generate from the vertexes</param>
        /// <param name="vertexes">An array for vertexes with position and texture information</param>
        /// <param name="indexes">An array of indexes for the index buffer; <see langword="null"/> for no indexes</param>
        /// <param name="material">The material to use for rendering</param>
        public VertexGroup(Engine engine, PrimitiveType primitiveType, PositionNormalTextured[] vertexes,
            short[] indexes, XMaterial material) : base(engine)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (vertexes == null) throw new ArgumentNullException("vertexes");
            #endregion

            _vertexCount = vertexes.Length;
            _vb = BufferHelper.CreateVertexBuffer(engine.Device, vertexes, PositionNormalTextured.Format);

            _primitiveType = primitiveType;
            Initialize(indexes);

            _material = material;
            // ReSharper disable ImpureMethodCallOnReadonlyValueField
            _material.HoldReference();
            // ReSharper restore ImpureMethodCallOnReadonlyValueField
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Is internally used by all constructors for initialization
        /// </summary>
        /// <param name="indexes">An array of indexes for the index buffer; <see langword="null"/> for no indexes</param>
        private void Initialize(short[] indexes)
        {
            #region Index Buffer
            int effectiveVertexes;
            if (indexes != null)
            {
                _ib = BufferHelper.CreateIndexBuffer(Engine.Device, indexes);
                effectiveVertexes = indexes.Length;
            }
            else effectiveVertexes = _vertexCount;
            #endregion

            #region Primitive Type
            // Plausibility checks
            if (_primitiveType >= PrimitiveType.TriangleFan && effectiveVertexes < 3)
                throw new ArgumentException("You need at least 3 vertexes for a Triangle");
            if (_primitiveType <= PrimitiveType.LineStrip && effectiveVertexes < 2)
                throw new ArgumentException("You need at least 2 vertexes for a Line");

            // Primitive count
            // ReSharper disable CompareOfFloatsByEqualityOperator
            switch (_primitiveType)
            {
                case PrimitiveType.TriangleStrip:
                case PrimitiveType.TriangleFan:
                    _primitiveCount = effectiveVertexes - 2;
                    break;
                case PrimitiveType.TriangleList:
                    if (Math.IEEERemainder(effectiveVertexes, 3) != 0)
                        throw new ArgumentException(Resources.WrongVertexCountTriangle);
                    _primitiveCount = effectiveVertexes / 3;
                    break;
                case PrimitiveType.LineStrip:
                    _primitiveCount = effectiveVertexes - 1;
                    Pickable = false;
                    break;
                case PrimitiveType.LineList:
                    if (Math.IEEERemainder(effectiveVertexes, 2) != 0)
                        throw new ArgumentException(Resources.WrongVertexCountLine);
                    _primitiveCount = effectiveVertexes / 2;
                    Pickable = false;
                    break;
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator
            #endregion

            // Calculate bounding bodies
            BoundingSphere = BufferHelper.ComputeBoundingSphere(_vb, _vertexCount);
            BoundingBox = BufferHelper.ComputeBoundingBox(_vb, _vertexCount);
        }
        #endregion

        #endregion

        #region Static access
        /// <summary>
        /// Creates a new vertex group using a cached <see cref="XTexture"/> (loading a new one if none is cached).
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> providing the cache and rendering capabilities.</param>
        /// <param name="vertexes">An array for vertexes with position and texture information.</param>
        /// <param name="primitiveType">The type of primitives to generate from the vertexes,</param>
        /// <param name="indexes">An array of indexes for the index buffer; <see langword="null"/> for no indexes.</param>
        /// <param name="id">The ID of the texture asset to use.</param>
        /// <returns>The vertex group that was created.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the specified file could not be found.</exception>
        /// <exception cref="IOException">Thrown if there was an error reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the file does not contain a valid texture.</exception>
        public static VertexGroup FromAsset(Engine engine, PrimitiveType primitiveType, PositionNormalTextured[] vertexes,
            short[] indexes, string id)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (vertexes == null) throw new ArgumentNullException("vertexes");
            #endregion

            return new VertexGroup(engine, primitiveType, vertexes, indexes,
                new XMaterial(XTexture.Get(engine, id, false)));
        }
        #endregion

        #region Predefined Quad
        /// <summary>
        /// Creates a new textured quad with uniformly sized borders.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
        /// <param name="texture">The texture to place on the vertex group; <see langword="null"/> for no texture.</param>
        /// <param name="size">The length of a border of the quad.</param>
        public static VertexGroup Quad(Engine engine, ITextureProvider texture, float size)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            #endregion

            var normalVector = new Vector3(0, 0, 1);
            var vertexes = new[]
            {
                new PositionNormalTextured(new Vector3(size * -0.5f, size * 0.5f, 0), normalVector, 0, 0),
                new PositionNormalTextured(new Vector3(size * 0.5f, size * 0.5f, 0), normalVector, 1, 0),
                new PositionNormalTextured(new Vector3(size * -0.5f, size * -0.5f, 0), normalVector, 0, 1),
                new PositionNormalTextured(new Vector3(size * 0.5f, size * -0.5f, 0), normalVector, 1, 1)
            };

            return new VertexGroup(engine, PrimitiveType.TriangleStrip, vertexes, null, new XMaterial(texture));
        }

        /// <summary>
        /// Creates a new textured quad with an external material
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the texture from</param>
        /// <param name="size">The length of a border of the quad</param>
        /// <param name="material">A material containing a texture and lighting information</param>
        /// <returns>The vertex group that was created</returns>
        public static VertexGroup Quad(Engine engine, float size, XMaterial material)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            #endregion

            var normalVector = new Vector3(0, 0, 1);
            var vertexes = new[]
            {
                new PositionNormalTextured(new Vector3(size * -0.5f, size * 0.5f, 0), normalVector, 0, 0),
                new PositionNormalTextured(new Vector3(size * 0.5f, size * 0.5f, 0), normalVector, 1, 0),
                new PositionNormalTextured(new Vector3(size * -0.5f, size * -0.5f, 0), normalVector, 0, 1),
                new PositionNormalTextured(new Vector3(size * 0.5f, size * -0.5f, 0), normalVector, 1, 1)
            };

            return new VertexGroup(engine, PrimitiveType.TriangleStrip, vertexes, null, material);
        }

        /// <summary>
        /// Creates a new textured quad with an external texture
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the texture from</param>
        /// <param name="size">The length of a border of the quad</param>
        /// <param name="textureSource">The <see cref="View"/> generating the texture</param>
        /// <returns>The vertex group that was created</returns>
        public static VertexGroup Quad(Engine engine, float size, TextureView textureSource)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (textureSource == null) throw new ArgumentNullException("textureSource");
            #endregion

            return Quad(engine, size, new XMaterial(textureSource.GetRenderTarget()));
        }
        #endregion

        //--------------------//

        #region Render
        /// <inheritdoc />
        internal override void Render(Camera camera, GetLights lights)
        {
            base.Render(camera, lights);

            // Set world transform in the engine
            Engine.WorldTransform = WorldTransform;

            #region Draw
            // Activate the VertexBuffer
            Engine.SetVertexBuffer(_vb);

            // Prepare a render delegate
            Action render;
            if (_ib != null)
            {
                // Activate the IndexBuffer and render indexed primitives
                Engine.Device.Indices = _ib;
                render = (() => Engine.Device.DrawIndexedPrimitives(_primitiveType, 0, 0, _vertexCount, 0, _primitiveCount));
            }
            else
            {
                // Render non-indexed primitives
                render = (() => Engine.Device.DrawPrimitives(_primitiveType, 0, _primitiveCount));
            }

            // Handle lights for fixed-function or shader rendering
            var effectiveLights = (SurfaceEffect < SurfaceEffect.FixedFunction) ? null : lights(Position, BoundingSphere.HasValue ? BoundingSphere.Value.Radius : 0);

            // Execute the render delegate
            RenderHelper(render, _material, camera, effectiveLights);
            #endregion
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

                    // ReSharper disable ImpureMethodCallOnReadonlyValueField
                    _material.HoldReference();
                    // ReSharper restore ImpureMethodCallOnReadonlyValueField
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
