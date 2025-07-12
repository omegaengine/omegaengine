/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using AlphaFramework.World.Terrains;
using FluentAssertions;
using NUnit.Framework;
using OmegaEngine.Values;

namespace AlphaFramework.World
{
    /// <summary>
    /// Contains test methods for <see cref="OcclusionIntervalMapGenerator"/>.
    /// </summary>
    [TestFixture]
    public class OcclusionIntervalMapGeneratorTest
    {
        #region Suppress parallelization
        [SetUp]
        public void SetUp()
        {
            Parallel.ThreadsCount = 1;
        }

        [TearDown]
        public void TearDown()
        {
            Parallel.ThreadsCount = 0;
        }
        #endregion

        [Test]
        public void TestMinimal1()
        {
            var generator = new OcclusionIntervalMapGenerator(new ByteGrid(new byte[,] {{0}, {0}, {1}}));
            generator.Run();

            generator.Result[0, 0].Should().Be(new ByteVector4(38, 255, 255, 255));
            generator.Result[1, 0].Should().Be(new ByteVector4(64, 255, 255, 255));
            generator.Result[2, 0].Should().Be(new ByteVector4(0, 255, 255, 255));
        }

        [Test]
        public void TestMinimal2()
        {
            var generator = new OcclusionIntervalMapGenerator(new ByteGrid(new byte[,] {{1, 0}, {1, 0}, {2, 1}}));
            generator.Run();

            generator.Result[0, 0].Should().Be(new ByteVector4(38, 255, 255, 255));
            generator.Result[0, 1].Should().Be(new ByteVector4(38, 255, 255, 255));
            generator.Result[1, 0].Should().Be(new ByteVector4(64, 255, 255, 255));
            generator.Result[1, 1].Should().Be(new ByteVector4(64, 255, 255, 255));
            generator.Result[2, 0].Should().Be(new ByteVector4(0, 255, 255, 255));
            generator.Result[2, 1].Should().Be(new ByteVector4(0, 255, 255, 255));
        }

        [Test]
        public void TestMinimal3()
        {
            var generator = new OcclusionIntervalMapGenerator(new ByteGrid(new byte[,] {{0, 0}, {0, 0}, {1, 225}}));
            generator.Run();

            generator.Result[0, 0].Should().Be(new ByteVector4(38, 255, 255, 255));
            generator.Result[0, 1].Should().Be(new ByteVector4(127, 255, 255, 255));
            generator.Result[1, 0].Should().Be(new ByteVector4(64, 255, 255, 255));
            generator.Result[1, 1].Should().Be(new ByteVector4(128, 255, 255, 255));
            generator.Result[2, 0].Should().Be(new ByteVector4(0, 255, 255, 255));
            generator.Result[2, 1].Should().Be(new ByteVector4(0, 255, 255, 255));
        }

        [Test]
        public void TestStretchV()
        {
            var generator = new OcclusionIntervalMapGenerator(new ByteGrid(new byte[,] {{0}, {0}, {1}}), stretchV: 2);
            generator.Run();

            generator.Result[0, 0].Should().Be(new ByteVector4(64, 255, 255, 255));
            generator.Result[1, 0].Should().Be(new ByteVector4(90, 255, 255, 255));
            generator.Result[2, 0].Should().Be(new ByteVector4(0, 255, 255, 255));
        }

        [Test]
        public void TestStretchH()
        {
            var generator = new OcclusionIntervalMapGenerator(new ByteGrid(new byte[,] {{0}, {0}, {1}}), stretchH: 2);
            generator.Run();

            generator.Result[0, 0].Should().Be(new ByteVector4(20, 255, 255, 255));
            generator.Result[1, 0].Should().Be(new ByteVector4(38, 255, 255, 255));
            generator.Result[2, 0].Should().Be(new ByteVector4(0, 255, 255, 255));
        }
    }
}
