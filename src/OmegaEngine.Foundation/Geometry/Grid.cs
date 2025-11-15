/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NanoByte.Common;
using NanoByte.Common.Streams;

namespace OmegaEngine.Foundation.Geometry;

/// <summary>
/// A 2D grid of values that can be stored in PNG files.
/// </summary>
/// <typeparam name="T">The type of values stored in the grid.</typeparam>
/// <param name="data">Used as the internal array (no defensive copy). Do not modify once passing in!</param>
public abstract class Grid<T>(T[,] data)
    where T : struct
{
    /// <summary>
    /// The internal array containing the values.
    /// </summary>
    protected internal readonly T[,] Data = data;

    /// <summary>
    /// The width of the grid (number of values along the X axis).
    /// </summary>
    public int Width => Data.GetLength(0);

    /// <summary>
    /// The height of the grid (number of values along the Y axis).
    /// </summary>
    public int Height => Data.GetLength(1);

    /// <summary>
    /// Creates a new empty grid.
    /// </summary>
    /// <param name="width">The width of the grid (number of values along the X axis).</param>
    /// <param name="height">The height of the grid (number of values along the Y axis).</param>
    protected Grid(int width, int height) : this(new T[width, height])
    {}

    public virtual T this[int x, int y] { get => Data[x, y]; set => Data[x, y] = value; }

    /// <summary>
    /// Reads a value in the grid and automatically clamps out of bound values of <paramref name="x"/> or <paramref name="y"/>.
    /// </summary>
    [Pure]
    public T ClampedRead(int x, int y) => Data[x.Clamp(0, Data.GetUpperBound(0)), y.Clamp(0, Data.GetUpperBound(1))];

    /// <summary>
    /// Saves the grid to a PNG file.
    /// </summary>
    public void Save([Localizable(false)] string path)
    {
        using var bitmap = GenerateBitmap();
        bitmap.Save(path, ImageFormat.Png);
    }

    /// <summary>
    /// Saves the grid to a PNG stream.
    /// </summary>
    public void Save(Stream stream)
    {
        // NOTE: Use intermediate RAM buffer because writing a PNG directly to a ZIP won't work
        using var bitmap = GenerateBitmap();
        using var memory = new MemoryStream();
        bitmap.Save(memory, ImageFormat.Png);
        memory.CopyToEx(stream);
    }

    /// <summary>
    /// Generates a bitmap representation of the grid.
    /// </summary>
    public abstract Bitmap GenerateBitmap();
}
