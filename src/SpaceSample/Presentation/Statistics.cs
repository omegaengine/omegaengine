/*
 * Copyright 2006-2013 Bastian Eicher
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
using System.Collections.Generic;
using System.Xml.Serialization;
using Common.Values;
using World;

namespace Presentation
{
    /// <summary>
    /// Represents a set of <see cref="TestCase"/>s that can be executed, recorded and serialized.
    /// </summary>
    [XmlRoot("statistics")]
    public class Statistics
    {
        /// <summary>
        /// The version number of the game.
        /// </summary>
        [XmlAttribute("game-version")]
        public string GameVersion { get; set; }

        /// <summary>
        /// The version number of the engine.
        /// </summary>
        [XmlAttribute("engine-version")]
        public string EngineVersion { get; set; }

        /// <summary>
        /// The set of <see cref="TestCase"/>s.
        /// </summary>
        [XmlElement("test-case")]
        public TestCase[] TestCases;

        #region Constructor
        /// <summary>
        /// Base-constructor for XML serialization. Do not call manually!
        /// </summary>
        public Statistics()
        {}

        /// <summary>
        /// Creates a set of <see cref="TestCase"/>s based on <see cref="BenchmarkPoint"/>s in a <see cref="Universe"/>.
        /// </summary>
        /// <param name="gameVersion">The version number of the game.</param>
        /// <param name="engineVersion">The version number of the engine.</param>
        /// <param name="universe">The <see cref="Universe"/> containing the <see cref="BenchmarkPoint"/>s.</param>
        public Statistics(string gameVersion, string engineVersion, Universe universe)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(gameVersion)) throw new ArgumentNullException("gameVersion");
            if (string.IsNullOrEmpty(engineVersion)) throw new ArgumentNullException("engineVersion");
            if (universe == null) throw new ArgumentNullException("universe");
            #endregion

            GameVersion = gameVersion;
            EngineVersion = engineVersion;

            var testCaseList = new List<TestCase>();

            #region Read benchmark points from settings
            foreach (var entity in universe.Positionables)
            {
                var target = entity as BenchmarkPoint;
                if (target == null) continue;

                // Handle all possible settings combinations
                for (int i = 0; i < TestCase.TestGraphicsSettingsUpperBound; i++)
                {
                    testCaseList.Add(new TestCase
                    {
                        Target = target,
                        GraphicsSettings = ((TestGraphicsSettings)i),
                        // Create screenshots only for "no effects" and for "all effects"
                        Screenshot = (i == 0 || i == 7)
                    });
                }

                if (target.TestWater)
                {
                    #region Water refraction only
                    for (int i = 0; i < 8; i++)
                    {
                        testCaseList.Add(new TestCase
                        {
                            Target = target,
                            GraphicsSettings = ((TestGraphicsSettings)i),
                            // Create screenshots only for "no effects" and for "all effects"
                            Screenshot = (i == 0 || i == 7),
                            WaterEffects = WaterEffectsType.RefractionOnly
                        });
                    }
                    #endregion

                    #region Water reflect all
                    for (int i = 0; i < 8; i++)
                    {
                        testCaseList.Add(new TestCase
                        {
                            Target = target,
                            GraphicsSettings = ((TestGraphicsSettings)i),
                            // Create screenshots only for "no effects" and for "all effects"
                            Screenshot = (i == 0 || i == 7),
                            WaterEffects = WaterEffectsType.ReflectAll
                        });
                    }
                    #endregion
                }

                if (target.TestParticleSystem)
                {
                    #region Particle-system high quality
                    for (int i = 0; i < 8; i++)
                    {
                        testCaseList.Add(new TestCase
                        {
                            Target = target,
                            GraphicsSettings = ((TestGraphicsSettings)i),
                            // Create screenshots only for "no effects" and for "all effects"
                            Screenshot = (i == 0 || i == 7),
                            ParticleSystemQuality = Quality.High
                        });
                    }
                    #endregion
                }
            }
            #endregion

            #region Create High-res and anti-aliasing variations
            // Copy the test-cases to an array four times
            TestCases = new TestCase[testCaseList.Count * 4];
            testCaseList.CopyTo(TestCases);
            testCaseList.CopyTo(TestCases, testCaseList.Count);
            testCaseList.CopyTo(TestCases, testCaseList.Count * 2);
            testCaseList.CopyTo(TestCases, testCaseList.Count * 3);

            // Create the test-case variations block-wise
            for (int i = 0; i < testCaseList.Count; i++)
            {
                TestCases[i].HighRes = false;
                TestCases[i].AntiAliasing = false;
                TestCases[i].Screenshot = false;
            }
            for (int i = testCaseList.Count; i < testCaseList.Count * 2; i++)
            {
                TestCases[i].HighRes = false;
                TestCases[i].AntiAliasing = true;
                // Screenshot is left on if it was already on
            }
            for (int i = testCaseList.Count * 2; i < testCaseList.Count * 3; i++)
            {
                TestCases[i].HighRes = true;
                TestCases[i].AntiAliasing = false;
                TestCases[i].Screenshot = false;
            }
            for (int i = testCaseList.Count * 3; i < testCaseList.Count * 4; i++)
            {
                TestCases[i].HighRes = true;
                TestCases[i].AntiAliasing = true;
                TestCases[i].Screenshot = false;
            }
            #endregion
        }
        #endregion
    }
}
