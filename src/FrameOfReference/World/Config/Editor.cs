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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using NanoByte.Common;
using NanoByte.Common.Collections;

namespace FrameOfReference.World.Config;

/// <summary>
/// Stores settings for the game's editor.
/// </summary>
/// <seealso cref="Settings.Editor"/>
public sealed class EditorSettings
{
    /// <summary>
    /// Occurs when a setting in this group is changed.
    /// </summary>
    [Description("Occurs when a setting in this group is changed.")]
    public event Action Changed = () => {};

    private bool _showWelcomeMessage = true;

    /// <summary>
    /// Show the welcome message on startup
    /// </summary>
    [DefaultValue(true), Description("Show the welcome message on startup")]
    public bool ShowWelcomeMessage { get => _showWelcomeMessage; set => value.To(ref _showWelcomeMessage, Changed); }

    private bool _editBase;

    /// <summary>
    /// May the user edit the base game?
    /// </summary>
    [DefaultValue(false), Description("May the user edit the base game?")]
    public bool EditBase { get => _editBase; set => value.To(ref _editBase, Changed); }

    private Size _windowSize;

    /// <summary>
    /// The size of the editor window
    /// </summary>
    [Description("The size of the editor window")]
    public Size WindowSize { get => _windowSize; set => value.To(ref _windowSize, Changed); }

    private bool _windowMaximized;

    /// <summary>
    /// Is the editor window maximized?
    /// </summary>
    [Description("Is the editor window maximized?")]
    public bool WindowMaximized { get => _windowMaximized; set => value.To(ref _windowMaximized, Changed); }

    // Note: Can not use ICollection<T> interface with XML Serialization
    private readonly MonitoredCollection<string> _recentMods = [];

    /// <summary>
    /// A list of mod directories recently opened by the editor
    /// </summary>
    public Collection<string> RecentMods => _recentMods;

    #region Constructor
    public EditorSettings()
    {
        _recentMods.Changed += Changed;
    }
    #endregion
}
