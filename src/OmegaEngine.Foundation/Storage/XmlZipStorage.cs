/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using NanoByte.Common;
using NanoByte.Common.Storage;

namespace OmegaEngine.Foundation.Storage;

/// <summary>
/// Provides easy serialization to XML files wrapped in ZIP archives.
/// </summary>
public static class XmlZipStorage
{
    /// <summary>
    /// Loads an object from an XML file embedded in a ZIP archive.
    /// </summary>
    /// <typeparam name="T">The type of object the XML stream shall be converted into.</typeparam>
    /// <param name="stream">The ZIP archive to load.</param>
    /// <param name="password">The password to use for decryption; <c>null</c> for no encryption.</param>
    /// <param name="additionalFiles">Additional files stored alongside the XML file in the ZIP archive to be read.</param>
    /// <returns>The loaded object.</returns>
    /// <exception cref="ZipException">A problem occurred while reading the ZIP data or if <paramref name="password"/> is wrong.</exception>
    /// <exception cref="InvalidDataException">A problem occurred while deserializing the XML data.</exception>
    public static T LoadXmlZip<T>(Stream stream, [Localizable(false)] string? password = null, params EmbeddedFile[] additionalFiles)
        where T : class
    {
        #region Sanity checks
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        if (additionalFiles == null) throw new ArgumentNullException(nameof(additionalFiles));
        #endregion

        T? output = null;

        using (var zipFile = new ZipFile(stream) {IsStreamOwner = false, Password = password})
        {
            foreach (ZipEntry zipEntry in zipFile)
            {
                if (StringUtils.EqualsIgnoreCase(zipEntry.Name, "data.xml"))
                {
                    // Read the XML file from the ZIP archive
                    var inputStream = zipFile.GetInputStream(zipEntry);
                    output = XmlStorage.LoadXml<T>(inputStream);
                }
                else
                {
                    // Read additional files from the ZIP archive
                    foreach (var file in additionalFiles)
                    {
                        if (StringUtils.EqualsIgnoreCase(zipEntry.Name, file.Filename))
                        {
                            var inputStream = zipFile.GetInputStream(zipEntry);
                            file.StreamDelegate(inputStream);
                        }
                    }
                }
            }
        }

        return output ?? throw new InvalidDataException(Properties.Resources.NoXmlDataInZip);
    }

    /// <summary>
    /// Loads an object from an XML file embedded in a ZIP archive.
    /// </summary>
    /// <typeparam name="T">The type of object the XML stream shall be converted into.</typeparam>
    /// <param name="path">The ZIP archive to load.</param>
    /// <param name="password">The password to use for decryption; <c>null</c> for no encryption.</param>
    /// <param name="additionalFiles">Additional files stored alongside the XML file in the ZIP archive to be read.</param>
    /// <returns>The loaded object.</returns>
    /// <exception cref="IOException">A problem occurred while reading the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
    /// <exception cref="ZipException">A problem occurred while reading the ZIP data or if <paramref name="password"/> is wrong.</exception>
    /// <exception cref="InvalidDataException">A problem occurred while deserializing the XML data.</exception>
    /// <remarks>Uses <see cref="AtomicRead"/> internally.</remarks>
    public static T LoadXmlZip<T>([Localizable(false)] string path, [Localizable(false)] string? password = null, params EmbeddedFile[] additionalFiles)
        where T : class
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        if (additionalFiles == null) throw new ArgumentNullException(nameof(additionalFiles));
        #endregion

        using (new AtomicRead(path))
        using (var fileStream = File.OpenRead(path))
            return LoadXmlZip<T>(fileStream, password, additionalFiles);
    }

    /// <summary>
    /// Saves an object in an XML file embedded in a ZIP archive.
    /// </summary>
    /// <typeparam name="T">The type of object to be saved in an XML stream.</typeparam>
    /// <param name="data">The object to be stored.</param>
    /// <param name="stream">The ZIP archive to be written.</param>
    /// <param name="password">The password to use for encryption; <c>null</c> for no encryption.</param>
    /// <param name="additionalFiles">Additional files to be stored alongside the XML file in the ZIP archive; can be <c>null</c>.</param>
    public static void SaveXmlZip<T>([DisallowNull] T data, Stream stream, [Localizable(false)] string? password = null, params EmbeddedFile[] additionalFiles)
        where T : notnull
    {
        #region Sanity checks
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        if (additionalFiles == null) throw new ArgumentNullException(nameof(additionalFiles));
        #endregion

        if (stream.CanSeek) stream.Position = 0;
        using var zipStream = new ZipOutputStream(stream) {IsStreamOwner = false};
        if (!string.IsNullOrEmpty(password)) zipStream.Password = password;

        // Write the XML file to the ZIP archive
        {
            var entry = new ZipEntry("data.xml") {DateTime = DateTime.Now};
            if (!string.IsNullOrEmpty(password)) entry.AESKeySize = 128;
            zipStream.SetLevel(9);
            zipStream.PutNextEntry(entry);
            data.SaveXml(zipStream);
            zipStream.CloseEntry();
        }

        // Write additional files to the ZIP archive
        foreach (var file in additionalFiles)
        {
            var entry = new ZipEntry(file.Filename) {DateTime = DateTime.Now};
            if (!string.IsNullOrEmpty(password)) entry.AESKeySize = 128;
            zipStream.SetLevel(file.CompressionLevel);
            zipStream.PutNextEntry(entry);
            file.StreamDelegate(zipStream);
            zipStream.CloseEntry();
        }
    }

    /// <summary>
    /// Saves an object in an XML file embedded in a ZIP archive.
    /// </summary>
    /// <typeparam name="T">The type of object to be saved in an XML stream.</typeparam>
    /// <param name="data">The object to be stored.</param>
    /// <param name="path">The ZIP archive to be written.</param>
    /// <param name="password">The password to use for encryption; <c>null</c> for no encryption.</param>
    /// <param name="additionalFiles">Additional files to be stored alongside the XML file in the ZIP archive; can be <c>null</c>.</param>
    /// <exception cref="IOException">A problem occurred while writing the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Write access to the file is not permitted.</exception>
    /// <remarks>Uses <seealso cref="AtomicWrite"/> internally.</remarks>
    public static void SaveXmlZip<T>([DisallowNull] T data, [Localizable(false)] string path, [Localizable(false)] string? password = null, params EmbeddedFile[] additionalFiles)
        where T : notnull
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        if (additionalFiles == null) throw new ArgumentNullException(nameof(additionalFiles));
        #endregion

        using var atomic = new AtomicWrite(path);
        using (var fileStream = File.Create(atomic.WritePath))
            SaveXmlZip(data, fileStream, password, additionalFiles);
        atomic.Commit();
    }
}
