/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.IO;
using AwesomeAssertions;
using NanoByte.Common.Storage;
using Xunit;

namespace OmegaEngine.Graphics.Shaders;

public class ShaderCacheTest
{
    private const string Source = "float4 main() : COLOR { return 1; }";
    private static readonly Dictionary<string, string> Defines = new() {["A"] = "1", ["B"] = "2"};

    private int _compileCount;

    private byte[] Compile()
    {
        _compileCount++;
        return [1, 2, 3, (byte)_compileCount];
    }

    [Fact]
    public void CompilesOnlyOnceAcrossInstances()
    {
        using var tempDir = new TemporaryDirectory("omegaengine-unit-tests");

        var first = new ShaderCache(tempDir).GetOrAdd(Source, Defines, Compile);
        var second = new ShaderCache(tempDir).GetOrAdd(Source, Defines, Compile);

        second.Should().Equal(first);
        _compileCount.Should().Be(1);
    }

    [Fact]
    public void IgnoresOrderOfDefines()
    {
        using var tempDir = new TemporaryDirectory("omegaengine-unit-tests");
        var cache = new ShaderCache(tempDir);

        cache.GetOrAdd(Source, new Dictionary<string, string> {["A"] = "1", ["B"] = "2"}, Compile);
        cache.GetOrAdd(Source, new Dictionary<string, string> {["B"] = "2", ["A"] = "1"}, Compile);

        _compileCount.Should().Be(1);
    }

    [Fact]
    public void SeparatesEntriesBySourceAndDefines()
    {
        using var tempDir = new TemporaryDirectory("omegaengine-unit-tests");
        var cache = new ShaderCache(tempDir);

        cache.GetOrAdd(Source, Defines, Compile);
        cache.GetOrAdd(Source + " ", Defines, Compile);
        cache.GetOrAdd(Source, new Dictionary<string, string> {["A"] = "1", ["B"] = "3"}, Compile);

        _compileCount.Should().Be(3);
    }

    [Fact]
    public void RecompilesCorruptedEntry()
    {
        using var tempDir = new TemporaryDirectory("omegaengine-unit-tests");
        var cache = new ShaderCache(tempDir);
        cache.GetOrAdd(Source, Defines, Compile);

        string entry = Directory.GetFiles(tempDir).Should().ContainSingle().Subject;
        var data = File.ReadAllBytes(entry);
        data[data.Length - 1] ^= 0xFF;
        File.WriteAllBytes(entry, data);

        cache.GetOrAdd(Source, Defines, Compile).Should().Equal(1, 2, 3, 2);
        _compileCount.Should().Be(2);
        new ShaderCache(tempDir).GetOrAdd(Source, Defines, Compile).Should().Equal([1, 2, 3, 2], "the corrupted entry should have been replaced");
    }

    [Fact]
    public void StillCompilesIfDirectoryIsUnusable()
    {
        using var tempDir = new TemporaryDirectory("omegaengine-unit-tests");
        string blocked = Path.Combine(tempDir, "file");
        File.WriteAllText(blocked, "not a directory");
        var cache = new ShaderCache(blocked);

        cache.GetOrAdd(Source, Defines, Compile).Should().Equal(1, 2, 3, 1);
        cache.GetOrAdd(Source, Defines, Compile).Should().Equal(1, 2, 3, 2);
    }
}
