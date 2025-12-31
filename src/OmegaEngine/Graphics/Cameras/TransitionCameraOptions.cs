/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using NanoByte.Common;

namespace OmegaEngine.Graphics.Cameras;

/// <summary>
/// Options controlling a camera transition.
/// </summary>
/// <param name="Duration">The duration of the complete transition.</param>
/// <param name="EasingFunction">The kind of easing function to use.</param>
/// <param name="EaseIn">Whether the transition should be eased in. I.e., start slowly and then speed up.</param>
/// <param name="EaseOut">Whether the transition should be eased out. I.e., slow down at the end.</param>
/// <param name="RotationBias">The fraction of the transition time after which the rotation should be complete. Value is clamped between 0 and 1.</param>
public record TransitionCameraOptions(
    TimeSpan Duration,
    EasingFunction EasingFunction = EasingFunction.Sinusoidal,
    bool EaseIn = true,
    bool EaseOut = true,
    double RotationBias = 1)
    : AnimationOptions(Duration, EasingFunction, EaseIn, EaseOut);
