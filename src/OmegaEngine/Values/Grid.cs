/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common.Streams;

namespace OmegaEngine.Values;

/// <summary>
/// A 2D grid of values that can be stored in PNG files.
/// </summary>
/// <typeparam name="T">The type of values stored in the grid.</typeparam>
public abstract class Grid<T>
    where T : struct
{
    /// <summary>
    /// The internal array containing the values.
    /// </summary>
    [NotNull]
    protected internal readonly T[,] Data;

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
    protected Grid(int width, int height)
    {
        Data = new T[width, height];
    }

    /// <summary>
    /// Creates a new grid based on an existing array.
    /// </summary>
    /// <param name="data">Used as the internal array (no defensive copy). Do not modify once passing in!</param>
    protected Grid(T[,] data)
    {
        Data = data;
    }

    public virtual T this[int x, int y] { get => Data[x, y]; set => Data[x, y] = value; }

    /// <summary>
    /// Reads a value in the grid and automatically clamps out of bound values of <paramref name="x"/> or <paramref name="y"/>.
    /// </summary>
    public T ClampedRead(int x, int y) => Data[x.Clamp(0, Data.GetUpperBound(0)), y.Clamp(0, Data.GetUpperBound(1))];

    /// <summary>
    /// Saves the grid to a PNG file.
    /// </summary>
    public void Save([NotNull, Localizable(false)] string path)
    {
        using var bitmap = GenerateBitmap();
        bitmap.Save(path, ImageFormat.Png);
    }

    /// <summary>
    /// Saves the grid to a PNG stream.
    /// </summary>
    public void Save([NotNull] Stream stream)
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
