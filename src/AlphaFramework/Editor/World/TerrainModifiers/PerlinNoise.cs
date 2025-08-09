/*
 * AForge Math Library
 * AForge.NET framework
 * http://www.aforgenet.com/framework/
 *
 * Copyright © Andrew Kirillov, 2005-2009
 * andrew.kirillov@aforgenet.com
 * Modified by Bastian Eicher, 2011
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NanoByte.Common;
using OmegaEngine.Foundation;

namespace AlphaFramework.Editor.World.TerrainModifiers;

/// <summary>
/// Perlin noise function.
/// </summary>
///
/// <remarks><para>The class implements 1-D and 2-D Perlin noise functions, which represent
/// sum of several smooth noise functions with different frequency and amplitude. The description
/// of Perlin noise function and its calculation may be found on
/// <a href="http://freespace.virgin.net/hugo.elias/models/m_perlin.htm" target="_blank">Hugo Elias's page</a>.
/// </para>
///
/// <para>The number of noise functions, which comprise the resulting Perlin noise function, is
/// set by <see cref="Octaves"/> property. Amplitude and frequency values for each octave
/// start from values, which are set by <see cref="InitFrequency"/> and <see cref="InitAmplitude"/>
/// properties.</para>
///
/// <para>Sample usage (clouds effect):</para>
/// <code>
/// // create Perlin noise function
/// PerlinNoise noise = new PerlinNoise( 8, 0.5, 1.0 / 32 );
/// // generate clouds effect
/// float[,] texture = new float[height, width];
///
/// for ( int y = 0; y &lt; height; y++ )
/// {
/// 	for ( int x = 0; x &lt; width; x++ )
/// 	{
/// 		texture[y, x] =
/// 			Math.Max( 0.0f, Math.Min( 1.0f,
/// 				(float) noise.Function2D( x, y ) * 0.5f + 0.5f
/// 			) );
/// 	}
/// }
/// </code>
/// </remarks>
internal class PerlinNoise
{
    /// <summary>
    /// Initial frequency.
    /// </summary>
    ///
    /// <remarks><para>The property sets initial frequency of the first octave. Frequencies for
    /// next octaves are calculated using the next equation:<br />
    /// frequency<sub>i</sub> = <see cref="InitFrequency"/> * 2<sup>i</sup>,
    /// where i = [0, <see cref="Octaves"/>).
    /// </para>
    ///
    /// <para>Default value is set to <b>1</b>.</para>
    /// </remarks>
    public double InitFrequency { get; set; }

    /// <summary>
    /// Initial amplitude.
    /// </summary>
    ///
    /// <remarks><para>The property sets initial amplitude of the first octave. Amplitudes for
    /// next octaves are calculated using the next equation:<br />
    /// amplitude<sub>i</sub> = <see cref="InitAmplitude"/> * <see cref="Persistence"/><sup>i</sup>,
    /// where i = [0, <see cref="Octaves"/>).
    /// </para>
    ///
    /// <para>Default value is set to <b>1</b>.</para>
    /// </remarks>
    public double InitAmplitude { get; set; }

    /// <summary>
    /// Persistence value.
    /// </summary>
    ///
    /// <remarks><para>The property sets so called persistence value, which controls the way
    /// how <see cref="InitAmplitude">amplitude</see> is calculated for each octave comprising
    /// the Perlin noise function.</para>
    ///
    /// <para>Default value is set to <b>0.65</b>.</para>
    /// </remarks>
    public double Persistence { get; set; }

    private int _octaves = 4;

    /// <summary>
    /// Number of octaves, [1, 32].
    /// </summary>
    ///
    /// <remarks><para>The property sets the number of noise functions, which sum up the resulting
    /// Perlin noise function.</para>
    ///
    /// <para>Default value is set to <b>4</b>.</para>
    /// </remarks>
    public int Octaves { get => _octaves; set => _octaves = value.Clamp(1, 32); }

    /// <summary>
    /// Initializes a new instance of the <see cref="PerlinNoise"/> class.
    /// </summary>
    public PerlinNoise()
    {
        Persistence = 0.65;
        InitAmplitude = 1.0;
        InitFrequency = 1.0;
    }

    /// <summary>
    /// 1-D Perlin noise function.
    /// </summary>
    ///
    /// <param name="x">x value.</param>
    ///
    /// <returns>Returns function's value at point <paramref name="x"/>.</returns>
    public double Function(double x)
    {
        double frequency = InitFrequency;
        double amplitude = InitAmplitude;
        double sum = 0;

        // octaves
        for (int i = 0; i < _octaves; i++)
        {
            sum += SmoothedNoise(x * frequency) * amplitude;

            frequency *= 2;
            amplitude *= Persistence;
        }
        return sum;
    }

    /// <summary>
    /// 2-D Perlin noise function.
    /// </summary>
    ///
    /// <param name="x">x value.</param>
    /// <param name="y">y value.</param>
    ///
    /// <returns>Returns function's value at point (<paramref name="x"/>, <paramref name="y"/>).</returns>
    public double Function2D(double x, double y)
    {
        double frequency = InitFrequency;
        double amplitude = InitAmplitude;
        double sum = 0;

        // octaves
        for (int i = 0; i < _octaves; i++)
        {
            sum += SmoothedNoise(x * frequency, y * frequency) * amplitude;

            frequency *= 2;
            amplitude *= Persistence;
        }
        return sum;
    }

    private readonly int
        _seed1 = RandomUtils.GetRandomInt(7865, 23596),
        _seed2 = RandomUtils.GetRandomInt(394610, 1183831),
        _seed3 = RandomUtils.GetRandomInt(688156294, 2064468883);

    private readonly double _seed4 = RandomUtils.GetRandomDouble(536870912, 1610612736);

    /// <summary>
    /// Ordinary noise function
    /// </summary>
    private double Noise(int x)
    {
        int n = (x << 13) ^ x;

        unchecked
        {
            return (1.0 - ((n * (n * n * _seed1 + _seed2) + _seed3) & 0x7fffffff) / _seed4);
        }
    }

    private double Noise(int x, int y)
    {
        int n = x + y * 57;
        n = (n << 13) ^ n;

        unchecked
        {
            return (1.0 - ((n * (n * n * _seed1 + _seed2) + _seed3) & 0x7fffffff) / _seed4);
        }
    }

    /// <summary>
    /// Smoothed noise.
    /// </summary>
    private double SmoothedNoise(double x)
    {
        var xInt = (int)x;
        double xFrac = x - xInt;

        return MathUtils.InterpolateTrigonometric(xFrac, Noise(xInt), Noise(xInt + 1));
    }

    private double SmoothedNoise(double x, double y)
    {
        var xInt = (int)x;
        var yInt = (int)y;
        double xFrac = x - xInt;
        double yFrac = y - yInt;

        // get four noise values
        // ReSharper disable InconsistentNaming
        double x0y0 = Noise(xInt, yInt);
        double x1y0 = Noise(xInt + 1, yInt);
        double x0y1 = Noise(xInt, yInt + 1);
        double x1y1 = Noise(xInt + 1, yInt + 1);
        // ReSharper restore InconsistentNaming

        // x interpolation
        double v1 = MathUtils.InterpolateTrigonometric(xFrac, x0y0, x1y0);
        double v2 = MathUtils.InterpolateTrigonometric(xFrac, x0y1, x1y1);
        // y interpolation
        return MathUtils.InterpolateTrigonometric(yFrac, v1, v2);
    }
}
