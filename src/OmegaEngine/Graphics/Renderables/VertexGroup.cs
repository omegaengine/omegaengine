/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.VertexDecl;
using SlimDX;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Renderables
{
    /// <summary>
    /// A simple set of vertexes that can be rendered.
    /// </summary>
    /// <remarks>No culling, intersect testing, etc.. Use <see cref="Model"/> for that.</remarks>
    public class VertexGroup : PositionableRenderable
    {
        #region Variables
        private VertexBuffer _vb;
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
        /// <param name="primitiveType">The type of primitives to generate from the vertexes</param>
        /// <param name="vertexes">An array for vertexes with position and color information</param>
        /// <param name="indexes">An array of indexes for the index buffer; <c>null</c> for no indexes</param>
        public VertexGroup(PrimitiveType primitiveType, PositionColored[] vertexes, short[] indexes = null)
        {
            #region Sanity checks
            if (vertexes == null) throw new ArgumentNullException("vertexes");
            #endregion

            _primitiveType = primitiveType;
            _vertexCount = vertexes.Length;

            _material = XMaterial.DefaultMaterial;

            _buildVertexBuffer = () => BufferHelper.CreateVertexBuffer(Engine.Device, vertexes, PositionColored.Format);
            Initialize(indexes);
        }

        /// <summary>
        /// Creates a new colored vertex group
        /// </summary>
        /// <param name="primitiveType">The type of primitives to generate from the vertexes</param>
        /// <param name="vertexes">An array for vertexes with position and color information</param>
        /// <param name="indexes">An array of indexes for the index buffer; <c>null</c> for no indexes</param>
        public VertexGroup(PrimitiveType primitiveType, PositionNormalColored[] vertexes, short[] indexes = null)
        {
            #region Sanity checks
            if (vertexes == null) throw new ArgumentNullException("vertexes");
            #endregion

            _primitiveType = primitiveType;
            _vertexCount = vertexes.Length;

            _buildVertexBuffer = () => BufferHelper.CreateVertexBuffer(Engine.Device, vertexes, PositionNormalColored.Format);
            Initialize(indexes);
        }
        #endregion

        #region Textured
        /// <summary>
        /// Creates a new textured vertex group
        /// </summary>
        /// <param name="primitiveType">The type of primitives to generate from the vertexes</param>
        /// <param name="vertexes">An array for vertexes with position and texture information</param>
        /// <param name="indexes">An array of indexes for the index buffer; <c>null</c> for no indexes</param>
        /// <param name="material">The material to use for rendering</param>
        public VertexGroup(PrimitiveType primitiveType, PositionTextured[] vertexes,
            short[] indexes, XMaterial material)
        {
            #region Sanity checks
            if (vertexes == null) throw new ArgumentNullException("vertexes");
            #endregion

            _primitiveType = primitiveType;
            _vertexCount = vertexes.Length;

            _material = material;
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            _material.HoldReference();

            _buildVertexBuffer = () => BufferHelper.CreateVertexBuffer(Engine.Device, vertexes, PositionTextured.Format);
            Initialize(indexes);
        }

        /// <summary>
        /// Creates a new textured vertex group
        /// </summary>
        /// <param name="primitiveType">The type of primitives to generate from the vertexes</param>
        /// <param name="vertexes">An array for vertexes with position and texture information</param>
        /// <param name="indexes">An array of indexes for the index buffer; <c>null</c> for no indexes</param>
        /// <param name="material">The material to use for rendering</param>
        public VertexGroup(PrimitiveType primitiveType, PositionNormalTextured[] vertexes,
            short[] indexes, XMaterial material)
        {
            #region Sanity checks
            if (vertexes == null) throw new ArgumentNullException("vertexes");
            #endregion

            _primitiveType = primitiveType;
            _vertexCount = vertexes.Length;

            _material = material;
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            _material.HoldReference();

            _buildVertexBuffer = () => BufferHelper.CreateVertexBuffer(Engine.Device, vertexes, PositionNormalTextured.Format);
            Initialize(indexes);
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Is internally used by all constructors for initialization
        /// </summary>
        /// <param name="indexes">An array of indexes for the index buffer; <c>null</c> for no indexes</param>
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
        }
        #endregion

        #endregion

        #region Predefined Quad
        /// <summary>
        /// Creates a new textured quad.
        /// </summary>
        /// <param name="size">The length of a border of the quad.</param>
        /// <param name="texture">The texture to place on the vertex group; <c>null</c> for no texture.</param>
        /// <returns>The vertex group that was created.</returns>
        public static VertexGroup Quad(float size, ITextureProvider texture = null)
        {
            var normalVector = new Vector3(0, 0, 1);
            var vertexes = new[]
            {
                new PositionNormalTextured(new Vector3(size * -0.5f, size * 0.5f, 0), normalVector, 0, 0),
                new PositionNormalTextured(new Vector3(size * 0.5f, size * 0.5f, 0), normalVector, 1, 0),
                new PositionNormalTextured(new Vector3(size * -0.5f, size * -0.5f, 0), normalVector, 0, 1),
                new PositionNormalTextured(new Vector3(size * 0.5f, size * -0.5f, 0), normalVector, 1, 1)
            };

            return new VertexGroup(PrimitiveType.TriangleStrip, vertexes, null, new XMaterial(texture));
        }
        #endregion

        //--------------------//

        #region Render
        /// <inheritdoc/>
        internal override void Render(Camera camera, GetLights getLights = null)
        {
            base.Render(camera, getLights);
            Engine.State.WorldTransform = WorldTransform;

            #region Draw
            // Activate the VertexBuffer
            Engine.State.SetVertexBuffer(_vb);

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
            var effectiveLights = (SurfaceEffect == SurfaceEffect.Plain || getLights == null)
                ? new LightSource[0]
                : getLights(Position, BoundingSphere.HasValue ? BoundingSphere.Value.Radius : 0);
            RenderHelper(render, _material, camera, effectiveLights);
            #endregion
        }
        #endregion

        //--------------------//

        #region Engine
        /// <summary>
        /// A callback used by different constructors to build the <see cref="_vb"/> as soon as <see cref="EngineElement.Engine"/> is set.
        /// </summary>
        private readonly Func<VertexBuffer> _buildVertexBuffer;

        /// <inheritdoc/>
        protected override void OnEngineSet()
        {
            base.OnEngineSet();

            _vb = _buildVertexBuffer();

            // Calculate bounding bodies
            BoundingSphere = BufferHelper.ComputeBoundingSphere(_vb, _vertexCount);
            BoundingBox = BufferHelper.ComputeBoundingBox(_vb, _vertexCount);
        }
        #endregion

        #region Dispose
        /// <inheritdoc/>
        protected override void OnDispose()
        {
            try
            {
                _vb?.Dispose();
                _ib?.Dispose();

                _material.HoldReference();
            }
            finally
            {
                base.OnDispose();
            }
        }
        #endregion
    }
}
