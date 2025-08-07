/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.ComponentModel;
using NanoByte.Common;

namespace FrameOfReference.World.Config;

/// <summary>
/// Stores general game settings (UI language, difficulty level, etc.).
/// </summary>
/// <seealso cref="Settings.General"/>
public sealed class GeneralSettings
{
    #region Constants
    /// <summary>
    /// The complete name of the application
    /// </summary>
    public const string AppName = "Frame of Reference";

    /// <summary>
    /// The short version of the application name (used for EXE name, AppModel IDs, etc.)
    /// </summary>
    public const string AppNameShort = "FrameOfReference";
    #endregion

    /// <summary>
    /// Occurs when a setting in this group is changed.
    /// </summary>
    [Description("Occurs when a setting in this group is changed.")]
    public event Action Changed = () => {};

    private string? _contentDir;

    /// <summary>
    /// Path to the directory to load the game content from
    /// </summary>
    [DefaultValue(""), Description("Path to the directory to load the game content from")]
    public string? ContentDir { get => _contentDir; set => value.To(ref _contentDir, Changed); }

    private string? _language;

    /// <summary>
    /// The current game language
    /// </summary>
    [DefaultValue(""), Description("The current game language")]
    public string? Language { get => _language; set => value.To(ref _language,  Changed); }
}
