/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using NanoByte.Common;
using OmegaEngine.Foundation.Geometry;
using SlimDX;

namespace OmegaEngine.Graphics.Cameras;

/// <summary>
/// A camera that smoothly transitions between two <see cref="Camera"/> states.
/// </summary>
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
    /// <param name="start">The camera state at the beginning of the transition.</param>
    /// <param name="end">The camera state at the end of the transition.</param>
    /// <param name="options">Options controlling the transition.</param>
    /// <param name="engine">The <see cref="Engine"/> used for update callbacks.</param>
    public TransitionCamera(Camera start, Camera end, TransitionCameraOptions options, Engine engine)
    {
        #region Sanity checks
        if (start == null) throw new ArgumentNullException(nameof(start));
        if (end == null) throw new ArgumentNullException(nameof(end));
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        #endregion

        var startRotation = Quaternion.RotationMatrix(start.View);
        var endRotation = Quaternion.RotationMatrix(end.View);

        // Make sure both quaternions are in the same hemisphere, to avoid going the long way round
        if (Quaternion.Dot(startRotation, endRotation) < 0)
            endRotation = -endRotation;

        AnimatePosition(start, end, options, engine);
        AnimateRotation(startRotation, endRotation, options with {Duration = options.Duration.Multiply(options.RotationBias.Clamp())}, engine);
    }

    private void AnimatePosition(Camera start, Camera end, AnimationOptions options, Engine engine)
    {
        FrustumCulling = end.FrustumCulling;
        MaxPositionOffset = end.MaxPositionOffset;

        engine.Animate(
            start: 0, end: 1,
            callback: value =>
            {
                FieldOfView = MathUtils.Lerp(start.FieldOfView, end.FieldOfView, (float)value);
                NearClip = MathUtils.Lerp(start.NearClip, end.NearClip, (float)value);
                FarClip = MathUtils.Lerp(start.FarClip, end.FarClip, (float)value);

                Position = DoubleVector3.Lerp(start.Position, end.Position, value);
                AdjustPositionBase();

                if (value == 1) IsComplete = true;
            },
            options);
    }

    private void AnimateRotation(Quaternion start, Quaternion end, AnimationOptions options, Engine engine)
    {
        engine.Animate(
            start: 0, end: 1,
            callback: value => Quaternion = Quaternion.Slerp(start, end, (float)value),
            options);
    }

    /// <inheritdoc/>
    public override void Navigate(DoubleVector3 translation = default, DoubleVector3 rotation = default)
    {
        // Ignore input while the transition is running
    }
}
