/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using Common.Utils;
using Common.Values;
using OmegaEngine.Properties;
using Vector3 = SlimDX.Vector3;

namespace OmegaEngine.Graphics.Cameras
{
    /// <summary>
    /// A camera that can be rotated around a specific point in space.
    /// </summary>
    public sealed class TrackCamera : MatrixCamera
    {
        #region Properties
        private double _radius;

        /// <summary>
        /// The distance between the camera and the center of the focues object.
        /// </summary>
        /// <remarks>Must be a positiv real number.</remarks>
        [Description("The distance between the camera and the center of the focues object."), Category("Layout")]
        public double Radius
        {
            get { return _radius; }
            set
            {
                #region Sanity checks
                if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException("value", Resources.NumberNotReal);
                if (value <= 0) throw new ArgumentOutOfRangeException("value", Resources.ValueNotPositive);
                #endregion

                // Apply limits (in case of conflict minimum is more important than maximum)
                value = Math.Max(Math.Min(value, MaxRadius), MinRadius);

                value.To(ref _radius, ref ViewDirty, ref ViewFrustumDirty);
            }
        }

        private double _horizontalRotation;

        /// <summary>
        /// The horizontal rotation in degrees.
        /// </summary>
        /// <remarks>Must be a real number.</remarks>
        [Description("The horizontal rotation in degrees."), Category("Layout")]
        public double HorizontalRotation
        {
            get { return _horizontalRotation.RadianToDegree(); }
            set
            {
                #region Sanity checks
                if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException("value", Resources.NumberNotReal);
                #endregion

                value.DegreeToRadian().To(ref _horizontalRotation, ref ViewDirty, ref ViewFrustumDirty);
            }
        }

        private double _verticalRotation;

        /// <summary>
        /// The vertical rotation in degrees.
        /// </summary>
        [Description("The vertical rotation in degrees."), Category("Layout")]
        public double VerticalRotation
        {
            get { return _verticalRotation.RadianToDegree(); }
            set
            {
                #region Sanity checks
                if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException("value", Resources.NumberNotReal);
                #endregion

                // Keep rotations between 0 and 2PI
                value = value.DegreeToRadian();
                while (value > 2 * Math.PI) value -= 2 * Math.PI;
                while (value < 0) value += 2 * Math.PI;

                value.To(ref _verticalRotation, ref ViewDirty, ref ViewFrustumDirty);
            }
        }

        private double _minRadius;

        /// <summary>
        /// The minimum radius allowed.
        /// </summary>
        /// <remarks>Must be a positive real number.</remarks>
        [Description("The minimum radius allowed."), Category("Behavior")]
        public double MinRadius
        {
            get { return _minRadius; }
            set
            {
                #region Sanity checks
                if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException("value", Resources.NumberNotReal);
                if (value <= 0) throw new ArgumentOutOfRangeException("value", Resources.ValueNotPositive);
                #endregion

                value.To(ref _minRadius, ref ViewDirty, ref ViewFrustumDirty);
            }
        }

        private double _maxRadius;

        /// <summary>
        /// The maximum radius allowed.
        /// </summary>
        /// <remarks>Must be a positive real number.</remarks>
        [Description("The maximum radius allowed."), Category("Behavior")]
        public double MaxRadius
        {
            get { return _maxRadius; }
            set
            {
                #region Sanity checks
                if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException("value", Resources.NumberNotReal);
                if (value <= 0) throw new ArgumentOutOfRangeException("value", Resources.ValueNotPositive);
                #endregion

                value.To(ref _maxRadius, ref ViewDirty, ref ViewFrustumDirty);
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new tracking camera.
        /// </summary>
        /// <param name="minRadius">The minimum radius allowed. Also used as the initial radius</param>
        /// <param name="maxRadius">The maximum radius allowed.</param>
        public TrackCamera(double minRadius = 50, double maxRadius = 100)
        {
            Radius = MinRadius = minRadius;
            MaxRadius = maxRadius;
        }
        #endregion

        //--------------------//

        #region Perspective change
        /// <inheritdoc/>
        public override void PerspectiveChange(float panX, float panY, float rotation, float zoom)
        {
            Target += new DoubleVector3(panX * -Math.Cos(_horizontalRotation), panY, panX * Math.Sin(_horizontalRotation)) * Radius;

            Radius *= zoom;
            HorizontalRotation += rotation;
        }
        #endregion

        #region Recalc View Matrix
        /// <summary>
        /// Update cached versions of <see cref="View"/> and related matrices.
        /// </summary>
        protected override void UpdateView()
        {
            // Only execute this if the view has changed
            if (!ViewDirty) return;

            var relativePosition = new DoubleVector3(
                _radius * Math.Cos(_verticalRotation) * -Math.Sin(_horizontalRotation),
                _radius * Math.Sin(_verticalRotation),
                _radius * Math.Cos(_verticalRotation) * -Math.Cos(_horizontalRotation));
            Position = relativePosition + Target;

            // Switch up-vector based on vertical rotation
            UpVector = (_verticalRotation > Math.PI / 2 && _verticalRotation < Math.PI / 2 * 3) ? new Vector3(0, -1, 0) : new Vector3(0, 1, 0);

            base.UpdateView();

            ViewDirty = false;
        }
        #endregion
    }
}
