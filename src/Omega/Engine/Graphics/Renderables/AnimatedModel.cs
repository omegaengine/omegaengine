/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Assets;

namespace OmegaEngine.Graphics.Renderables
{
    /// <summary>
    /// A mesh-based model with frame-hierarchy animation that can be rendered by the engine
    /// </summary>
    public class AnimatedModel : PositionableRenderable
    {
        #region Variables
        /// <summary>A reference to the asset providing the data for this model.</summary>
        private XAnimatedMesh _asset;

        /// <summary>
        /// An array of materials used to render this mesh
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly", Justification = "It is deliberate that the array size cannot be changed while the elements can be modified")]
        public readonly XMaterial[] Materials;
        #endregion

        #region Properties
        /// <summary>
        /// The numbers of subsets in this model
        /// </summary>
        [Description("The numbers of subsets in this model"), Category("Design")]
        public int NumberSubsets { get; protected set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new animated model based upon a cached animated mesh, using its internal material data if available
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
        /// <param name="mesh">The animated mesh to use for rendering</param>
        public AnimatedModel(Engine engine, XAnimatedMesh mesh) : base(engine)
        {
            #region Sanity checks
            if (mesh == null) throw new ArgumentNullException("mesh");
            #endregion

            _asset = mesh;
            _asset.HoldReference();
            Materials = mesh.Materials;
            NumberSubsets = Materials.Length;

            // Get bounding bodies
            BoundingSphere = mesh.BoundingSphere;
            BoundingBox = mesh.BoundingBox;

            // Setup the matrices for animation
            //SetupBoneMatrices(_rootFrame.FrameHierarchy as AnimationFrame);
        }
        #endregion

        #region Static access
        /// <summary>
        /// Creates a new animated model using a cached <see cref="XAnimatedMesh"/> (loading a new one if none is cached).
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> providing the cache and rendering capabilities.</param>
        /// <param name="id">The ID of the asset to use.</param>
        /// <returns>The static (non-animated) model that was created.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the specified file could not be found.</exception>
        /// <exception cref="IOException">Thrown if there was an error reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the file does not contain a valid animated mesh.</exception>
        public static AnimatedModel FromAsset(Engine engine, string id)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");
            #endregion

            return new AnimatedModel(engine, XAnimatedMesh.Get(engine, id));
        }
        #endregion

        //--------------------//

        #region Render
        /// <inheritdoc />
        internal override void Render(Camera camera, GetLights lights)
        {
            base.Render(camera, lights);

            // Set world transform in the engine
            Engine.State.WorldTransform = WorldTransform;

            // ToDo: Implement rendering
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
                    if (_asset != null)
                    {
                        _asset.ReleaseReference();
                        _asset = null;
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
