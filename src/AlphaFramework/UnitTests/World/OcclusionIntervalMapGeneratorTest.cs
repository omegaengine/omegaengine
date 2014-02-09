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
            var generator = new OcclusionIntervalMapGenerator(new TerrainSize(4, 4), CreateTestHeightMap(), lightSourceInclination: 0);

            generator.RunSync();

            Assert.AreEqual(0, generator.Result[0, 0].X);
            Assert.AreEqual(63, generator.Result[0, 1].X);
            Assert.AreEqual(63, generator.Result[0, 2].X);
            Assert.AreEqual(0, generator.Result[0, 3].X);
            Assert.AreEqual(0, generator.Result[1, 0].X);
            Assert.AreEqual(0, generator.Result[1, 1].X);
            Assert.AreEqual(0, generator.Result[1, 2].X);
            Assert.AreEqual(0, generator.Result[1, 3].X);

            Assert.AreEqual(255, generator.Result[0, 0].Y);
            Assert.AreEqual(255, generator.Result[0, 1].Y);
            Assert.AreEqual(255, generator.Result[0, 2].Y);
            Assert.AreEqual(255, generator.Result[0, 3].Y);
            Assert.AreEqual(255, generator.Result[1, 0].Y);
            Assert.AreEqual(255, generator.Result[1, 1].Y);
            Assert.AreEqual(255, generator.Result[1, 2].Y);
            Assert.AreEqual(255, generator.Result[1, 3].Y);
            Assert.AreEqual(255, generator.Result[2, 0].Y);
            Assert.AreEqual(191, generator.Result[2, 1].Y);
            Assert.AreEqual(191, generator.Result[2, 2].Y);
            Assert.AreEqual(255, generator.Result[2, 3].Y);
            Assert.AreEqual(255, generator.Result[3, 0].Y);
            Assert.AreEqual(217, generator.Result[3, 1].Y);
            Assert.AreEqual(217, generator.Result[3, 2].Y);
            Assert.AreEqual(255, generator.Result[3, 3].Y);
        }
    }
}
