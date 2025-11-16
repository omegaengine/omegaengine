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
/// Contains test methods for <see cref="ByteVector4Grid"/>.
/// </summary>
public class ByteVector4GridTest
{
    [Fact]
    public void TestSaveLoad()
    {
        using var tempFile = new TemporaryFile("unit-tests");
        var grid = new ByteVector4Grid(new[,]
        {
            {new ByteVector4(0, 1, 2, 3), new ByteVector4(3, 2, 1, 0)},
            {new ByteVector4(0, 10, 20, 30), new ByteVector4(30, 20, 10, 0)}
        });
        grid.Save(tempFile);

        using (var stream = File.OpenRead(tempFile))
            grid = ByteVector4Grid.Load(stream);

        grid[0, 0].Should().Be(new ByteVector4(0, 1, 2, 3));
        grid[0, 1].Should().Be(new ByteVector4(3, 2, 1, 0));
        grid[1, 0].Should().Be(new ByteVector4(0, 10, 20, 30));
        grid[1, 1].Should().Be(new ByteVector4(30, 20, 10, 0));
    }
}
