/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace OmegaGUI.Render;

/// <summary>
/// Text formatting options for <see cref="GuiFont.DrawString"/>.
/// </summary>
/// <remarks>The values match the Win32 <c>DT_*</c> flags.</remarks>
[Flags]
public enum DrawTextFormat
{
    /// <summary>Aligns text to the top.</summary>
    Top = 0x0,

    /// <summary>Aligns text to the left.</summary>
    Left = 0x0,

    /// <summary>Centers text horizontally.</summary>
    Center = 0x1,

    /// <summary>Aligns text to the right.</summary>
    Right = 0x2,

    /// <summary>Centers text vertically.</summary>
    VerticalCenter = 0x4,

    /// <summary>Aligns text to the bottom.</summary>
    Bottom = 0x8,

    /// <summary>Breaks lines between words that would extend past the edge of the rectangle.</summary>
    WordBreak = 0x10,

    /// <summary>Displays text on a single line, ignoring line breaks.</summary>
    SingleLine = 0x20,

    /// <summary>Expands tab characters to tab stops every eight average character widths.</summary>
    ExpandTabs = 0x40,

    /// <summary>Draws without clipping to the rectangle.</summary>
    NoClip = 0x100
}

/// <summary>
/// The weight of a <see cref="GuiFont"/>.
/// </summary>
/// <remarks>The values match the Win32 <c>FW_*</c> constants.</remarks>
public enum FontWeight
{
    DoNotCare = 0,
    Thin = 100,
    ExtraLight = 200,
    Light = 300,
    Normal = 400,
    Medium = 500,
    SemiBold = 600,
    Bold = 700,
    ExtraBold = 800,
    Heavy = 900
}
