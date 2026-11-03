/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Numerics;
using Vortice.Direct3D9;

namespace OmegaEngine.Graphics;

/// <summary>
/// Helper methods for creating, reading from and writing to <see cref="IDirect3DVertexBuffer9"/>s and <see cref="IDirect3DIndexBuffer9"/>s.
/// </summary>
public static class BufferUtils
{
    /// <summary>
    /// Creates a <see cref="IDirect3DVertexBuffer9"/> and fills it with data.
    /// </summary>
    /// <typeparam name="T">The vertex format used in the buffer.</typeparam>
    /// <param name="device">The device to create the buffer on.</param>
    /// <param name="data">The vertexes to fill the buffer with.</param>
    /// <param name="format">The fixed-function vertex format.</param>
    /// <returns>The newly created and filled <see cref="IDirect3DVertexBuffer9"/>.</returns>
    public static unsafe IDirect3DVertexBuffer9 CreateVertexBuffer<T>(this IDirect3DDevice9 device, T[] data, VertexFormat format) where T : unmanaged
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (data == null) throw new ArgumentNullException(nameof(data));
        #endregion

        var buffer = device.CreateVertexBuffer((uint)(sizeof(T) * data.Length), Usage.None, format, Pool.Default);
        try
        {
            buffer.Write(data);
        }
        catch
        {
            buffer.Dispose();
            throw;
        }
        return buffer;
    }

    /// <summary>
    /// Copies the content of a <see cref="IDirect3DVertexBuffer9"/> to an array of structs.
    /// </summary>
    /// <typeparam name="T">The vertex format used in the buffer.</typeparam>
    /// <param name="buffer">The buffer to read from.</param>
    /// <returns>The content of the <paramref name="buffer"/>.</returns>
    public static unsafe T[] Read<T>(this IDirect3DVertexBuffer9 buffer) where T : unmanaged
    {
        #region Sanity checks
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        #endregion

        uint size = buffer.Description.SizeInBytes;
        var data = buffer.LockToPointer(0, size, LockFlags.ReadOnly);
        try
        {
            return new ReadOnlySpan<T>((void*)data, (int)size / sizeof(T)).ToArray();
        }
        finally
        {
            buffer.Unlock();
        }
    }

    /// <summary>
    /// Fills a <see cref="IDirect3DVertexBuffer9"/> with data.
    /// </summary>
    /// <typeparam name="T">The vertex format used in the buffer.</typeparam>
    /// <param name="buffer">The buffer to write to.</param>
    /// <param name="data">The vertexes to fill the buffer with.</param>
    public static unsafe void Write<T>(this IDirect3DVertexBuffer9 buffer, T[] data) where T : unmanaged
    {
        #region Sanity checks
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (data == null) throw new ArgumentNullException(nameof(data));
        #endregion

        var target = buffer.LockToPointer(0, (uint)(sizeof(T) * data.Length));
        try
        {
            data.CopyTo(new Span<T>((void*)target, data.Length));
        }
        finally
        {
            buffer.Unlock();
        }
    }

    /// <summary>
    /// Creates an <see cref="IDirect3DIndexBuffer9"/> and fills it with data.
    /// </summary>
    /// <param name="device">The device to create the buffer on.</param>
    /// <param name="data">The values to fill the buffer with.</param>
    /// <returns>The newly created and filled <see cref="IDirect3DIndexBuffer9"/>.</returns>
    public static IDirect3DIndexBuffer9 CreateIndexBuffer(this IDirect3DDevice9 device, short[] data)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (data == null) throw new ArgumentNullException(nameof(data));
        #endregion

        var buffer = device.CreateIndexBuffer((uint)(sizeof(short) * data.Length), Usage.None, sixteenBit: true, Pool.Default);
        try
        {
            buffer.Write(data);
        }
        catch
        {
            buffer.Dispose();
            throw;
        }
        return buffer;
    }

    /// <summary>
    /// Creates an <see cref="IDirect3DIndexBuffer9"/> and fills it with data.
    /// </summary>
    /// <param name="device">The device to create the buffer on.</param>
    /// <param name="data">The values to fill the buffer with.</param>
    /// <returns>The newly created and filled <see cref="IDirect3DIndexBuffer9"/>.</returns>
    public static IDirect3DIndexBuffer9 CreateIndexBuffer(this IDirect3DDevice9 device, int[] data)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (data == null) throw new ArgumentNullException(nameof(data));
        #endregion

        var buffer = device.CreateIndexBuffer((uint)(sizeof(int) * data.Length), Usage.None, sixteenBit: false, Pool.Default);
        try
        {
            buffer.Write(data);
        }
        catch
        {
            buffer.Dispose();
            throw;
        }
        return buffer;
    }

    /// <summary>
    /// Copies the content of an <see cref="IDirect3DIndexBuffer9"/> to an array of 16-bit values.
    /// </summary>
    /// <param name="buffer">The buffer to read from.</param>
    /// <remarks>Warning! No check is performed to ensure the buffer actually uses 16-bit values.</remarks>
    public static unsafe short[] Read16Bit(this IDirect3DIndexBuffer9 buffer)
    {
        #region Sanity checks
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        #endregion

        uint size = buffer.Description.SizeInBytes;
        var data = buffer.LockToPointer(0, size, LockFlags.ReadOnly);
        try
        {
            return new ReadOnlySpan<short>((void*)data, (int)size / sizeof(short)).ToArray();
        }
        finally
        {
            buffer.Unlock();
        }
    }

    /// <summary>
    /// Copies the content of an <see cref="IDirect3DIndexBuffer9"/> to an array of 32-bit values.
    /// </summary>
    /// <param name="buffer">The buffer to read from.</param>
    /// <remarks>Warning! No check is performed to ensure the buffer actually uses 32-bit values.</remarks>
    public static unsafe int[] Read32Bit(this IDirect3DIndexBuffer9 buffer)
    {
        #region Sanity checks
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        #endregion

        uint size = buffer.Description.SizeInBytes;
        var data = buffer.LockToPointer(0, size, LockFlags.ReadOnly);
        try
        {
            return new ReadOnlySpan<int>((void*)data, (int)size / sizeof(int)).ToArray();
        }
        finally
        {
            buffer.Unlock();
        }
    }

    /// <summary>
    /// Fills an <see cref="IDirect3DIndexBuffer9"/> with 16-bit values.
    /// </summary>
    /// <param name="buffer">The buffer to write to.</param>
    /// <param name="data">The values to fill the buffer with.</param>
    /// <remarks>Warning! No check is performed to ensure the buffer actually uses 16-bit values.</remarks>
    public static unsafe void Write(this IDirect3DIndexBuffer9 buffer, short[] data)
    {
        #region Sanity checks
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (data == null) throw new ArgumentNullException(nameof(data));
        #endregion

        var target = buffer.LockToPointer(0, (uint)(sizeof(short) * data.Length));
        try
        {
            data.CopyTo(new Span<short>((void*)target, data.Length));
        }
        finally
        {
            buffer.Unlock();
        }
    }

    /// <summary>
    /// Fills an <see cref="IDirect3DIndexBuffer9"/> with 32-bit values.
    /// </summary>
    /// <param name="buffer">The buffer to write to.</param>
    /// <param name="data">The values to fill the buffer with.</param>
    /// <remarks>Warning! No check is performed to ensure the buffer actually uses 32-bit values.</remarks>
    public static unsafe void Write(this IDirect3DIndexBuffer9 buffer, int[] data)
    {
        #region Sanity checks
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (data == null) throw new ArgumentNullException(nameof(data));
        #endregion

        var target = buffer.LockToPointer(0, (uint)(sizeof(int) * data.Length));
        try
        {
            data.CopyTo(new Span<int>((void*)target, data.Length));
        }
        finally
        {
            buffer.Unlock();
        }
    }

    /// <summary>
    /// Gets all points contained within a <see cref="IDirect3DVertexBuffer9"/>.
    /// </summary>
    /// <param name="buffer">The <see cref="IDirect3DVertexBuffer9"/> to get points from.</param>
    /// <param name="vertexCount">The total number of vertex contained within <paramref name="buffer"/>.</param>
    /// <returns>An array of points defined by the object.</returns>
    public static unsafe Vector3[] GetPoints(this IDirect3DVertexBuffer9 buffer, int vertexCount)
    {
        #region Sanity checks
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        #endregion

        uint size = buffer.Description.SizeInBytes;
        var data = buffer.LockToPointer(0, size, LockFlags.ReadOnly);
        try
        {
            return GetPoints(data, vertexCount, (int)size / vertexCount);
        }
        finally
        {
            buffer.Unlock();
        }
    }

    /// <summary>
    /// Reads the position, i.e. the first 3 floats, of each vertex.
    /// </summary>
    private static unsafe Vector3[] GetPoints(nint data, int vertexCount, int stride)
    {
        var points = new Vector3[vertexCount];
        for (int i = 0; i < vertexCount; i++)
            points[i] = *(Vector3*)((byte*)data + i * stride);
        return points;
    }

}
