/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using JetBrains.Annotations;

namespace OmegaEngine;

/// <summary>
/// Designed to keep other code clean of messy spaghetti code required for some math operations.
/// </summary>
public static class MathUtils
{
    #region Clamp
    /// <summary>
    /// Makes a value stay within a certain range
    /// </summary>
    /// <param name="value">The number to clamp</param>
    /// <param name="min">The minimum number to return</param>
    /// <param name="max">The maximum number to return</param>
    /// <returns>The <paramref name="value"/> if it was in range, otherwise <paramref name="min"/> or <paramref name="max"/>.</returns>
    [Pure]
    public static double Clamp(this double value, double min = 0, double max = 1)
    {
        #region Sanity checks
        if (value < min) return min;
        if (value > max) return max;
        if (min > max) throw new ArgumentException(Properties.Resources.MinLargerMax, nameof(min));
        #endregion

        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    /// <summary>
    /// Makes a value stay within a certain range
    /// </summary>
    /// <param name="value">The number to clamp</param>
    /// <param name="min">The minimum number to return</param>
    /// <param name="max">The maximum number to return</param>
    /// <returns>The <paramref name="value"/> if it was in range, otherwise <paramref name="min"/> or <paramref name="max"/>.</returns>
    [Pure]
    public static float Clamp(this float value, float min = 0, float max = 1)
    {
        #region Sanity checks
        if (value < min) return min;
        if (value > max) return max;
        if (min > max) throw new ArgumentException(Properties.Resources.MinLargerMax, nameof(min));
        #endregion

        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    /// <summary>
    /// Makes a value stay within a certain range
    /// </summary>
    /// <param name="value">The number to clamp</param>
    /// <param name="min">The minimum number to return</param>
    /// <param name="max">The maximum number to return</param>
    /// <returns>The <paramref name="value"/> if it was in range, otherwise <paramref name="min"/> or <paramref name="max"/>.</returns>
    [Pure]
    public static int Clamp(this int value, int min = 0, int max = 1)
    {
        #region Sanity checks
        if (value < min) return min;
        if (value > max) return max;
        if (min > max) throw new ArgumentException(Properties.Resources.MinLargerMax, nameof(min));
        #endregion

        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
    #endregion

    #region Modulo
    /// <summary>
    /// Calculates a modulus (always positive).
    /// </summary>
    [Pure]
    public static double Modulo(this double dividend, double divisor)
    {
        double result = dividend % divisor;
        if (result < 0) result += divisor;
        return result;
    }

    /// <summary>
    /// Calculates a modulus (always positive).
    /// </summary>
    [Pure]
    public static float Modulo(this float dividend, float divisor)
    {
        float result = dividend % divisor;
        if (result < 0) result += divisor;
        return result;
    }

    /// <summary>
    /// Calculates a modulus (always positive).
    /// </summary>
    [Pure]
    public static int Modulo(this int dividend, int divisor)
    {
        int result = dividend % divisor;
        if (result < 0) result += divisor;
        return result;
    }
    #endregion

    #region Degree-Radian Conversion
    /// <summary>
    /// Converts an angle in degrees to radians
    /// </summary>
    /// <param name="value">The angle in degrees</param>
    /// <returns>The angle in radians</returns>
    [Pure]
    public static float DegreeToRadian(this float value) => value * ((float)Math.PI / 180);

    /// <summary>
    /// Converts an angle in degrees to radians
    /// </summary>
    /// <param name="value">The angle in degrees</param>
    /// <returns>The angle in radians</returns>
    [Pure]
    public static double DegreeToRadian(this double value) => value * (Math.PI / 180);

    /// <summary>
    /// Converts an angle in radians to degrees
    /// </summary>
    /// <param name="value">The angle in radians</param>
    /// <returns>The angle in degrees</returns>
    [Pure]
    public static float RadianToDegree(this float value) => value * (180 / (float)Math.PI);

    /// <summary>
    /// Converts an angle in radians to degrees
    /// </summary>
    /// <param name="value">The angle in radians</param>
    /// <returns>The angle in degrees</returns>
    [Pure]
    public static double RadianToDegree(this double value) => value * (180 / Math.PI);
    #endregion

    //--------------------//

    #region Interpolate
    /// <summary>
    /// Performs smooth (trigonometric) interpolation between two or more values
    /// </summary>
    /// <param name="factor">A factor between 0 and <paramref name="values"/>.Length</param>
    /// <param name="values">The value checkpoints</param>
    [Pure]
    public static double InterpolateTrigonometric(double factor, [NotNull] params double[] values)
    {
        #region Sanity checks
        if (values == null) throw new ArgumentNullException(nameof(values));
        if (values.Length < 2) throw new ArgumentException(Properties.Resources.AtLeast2Values);
        #endregion

        // Handle value overflows
        if (factor <= 0) return values[0];
        if (factor >= values.Length - 1) return values[values.Length - 1];

        // Isolate index shift from factor
        int index = (int)Math.Floor(factor);

        // Remove index shift from factor
        factor -= index;

        // Apply sinus smoothing to factor
        factor = -0.5 * Math.Cos(factor * Math.PI) + 0.5;

        return values[index] + factor * (values[index + 1] - values[index]);
    }
    #endregion

    #region Gauss
    /// <summary>
    /// Generates a Gaussian kernel.
    /// </summary>
    /// <param name="sigma">The standard deviation of the Gaussian distribution.</param>
    /// <param name="kernelSize">The size of the kernel. Should be an uneven number.</param>
    [Pure]
    public static double[] GaussKernel(double sigma, int kernelSize)
    {
        var kernel = new double[kernelSize];
        double sum = 0;
        for (int i = 0; i < kernel.Length; i++)
        {
            double x = i - kernelSize / 2;
            sum += kernel[i] = Math.Exp(-x * x / (2 * sigma * sigma));
        }

        // Normalize
        for (int i = 0; i < kernel.Length; i++)
            kernel[i] /= sum;

        return kernel;
    }
    #endregion

    //--------------------//

    #region High/low values
    [Pure]
    public static short LoWord(uint l)
    {
        unchecked
        {
            return (short)(l & 0xffff);
        }
    }

    [Pure]
    public static short HiWord(uint l)
    {
        unchecked
        {
            return (short)(l >> 16);
        }
    }

    [Pure] public static byte CombineHiLoByte(int high, int low) => (byte)((high << 4) + low);
    #endregion
}
