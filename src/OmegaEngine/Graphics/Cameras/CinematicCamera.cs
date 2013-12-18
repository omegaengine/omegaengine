/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using Common.Values;
using SlimDX;

namespace OmegaEngine.Graphics.Cameras
{
    /// <summary>
    /// A camera that cinematically swings from one view to another.
    /// </summary>
    /// <remarks>"Cinematic" means that the movement starts slowly, speeds up dramatically and then slows down again before reaching the target.</remarks>
    public class CinematicCamera : QuaternionCamera
    {
        #region Variables
        private readonly DoubleVector3 _sourcePosition, _targetPosition;
        private readonly Quaternion _sourceQuat, _targetQuat;

        private double _factor;
        private bool _needsRecalc = true;
        #endregion

        #region Properties
        /// <summary>
        /// Is this <see cref="CinematicCamera"/> currently moving?
        /// </summary>
        [Description("Is this cinematic camera currently moving?"), Category("Behavior")]
        public bool Moving { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new cinematic camera for the engine
        /// </summary>
        /// <param name="sourcePosition">The initial camera position</param>
        /// <param name="targetPosition">The target camera position</param>
        /// <param name="sourceQuat">The initial view as a quaternion</param>
        /// <param name="targetQuat">The target view as a quaternion</param>
        /// <param name="duration">The complete transition time in seconds</param>
        /// <param name="engine">The <see cref="Engine"/> containing this camera</param>
        public CinematicCamera(DoubleVector3 sourcePosition, DoubleVector3 targetPosition, Quaternion sourceQuat, Quaternion targetQuat, float duration, Engine engine)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            #endregion

            _sourcePosition = sourcePosition;
            _targetPosition = targetPosition;

            _sourceQuat = sourceQuat;
            _targetQuat = targetQuat;

            engine.Interpolate(
                start: 0, target: 1,
                callback: value =>
                {
                    _factor = value;
                    _needsRecalc = true;
                },
                duration: duration);
            Moving = true;

            Position = sourcePosition;
            ViewQuat = sourceQuat;
        }
        #endregion

        //--------------------//

        #region View control
        /// <inheritdoc/>
        public override void PerspectiveChange(float panX, float panY, float rotation, float zoom)
        {
            // Ignore input while the animation is running
        }
        #endregion

        #region Recalc View Matrix
        /// <summary>
        /// Update cached versions of <see cref="View"/> and related matrices
        /// </summary>
        protected override void UpdateView()
        {
            // Note: No updateView check, instead recalc once per frame
            if (_needsRecalc)
            {
                Position = _sourcePosition + (_targetPosition - _sourcePosition) * _factor;
                ViewQuat = Quaternion.Slerp(_sourceQuat, _targetQuat, (float)_factor);

                _needsRecalc = false;
            }

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (_factor == 1) Moving = false;
            // ReSharper restore CompareOfFloatsByEqualityOperator

            base.UpdateView();
        }
        #endregion
    }
}
