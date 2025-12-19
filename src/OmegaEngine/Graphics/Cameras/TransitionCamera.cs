/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using OmegaEngine.Foundation.Geometry;
using SlimDX;

namespace OmegaEngine.Graphics.Cameras;

/// <summary>
/// A camera that smoothly transitions from one position and rotation to another.
/// </summary>
/// <remarks>The movement starts slowly, speeds up, and then slows down again before reaching the target.</remarks>
public class TransitionCamera : QuaternionCamera
{
    /// <summary>
    /// Is the transition finished?
    /// </summary>
    [Browsable(false)]
    public bool IsComplete { get; private set; }

    /// <summary>
    /// Creates a new transition camera.
    /// </summary>
    /// <param name="sourcePosition">The initial camera position</param>
    /// <param name="sourceRotation">The initial camera rotation</param>
    /// <param name="targetPosition">The target camera position</param>
    /// <param name="targetRotation">The target camera rotation</param>
    /// <param name="duration">The complete transition time in seconds</param>
    /// <param name="engine">The <see cref="Engine"/> containing this camera</param>
    public TransitionCamera(DoubleVector3 sourcePosition, Quaternion sourceRotation, DoubleVector3 targetPosition, Quaternion targetRotation, float duration, Engine engine)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        #endregion

        engine.Interpolate(
            start: 0, target: 1,
            callback: value =>
            {
                Position = sourcePosition + (targetPosition - sourcePosition) * value;
                Quaternion = Quaternion.Slerp(sourceRotation, targetRotation, (float)value);

                if (value == 1) IsComplete = true;
            },
            duration: duration);

        Position = sourcePosition;
        Quaternion = sourceRotation;
    }

    /// <inheritdoc/>
    public override void Navigate(DoubleVector3 translation, DoubleVector3 rotation)
    {
        // Ignore input while the transition is running
    }
}
