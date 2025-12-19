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
/// A camera that cinematically swings from one position and rotation to another.
/// </summary>
/// <remarks>"Cinematic" means that the movement starts slowly, speeds up and then slows down again before reaching the target.</remarks>
public class CinematicCamera : QuaternionCamera
{
    /// <summary>
    /// Is this <see cref="CinematicCamera"/> currently moving?
    /// </summary>
    [Description("Is this cinematic camera currently moving?"), Category("Behavior")]
    public bool Moving { get; private set; }

    /// <summary>
    /// Creates a new cinematic camera for the engine
    /// </summary>
    /// <param name="sourcePosition">The initial camera position</param>
    /// <param name="sourceRotation">The initial camera rotation</param>
    /// <param name="targetPosition">The target camera position</param>
    /// <param name="targetRotation">The target camera rotation</param>
    /// <param name="duration">The complete transition time in seconds</param>
    /// <param name="engine">The <see cref="Engine"/> containing this camera</param>
    public CinematicCamera(DoubleVector3 sourcePosition, Quaternion sourceRotation, DoubleVector3 targetPosition, Quaternion targetRotation, float duration, Engine engine)
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

                if (value == 1) Moving = false;
            },
            duration: duration);
        Moving = true;

        Position = sourcePosition;
        Quaternion = sourceRotation;
    }

    /// <inheritdoc/>
    public override void Navigate(DoubleVector3 translation, DoubleVector3 rotation)
    {
        // Ignore input while the animation is running
    }
}
