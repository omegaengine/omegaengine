/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using FontStashSharp;
using FontStashSharp.Interfaces;
using OmegaEngine.Graphics;
using Vortice.Direct3D9;
using Rectangle = System.Drawing.Rectangle;

namespace OmegaGUI.Render;

/// <summary>
/// Renders text with a specific font face and size into a <see cref="SpriteBatch"/>.
/// </summary>
/// <remarks>
/// Glyphs are rasterized on demand into texture atlases shared by all sizes of the same font face.
/// Characters are placed using GDI's hinted advance widths where available, so text takes up the same space as with GDI-based renderers.
/// </remarks>
public sealed class GuiFont
{
    private readonly SpriteFontBase _font;
    private readonly ITexture2DManager _textureManager;

    /// <summary>The advance width of each character from <see cref="SystemFontData.FirstWidthChar"/> on; <c>null</c> to use the unhinted outlines.</summary>
    private readonly int[]? _charWidths;

    /// <summary>The horizontal scale applied to glyphs, so their unhinted outlines fit into the narrower hinted advance widths.</summary>
    private readonly Vector2 _glyphScale = Vector2.One;

    /// <summary>The width of a tab stop in pixels.</summary>
    private readonly float _tabWidth;

    /// <summary>
    /// The height of a line of text in pixels.
    /// </summary>
    public int LineHeight { get; }

    /// <summary>
    /// Creates a font of a specific size.
    /// </summary>
    /// <param name="fontSystem">The font face.</param>
    /// <param name="textureManager">Creates the textures glyphs are rasterized into.</param>
    /// <param name="height">The cell height (ascent plus descent) in pixels.</param>
    /// <param name="charWidths">The advance width of each character from <see cref="SystemFontData.FirstWidthChar"/> on; <c>null</c> to use the unhinted outlines.</param>
    internal GuiFont(FontSystem fontSystem, ITexture2DManager textureManager, int height, int[]? charWidths = null)
    {
        _font = (fontSystem ?? throw new ArgumentNullException(nameof(fontSystem))).GetFont(height);
        _textureManager = textureManager ?? throw new ArgumentNullException(nameof(textureManager));
        _charWidths = charWidths;
        LineHeight = height;

        const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        float alphabetWidth = WalkLine(alphabet, DrawTextFormat.Left);
        if (charWidths != null)
        {
            // Hinting makes characters narrower at small sizes, so condense the glyphs by the same amount on average
            float unhintedWidth = _font.MeasureString(alphabet).X;
            if (unhintedWidth > 0) _glyphScale = new(Math.Clamp(alphabetWidth / unhintedWidth, 0.75f, 1), 1);
        }

        // Like GDI: Tab stops every eight average character widths
        _tabWidth = 8 * alphabetWidth / alphabet.Length;
    }

    /// <summary>
    /// Draws text.
    /// </summary>
    /// <param name="sprite">The sprite batch to add the text to.</param>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle to format the text in.</param>
    /// <param name="format">The text formatting options.</param>
    /// <param name="color">The color of the text as an ARGB value.</param>
    /// <returns>The height of the text in pixels.</returns>
    public int DrawString(SpriteBatch sprite, string text, Rectangle rect, DrawTextFormat format, int color)
    {
        #region Sanity checks
        if (sprite == null) throw new ArgumentNullException(nameof(sprite));
        if (text == null) throw new ArgumentNullException(nameof(text));
        #endregion

        var lines = Layout(text, rect.Width, format);
        int height = lines.Count * LineHeight;

        float y = rect.Top;
        if (format.HasFlag(DrawTextFormat.Bottom)) y = rect.Bottom - height;
        else if (format.HasFlag(DrawTextFormat.VerticalCenter)) y = rect.Top + (rect.Height - height) / 2f;

        var renderer = new Renderer(sprite, _textureManager, format.HasFlag(DrawTextFormat.NoClip) ? null : rect);
        var fontColor = new FSColor((color >> 16) & 0xFF, (color >> 8) & 0xFF, color & 0xFF, (color >> 24) & 0xFF);
        foreach (string line in lines)
        {
            float width = MeasureLine(line, format);
            float x = rect.Left;
            if (format.HasFlag(DrawTextFormat.Right)) x = rect.Right - width;
            else if (format.HasFlag(DrawTextFormat.Center)) x = rect.Left + (rect.Width - width) / 2;

            DrawLine(renderer, line, new(MathF.Round(x), MathF.Round(y)), fontColor, format);
            y += LineHeight;
        }

        return height;
    }

    /// <summary>
    /// Calculates the size of the rectangle needed to draw text.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="format">The text formatting options. <see cref="DrawTextFormat.WordBreak"/> is ignored, since there is no width to wrap at.</param>
    /// <returns>A rectangle with its top-left corner at the origin.</returns>
    public Rectangle MeasureString(string text, DrawTextFormat format)
    {
        #region Sanity checks
        if (text == null) throw new ArgumentNullException(nameof(text));
        #endregion

        var lines = Layout(text, width: 0, format);
        float width = 0;
        foreach (string line in lines) width = Math.Max(width, MeasureLine(line, format));
        return new(0, 0, (int)MathF.Ceiling(width), lines.Count * LineHeight);
    }

    /// <summary>
    /// Splits text into lines, at line breaks and (with <see cref="DrawTextFormat.WordBreak"/>) between words that would exceed the <paramref name="width"/>.
    /// </summary>
    private List<string> Layout(string text, int width, DrawTextFormat format)
    {
        if (format.HasFlag(DrawTextFormat.SingleLine))
            return [text.Replace("\r", "").Replace('\n', ' ')];

        var lines = new List<string>();
        foreach (string paragraph in text.Replace("\r\n", "\n").Split('\n'))
        {
            if (!format.HasFlag(DrawTextFormat.WordBreak) || width <= 0 || MeasureLine(paragraph, format) <= width)
            {
                lines.Add(paragraph);
                continue;
            }

            // Greedily add words to the current line until the next one would not fit anymore
            string current = "";
            foreach (string word in paragraph.Split(' '))
            {
                string candidate = current.Length == 0 ? word : current + " " + word;
                if (current.Length > 0 && MeasureLine(candidate, format) > width)
                {
                    lines.Add(current);
                    current = word;
                }
                else current = candidate;
            }
            lines.Add(current);
        }
        return lines;
    }

    /// <summary>
    /// Measures the width of a single line of text.
    /// </summary>
    private float MeasureLine(string line, DrawTextFormat format) => WalkLine(line, format);

    /// <summary>
    /// Draws a single line of text.
    /// </summary>
    private void DrawLine(Renderer renderer, string line, Vector2 position, FSColor color, DrawTextFormat format)
        => WalkLine(line, format, (character, x) => _font.DrawText(renderer, character, position + new Vector2(x, 0), color, scale: _glyphScale));

    /// <summary>
    /// Places the characters of a single line of text one after another.
    /// </summary>
    /// <param name="line">The text without line breaks.</param>
    /// <param name="format">The text formatting options.</param>
    /// <param name="draw">Called with each character (or surrogate pair) and its horizontal offset; <c>null</c> to only measure.</param>
    /// <returns>The width of the line.</returns>
    private float WalkLine(string line, DrawTextFormat format, Action<string, float>? draw = null)
    {
        float x = 0;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '\t' && format.HasFlag(DrawTextFormat.ExpandTabs))
            {
                x = NextTabStop(x);
                continue;
            }

            string character = char.IsHighSurrogate(c) && i + 1 < line.Length ? line.Substring(i++, 2) : GetCharString(c);
            draw?.Invoke(character, x);
            x += GetAdvance(character);
        }
        return x;
    }

    /// <summary>
    /// Gets the horizontal distance from the start of a character to the start of the next one.
    /// </summary>
    private float GetAdvance(string character)
    {
        if (_charWidths != null && character.Length == 1)
        {
            int index = character[0] - SystemFontData.FirstWidthChar;
            if (index >= 0 && index < _charWidths.Length) return _charWidths[index];
        }
        return _font.MeasureString(character).X;
    }

    private static readonly string?[] _charStrings = new string?[SystemFontData.LastWidthChar + 1];

    /// <summary>
    /// Gets a single character as a string, cached for common characters to avoid allocations on every frame.
    /// </summary>
    private static string GetCharString(char c)
        => c < _charStrings.Length ? _charStrings[c] ??= c.ToString() : c.ToString();

    private float NextTabStop(float x)
        => _tabWidth <= 0 ? x : (MathF.Floor(x / _tabWidth) + 1) * _tabWidth;

    /// <summary>
    /// Adds the glyph quads generated by FontStashSharp to a <see cref="SpriteBatch"/>, clipped to a rectangle.
    /// </summary>
    private sealed class Renderer(SpriteBatch sprite, ITexture2DManager textureManager, RectangleF? clip) : IFontStashRenderer2
    {
        public ITexture2DManager TextureManager => textureManager;

        public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
        {
            float left = topLeft.Position.X, top = topLeft.Position.Y, right = bottomRight.Position.X, bottom = bottomRight.Position.Y;
            float u0 = topLeft.TextureCoordinate.X, v0 = topLeft.TextureCoordinate.Y, u1 = bottomRight.TextureCoordinate.X, v1 = bottomRight.TextureCoordinate.Y;
            if (right <= left || bottom <= top) return;

            if (clip is {} rect)
            {
                if (right <= rect.Left || left >= rect.Right || bottom <= rect.Top || top >= rect.Bottom) return;

                // Cut off the parts outside the rectangle, adjusting the texture coordinates to match
                float du = (u1 - u0) / (right - left), dv = (v1 - v0) / (bottom - top);
                if (left < rect.Left) { u0 += (rect.Left - left) * du; left = rect.Left; }
                if (right > rect.Right) { u1 -= (right - rect.Right) * du; right = rect.Right; }
                if (top < rect.Top) { v0 += (rect.Top - top) * dv; top = rect.Top; }
                if (bottom > rect.Bottom) { v1 -= (bottom - rect.Bottom) * dv; bottom = rect.Bottom; }
            }

            var color = topLeft.Color;
            sprite.DrawQuad((IDirect3DTexture9)texture, RectangleF.FromLTRB(left, top, right, bottom), u0, v0, u1, v1,
                (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B);
        }
    }
}

/// <summary>
/// Creates and updates the glyph atlas textures for FontStashSharp.
/// </summary>
/// <remarks>Writes to a system memory copy and uploads only the modified parts to the GPU.</remarks>
internal sealed class GlyphTextureManager(IDirect3DDevice9 device) : ITexture2DManager, IDisposable
{
    private readonly Dictionary<IDirect3DTexture9, IDirect3DTexture9> _stagingTextures = new(ReferenceEqualityComparer.Instance);

    public object CreateTexture(int width, int height)
    {
        var staging = device.CreateTexture((uint)width, (uint)height, 1, Usage.None, Format.A8R8G8B8, Pool.SystemMemory);
        var texture = device.CreateTexture((uint)width, (uint)height, 1, Usage.None, Format.A8R8G8B8, Pool.Default);
        _stagingTextures.Add(texture, staging);
        return texture;
    }

    public Point GetTextureSize(object texture)
    {
        var description = ((IDirect3DTexture9)texture).GetLevelDesc(0);
        return new((int)description.Width, (int)description.Height);
    }

    public void SetTextureData(object texture, Rectangle bounds, byte[] data)
    {
        var target = (IDirect3DTexture9)texture;
        var staging = _stagingTextures[target];

        // Locking just the modified rectangle marks only that part as dirty for UpdateTexture()
        var locked = staging.LockRectangle(0, bounds, LockFlags.None);
        try
        {
            var row = new byte[bounds.Width * 4];
            for (int y = 0; y < bounds.Height; y++)
            {
                for (int x = 0; x < bounds.Width; x++)
                {
                    // RGBA to BGRA
                    int source = (y * bounds.Width + x) * 4;
                    row[x * 4] = data[source + 2];
                    row[x * 4 + 1] = data[source + 1];
                    row[x * 4 + 2] = data[source];
                    row[x * 4 + 3] = data[source + 3];
                }
                Marshal.Copy(row, 0, locked.DataPointer + y * locked.Pitch, row.Length);
            }
        }
        finally
        {
            staging.UnlockRect(0);
        }

        device.UpdateTexture(staging, target);
    }

    public void Dispose()
    {
        foreach (var (texture, staging) in _stagingTextures)
        {
            texture.Dispose();
            staging.Dispose();
        }
        _stagingTextures.Clear();
    }
}
