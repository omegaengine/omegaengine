/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Xml.Serialization;
using AlphaFramework.Presentation.Config;
using FrameOfReference.World.Positionables;
using OmegaEngine.Foundation.Light;
using SlimDX;

namespace FrameOfReference.Presentation;

#region Enumerations
/// <summary>
/// Boolean flags for <see cref="SettingsBase.Graphics"/>
/// </summary>
/// <seealso cref="TestCase.GraphicsSettings"/>
[Flags]
public enum TestGraphicsSettings
{
    /// <summary>Activate anisotropic texture filtering.</summary>
    Anisotropic = 1,

    /// <summary>Activate terrain texture double sampling.</summary>
    DoubleSampling = 2,

    /// <summary>Activate post-screen effects.</summary>
    PostScreenEffects = 4
}
#endregion

/// <summary>
/// A specific test-case (with target and graphics settings) to profile.
/// </summary>
public struct TestCase
{
    /// <summary>
    /// The camera settings to use.
    /// </summary>
    public BenchmarkPoint<Vector2> Target;

    /// <summary>
    /// Boolean flags for <see cref="SettingsBase.Graphics"/>.
    /// </summary>
    public TestGraphicsSettings GraphicsSettings;

    /// <summary>
    /// The exclusive upper bound for values valid for <see cref="GraphicsSettings"/>.
    /// </summary>
    public const int TestGraphicsSettingsUpperBound = 8;

    /// <summary>
    /// What kind of effects to display on water (e.g. reflections).
    /// </summary>
    public WaterEffectsType WaterEffects;

    /// <summary>
    /// Run this test-case with twice the default benchmark resolution.
    /// </summary>
    [XmlAttribute("high-res")]
    public bool HighRes;

    /// <summary>
    /// Run this test-case with 2x anti-aliasing.
    /// </summary>
    [XmlAttribute("anti-aliasing")]
    public bool AntiAliasing;

    /// <summary>
    /// Create a screenshot of the rendering of this test-case?
    /// </summary>
    [XmlAttribute("screenshot")]
    public bool Screenshot;

    /// <summary>
    /// The average "frames per second".
    /// </summary>
    [XmlAttribute("average-fps")]
    public float AverageFps;

    /// <summary>
    /// The average "milliseconds per frames".
    /// </summary>
    [XmlAttribute("average-frame-ms")]
    public float AverageFrameMs;
}
