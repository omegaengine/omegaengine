/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using AlphaFramework.World.Terrains;
using Common.Tasks;
using Common.Values;
using NUnit.Framework;

namespace AlphaFramework.World
{
    /// <summary>
    /// Contains test methods for <see cref="OcclusionIntervalMapGenerator"/>.
    /// </summary>
    [TestFixture]
    public class OcclusionIntervalMapGeneratorTest
    {
        #region Helpers
        /// <summary>
        /// Creates a height-map for testing purposes.
        /// </summary>
        private static ByteGrid CreateTestHeightMap()
        {
            return new ByteGrid(new byte[,]
            {
                {0, 0, 0, 0},
                {0, 1, 1, 0},
                {0, 0, 0, 0},
                {0, 0, 0, 0}
            });
        }
        #endregion

        /// <summary>
        /// Ensures the generator provides correct results when called via <see cref="ThreadTask.RunSync"/>
        /// </summary>
        [Test]
        public void TestRunSync()
        {
            var generator = new OcclusionIntervalMapGenerator(new TerrainSize(4, 4), CreateTestHeightMap(), sunInclination: 0);

            generator.RunSync();

            Assert.AreEqual(0, generator.OcclusionEndMap[0, 0]);
            Assert.AreEqual(127, generator.OcclusionEndMap[0, 1]);
            Assert.AreEqual(127, generator.OcclusionEndMap[0, 2]);
            Assert.AreEqual(0, generator.OcclusionEndMap[0, 3]);
            Assert.AreEqual(0, generator.OcclusionEndMap[1, 0]);
            Assert.AreEqual(0, generator.OcclusionEndMap[1, 1]);
            Assert.AreEqual(0, generator.OcclusionEndMap[1, 2]);
            Assert.AreEqual(0, generator.OcclusionEndMap[1, 3]);

            Assert.AreEqual(255, generator.OcclusionBeginMap[0, 0]);
            Assert.AreEqual(255, generator.OcclusionBeginMap[0, 1]);
            Assert.AreEqual(255, generator.OcclusionBeginMap[0, 2]);
            Assert.AreEqual(255, generator.OcclusionBeginMap[0, 3]);
            Assert.AreEqual(255, generator.OcclusionBeginMap[1, 0]);
            Assert.AreEqual(255, generator.OcclusionBeginMap[1, 1]);
            Assert.AreEqual(255, generator.OcclusionBeginMap[1, 2]);
            Assert.AreEqual(255, generator.OcclusionBeginMap[1, 3]);
            Assert.AreEqual(255, generator.OcclusionBeginMap[2, 0]);
            Assert.AreEqual(127, generator.OcclusionBeginMap[2, 1]);
            Assert.AreEqual(127, generator.OcclusionBeginMap[2, 2]);
            Assert.AreEqual(255, generator.OcclusionBeginMap[2, 3]);
            Assert.AreEqual(255, generator.OcclusionBeginMap[3, 0]);
            Assert.AreEqual(179, generator.OcclusionBeginMap[3, 1]);
            Assert.AreEqual(179, generator.OcclusionBeginMap[3, 2]);
            Assert.AreEqual(255, generator.OcclusionBeginMap[3, 3]);
        }
    }
}
