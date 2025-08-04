/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace AlphaFramework.Editor;

/// <summary>
/// Contains basic information about a mod.
/// </summary>
public sealed class ModInfo
{
    /// <summary>
    /// The file extensions used when this class is stored as a file including the leading dot.
    /// </summary>
    public static string FileExt { get; set; } = ".OmegaMod";

    /// <summary>
    /// Information about the currently loaded mod.
    /// </summary>
    public static ModInfo? Current { get; set; }

    /// <summary>
    /// The path to the file containing the mod information.
    /// </summary>
    public static string? CurrentLocation { get; set; }

    /// <summary>
    /// Shall the application be used to edit the main game instead of a mod?
    /// </summary>
    public static bool MainGame { get; set; }

    //--------------------//

    /// <summary>
    /// The name of the mod.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The version of the mod.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// The author of the mod.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// The website of the author.
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// A short description of the mod.
    /// </summary>
    public string? Description { get; set; }
}
