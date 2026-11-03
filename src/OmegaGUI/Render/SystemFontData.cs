/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace OmegaGUI.Render;

/// <summary>
/// Gets the TrueType data of fonts installed on the system, using GDI's font matching.
/// </summary>
internal static class SystemFontData
{
    /// <summary>
    /// Gets the TrueType data of the installed font closest to the requested face and weight.
    /// </summary>
    /// <param name="faceName">The name of the font family, e.g. <c>Arial</c>.</param>
    /// <param name="weight">The weight of the font. Selects e.g. the bold variant of the family.</param>
    /// <returns>The content of a standalone TrueType or OpenType font file.</returns>
    /// <exception cref="IOException">No TrueType font could be retrieved.</exception>
    public static byte[] Get(string faceName, FontWeight weight)
        => WithFont(faceName, weight, height: -16, hdc =>
            // Fonts in a TrueType collection need to be extracted into a standalone font
            GetFontData(hdc, TagTrueTypeCollection, 0, null, 0) == GdiError
                ? GetTable(hdc, 0)
                : ExtractFromCollection(hdc));

    /// <summary>
    /// Gets the advance widths GDI uses for characters at a specific size, which are hinted and therefore narrower than the unhinted outlines at small sizes.
    /// </summary>
    /// <param name="faceName">The name of the font family, e.g. <c>Arial</c>.</param>
    /// <param name="weight">The weight of the font.</param>
    /// <param name="height">The cell height of the font in pixels.</param>
    /// <returns>The width of each character from <see cref="FirstWidthChar"/> on, in pixels.</returns>
    /// <exception cref="IOException">The font could not be created.</exception>
    public static int[] GetCharWidths(string faceName, FontWeight weight, int height)
        => WithFont(faceName, weight, height, hdc =>
        {
            var widths = new int[LastWidthChar - FirstWidthChar + 1];
            if (!GetCharWidth32W(hdc, FirstWidthChar, LastWidthChar, widths)) throw new IOException($"Failed to get character widths of font {faceName}.");
            return widths;
        });

    /// <summary>The first character <see cref="GetCharWidths"/> returns a width for.</summary>
    public const char FirstWidthChar = ' ';

    /// <summary>The last character <see cref="GetCharWidths"/> returns a width for (end of Latin Extended-B).</summary>
    public const char LastWidthChar = 'ɏ';

    /// <summary>
    /// Runs an action with a device context that has the installed font closest to the requested face and weight selected.
    /// </summary>
    /// <param name="faceName">The name of the font family.</param>
    /// <param name="weight">The weight of the font.</param>
    /// <param name="height">The cell height (positive) or character height (negative) in pixels.</param>
    /// <param name="action">Called with the device context.</param>
    private static T WithFont<T>(string faceName, FontWeight weight, int height, Func<nint, T> action)
    {
        #region Sanity checks
        if (faceName == null) throw new ArgumentNullException(nameof(faceName));
        #endregion

        var hdc = CreateCompatibleDC(0);
        if (hdc == 0) throw new IOException("Failed to create GDI device context.");
        var font = CreateFontW(height, 0, 0, 0, (int)weight, 0, 0, 0, DefaultCharset, OutTrueTypeOnlyPrecision, 0, 0, 0, faceName);
        try
        {
            if (font == 0) throw new IOException($"Failed to create font {faceName}.");
            var previous = SelectObject(hdc, font);
            try
            {
                return action(hdc);
            }
            finally
            {
                SelectObject(hdc, previous);
            }
        }
        finally
        {
            if (font != 0) DeleteObject(font);
            DeleteDC(hdc);
        }
    }

    /// <summary>
    /// Gets a table of the selected font; the whole font file for table <c>0</c>.
    /// </summary>
    private static byte[] GetTable(nint hdc, uint tag)
    {
        uint size = GetFontData(hdc, tag, 0, null, 0);
        if (size is GdiError or 0) throw new IOException("The font does not contain TrueType data.");

        var data = new byte[size];
        if (GetFontData(hdc, tag, 0, data, size) != size) throw new IOException("Failed to read TrueType data.");
        return data;
    }

    /// <summary>
    /// Builds a standalone font file from the tables of the selected font in a TrueType collection.
    /// </summary>
    private static byte[] ExtractFromCollection(nint hdc)
    {
        // For fonts in collections GDI returns the font's own offset table, but its table offsets are relative to the collection
        var header = new byte[12];
        if (GetFontData(hdc, 0, 0, header, 12) != 12) throw new IOException("Failed to read TrueType header.");
        int tableCount = BinaryPrimitives.ReadUInt16BigEndian(header.AsSpan(4));

        var directory = new byte[12 + 16 * tableCount];
        if (GetFontData(hdc, 0, 0, directory, (uint)directory.Length) != directory.Length) throw new IOException("Failed to read TrueType table directory.");

        var tables = new List<(uint Tag, uint Checksum, byte[] Data)>();
        for (int i = 0; i < tableCount; i++)
        {
            var record = directory.AsSpan(12 + 16 * i);
            uint tag = BinaryPrimitives.ReadUInt32BigEndian(record);
            uint checksum = BinaryPrimitives.ReadUInt32BigEndian(record[4..]);
            tables.Add((tag, checksum, GetTable(hdc, BinaryPrimitives.ReverseEndianness(tag))));
        }

        // Lay out the tables again, 4-byte aligned, after the new directory
        int offset = directory.Length;
        var layout = new List<int>();
        foreach (var table in tables)
        {
            layout.Add(offset);
            offset += (table.Data.Length + 3) & ~3;
        }

        var result = new byte[offset];
        header.AsSpan(0, 12).CopyTo(result);
        for (int i = 0; i < tables.Count; i++)
        {
            var record = result.AsSpan(12 + 16 * i);
            BinaryPrimitives.WriteUInt32BigEndian(record, tables[i].Tag);
            BinaryPrimitives.WriteUInt32BigEndian(record[4..], tables[i].Checksum);
            BinaryPrimitives.WriteUInt32BigEndian(record[8..], (uint)layout[i]);
            BinaryPrimitives.WriteUInt32BigEndian(record[12..], (uint)tables[i].Data.Length);
            tables[i].Data.CopyTo(result, layout[i]);
        }
        return result;
    }

    #region Native
    private const uint GdiError = 0xFFFFFFFF;
    private const uint TagTrueTypeCollection = 0x66637474; // "ttcf" as a little-endian DWORD
    private const uint DefaultCharset = 1, OutTrueTypeOnlyPrecision = 7;

    [DllImport("gdi32.dll")]
    private static extern nint CreateCompatibleDC(nint hdc);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(nint hdc);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    private static extern nint CreateFontW(int height, int width, int escapement, int orientation, int weight, uint italic, uint underline, uint strikeOut, uint charSet, uint outPrecision, uint clipPrecision, uint quality, uint pitchAndFamily, string faceName);

    [DllImport("gdi32.dll")]
    private static extern nint SelectObject(nint hdc, nint obj);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(nint obj);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    private static extern bool GetCharWidth32W(nint hdc, uint first, uint last, [Out] int[] widths);

    [DllImport("gdi32.dll")]
    private static extern uint GetFontData(nint hdc, uint table, uint offset, byte[]? buffer, uint length);
    #endregion
}
