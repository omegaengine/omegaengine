/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;

namespace AlphaFramework.World;

/// <summary>
/// A game world (but not a running game). Equivalent to the content of a map file.
/// </summary>
public interface IUniverse
{
    /// <summary>
    /// Total elapsed game time in seconds.
    /// </summary>
    public double GameTime { get; set; }

    /// <summary>
    /// Updates the universe.
    /// </summary>
    /// <param name="elapsedGameTime">How much game time in seconds has elapsed since this method was last called.</param>
    void Update(double elapsedGameTime);

    /// <summary>
    /// The map file this world was loaded from.
    /// </summary>
    /// <remarks>Is not serialized/stored, is set by whatever method loads the universe.</remarks>
    string? SourceFile { get; set; }

    /// <summary>
    /// Saves this <see cref="CoordinateUniverse{TCoordinates}"/> in a compressed XML file (map file).
    /// </summary>
    /// <param name="path">The file to save in.</param>
    /// <exception cref="IOException">A problem occurred while writing the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Write access to the file is not permitted.</exception>
    void Save(string path);
}
