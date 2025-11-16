/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using FluentAssertions;
using NanoByte.Common.Storage;
using Xunit;

namespace OmegaEngine.Foundation.Geometry;

/// <summary>
/// Contains test methods for <see cref="NibbleGrid"/>.
/// </summary>
public class NibbleGridTest
{
    [Fact]
    public void TestSaveLoad()
    {
        using var tempFile = new TemporaryFile("unit-tests");
        var grid = new NibbleGrid(new byte[,] {{2, 4}, {5, 10}});
        grid.Save(tempFile);

        using (var stream = File.OpenRead(tempFile))
            grid = NibbleGrid.Load(stream);

        grid[0, 0].Should().Be(2);
        grid[0, 1].Should().Be(4);
        grid[1, 0].Should().Be(5);
        grid[1, 1].Should().Be(10);
    }
}
