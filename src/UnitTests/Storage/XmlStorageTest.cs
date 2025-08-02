/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NanoByte.Common.Storage;
using FluentAssertions;
using Xunit;

namespace OmegaEngine.Storage
{
    /// <summary>
    /// Contains test methods for <see cref="NanoByte.Common.Storage.XmlStorage"/>.
    /// </summary>
    public class XmlStorageTest
    {
        // ReSharper disable MemberCanBePrivate.Global
        /// <summary>
        /// A data-structure used to test serialization.
        /// </summary>
        [XmlNamespace("", "")]
        public class TestData
        {
            public string Data { get; set; }
        }
        // ReSharper restore MemberCanBePrivate.Global

        /// <summary>
        /// Ensures <see cref="NanoByte.Common.Storage.XmlStorage.SaveXml{T}(T,string,string)"/> and <see cref="NanoByte.Common.Storage.XmlStorage.LoadXml{T}(string)"/> work correctly.
        /// </summary>
        [Fact]
        public void TestFile()
        {
            TestData testData1 = new() {Data = "Hello"}, testData2;
            using (var tempFile = new TemporaryFile("unit-tests"))
            {
                // Write and read file
                NanoByte.Common.Storage.XmlStorage.SaveXml(testData1, tempFile);
                testData2 = NanoByte.Common.Storage.XmlStorage.LoadXml<TestData>(tempFile);
            }

            // Ensure data stayed the same
            testData2.Data.Should().Be(testData1.Data);
        }

        /// <summary>
        /// Ensures <see cref="NanoByte.Common.Storage.XmlStorage.SaveXml{T}(T,string,string)"/> and <see cref="NanoByte.Common.Storage.XmlStorage.LoadXml{T}(string)"/> work correctly with relative paths.
        /// </summary>
        [Fact]
        public void TestFileRelative()
        {
            TestData testData1 = new() {Data = "Hello"}, testData2;
            using (new TemporaryWorkingDirectory("unit-tests"))
            {
                // Write and read file
                NanoByte.Common.Storage.XmlStorage.SaveXml(testData1, "file.xml");
                testData2 = NanoByte.Common.Storage.XmlStorage.LoadXml<TestData>("file.xml");
            }

            // Ensure data stayed the same
            testData2.Data.Should().Be(testData1.Data);
        }

        [Fact]
        public void TestToXmlString()
            => NanoByte.Common.Storage.XmlStorage.ToXmlString(new TestData {Data = "Hello"}).Should().Be("<?xml version=\"1.0\"?>\n<TestData>\n  <Data>Hello</Data>\n</TestData>\n");

        [Fact]
        public void TestFromXmlString()
            => NanoByte.Common.Storage.XmlStorage.FromXmlString<TestData>("<?xml version=\"1.0\"?><TestData><Data>Hello</Data></TestData>").Data.Should().Be("Hello");

#if SLIMDX
        /// <summary>
        /// Ensures <see cref="XmlStorage.SaveXmlZip{T}(T,string,string,EmbeddedFile[])"/> and <see cref="XmlStorage.LoadXmlZip{T}(string,string,EmbeddedFile[])"/> work correctly with no password.
        /// </summary>
        [Fact]
        public void TestZipNoPassword()
        {
            // Write and read file
            var testData1 = new TestData {Data = "Hello"};
            var tempStream = new MemoryStream();
            testData1.SaveXmlZip(tempStream);
            tempStream.Seek(0, SeekOrigin.Begin);
            var testData2 = XmlStorage.LoadXmlZip<TestData>(tempStream);

            // Ensure data stayed the same
            testData2.Data.Should().Be(testData1.Data);
        }

        /// <summary>
        /// Ensures <see cref="XmlStorage.SaveXmlZip{T}(T,string,string,EmbeddedFile[])"/> and <see cref="XmlStorage.LoadXmlZip{T}(string,string,EmbeddedFile[])"/> work correctly with a password.
        /// </summary>
        [Fact]
        public void TestZipPassword()
        {
            // Write and read file
            var testData1 = new TestData {Data = "Hello"};
            var tempStream = new MemoryStream();
            testData1.SaveXmlZip(tempStream, "Test password");
            tempStream.Seek(0, SeekOrigin.Begin);
            var testData2 = XmlStorage.LoadXmlZip<TestData>(tempStream, password: "Test password");

            // Ensure data stayed the same
            testData2.Data.Should().Be(testData1.Data);
        }

        /// <summary>
        /// Ensures <see cref="XmlStorage.LoadXmlZip{T}(string,string,EmbeddedFile[])"/> correctly detects incorrect passwords.
        /// </summary>
        [Fact]
        public void TestIncorrectPassword()
        {
            var tempStream = new MemoryStream();
            var testData = new TestData {Data = "Hello"};
            testData.SaveXmlZip(tempStream, "Correct password");
            tempStream.Seek(0, SeekOrigin.Begin);
            Assert.Throws<ZipException>(() => XmlStorage.LoadXmlZip<TestData>(tempStream, password: "Wrong password"));
        }
#endif
    }
}
