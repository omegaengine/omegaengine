/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Shaders;
using OmegaEngine.Graphics.VertexDecl;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Renderables
{
    /// <summary>
    /// Displays a water plane with reflections and refraction
    /// </summary>
    public class Water : Model
    {
        #region Variables
        private WaterViewSource _viewSource;
        //private readonly WaterShader _simpleWaterShader;
        private readonly ITextureProvider _waterTexture;
        #endregion

        #region Properties
        private readonly SizeF _size;

        /// <summary>
        /// The size of the water plane
        /// </summary>
        public SizeF Size { get { return _size; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new water plane.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
        /// <param name="size">The size of the water plane.</param>
        public Water(Engine engine, SizeF size) : base(engine, BuildMesh(engine, size), XMaterial.DefaultMaterial)
        {
            _size = size;

            RenderIn = ViewType.NormalOnly;
            Pickable = false;
            SurfaceEffect = SurfaceEffect.Shader;
            Materials[0].Emissive = Color.LightBlue;

            BoundingBox = new BoundingBox(new Vector3(0, 0, 0), new Vector3(size.Width, 0, -size.Height));
            _waterTexture = XTexture.Get(Engine, @"Water\surface.png", false);
            _waterTexture.HoldReference();
        }

        private static Mesh BuildMesh(Engine engine, SizeF size)
        {
            // Compensate for texture stretch (ignored by all but the simple shader)
            float tu = size.Width / 1500;
            float tv = size.Height / 1500;

            var vertexes = new[]
            {
                new PositionNormalTextured(new Vector3(0, 0, 0), new Vector3(0, 1, 0), 0, 0),
                new PositionNormalTextured(new Vector3(size.Width, 0, 0), new Vector3(0, 1, 0), tu, 0),
                new PositionNormalTextured(new Vector3(0, 0, -size.Height), new Vector3(0, 1, 0), 0, tv),
                new PositionNormalTextured(new Vector3(size.Width, 0, -size.Height), new Vector3(0, 1, 0), tu, tv)
            };
            short[] indexes = {0, 1, 3, 3, 2, 0};

            var mesh = new Mesh(engine.Device, indexes.Length / 3, vertexes.Length, MeshFlags.Managed, PositionNormalTextured.Format);
            BufferHelper.WriteVertexBuffer(mesh, vertexes);
            BufferHelper.WriteIndexBuffer(mesh, indexes);

            return mesh;
        }
        #endregion

        //--------------------//

        #region Setup
        /// <summary>
        /// Creates views as reflection and refraction sources - Call after setting position!
        /// </summary>
        /// <param name="view">The original view to reflect</param>
        /// <param name="clipTolerance">How far to shift the clip plane along its normal vector to reduce graphical glitches at corners</param>
        /// <remarks>This method may be called only once on an instance</remarks>
        public void SetupChildViews(View view, float clipTolerance)
        {
            #region Sanity checks
            if (view == null) throw new ArgumentNullException("view");
            if (_viewSource != null) throw new InvalidOperationException(Resources.CallMethodOnlyOnce);
            #endregion

            // Make sure the required views get rendered first
            _viewSource = WaterViewSource.FromEngine(Engine, Position.Y, view, clipTolerance);
            RequiredViews.Add(_viewSource.RefractedView);
            RequiredViews.Add(_viewSource.ReflectedView);
        }

        /// <summary>
        /// Creates views as reflection and refraction sources - Call after setting position!
        /// </summary>
        /// <param name="view">The original view to reflect</param>
        public void SetupChildViews(View view)
        {
            SetupChildViews(view, 2);
        }
        #endregion

        #region Render
        /// <inheritdoc />
        internal override void Render(Camera camera, GetLights lights)
        {
            // Rendering this without a shader isn't possible (non-standard FVF)
            if (SurfaceEffect < SurfaceEffect.Shader) SurfaceEffect = SurfaceEffect.Shader;

            // Note: Doesn't call base methods
            PrepareRender();

            // Set world transform in the engine
            Engine.WorldTransform = WorldTransform;

            #region Auto-select shader
            SurfaceEffect = SurfaceEffect.Shader;
            switch (Engine.WaterEffects)
            {
                case WaterEffectsType.None:
                    Alpha = 128;
                    if (WaterShader.MinShaderModel > Engine.MaxShaderModel)
                    {
                        // No shader usage at all, render the surface map with the fixed-function pipeline
                        Materials[0].DiffuseMaps[0] = _waterTexture;
                        SurfaceEffect = SurfaceEffect.FixedFunction;
                    }
                    else SurfaceShader = Engine.SimpleWaterShader;
                    break;
                case WaterEffectsType.RefractionOnly:
                    Alpha = Engine.Opaque;
                    SurfaceShader = _viewSource.RefractionOnlyShader;
                    break;
                case WaterEffectsType.ReflectTerrain:
                case WaterEffectsType.ReflectAll:
                    Alpha = Engine.Opaque;
                    SurfaceShader = _viewSource.RefractionReflectionShader;
                    break;
            }
            #endregion

            // Transfer the reflection view matrix and the current time value to the shader
            if (_viewSource != null)
                _viewSource.RefractionReflectionShader.ReflectionViewProjection = _viewSource.ReflectedView.Camera.ViewProjection;

            // Render the mesh
            RenderHelper(() => Mesh.DrawSubset(0), Materials[0], camera, null);

            // Only allow the visualization of bounding bodies in normal view
            if (SurfaceEffect < SurfaceEffect.Glow)
                if (DrawBoundingBox && WorldBoundingBox.HasValue) Engine.DrawBoundingBox(WorldBoundingBox.Value);
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
                    if (_waterTexture != null) _waterTexture.ReleaseReference();

                    foreach (XMaterial material in Materials)
                        material.ReleaseReference();

                    if (_viewSource != null)
                    {
                        // Remove this water plane from the view source dependency list
                        _viewSource.ReleaseReference();

                        // Remove the view source, if it is no longer required
                        if (_viewSource.ReferenceCount == 0)
                        {
                            _viewSource.Dispose();
                            Engine.WaterViewSources.Remove(_viewSource);
                        }
                    }
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
