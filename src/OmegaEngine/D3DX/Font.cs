/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using SharpGen.Runtime;
using Vortice.Direct3D9;

namespace OmegaEngine.D3DX;

/// <summary>
/// Wraps an <c>ID3DXFont</c>: renders text using a GDI font cached in textures.
/// </summary>
public sealed unsafe class Font : ComObject
{
    // ID3DXFont (d3dx9core.h)
    private const int VtDrawTextW = 15, VtOnLostDevice = 16, VtOnResetDevice = 17;

    /// <summary>
    /// Creates a font (<c>D3DXCreateFont</c>).
    /// </summary>
    /// <exception cref="SharpGenException">The font could not be created.</exception>
    public Font(IDirect3DDevice9 device, int height, int width, FontWeight weight, int mipLevels, bool italic, CharacterSet characterSet, Precision outputPrecision, FontQuality quality, PitchAndFamily pitchAndFamily, string faceName)
        : base(Create(device, height, width, weight, mipLevels, italic, characterSet, outputPrecision, quality, pitchAndFamily, faceName))
    {}

    private static nint Create(IDirect3DDevice9 device, int height, int width, FontWeight weight, int mipLevels, bool italic, CharacterSet characterSet, Precision outputPrecision, FontQuality quality, PitchAndFamily pitchAndFamily, string faceName)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (faceName == null) throw new ArgumentNullException(nameof(faceName));
        #endregion

        nint font;
        new Result(D3DX9Native.D3DXCreateFontW(device.NativePointer, height, (uint)width, (uint)weight, (uint)mipLevels, italic ? 1 : 0, (uint)characterSet, (uint)outputPrecision, (uint)quality, (uint)pitchAndFamily, faceName, &font)).CheckError();
        return font;
    }

    /// <summary>
    /// Draws text.
    /// </summary>
    /// <param name="sprite">The sprite to batch the text with; <c>null</c> to draw immediately.</param>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle to format the text in.</param>
    /// <param name="format">The text formatting options.</param>
    /// <param name="color">The color of the text as an ARGB value.</param>
    /// <returns>The height of the text in pixels.</returns>
    public int DrawString(Sprite? sprite, string text, Rectangle rect, DrawTextFormat format, int color)
    {
        var nativeRect = new NativeRect(rect);
        return DrawText(sprite, text, &nativeRect, format, color);
    }

    /// <summary>
    /// Calculates the size of the rectangle needed to draw text.
    /// </summary>
    /// <param name="sprite">The sprite the text will be batched with; may be <c>null</c>.</param>
    /// <param name="text">The text to measure.</param>
    /// <param name="format">The text formatting options.</param>
    public Rectangle MeasureString(Sprite? sprite, string text, DrawTextFormat format)
    {
        var nativeRect = new NativeRect();
        DrawText(sprite, text, &nativeRect, format | DrawTextFormat.CalcRect, color: 0);
        return Rectangle.FromLTRB(nativeRect.Left, nativeRect.Top, nativeRect.Right, nativeRect.Bottom);
    }

    private int DrawText(Sprite? sprite, string text, NativeRect* rect, DrawTextFormat format, int color)
    {
        #region Sanity checks
        if (text == null) throw new ArgumentNullException(nameof(text));
        #endregion

        int result;
        fixed (char* pText = text)
            result = ((delegate* unmanaged[Stdcall]<nint, nint, char*, int, NativeRect*, uint, int, int>)this[VtDrawTextW])(NativePointer, sprite?.NativePointer ?? 0, pText, text.Length, rect, (uint)format, color);
        GC.KeepAlive(sprite);
        return result;
    }

    /// <summary>
    /// Releases video memory resources before the device is reset.
    /// </summary>
    public void OnLostDevice() => new Result(((delegate* unmanaged[Stdcall]<nint, int>)this[VtOnLostDevice])(NativePointer)).CheckError();

    /// <summary>
    /// Restores video memory resources after the device was reset.
    /// </summary>
    public void OnResetDevice() => new Result(((delegate* unmanaged[Stdcall]<nint, int>)this[VtOnResetDevice])(NativePointer)).CheckError();

    private struct NativeRect(Rectangle rect)
    {
        public int Left = rect.Left, Top = rect.Top, Right = rect.Right, Bottom = rect.Bottom;
    }
}

/// <summary>
/// Text formatting options for <see cref="Font.DrawString"/> (<c>DT_*</c>).
/// </summary>
[Flags]
public enum DrawTextFormat
{
    Top = 0x0,
    Left = 0x0,
    Center = 0x1,
    Right = 0x2,
    VerticalCenter = 0x4,
    Bottom = 0x8,
    WordBreak = 0x10,
    SingleLine = 0x20,
    ExpandTabs = 0x40,
    TabStop = 0x80,
    NoClip = 0x100,
    ExternalLeading = 0x200,
    CalcRect = 0x400,
    NoPrefix = 0x800,
    Internal = 0x1000,
    RtlReading = 0x20000
}

/// <summary>
/// The weight of a <see cref="Font"/> (<c>FW_*</c>).
/// </summary>
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

/// <summary>
/// The character set of a <see cref="Font"/> (<c>*_CHARSET</c>).
/// </summary>
public enum CharacterSet
{
    Ansi = 0,
    Default = 1,
    Symbol = 2
}

/// <summary>
/// How closely a <see cref="Font"/> must match the requested characteristics (<c>OUT_*_PRECIS</c>).
/// </summary>
public enum Precision
{
    Default = 0,
    String = 1,
    Character = 2,
    Stroke = 3,
    TrueType = 4,
    Device = 5,
    Raster = 6,
    TrueTypeOnly = 7,
    Outline = 8,
    ScreenOutline = 9,
    PostScriptOnly = 10
}

/// <summary>
/// The output quality of a <see cref="Font"/> (<c>*_QUALITY</c>).
/// </summary>
public enum FontQuality
{
    Default = 0,
    Draft = 1,
    Proof = 2,
    NonAntialiased = 3,
    Antialiased = 4,
    ClearType = 5,
    ClearTypeNatural = 6
}

/// <summary>
/// The pitch and family of a <see cref="Font"/>.
/// </summary>
[Flags]
public enum PitchAndFamily
{
    Default = 0x0,
    Fixed = 0x1,
    Variable = 0x2,
    Mono = 0x8,
    DontCare = 0x0,
    Roman = 0x10,
    Swiss = 0x20,
    Modern = 0x30,
    Script = 0x40,
    Decorative = 0x50
}
