/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;
using OmegaEngine.Foundation.Properties;

namespace OmegaEngine.Foundation.Storage;

/// <summary>
/// Provides a virtual file system for combining data from multiple directories (useful for modding).
/// </summary>
public static class ContentManager
{
    /// <summary>
    /// The name of an environment variable that can be used to configure the content manager externally.
    /// </summary>
    [PublicAPI]
    public const string
        EnvVarNameBaseDir = "CONTENTMANAGER_BASE_DIR",
        EnvVarNameModDir = "CONTENTMANAGER_MOD_DIR";

    private static readonly string?
        _envVarBaseDir = Environment.GetEnvironmentVariable(EnvVarNameBaseDir),
        _envVarModDir = Environment.GetEnvironmentVariable(EnvVarNameModDir);

    private static DirectoryInfo?
        _baseDir = new(_envVarBaseDir ?? Path.Combine(Locations.InstallBase, "content")),
        _modDir = (_envVarModDir == null) ? null : new DirectoryInfo(_envVarModDir);

    /// <summary>
    /// The base directory where all the content files are stored; should not be <c>null</c>.
    /// </summary>
    /// <remarks>Can be set externally with <see cref="EnvVarNameBaseDir"/>.</remarks>
    /// <exception cref="DirectoryNotFoundException">The specified directory could not be found.</exception>
    public static DirectoryInfo? BaseDir
    {
        get => _baseDir;
        set
        {
            if (value is { Exists: false })
                throw new DirectoryNotFoundException(Resources.NotFoundGameContentDir + Environment.NewLine + value.FullName);
            _baseDir = value;
        }
    }

    /// <summary>
    /// A directory overriding the base directory for creating mods; can be <c>null</c>.
    /// </summary>
    /// <remarks>Can be set externally with <see cref="EnvVarNameModDir"/>.</remarks>
    /// <exception cref="DirectoryNotFoundException">The specified directory could not be found.</exception>
    public static DirectoryInfo? ModDir
    {
        get => _modDir;
        set
        {
            if (value is { Exists: false })
                throw new DirectoryNotFoundException(Resources.NotFoundModContentDir + Environment.NewLine + value.FullName);
            _modDir = value;
        }
    }

    /// <summary>
    /// Creates a path for a content directory (using the <see cref="ModDir"/> if available).
    /// </summary>
    /// <param name="type">The type of file (e.g. Textures, Sounds, ...).</param>
    /// <returns>The absolute path to the requested directory.</returns>
    /// <exception cref="DirectoryNotFoundException">The specified directory could not be found.</exception>
    public static string CreateDirPath([Localizable(false)] string type)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(type)) throw new ArgumentNullException(nameof(type));
        #endregion

        type = type.ToNativePath();

        // Use mod directory if available
        string pathBase;
        if (ModDir != null) pathBase = ModDir.FullName;
        else if (BaseDir != null) pathBase = BaseDir.FullName;
        else throw new DirectoryNotFoundException(Resources.NotFoundGameContentDir + "\n-");

        // Check the path before returning it
        var directory = new DirectoryInfo(Path.Combine(pathBase, type));
        if (!directory.Exists) directory.Create();
        return directory.FullName;
    }

    /// <summary>
    /// Creates a path for a content file (using <see cref="ModDir"/> if available).
    /// </summary>
    /// <param name="type">The type of file (e.g. Textures, Sounds, ...).</param>
    /// <param name="id">The file name of the content.</param>
    /// <returns>The absolute path to the requested content file.</returns>
    public static string CreateFilePath([Localizable(false)] string type, [Localizable(false)] string id)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(type)) throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
        #endregion

        type = type.ToNativePath();
        id = id.ToNativePath();
        return Path.Combine(CreateDirPath(type), id);
    }

    /// <summary>
    /// Checks whether a certain content file exists.
    /// </summary>
    /// <param name="type">The type of file (e.g. Textures, Sounds, ...).</param>
    /// <param name="id">The file name of the content.</param>
    /// <returns><c>true</c> if the requested content file exists.</returns>
    public static bool FileExists([Localizable(false)] string type, [Localizable(false)] string id)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(type)) throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
        #endregion

        type = type.ToNativePath();
        id = id.ToNativePath();
        string fullID = Path.Combine(type, id);

        if (ModDir != null && File.Exists(Path.Combine(ModDir.FullName, fullID)))
            return true;
        if (BaseDir != null && File.Exists(Path.Combine(BaseDir.FullName, fullID)))
            return true;
        return false;
    }

    /// <summary>
    /// Adds a specific file to the <paramref name="files"/> list.
    /// </summary>
    /// <param name="files">The collection to add the file to.</param>
    /// <param name="type">The type-subdirectory the file belongs to.</param>
    /// <param name="name">The file name to be added to the list.</param>
    /// <param name="flagAsMod">Set to <c>true</c> when handling mod files to detect added and changed files.</param>
    private static void AddFileToList(NamedCollection<FileEntry> files, string type, string name, bool flagAsMod)
    {
        if (flagAsMod)
        {
            // Detect whether this is a new file or a replacement for an existing one
            if (files.Contains(name))
            {
                var previousEntry = files[name];

                // Only mark as modified if the pre-existing file isn't already a mod file itself
                if (previousEntry.EntryType == FileEntryType.Normal)
                {
                    files.Remove(previousEntry);
                    files.Add(new(type, name, FileEntryType.Modified));
                }
            }
            else files.Add(new(type, name, FileEntryType.Added));
        }
        else
        {
            // Prevent duplicate entries
            if (!files.Contains(name)) files.Add(new(type, name));
        }
    }

    /// <summary>
    /// Recursively finds all files in <paramref name="directory"/> ending with <paramref name="extension"/> and adds them to the <paramref name="files"/> list.
    /// </summary>
    /// <param name="files">The collection to add the found files to.</param>
    /// <param name="type">The type-subdirectory the files belong to.</param>
    /// <param name="extension">The file extension to look for.</param>
    /// <param name="directory">The directory to look in.</param>
    /// <param name="prefix">A prefix to add before the file name in the list (used to indicate current sub-directory).</param>
    /// <param name="flagAsMod">Set to <c>true</c> when handling mod files to detect added and changed files.</param>
    private static void AddDirectoryToList(NamedCollection<FileEntry> files, string type, string extension, DirectoryInfo directory, string prefix, bool flagAsMod)
    {
        // Add the files in this directory to the list
        foreach (var file in directory.GetFiles("*" + extension))
            AddFileToList(files, type, prefix + file.Name, flagAsMod);

        // Recursively call this method for all sub-directories
        foreach (var subDir in directory.GetDirectories()
                                        .Where(subDir => !subDir.Name.StartsWith("."))) // Don't add dot directories (e.g. .svn)
            AddDirectoryToList(files, type, extension, subDir, prefix + subDir.Name + Path.DirectorySeparatorChar, flagAsMod);
    }

    /// <summary>
    /// Gets a list of all files of a certain type
    /// </summary>
    /// <param name="type">The type of files you want (e.g. Textures, Sounds, ...)</param>
    /// <param name="extension">The file extension to so search for</param>
    /// <returns>A collection of strings with file IDs</returns>
    public static NamedCollection<FileEntry> GetFileList([Localizable(false)] string type, [Localizable(false)] string extension)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(type)) throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(extension)) throw new ArgumentNullException(nameof(extension));
        #endregion

        type = type.ToNativePath();

        // Create an alphabetical list of files without duplicates
        var files = new NamedCollection<FileEntry>();

        if (BaseDir != null && Directory.Exists(Path.Combine(BaseDir.FullName, type)))
        {
            AddDirectoryToList(files, type, extension,
                new(Path.Combine(BaseDir.FullName, type)), "", false);
        }

        if (ModDir != null)
        {
            if (Directory.Exists(Path.Combine(ModDir.FullName, type)))
            {
                AddDirectoryToList(files, type, extension,
                    new(Path.Combine(ModDir.FullName, type)), "", true);
            }
        }

        return files;
    }

    /// <summary>
    /// Gets the file path for a content file
    /// </summary>
    /// <param name="type">The type of file (e.g. Textures, Sounds, ...).</param>
    /// <param name="id">The file name of the content.</param>
    /// <returns>The absolute path to the requested content file</returns>
    /// <exception cref="FileNotFoundException">The specified file could not be found.</exception>
    public static string GetFilePath([Localizable(false)] string type, [Localizable(false)] string id)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(type)) throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
        #endregion

        type = type.ToNativePath();
        id = id.ToNativePath();

        string path;

        if (ModDir != null)
        {
            path = Path.Combine(ModDir.FullName, Path.Combine(type, id));
            if (File.Exists(path)) return path;
        }

        if (BaseDir != null)
        {
            path = Path.Combine(BaseDir.FullName, Path.Combine(type, id));
            if (File.Exists(path)) return path;
        }

        throw new FileNotFoundException(Resources.NotFoundGameContentFile + Environment.NewLine + Path.Combine(type, id), Path.Combine(type, id));
    }

    /// <summary>
    /// Gets a reading stream for a content file
    /// </summary>
    /// <param name="type">The type of file (e.g. Textures, Sounds, ...).</param>
    /// <param name="id">The file name of the content.</param>
    /// <returns>The absolute path to the requested content file</returns>
    /// <exception cref="FileNotFoundException">The specified file could not be found.</exception>
    /// <exception cref="IOException">There was an error reading the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
    public static Stream GetFileStream([Localizable(false)] string type, [Localizable(false)] string id)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(type)) throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
        #endregion

        type = type.ToNativePath();
        id = id.ToNativePath();

        if (!FileExists(type, id))
            throw new FileNotFoundException(Resources.NotFoundGameContentFile + Environment.NewLine + Path.Combine(type, id), Path.Combine(type, id));

        return File.OpenRead(GetFilePath(type, id));

    }

    /// <summary>
    /// Deletes a file in <see cref="ModDir"/>. Will not touch files in <see cref="BaseDir"/>.
    /// </summary>
    /// <param name="type">The type of file (e.g. Textures, Sounds, ...).</param>
    /// <param name="id">The file name of the content.</param>
    /// <exception cref="InvalidOperationException"><see cref="ModDir"/> is not set.</exception>
    /// <exception cref="FileNotFoundException">The specified file could not be found.</exception>
    /// <exception cref="IOException">The specified file could not be deleted.</exception>
    /// <exception cref="UnauthorizedAccessException">You have insufficient rights to delete the file.</exception>
    public static void DeleteModFile([Localizable(false)] string type, [Localizable(false)] string id)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(type)) throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
        #endregion

        // Ensure there is an active mod
        if (ModDir == null) throw new InvalidOperationException(Resources.NoModActive);

        // Try to delete a file in that mod
        File.Delete(Path.Combine(ModDir.FullName, Path.Combine(type, id)));
    }
}
