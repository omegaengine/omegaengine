/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using NanoByte.Common;

namespace OmegaEngine;

/// <summary>
/// Options controlling an animation.
/// </summary>
/// <param name="Duration">The duration of the complete animation.</param>
/// <param name="EasingFunction">The kind of easing function to use.</param>
/// <param name="EaseIn">Whether the animation should be eased in. I.e., start slowly and then speed up.</param>
/// <param name="EaseOut">Whether the animation should be eased out. I.e., slow down at the end.</param>
public record AnimationOptions(
    TimeSpan Duration,
    EasingFunction EasingFunction = EasingFunction.Sinusoidal,
    bool EaseIn = true,
    bool EaseOut = true);
