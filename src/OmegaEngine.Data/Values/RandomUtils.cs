/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using JetBrains.Annotations;
using SlimDX;

namespace OmegaEngine.Values;

/// <summary>
/// Provides helper methods for creating different types of variables with random content.
/// </summary>
public static class RandomUtils
{
    /// <summary>
    /// Global random generator
    /// </summary>
    private static readonly Random _randomGenerator = new();

    /// <summary>
    /// Get random a integer value
    /// </summary>
    /// <param name="min">The minimum value</param>
    /// <param name="max">The maximum value</param>
    [Pure]
    public static int GetRandomInt(int min, int max) => _randomGenerator.Next(min, max);

    /// <summary>
    /// Get a random float value between <paramref name="min"/> and <paramref name="max"/>
    /// </summary>
    /// <param name="min">The minimum value</param>
    /// <param name="max">The maximum value</param>
    [Pure]
    public static float GetRandomFloat(float min, float max) => (float)_randomGenerator.NextDouble() * (max - min) + min;

    /// <summary>
    /// Get a random double value between <paramref name="min"/> and <paramref name="max"/>
    /// </summary>
    /// <param name="min">The minimum value</param>
    /// <param name="max">The maximum value</param>
    [Pure]
    public static double GetRandomDouble(float min, float max) => _randomGenerator.NextDouble() * (max - min) + min;

    /// <summary>
    /// Get a random Vector3 value between <paramref name="min"/> and <paramref name="max"/>
    /// </summary>
    /// <param name="min">minimum for each component</param>
    /// <param name="max">maximum for each component</param>
    [Pure]
    public static Vector3 GetRandomVector3(Vector3 min, Vector3 max) => new(
        GetRandomFloat(min.X, max.X),
        GetRandomFloat(min.Y, max.Y),
        GetRandomFloat(min.Z, max.Z));

    /// <summary>
    /// Get a random color value between <paramref name="limit1"/> and <paramref name="limit2"/>
    /// </summary>
    /// <param name="limit1">One limit for the color values</param>
    /// <param name="limit2">The other limit for the color values</param>
    [Pure]
    public static Color GetRandomColor(Color limit1, Color limit2) => ColorUtils.Interpolate(GetRandomFloat(0, 1), limit1, limit2);
}
