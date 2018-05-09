/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using ICSharpCode.SharpZipLib.Zip;

namespace OmegaEngine.Storage
{
    /// <summary>
    /// Represents a file in a content archive.
    /// </summary>
    internal struct ContentArchiveEntry
    {
        /// <summary>
        /// The archive containing the file.
        /// </summary>
        public ZipFile ZipFile { get; }

        /// <summary>
        /// The actual content file.
        /// </summary>
        public ZipEntry ZipEntry { get; }

        /// <summary>
        /// Creates a new content file representation
        /// </summary>
        /// <param name="zipFile">The archive containing the file</param>
        /// <param name="zipEntry">The actual content file</param>
        public ContentArchiveEntry(ZipFile zipFile, ZipEntry zipEntry)
        {
            ZipFile = zipFile;
            ZipEntry = zipEntry;
        }
    }
}
