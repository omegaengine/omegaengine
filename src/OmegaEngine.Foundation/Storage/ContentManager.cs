/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
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
    /// The name of an environment variable that can be used to configure the content manager.
    /// </summary>
    [PublicAPI]
    public const string EnvVarNameBaseDir = "OMEGAENGINE_CONTENT";

    /// <summary>
    /// The name of an environment variable that can be used to configure the content manager.
    /// </summary>
    [PublicAPI]
    public const string EnvVarNameModDir = "OMEGAENGINE_CONTENT_MOD";

    private static readonly List<DirectoryInfo>
        _baseDirs = GetDirsFromEnv(EnvVarNameBaseDir) ?? [new(Path.Combine(Locations.InstallBase, "content"))],
        _modDirs = GetDirsFromEnv(EnvVarNameModDir) ?? [];

    private static List<DirectoryInfo>? GetDirsFromEnv(string envName)
        => Environment.GetEnvironmentVariable(envName)
                     ?.Split(Path.PathSeparator)
                      .Select(dir => new DirectoryInfo(dir))
                      .ToList();

    /// <summary>
    /// The base directory where content files are stored.
    /// </summary>
    /// <exception cref="DirectoryNotFoundException">The specified directory could not be found.</exception>
    /// <remarks>
    /// You can use the environment variable <see cref="EnvVarNameBaseDir"/> to set this value.
    /// When using the environment variable, you can also specify multiple directories to be overlaid. This property will then only expose the last directory.
    /// </remarks>
    public static DirectoryInfo BaseDir
    {
        get => _baseDirs.First();
        set
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException(nameof(value));
            #endregion

            if (value is { Exists: false })
                throw new DirectoryNotFoundException(Resources.NotFoundGameContentDir + Environment.NewLine + value.FullName);

            _baseDirs.Clear();
            _baseDirs.Add(value);
        }
    }

    /// <summary>
    /// A directory overriding the base directory for creating mods. <c>null</c> if there is no active mod.
    /// </summary>
    /// <exception cref="DirectoryNotFoundException">The specified directory could not be found.</exception>
    /// <remarks>
    /// You can use the environment variable <see cref="EnvVarNameModDir"/> to set this value.
    /// When using the environment variable, you can also specify multiple directories to be overlaid. This property will then only expose the last directory.
    /// </remarks>
    public static DirectoryInfo? ModDir
    {
        get => _modDirs.FirstOrDefault();
        set
        {
            if (value is { Exists: false })
                throw new DirectoryNotFoundException(Resources.NotFoundModContentDir + Environment.NewLine + value.FullName);

            _modDirs.Clear();
            if (value != null) _modDirs.Add(value);
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

        string pathBase = ModDir?.FullName ?? BaseDir.FullName;
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

        return _modDirs.Any(dir => File.Exists(Path.Combine(dir.FullName, Path.Combine(type, id))))
            || _baseDirs.Any(dir => File.Exists(Path.Combine(dir.FullName, Path.Combine(type, id))));
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
        var files = new NamedCollection<FileEntry>();

        foreach (var dir in _baseDirs)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, type)))
                AddDirectoryToList(files, type, extension, new DirectoryInfo(Path.Combine(dir.FullName, type)), prefix: "", flagAsMod: false);
        }

        foreach (var dir in _modDirs)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, type)))
                AddDirectoryToList(files, type, extension, new DirectoryInfo(Path.Combine(dir.FullName, type)), prefix: "", flagAsMod: true);
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

        foreach (var dir in _modDirs)
        {
            string path = Path.Combine(dir.FullName, Path.Combine(type, id));
            if (File.Exists(path)) return path;
        }

        foreach (var dir in _baseDirs)
        {
            string path = Path.Combine(dir.FullName, Path.Combine(type, id));
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
