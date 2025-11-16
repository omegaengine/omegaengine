/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using FluentAssertions;
using ICSharpCode.SharpZipLib.Zip;
using NanoByte.Common.Storage;
using Xunit;

namespace OmegaEngine.Foundation.Storage;

/// <summary>
/// Contains test methods for <see cref="XmlZipStorage"/>.
/// </summary>
public class XmlZipStorageTest
{
    // ReSharper disable MemberCanBePrivate.Global
    /// <summary>
    /// A data-structure used to test serialization.
    /// </summary>
    [XmlNamespace("", "")]
    public class TestData
    {
        public string Data { get; set; } = null!;
    }
    // ReSharper restore MemberCanBePrivate.Global

    /// <summary>
    /// Ensures <see cref="XmlZipStorage.SaveXmlZip{T}(T,string,string,EmbeddedFile[])"/> and <see cref="XmlZipStorage.LoadXmlZip{T}(string,string,EmbeddedFile[])"/> work correctly with no password.
    /// </summary>
    [Fact]
    public void TestZipNoPassword()
    {
        // Write and read file
        var testData1 = new TestData { Data = "Hello" };
        var tempStream = new MemoryStream();
        testData1.SaveXmlZip(tempStream);
        tempStream.Seek(0, SeekOrigin.Begin);
        var testData2 = XmlZipStorage.LoadXmlZip<TestData>(tempStream);

        // Ensure data stayed the same
        testData2.Data.Should().Be(testData1.Data);
    }

    /// <summary>
    /// Ensures <see cref="XmlZipStorage.SaveXmlZip{T}(T,string,string,EmbeddedFile[])"/> and <see cref="XmlZipStorage.LoadXmlZip{T}(string,string,EmbeddedFile[])"/> work correctly with a password.
    /// </summary>
    [Fact]
    public void TestZipPassword()
    {
        // Write and read file
        var testData1 = new TestData { Data = "Hello" };
        var tempStream = new MemoryStream();
        testData1.SaveXmlZip(tempStream, "Test password");
        tempStream.Seek(0, SeekOrigin.Begin);
        var testData2 = XmlZipStorage.LoadXmlZip<TestData>(tempStream, password: "Test password");

        // Ensure data stayed the same
        testData2.Data.Should().Be(testData1.Data);
    }

    /// <summary>
    /// Ensures <see cref="XmlZipStorage.LoadXmlZip{T}(string,string,EmbeddedFile[])"/> correctly detects incorrect passwords.
    /// </summary>
    [Fact]
    public void TestIncorrectPassword()
    {
        var tempStream = new MemoryStream();
        var testData = new TestData { Data = "Hello" };
        testData.SaveXmlZip(tempStream, "Correct password");
        tempStream.Seek(0, SeekOrigin.Begin);
        Assert.Throws<ZipException>(() => XmlZipStorage.LoadXmlZip<TestData>(tempStream, password: "Wrong password"));
    }
}
