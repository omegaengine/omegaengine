/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using FluentAssertions;
using Xunit;

namespace OmegaEngine.Values
{
    /// <summary>
    /// Contains test methods for <see cref="ExpandableRectangleArray{T}"/>.
    /// </summary>
    public class ExpandableRectangleArrayTest
    {
        [Fact]
        public void TestGetArrayBase()
        {
            var expandableRectangleArray = new ExpandableRectangleArray<int>();
            expandableRectangleArray.AddLast(new Point(1, 1), new[,] {{5, 6}, {7, 8}});
            expandableRectangleArray.AddFirst(new Point(2, 2), new[,] {{1, 2}, {3, 4}});

            var result = expandableRectangleArray.GetArray(new[,] {{-1, -2, -3, -4}, {-5, -6, -7, -8}, {-9, -10, -11, -12}, {-13, -14, -15, -16}});
            result.Should().BeEquivalentTo(new[,] {{5, 6, -8}, {7, 8, 2}, {-14, 3, 4}});
            expandableRectangleArray.TotalArea.Should().BeEquivalentTo(new Rectangle(1, 1, 3, 3));
        }

        [Fact]
        public void TestGetArraySmallBase()
        {
            var expandableRectangleArray = new ExpandableRectangleArray<int>();
            expandableRectangleArray.AddLast(new Point(1, 1), new[,] {{5, 6}, {7, 8}});
            expandableRectangleArray.AddFirst(new Point(2, 2), new[,] {{1, 2}, {3, 4}});

            var result = expandableRectangleArray.GetArray(new[,] {{-1, -2, -3}, {-5, -6, -7}, {-9, -10, -11}, {-13, -14, -15}});
            result.Should().BeEquivalentTo(new[,] {{5, 6}, {7, 8}, {-14, 3}});
            expandableRectangleArray.TotalArea.Should().BeEquivalentTo(new Rectangle(1, 1, 3, 3));
        }

        [Fact]
        public void TestGetArrayNoBase()
        {
            var expandableRectangleArray = new ExpandableRectangleArray<int>();
            expandableRectangleArray.AddLast(new Point(1, 1), new[,] {{5, 6}, {7, 8}});
            expandableRectangleArray.AddFirst(new Point(2, 2), new[,] {{1, 2}, {3, 4}});

            var result = expandableRectangleArray.GetArray();
            result.Should().BeEquivalentTo(new[,] {{5, 6, 0}, {7, 8, 2}, {0, 3, 4}});
            expandableRectangleArray.TotalArea.Should().BeEquivalentTo(new Rectangle(1, 1, 3, 3));
        }

        [Fact]
        public void TestNegativeArea()
        {
            var expandableRectangleArray = new ExpandableRectangleArray<int>();
            expandableRectangleArray.AddLast(new Point(-1, -1), new[,] {{1, 2}, {1, 2}});

            var result = expandableRectangleArray.GetArray();
            result.Should().BeEquivalentTo(new[,] {{2}});
            expandableRectangleArray.TotalArea.Should().BeEquivalentTo(new Rectangle(0, 0, 1, 1));
        }
    }
}
