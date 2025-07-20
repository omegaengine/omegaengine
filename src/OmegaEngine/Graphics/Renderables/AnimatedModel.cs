/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
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
        /// <param name="mesh">The animated mesh to use for rendering</param>
        public AnimatedModel(XAnimatedMesh mesh)
        {
            _asset = mesh ?? throw new ArgumentNullException(nameof(mesh));
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

        //--------------------//

        #region Render
        /// <inheritdoc/>
        internal override void Render(Camera camera, GetLights getLights = null)
        {
            base.Render(camera, getLights);
            Engine.State.WorldTransform = WorldTransform;

            // ToDo: Implement rendering
        }
        #endregion

        //--------------------//

        #region Dispose
        /// <inheritdoc/>
        protected override void OnDispose()
        {
            try
            {
                if (_asset != null)
                {
                    _asset.ReleaseReference();
                    _asset = null;
                }
            }
            finally
            {
                base.OnDispose();
            }
        }
        #endregion
    }
}
