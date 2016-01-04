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

namespace OmegaEngine
{
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
        /// <param name="target">The value to end up at</param>
        /// <param name="callback">The delegate to call for with the updated interpolated value each frame</param>
        /// <param name="duration">The time for complete transition in seconds</param>
        /// <param name="trigonometric"><c>true</c> smooth (trigonometric) and <c>false</c> for linear interpolation</param>
        public static void Interpolate(this Engine engine, double start, double target, Action<double> callback, double duration = 1, bool trigonometric = true)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (callback == null) throw new ArgumentNullException("callback");
            #endregion

            bool negative = start > target;

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
                    // Calc the interpolated value based on the elapsed time
                    double interpolationTime = interpolationTimer.Elapsed.TotalSeconds;
                    if (trigonometric) value = MathUtils.InterpolateTrigonometric(interpolationTime / duration, start, target);
                    else value = interpolationTime / duration * (target - start) + start;

                    // Don't shoot past the target
                    if ((negative && value < target) || (!negative && value > target)) value = target;
                }

                // Return the value to the delegate
                callback(value);

                // Delegate removes itself from PostRender once Max value has been reached
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (value == target) engine.PreRender -= interpolate;
            };

            // Hook interpolation delegate into PostRender sequence
            engine.PreRender += interpolate;
        }

        /// <summary>
        /// Fades in the screen from total black in one second
        /// </summary>
        public static void FadeIn(this Engine engine)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            #endregion

            engine.Interpolate(
                start: 255, target: 0,
                callback: value => engine.FadeLevel = (int)value);
            engine.FadeExtra = true;
        }

        /// <summary>
        /// Dims in the screen down
        /// </summary>
        public static void DimDown(this Engine engine)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            #endregion

            engine.Interpolate(
                start: engine.FadeLevel, target: 80,
                callback: value => engine.FadeLevel = (int)value);
            engine.FadeExtra = false;
        }

        /// <summary>
        /// Dims in the screen back up
        /// </summary>
        public static void DimUp(this Engine engine)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            #endregion

            engine.Interpolate(
                start: engine.FadeLevel, target: 0,
                callback: value => engine.FadeLevel = (int)value);
            engine.FadeExtra = false;
        }
    }
}
