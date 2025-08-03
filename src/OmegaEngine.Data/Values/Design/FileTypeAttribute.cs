/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using JetBrains.Annotations;

namespace OmegaEngine.Values.Design;

/// <summary>
/// Stores the file type describing the kind of data a property stores.
/// Controls the behaviour of <see cref="CodeEditor"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
[PublicAPI]
public sealed class FileTypeAttribute : Attribute
{
    /// <summary>
    /// The name of the file type (e.g. XML, JavaScript, Lua).
    /// </summary>
    public string FileType { get; }

    /// <summary>
    /// Creates a new file type attribute.
    /// </summary>
    /// <param name="fileType">The name of the file type (e.g. XML, JavaScript, Lua).</param>
    public FileTypeAttribute([NotNull] string fileType)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(fileType)) throw new ArgumentNullException(nameof(fileType));
        #endregion

        FileType = fileType;
    }
}
