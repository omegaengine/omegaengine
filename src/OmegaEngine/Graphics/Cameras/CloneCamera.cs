/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using NanoByte.Common;

namespace OmegaEngine.Graphics.Cameras
{
    /// <summary>
    /// A camera that immitates the perspective of another <see cref="Camera"/>.
    /// </summary>
    public class CloneCamera : Camera
    {
        #region Properties
        /// <summary>
        /// The parent camera to track
        /// </summary>
        [Description("The parent camera to track"), Category("Behavior")]
        public Camera ParentCamera { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new clone camera
        /// </summary>
        /// <param name="parentCamera">The parent camera to track</param>
        public CloneCamera(Camera parentCamera)
        {
            ParentCamera = parentCamera;
        }
        #endregion

        //--------------------//

        #region View control
        /// <inheritdoc/>
        public override void PerspectiveChange(float panX, float panY, float rotation, float zoom)
        {
            // User input should never be routed here
        }
        #endregion

        #region Recalc View Matrix
        /// <summary>
        /// Update cached versions of <see cref="View"/> and related matrices
        /// </summary>
        protected override void UpdateView()
        {
            // Note: External update check, clone and conditionally recalc
            bool update = false;
            ParentCamera.View.To(ref ViewCached, ref update);
            if (!update) return;

            CacheSpecialMatrices();
            ViewFrustumDirty = true;
        }
        #endregion

        #region Recalc Projection Matrix
        /// <summary>
        /// Update cached versions of <see cref="Camera.Projection"/> and related matrices
        /// </summary>
        protected override void UpdateProjection()
        {
            NearClip = ParentCamera.NearClip;
            FarClip = ParentCamera.FarClip;

            base.UpdateProjection();
        }
        #endregion
    }
}
