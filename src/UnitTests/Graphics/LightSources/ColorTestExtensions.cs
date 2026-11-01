/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using AwesomeAssertions;

namespace OmegaEngine.Graphics.LightSources;

internal static class ColorTestExtensions
{
    /// <summary>
    /// Asserts that each channel of <paramref name="actual"/> is within one step of <paramref name="expected"/>, allowing for rounding of float results to bytes.
    /// </summary>
    public static void ShouldBeCloseTo(this Color actual, Color expected)
    {
        actual.A.Should().BeCloseTo(expected.A, 1);
        actual.R.Should().BeCloseTo(expected.R, 1, $"{actual} should match {expected}");
        actual.G.Should().BeCloseTo(expected.G, 1, $"{actual} should match {expected}");
        actual.B.Should().BeCloseTo(expected.B, 1, $"{actual} should match {expected}");
    }
}
