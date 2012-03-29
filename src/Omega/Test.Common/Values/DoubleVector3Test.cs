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

using System;
using NUnit.Framework;
using SlimDX;

namespace Common.Values
{
    /// <summary>
    /// Contains test methods for <see cref="DoubleVector3"/>.
    /// </summary>
    [TestFixture]
    public class DoubleVector3Test
    {
        [Test]
        public void TestAdd()
        {
            Assert.AreEqual(new DoubleVector3(4, 4, 4), new DoubleVector3(1, 2, 3) + new DoubleVector3(3, 2, 1));
            Assert.AreEqual(new DoubleVector3(4, 4, 4), new DoubleVector3(1, 2, 3) + new Vector3(3, 2, 1));
            Assert.AreEqual(new DoubleVector3(4, 4, 4), new Vector3(1, 2, 3) + new DoubleVector3(3, 2, 1));
        }

        [Test]
        public void TestSubtract()
        {
            Assert.AreEqual(new DoubleVector3(1, 2, 3), new DoubleVector3(4, 4, 4) - new DoubleVector3(3, 2, 1));
            Assert.AreEqual(new DoubleVector3(1, 2, 3), new DoubleVector3(4, 4, 4) - new Vector3(3, 2, 1));
            Assert.AreEqual(new DoubleVector3(1, 2, 3), new Vector3(4, 4, 4) - new DoubleVector3(3, 2, 1));
        }

        [Test]
        public void TestApplyOffset()
        {
            Assert.AreEqual(new Vector3(1, 2, 3), new DoubleVector3(4, 4, 4).ApplyOffset(new DoubleVector3(3, 2, 1)));
        }

        [Test]
        public void TestMultiply()
        {
            Assert.AreEqual(new DoubleVector3(2, 4, 6), 2f * new DoubleVector3(1, 2, 3));
            Assert.AreEqual(new DoubleVector3(2, 4, 6), 2d * new DoubleVector3(1, 2, 3));
        }

        [Test]
        public void TestDotProduct()
        {
            Assert.AreEqual(10, new DoubleVector3(1, 2, 3).DotProduct(new DoubleVector3(3, 2, 1)));
        }

        [Test]
        public void TestLength()
        {
            Assert.AreEqual(Math.Sqrt(12), new DoubleVector3(2, 2, 2).Length(), 0.0000000001);
        }
    }
}
