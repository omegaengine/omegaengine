/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using NanoByte.Common;

namespace OmegaEngine;

/// <summary>
/// Provides simple animation helpers for the <see cref="Engine"/>.
/// </summary>
public static class EngineUtilsAnimation
{
    /// <summary>
    /// Automatically interpolates between two numeric values while rendering
    /// </summary>
    /// <param name="engine">The engine to use for rendering</param>
    /// <param name="start">The value to start off with</param>
    /// <param name="end">The value to end up at</param>
    /// <param name="callback">The delegate to call for with the updated interpolated value each frame</param>
    /// <param name="options">Options controlling the animation</param>
    public static void Animate(this Engine engine, double start, double end, Action<double> callback, AnimationOptions options)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (callback == null) throw new ArgumentNullException(nameof(callback));
        #endregion

        bool negative = start > end;

        Stopwatch interpolationTimer = null;
        Action interpolate = null;
        interpolate = delegate
        {
            double value;

            if (interpolationTimer == null)
            {
                // Start timer the first time this delegate gets called;
                interpolationTimer = Stopwatch.StartNew();
                value = start;
            }
            else
            {
                double fraction = interpolationTimer.Elapsed.Divide(options.Duration);
                double factor = options switch
                {
                    { EaseIn: true, EaseOut: true } => MathUtils.EaseInOut(fraction, options.EasingFunction),
                    { EaseIn: true } => MathUtils.EaseIn(fraction, options.EasingFunction),
                    { EaseOut: true } => MathUtils.EaseOut(fraction, options.EasingFunction),
                    _ => fraction
                };
                value = MathUtils.Lerp(start, end, factor);

                // Don't shoot past the target
                if ((negative && value < end) || (!negative && value > end)) value = end;
            }

            // Return the value to the delegate
            callback(value);

            // Delegate removes itself from PostRender once Max value has been reached
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (value == end) engine.PreRender -= interpolate;
        };

        // Hook interpolation delegate into PostRender sequence
        engine.PreRender += interpolate;

        // Run callback for t=0 immediately
        callback(start);
    }

    /// <summary>
    /// Fades in the screen from total black
    /// </summary>
    /// <param name="engine">The engine to use for rendering</param>
    /// <param name="options">Options controlling the animation</param>
    public static void FadeIn(this Engine engine, AnimationOptions? options = null)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        #endregion

        engine.Animate(
            start: 255, end: 0,
            callback: value => engine.FadeLevel = (int)value,
            options ?? new(Duration: TimeSpan.FromSeconds(2), EaseIn: false));
        engine.FadeExtra = true;
    }

    /// <summary>
    /// Dims in the screen down
    /// </summary>
    /// <param name="engine">The engine to use for rendering</param>
    /// <param name="options">Options controlling the animation</param>
    public static void DimDown(this Engine engine, AnimationOptions? options = null)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        #endregion

        engine.Animate(
            start: engine.FadeLevel, end: 80,
            callback: value => engine.FadeLevel = (int)value,
            options ?? new(Duration: TimeSpan.FromSeconds(1)));
        engine.FadeExtra = false;
    }

    /// <summary>
    /// Dims in the screen back up
    /// </summary>
    /// <param name="engine">The engine to use for rendering</param>
    /// <param name="options">Options controlling the animation</param>
    public static void DimUp(this Engine engine, AnimationOptions? options = null)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        #endregion

        engine.Animate(
            start: engine.FadeLevel, end: 0,
            callback: value => engine.FadeLevel = (int)value,
            options ?? new(Duration: TimeSpan.FromSeconds(1)));
        engine.FadeExtra = false;
    }
}
