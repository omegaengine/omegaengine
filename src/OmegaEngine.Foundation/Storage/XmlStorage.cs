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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ICSharpCode.SharpZipLib.Zip;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using SlimDX;

namespace OmegaEngine.Foundation.Storage;

/// <summary>
/// Provides easy serialization to XML files (optionally wrapped in ZIP archives).
/// </summary>
public static class XmlStorage
{
    #region Constants
    /// <summary>
    /// The XML namespace used for XML Schema instance.
    /// </summary>
    public const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";
    #endregion

    #region Serializer generation
    /// <summary>An internal cache of XML serializers identified by the target type and ignored sub-types.</summary>
    private static readonly TransparentCache<Type, XmlSerializer> _serializers = new(CreateSerializer);

    /// <summary>Used to mark something as "serialize as XML attribute".</summary>
    private static readonly XmlAttributes _asAttribute = new() {XmlAttribute = new()};

    /// <summary>Used to mark something to be ignored when serializing.</summary>
    private static readonly XmlAttributes _ignore = new() {XmlIgnore = true};

    /// <summary>
    /// Creates a new <see cref="XmlSerializer"/> for the type <paramref name="type"/> and applies a set of default augmentations for .NET types.
    /// </summary>
    /// <param name="type">The type to create the serializer for.</param>
    /// <returns>The newly created <see cref="XmlSerializer"/>.</returns>
    /// <remarks>This method may be rather slow, so its results should be cached.</remarks>
    private static XmlSerializer CreateSerializer(Type type)
    {
        var overrides = new XmlAttributeOverrides();

        #region Augment .NET BCL types
        MembersAsAttributes<Point>(overrides, "X", "Y");
        MembersAsAttributes<Size>(overrides, "Width", "Height");
        MembersAsAttributes<Rectangle>(overrides, "X", "Y", "Width", "Height");
        overrides.Add(typeof(Rectangle), "Location", _ignore);
        overrides.Add(typeof(Rectangle), "Size", _ignore);
        overrides.Add(typeof(Exception), "Data", _ignore);
        #endregion

        #region Augement SlimDX types
        MembersAsAttributes<Color3>(overrides, "Red", "Green", "Blue");
        MembersAsAttributes<Color4>(overrides, "Alpha", "Red", "Green", "Blue");
        MembersAsAttributes<Vector2>(overrides, "X", "Y");
        MembersAsAttributes<Vector3>(overrides, "X", "Y", "Z");
        MembersAsAttributes<Vector4>(overrides, "X", "Y", "Z", "W");
        MembersAsAttributes<Quaternion>(overrides, "X", "Y", "Z", "W");
        #endregion

        var serializer = new XmlSerializer(type, overrides);
        serializer.UnknownAttribute += (_, e) => Log.Warn($"Ignored XML attribute while deserializing: {e.Attr.Name}={e.Attr.Value}");
        serializer.UnknownElement += (_, e) => Log.Warn($"Ignored XML element while deserializing: {e.Element.Name}");
        return serializer;
    }

    /// <summary>
    /// Configures a set of members of a type to be serialized as XML attributes.
    /// </summary>
    private static void MembersAsAttributes<T>(XmlAttributeOverrides overrides, params string[] members)
    {
        var type = typeof(T);
        foreach (string memeber in members)
            overrides.Add(type, memeber, _asAttribute);
    }
    #endregion

    //--------------------//

    #region Load plain
    /// <summary>
    /// Loads an object from an XML file.
    /// </summary>
    /// <typeparam name="T">The type of object the XML stream shall be converted into.</typeparam>
    /// <param name="stream">The stream to read the encoded XML data from.</param>
    /// <returns>The loaded object.</returns>
    /// <exception cref="InvalidDataException">A problem occurred while deserializing the XML data.</exception>
    public static T LoadXml<T>(Stream stream)
        where T : class
    {
        #region Sanity checks
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        #endregion

        if (stream.CanSeek) stream.Position = 0;
        try
        {
            return (T?)_serializers[typeof(T)].Deserialize(stream) ?? throw new InvalidDataException();
        }
        #region Error handling
        catch (InvalidOperationException ex)
        {
            // Convert exception type
            throw new InvalidDataException(ex.Message, ex.InnerException) {Source = ex.Source};
        }
        #endregion
    }

    /// <summary>
    /// Loads an object from an XML file.
    /// </summary>
    /// <typeparam name="T">The type of object the XML stream shall be converted into.</typeparam>
    /// <param name="path">The path of the file to load.</param>
    /// <returns>The loaded object.</returns>
    /// <exception cref="IOException">A problem occurred while reading the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
    /// <exception cref="InvalidDataException">A problem occurred while deserializing the XML data.</exception>
    /// <remarks>Uses <see cref="AtomicRead"/> internally.</remarks>
    public static T LoadXml<T>([Localizable(false)] string path)
        where T : class
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        #endregion

        try
        {
            using (new AtomicRead(path))
            using (var fileStream = File.OpenRead(path))
                return LoadXml<T>(fileStream);
        }
        #region Error handling
        catch (InvalidDataException ex)
        {
            // Change exception message to add context information
            throw new InvalidDataException(string.Format(Properties.Resources.ProblemLoading, path) + Environment.NewLine + ex.Message, ex.InnerException);
        }
        #endregion
    }

    /// <summary>
    /// Loads an object from an XML string.
    /// </summary>
    /// <typeparam name="T">The type of object the XML string shall be converted into.</typeparam>
    /// <param name="data">The XML string to be parsed.</param>
    /// <returns>The loaded object.</returns>
    /// <exception cref="InvalidDataException">A problem occurred while deserializing the XML data.</exception>
    public static T FromXmlString<T>([Localizable(false)] string data)
        where T : class
    {
        #region Sanity checks
        if (data == null) throw new ArgumentNullException(nameof(data));
        #endregion

        // Copy string to a stream and then parse
        using var stream = data.ToStream();
        return LoadXml<T>(stream);
    }
    #endregion

    #region Save plain
    /// <summary>
    /// Saves an object in an XML stream ending with a line break.
    /// </summary>
    /// <typeparam name="T">The type of object to be saved in an XML stream.</typeparam>
    /// <param name="data">The object to be stored.</param>
    /// <param name="stream">The stream to write the encoded XML data to.</param>
    /// <param name="stylesheet">The path of an XSL stylesheet for <typeparamref name="T"/>; can be <c>null</c>.</param>
    public static void SaveXml<T>([DisallowNull] this T data, Stream stream, [Localizable(false)] string? stylesheet = null)
    {
        #region Sanity checks
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        #endregion

        var type = data.GetType();
        var serializer = _serializers[typeof(T)];

        var xmlWriter = XmlWriter.Create(stream, new()
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\n"
        });

        // Add stylesheet processor instruction
        if (!string.IsNullOrEmpty(stylesheet))
            xmlWriter.WriteProcessingInstruction("xml-stylesheet", $"type=\"text/xsl\" href=\"{stylesheet}\"");

        var qualifiedNames = GetQualifiedNames(type);
        if (qualifiedNames.Length == 0) serializer.Serialize(xmlWriter, data);
        else serializer.Serialize(xmlWriter, data, new(qualifiedNames));

        // End file with line break
        xmlWriter.Flush();
        if (xmlWriter.Settings != null)
        {
            var newLine = xmlWriter.Settings.Encoding.GetBytes(xmlWriter.Settings.NewLineChars);
            stream.Write(newLine, 0, newLine.Length);
        }
    }

    private static XmlQualifiedName[] GetQualifiedNames(Type type)
    {
        var qualifiedNames = type.GetCustomAttributes<XmlNamespaceAttribute>().Select(attr => attr.QualifiedName);

        var rootAttribute = type.GetCustomAttribute<XmlRootAttribute>();
        if (rootAttribute != null) qualifiedNames = qualifiedNames.Concat([new("", rootAttribute.Namespace)]);

        return qualifiedNames.ToArray();
    }

    /// <summary>
    /// Saves an object in an XML file ending with a line break.
    /// </summary>
    /// <remarks>This method performs an atomic write operation when possible.</remarks>
    /// <typeparam name="T">The type of object to be saved in an XML stream.</typeparam>
    /// <param name="data">The object to be stored.</param>
    /// <param name="path">The path of the file to write.</param>
    /// <param name="stylesheet">The path of an XSL stylesheet for <typeparamref name="T"/>; can be <c>null</c>.</param>
    /// <exception cref="IOException">A problem occurred while writing the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Write access to the file is not permitted.</exception>
    /// <remarks>Uses <seealso cref="AtomicWrite"/> internally.</remarks>
    public static void SaveXml<T>([DisallowNull] this T data, [Localizable(false)] string path, [Localizable(false)] string? stylesheet = null)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        #endregion

        using var atomic = new AtomicWrite(path);
        using (var fileStream = File.Create(atomic.WritePath))
            SaveXml(data, fileStream, stylesheet);
        atomic.Commit();
    }

    /// <summary>
    /// Returns an object as an XML string ending with a line break.
    /// </summary>
    /// <typeparam name="T">The type of object to be saved in an XML string.</typeparam>
    /// <param name="data">The object to be stored.</param>
    /// <param name="stylesheet">The path of an XSL stylesheet for <typeparamref name="T"/>; can be <c>null</c>.</param>
    /// <returns>A string containing the XML code.</returns>
    public static string ToXmlString<T>([DisallowNull] this T data, [Localizable(false)] string? stylesheet = null)
    {
        using var stream = new MemoryStream();
        // Write to a memory stream
        SaveXml(data, stream, stylesheet);

        // Copy the stream to a string
        string result = stream.ReadToString();

        // Remove encoding="utf-8" because we don't know how the string will actually be encoded on-dik
        const string prefixWithEncoding = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
        const string prefixWithoutEncoding = "<?xml version=\"1.0\"?>";
        return prefixWithoutEncoding + result[prefixWithEncoding.Length..];
    }
    #endregion

    #region Load ZIP
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

        using (var zipFile = new ZipFile(stream) {IsStreamOwner = false})
        {
            zipFile.Password = password;

            foreach (ZipEntry zipEntry in zipFile)
            {
                if (StringUtils.EqualsIgnoreCase(zipEntry.Name, "data.xml"))
                {
                    // Read the XML file from the ZIP archive
                    var inputStream = zipFile.GetInputStream(zipEntry);
                    output = LoadXml<T>(inputStream);
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
    #endregion

    #region Save ZIP
    /// <summary>
    /// Saves an object in an XML file embedded in a ZIP archive.
    /// </summary>
    /// <typeparam name="T">The type of object to be saved in an XML stream.</typeparam>
    /// <param name="data">The object to be stored.</param>
    /// <param name="stream">The ZIP archive to be written.</param>
    /// <param name="password">The password to use for encryption; <c>null</c> for no encryption.</param>
    /// <param name="additionalFiles">Additional files to be stored alongside the XML file in the ZIP archive; can be <c>null</c>.</param>
    public static void SaveXmlZip<T>([DisallowNull] this T data, Stream stream, [Localizable(false)] string? password = null, params EmbeddedFile[] additionalFiles)
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
            SaveXml(data, zipStream);
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
    public static void SaveXmlZip<T>([DisallowNull] this T data, [Localizable(false)] string path, [Localizable(false)] string? password = null, params EmbeddedFile[] additionalFiles)
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
    #endregion
}
