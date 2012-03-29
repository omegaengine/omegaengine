/*
 * Copyright 2006-2012 Bastian Eicher
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

using NUnit.Framework;

namespace Common.Utils
{
    /// <summary>
    /// Contains test methods for <see cref="MathUtils"/>.
    /// </summary>
    [TestFixture]
    public class MathUtilsTest
    {
        /// <summary>
        /// Ensures all maximum determining functions work correctly.
        /// </summary>
        [Test]
        public void TestMax()
        {
            Assert.AreEqual(5, MathUtils.Max((byte)2, (byte)3, (byte)1, (byte)5));
            Assert.AreEqual(5, MathUtils.Max(2, 3, 1, 5));
            Assert.AreEqual(5L, MathUtils.Max(2L, 3L, 1L, 5L));
            Assert.AreEqual(5L, MathUtils.Max(2f, 3f, 1f, 5f));
            Assert.AreEqual(5L, MathUtils.Max(2d, 3d, 1d, 5d));
            Assert.AreEqual(5L, MathUtils.Max(2d, 3d, 1d, 5d));
            Assert.AreEqual(5, MathUtils.Max((decimal)2, (decimal)3, (decimal)1, (decimal)5));
        }

        /// <summary>
        /// Ensures all minimum determining functions work correctly.
        /// </summary>
        [Test]
        public void TestMin()
        {
            Assert.AreEqual(1, MathUtils.Min((byte)2, (byte)3, (byte)1, (byte)5));
            Assert.AreEqual(1, MathUtils.Min(2, 3, 1, 5));
            Assert.AreEqual(1L, MathUtils.Min(2L, 3L, 1L, 5L));
            Assert.AreEqual(1L, MathUtils.Min(2f, 3f, 1f, 5f));
            Assert.AreEqual(1L, MathUtils.Min(2d, 3d, 1d, 5d));
            Assert.AreEqual(1L, MathUtils.Min(2d, 3d, 1d, 5d));
            Assert.AreEqual(1, MathUtils.Min((decimal)2, (decimal)3, (decimal)1, (decimal)5));
        }
    }
}
