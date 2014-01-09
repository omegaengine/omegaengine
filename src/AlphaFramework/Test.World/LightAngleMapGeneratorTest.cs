/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using AlphaFramework.World.Terrains;
using Common.Tasks;
using NUnit.Framework;

namespace AlphaFramework.World
{
    /// <summary>
    /// Contains test methods for <see cref="LightAngleMapGenerator"/>.
    /// </summary>
    [TestFixture]
    public class LightAngleMapGeneratorTest
    {
        #region Helpers
        /// <summary>
        /// Creates a height-map for testing purposes.
        /// </summary>
        public static byte[,] CreateTestHeightMap()
        {
            return new byte[,]
            {
                {0, 0, 0, 0},
                {0, 1, 1, 0},
                {0, 0, 0, 0},
                {0, 0, 0, 0}
            };
        }
        #endregion

        /// <summary>
        /// Ensures the generator provides correct results when called via <see cref="ThreadTask.RunSync"/>
        /// </summary>
        [Test]
        public void TestRunSync()
        {
            var generator = new LightAngleMapGenerator(new TerrainSize(4, 4), CreateTestHeightMap());

            generator.RunSync();

            Assert.AreEqual(0, generator.LightRiseAngleMap[0, 0]);
            Assert.AreEqual(127, generator.LightRiseAngleMap[0, 1]);
            Assert.AreEqual(127, generator.LightRiseAngleMap[0, 2]);
            Assert.AreEqual(0, generator.LightRiseAngleMap[0, 3]);
            Assert.AreEqual(0, generator.LightRiseAngleMap[1, 0]);
            Assert.AreEqual(0, generator.LightRiseAngleMap[1, 1]);
            Assert.AreEqual(0, generator.LightRiseAngleMap[1, 2]);
            Assert.AreEqual(0, generator.LightRiseAngleMap[1, 3]);

            Assert.AreEqual(255, generator.LightSetAngleMap[0, 0]);
            Assert.AreEqual(255, generator.LightSetAngleMap[0, 1]);
            Assert.AreEqual(255, generator.LightSetAngleMap[0, 2]);
            Assert.AreEqual(255, generator.LightSetAngleMap[0, 3]);
            Assert.AreEqual(255, generator.LightSetAngleMap[1, 0]);
            Assert.AreEqual(255, generator.LightSetAngleMap[1, 1]);
            Assert.AreEqual(255, generator.LightSetAngleMap[1, 2]);
            Assert.AreEqual(255, generator.LightSetAngleMap[1, 3]);
            Assert.AreEqual(255, generator.LightSetAngleMap[2, 0]);
            Assert.AreEqual(127, generator.LightSetAngleMap[2, 1]);
            Assert.AreEqual(127, generator.LightSetAngleMap[2, 2]);
            Assert.AreEqual(255, generator.LightSetAngleMap[2, 3]);
            Assert.AreEqual(255, generator.LightSetAngleMap[3, 0]);
            Assert.AreEqual(179, generator.LightSetAngleMap[3, 1]);
            Assert.AreEqual(179, generator.LightSetAngleMap[3, 2]);
            Assert.AreEqual(255, generator.LightSetAngleMap[3, 3]);
        }
    }
}
