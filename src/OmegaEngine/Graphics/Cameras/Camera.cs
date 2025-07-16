/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using NanoByte.Common;
using OmegaEngine.Values;
using SlimDX;

namespace OmegaEngine.Graphics.Cameras
{
    /// <summary>
    /// Determines the perspective from which a <see cref="Scene"/> is displayed.
    /// </summary>
    /// <seealso cref="OmegaEngine.Graphics.View.Camera"/>
    public abstract class Camera : IPositionable
    {
        #region Variables
        /// <summary>Does <see cref="ViewCached"/> need to be recalculated?</summary>
        protected bool ViewDirty = true;

        /// <summary>Does <see cref="_projection"/> need to be recalculated?</summary>
        protected bool ProjectionDirty = true;

        /// <summary>Does the view frustum need to be recalculated?</summary>
        protected bool ViewFrustumDirty = true;

        private readonly Plane[] _viewFrustum = new Plane[6];
        #endregion

        #region Properties

        #region Name
        /// <summary>
        /// Text value to make it easier to identify a particular camera
        /// </summary>
        [Description("Text value to make it easier to identify a particular camera"), Category("Design")]
        public string Name { get; set; }

        public override string ToString()
        {
            string value = GetType().Name;
            if (!string.IsNullOrEmpty(Name))
                value += ": " + Name;
            return value;
        }
        #endregion

        #region View
        protected DoubleVector3 PositionCached;

        /// <summary>
        /// The camera's position in 3D-space
        /// </summary>
        [Description("The camera's position in 3D-space"), Category("Layout")]
        public DoubleVector3 Position
        {
            get
            {
                UpdateView(); // Some cameras automatically update their positions
                return PositionCached;
            }
            set => value.To(ref PositionCached, ref ViewDirty, ref ViewFrustumDirty);
        }

        protected DoubleVector3 PositionBaseCached;

        /// <summary>
        /// A value that is subtracted from all positions (including the <see cref="Camera"/>'s) before handing them to the graphics hardware
        /// </summary>
        /// <remarks>Used to improve floating-point precision by keeping effective values small</remarks>
        /// <seealso cref="IPositionableOffset"/>
        [Description("A value that is subtracted from all positions (including the camera's) before handing them to the graphics hardware"), Category("Behavior")]
        public DoubleVector3 PositionBase
        {
            get
            {
                UpdateView(); // Some cameras automatically update their positions
                return PositionBaseCached;
            }
            set => value.To(ref PositionBaseCached, ref ViewDirty, ref ViewFrustumDirty);
        }

        protected Matrix ViewCached;

        /// <summary>
        /// A left-handed view matrix for the current camera setting
        /// </summary>
        protected internal Matrix View
        {
            get
            {
                UpdateView();
                return ViewCached;
            }
        }

        protected Matrix SimpleViewCached;

        /// <summary>
        /// A left-handed view matrix with absolutely no translation information
        /// </summary>
        protected internal Matrix SimpleView
        {
            get
            {
                UpdateView();
                return SimpleViewCached;
            }
        }

        private Matrix _viewInverse;

        /// <summary>
        /// An inverted view matrix for the current camera setting
        /// </summary>
        protected internal Matrix ViewInverse
        {
            get
            {
                UpdateView();
                return _viewInverse;
            }
        }

        private Matrix _viewTranspose;

        /// <summary>
        /// A transposed view matrix for the current camera setting
        /// </summary>
        protected internal Matrix ViewTranspose
        {
            get
            {
                UpdateView();
                return _viewTranspose;
            }
        }

        private Matrix _viewInverseTranspose;

        /// <summary>
        /// An inverted and transposed view matrix for the current camera setting
        /// </summary>
        protected internal Matrix ViewInverseTranspose
        {
            get
            {
                UpdateView();
                return _viewInverseTranspose;
            }
        }

        private Matrix _sphericalBillboard;

        /// <summary>
        /// A rotation matrix for a faked spherical billboard effect
        /// </summary>
        protected internal Matrix SphericalBillboard
        {
            get
            {
                UpdateView();
                return _sphericalBillboard;
            }
        }

        private Matrix _cylindricBillboard;

        /// <summary>
        /// A rotation matrix for a faked cylindrical billboard effect
        /// </summary>
        protected internal Matrix CylindricalBillboard
        {
            get
            {
                UpdateView();
                return _cylindricBillboard;
            }
        }
        #endregion

        #region Projection
        private Size _size;

        /// <summary>
        /// The size of the output (i.e. screen size)
        /// </summary>
        internal Size Size { get => _size; set => value.To(ref _size, ref ProjectionDirty, ref ViewFrustumDirty); }

        private float _fieldOfView = (float)Math.PI / 4.0f;

        /// <summary>
        /// The view angle in degrees
        /// </summary>
        [DefaultValue(45f), Description("The view angle in degrees"), Category("Layout")]
        public float FieldOfView { get => _fieldOfView.RadianToDegree(); set => value.DegreeToRadian().To(ref _fieldOfView, ref ProjectionDirty, ref ViewFrustumDirty); }

        private float _nearClip = 20.0f;

        /// <summary>
        /// Minimum distance of objects to the camera
        /// </summary>
        [DefaultValue(20.0f), Description("Minimum distance of objects to the camera"), Category("Clipping")]
        public float NearClip { get => _nearClip; set => value.To(ref _nearClip, ref ProjectionDirty, ref ViewFrustumDirty); }

        private float _farClip = 1e+6f;

        /// <summary>
        /// Maximum distance of objects to the camera
        /// </summary>
        [DefaultValue(1e+6f), Description("Maximum distance of objects to the camera"), Category("Clipping")]
        public float FarClip { get => _farClip; set => value.To(ref _farClip, ref ProjectionDirty, ref ViewFrustumDirty); }

        private DoublePlane _clipPlane;

        /// <summary>
        /// A custom clip plane behind which all objects are culled
        /// </summary>
        [DefaultValue(typeof(DoublePlane), "0; 0; 0; 0; 0; 0"), Description("A custom clip plane behind which all objects are culled"), Category("Clipping")]
        public DoublePlane ClipPlane { get => _clipPlane; set => value.To(ref _clipPlane, ref ProjectionDirty, ref ViewFrustumDirty); }

        private Plane _effectiveClipPlane;

        /// <summary>
        /// The effective clip plane to use for rendering
        /// </summary>
        internal Plane EffectiveClipPlane
        {
            get
            {
                UpdateView();
                UpdateProjection();
                return _effectiveClipPlane;
            }
        }

        private Matrix _projection;

        /// <summary>
        /// A projection matrix for the current camera setting
        /// </summary>
        internal Matrix Projection
        {
            get
            {
                UpdateProjection();
                return _projection;
            }
        }

        private Matrix _simpleProjection;

        /// <summary>
        /// A projection matrix overriding the clip planes (defaulting to near=1 and far=10)
        /// </summary>
        internal Matrix SimpleProjection
        {
            get
            {
                UpdateProjection();
                return _simpleProjection;
            }
        }

        private Matrix _projectionInverse;

        /// <summary>
        /// An inverted projection matrix for the current camera setting
        /// </summary>
        internal Matrix ProjectionInverse
        {
            get
            {
                UpdateProjection();
                return _projectionInverse;
            }
        }

        private Matrix _projectionTranspose;

        /// <summary>
        /// A transposed projection matrix for the current camera setting
        /// </summary>
        internal Matrix ProjectionTranspose
        {
            get
            {
                UpdateProjection();
                return _projectionTranspose;
            }
        }

        private Matrix _projectionInverseTranspose;

        /// <summary>
        /// An inverted and transposed projection matrix for the current camera setting
        /// </summary>
        internal Matrix ProjectionInverseTranspose
        {
            get
            {
                UpdateProjection();
                return _projectionInverseTranspose;
            }
        }
        #endregion

        #region ViewProjection
        private Matrix _viewProjection;

        /// <summary>
        /// A view-projection matrix for the current camera setting
        /// </summary>
        internal Matrix ViewProjection
        {
            get
            {
                UpdateViewProjection();
                return _viewProjection;
            }
        }

        private Matrix _viewProjectionInverse;

        /// <summary>
        /// An inverted view-projection matrix for the current camera setting
        /// </summary>
        internal Matrix ViewProjectionInverse
        {
            get
            {
                UpdateViewProjection();
                return _viewProjectionInverse;
            }
        }

        private Matrix _viewProjectionTranspose;

        /// <summary>
        /// A transposed view-projection matrix for the current camera setting
        /// </summary>
        internal Matrix ViewProjectionTranspose
        {
            get
            {
                UpdateViewProjection();
                return _viewProjectionTranspose;
            }
        }

        private Matrix _viewProjectionInverseTranspose;

        /// <summary>
        /// An inverted and transposed view-projection matrix for the current camera setting
        /// </summary>
        internal Matrix ViewProjectionInverseTranspose
        {
            get
            {
                UpdateViewProjection();
                return _viewProjectionInverseTranspose;
            }
        }
        #endregion

        #endregion

        #region Properties
        /// <summary>
        /// Shall the engine use view frustum culling to optimize the rendering performance?
        /// </summary>
        [DefaultValue(true), Description("Shall the engine use view frustum culling to optimize the rendering performance?"), Category("Behavior")]
        public bool FrustumCulling { get; set; } = true;
        #endregion

        //--------------------//

        #region View control
        /// <summary>
        /// Called when the user changes the view perspective.
        /// </summary>
        /// <param name="panX">The number of pixels panned along the X-axis divided by the number of pixels of the longest side of the viewport.</param>
        /// <param name="panY">The number of pixels panned along the Y-axis divided by the number of pixels of the longest side of the viewport.</param>
        /// <param name="rotation">Horizontal rotation in degrees.</param>
        /// <param name="zoom">Scaling factor; 1 for no change, must not be 0.</param>
        public abstract void PerspectiveChange(float panX, float panY, float rotation, float zoom);
        #endregion

        #region Update View Matrix
        /// <summary>
        /// Update cached versions of <see cref="View"/> and related matrices if necessary
        /// </summary>
        protected abstract void UpdateView();

        /// <summary>
        /// Calculate cached versions of special matrices (e.g. <see cref="ViewInverse"/> calculated from <see cref="View"/>)
        /// </summary>
        protected void CacheSpecialMatrices()
        {
            _viewInverse = Matrix.Invert(ViewCached);
            _viewTranspose = Matrix.Transpose(ViewCached);
            _viewInverseTranspose = Matrix.Transpose(_viewInverse);

            Quaternion spherical = Quaternion.RotationMatrix(_viewInverse);
            _sphericalBillboard = Matrix.RotationQuaternion(spherical);

            Quaternion cylindric = Quaternion.Normalize(new(0, spherical.Y, 0, spherical.W));
            _cylindricBillboard = Matrix.RotationQuaternion(cylindric);

            _effectiveClipPlane = _clipPlane.ApplyOffset(PositionBaseCached);
        }
        #endregion

        #region Recalc Projection Matrix
        /// <summary>
        /// Update <see cref="Projection"/> if necessary
        /// </summary>
        protected virtual void UpdateProjection()
        {
            if (!ProjectionDirty) return;

            _projection = Matrix.PerspectiveFovLH(_fieldOfView, (float)_size.Width / _size.Height, _nearClip, _farClip);
            _simpleProjection = Matrix.PerspectiveFovLH(_fieldOfView, (float)_size.Width / _size.Height, 1, 10);

            // Cache special matrices
            _projectionInverse = Matrix.Invert(_projection);
            _projectionTranspose = Matrix.Transpose(_projection);
            _projectionInverseTranspose = Matrix.Transpose(_projectionInverse);

            _effectiveClipPlane = _clipPlane.ApplyOffset(PositionBaseCached);

            ProjectionDirty = false;
        }
        #endregion

        #region Recalc ViewProjection Matrix
        /// <summary>
        /// Recalculate cached versions of <see cref="_viewProjection"/> and related matrices
        /// </summary>
        private void UpdateViewProjection()
        {
            // Recalculate only when necessary
            (View * Projection).To(ref _viewProjection, delegate
            {
                // Cache special matrices
                _viewProjectionInverse = Matrix.Invert(_viewProjection);
                _viewProjectionTranspose = Matrix.Transpose(_viewProjection);
                _viewProjectionInverseTranspose = Matrix.Transpose(_viewProjectionInverse);
            });
        }
        #endregion

        //--------------------//

        #region Check ViewFrustum
        /// <summary>
        /// Checks whether a <see cref="BoundingSphere"/> is inside the camera's view frustum
        /// </summary>
        /// <param name="boundingSphere">A sphere that completely encompasses the body in world space</param>
        /// <returns><c>true</c> if the sphere is in the frustum</returns> 
        internal bool InFrustum(BoundingSphere boundingSphere)
        {
            // Pre-checks
            if (!FrustumCulling) return true;

            UpdateViewFrustum();

            // Check if the sphere lies completley behind one of the frustum planes
            return _viewFrustum.All(t => BoundingSphere.Intersects(boundingSphere, t) != PlaneIntersectionType.Back);

            // Otherwise the object is at least partially visible
        }

        /// <summary>
        /// Checks whether a <see cref="BoundingBox"/> is inside the camera's view frustum
        /// </summary>
        /// <param name="boundingBox">An axis-aligned box that completely encompasses the body in world space</param>
        /// <returns><c>true</c> if the box is in the frustum</returns> 
        internal bool InFrustum(BoundingBox boundingBox)
        {
            // Pre-checks
            if (!FrustumCulling) return true;
            if ((boundingBox.Maximum - boundingBox.Minimum) == new Vector3()) return true;

            UpdateViewFrustum();

            // Check if the box lies completley behind one of the frustum planes
            return _viewFrustum.All(plane => BoundingBox.Intersects(boundingBox, plane) != PlaneIntersectionType.Back);

            // Otherwise the object is at least partially visible
        }
        #endregion

        #region Update ViewFrustum
        /// <summary>
        /// Update <see cref="_viewFrustum"/> if necessary
        /// </summary>
        private void UpdateViewFrustum()
        {
            // Recalculate only when necessary
            UpdateView();
            if (!ViewFrustumDirty) return;

            // Left plane
            _viewFrustum[0].Normal.X = ViewProjection.M14 + ViewProjection.M11;
            _viewFrustum[0].Normal.Y = ViewProjection.M24 + ViewProjection.M21;
            _viewFrustum[0].Normal.Z = ViewProjection.M34 + ViewProjection.M31;
            _viewFrustum[0].D = ViewProjection.M44 + ViewProjection.M41;

            // Right plane
            _viewFrustum[1].Normal.X = ViewProjection.M14 - ViewProjection.M11;
            _viewFrustum[1].Normal.Y = ViewProjection.M24 - ViewProjection.M21;
            _viewFrustum[1].Normal.Z = ViewProjection.M34 - ViewProjection.M31;
            _viewFrustum[1].D = ViewProjection.M44 - ViewProjection.M41;

            // Top plane
            _viewFrustum[2].Normal.X = ViewProjection.M14 - ViewProjection.M12;
            _viewFrustum[2].Normal.Y = ViewProjection.M24 - ViewProjection.M22;
            _viewFrustum[2].Normal.Z = ViewProjection.M34 - ViewProjection.M32;
            _viewFrustum[2].D = ViewProjection.M44 - ViewProjection.M42;

            // Bottom plane
            _viewFrustum[3].Normal.X = ViewProjection.M14 + ViewProjection.M12;
            _viewFrustum[3].Normal.Y = ViewProjection.M24 + ViewProjection.M22;
            _viewFrustum[3].Normal.Z = ViewProjection.M34 + ViewProjection.M32;
            _viewFrustum[3].D = ViewProjection.M44 + ViewProjection.M42;

            // Near plane
            if (_clipPlane == default(DoublePlane))
            {
                _viewFrustum[4].Normal.X = ViewProjection.M13;
                _viewFrustum[4].Normal.Y = ViewProjection.M23;
                _viewFrustum[4].Normal.Z = ViewProjection.M33;
                _viewFrustum[4].D = ViewProjection.M43;
            }
            else _viewFrustum[4] = EffectiveClipPlane;

            // Far plane
            _viewFrustum[5].Normal.X = ViewProjection.M14 - ViewProjection.M13;
            _viewFrustum[5].Normal.Y = ViewProjection.M24 - ViewProjection.M23;
            _viewFrustum[5].Normal.Z = ViewProjection.M34 - ViewProjection.M33;
            _viewFrustum[5].D = ViewProjection.M44 - ViewProjection.M43;

            // Normalize planes
            for (int i = 0; i < 6; i++)
                _viewFrustum[i] = Plane.Normalize(_viewFrustum[i]);

            ViewFrustumDirty = false;
        }
        #endregion
    }
}
