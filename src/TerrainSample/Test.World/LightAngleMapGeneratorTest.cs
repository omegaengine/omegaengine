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

using Common.Tasks;
using NUnit.Framework;
using World.Terrains;

namespace World
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
