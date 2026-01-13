/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics;

/// <summary>
/// Helper methods for creating, reading from and writing to <see cref="VertexBuffer"/>s, <see cref="IndexBuffer"/>s and attribute buffers.
/// </summary>
public static class BufferUtils
{
    /// <summary>
    /// Creates a <see cref="VertexBuffer"/> and fills it with data.
    /// </summary>
    /// <typeparam name="T">The vertex format used in the buffer.</typeparam>
    /// <param name="device">The device to create the buffer on.</param>
    /// <param name="data">The vertexes to fill the buffer with.</param>
    /// <param name="format">The fixed-function vertex format.</param>
    /// <returns>The newly created and filled <see cref="VertexBuffer"/>.</returns>
    public static VertexBuffer CreateVertexBuffer<T>(this Device device, T[] data, VertexFormat format) where T : struct
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (data == null) throw new ArgumentNullException(nameof(data));
        #endregion

        var buffer = new VertexBuffer(device, Marshal.SizeOf(typeof(T)) * data.Length, Usage.WriteOnly, format, Pool.Managed);
        buffer.Write(data);
        return buffer;
    }

    /// <summary>
    /// Copies the content of a <see cref="VertexBuffer"/> to an array of structs.
    /// </summary>
    /// <typeparam name="T">The vertex format used in the buffer.</typeparam>
    /// <param name="buffer">The buffer to read from.</param>
    /// <returns>The content of the <paramref name="buffer"/>.</returns>
    public static T[] Read<T>(this VertexBuffer buffer) where T : struct
    {
        #region Sanity checks
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        #endregion

        var vertexStream = buffer.Lock(0, buffer.Description.SizeInBytes, LockFlags.ReadOnly);
        int vertexCount = buffer.Description.SizeInBytes / Marshal.SizeOf(typeof(T));
        var ret = vertexStream.ReadRange<T>(vertexCount);
        buffer.Unlock();
        vertexStream.Dispose();

        return ret;
    }

    /// <summary>
    /// Copies the content of the <see cref="VertexBuffer"/> of a <see cref="Mesh"/> to an array of structs.
    /// </summary>
    /// <typeparam name="T">The vertex format used in the buffer.</typeparam>
    /// <param name="mesh">The <see cref="Mesh"/> containing the buffer to read from.</param>
    /// <returns>The content of the buffer in <paramref name="mesh"/>.</returns>
    public static T[] ReadVertexBuffer<T>(this Mesh mesh) where T : struct
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        #endregion

        var vertexStream = mesh.LockVertexBuffer(LockFlags.ReadOnly);
        var ret = vertexStream.ReadRange<T>(mesh.VertexCount);
        mesh.UnlockVertexBuffer();
        vertexStream.Dispose();

        return ret;
    }

    /// <summary>
    /// Fills a <see cref="VertexBuffer"/> with data.
    /// </summary>
    /// <typeparam name="T">The vertex format used in the buffer.</typeparam>
    /// <param name="buffer">The buffer to write to.</param>
    /// <param name="data">The vertexes to fill the buffer with.</param>
    public static void Write<T>(this VertexBuffer buffer, T[] data) where T : struct
    {
        #region Sanity checks
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (data == null) throw new ArgumentNullException(nameof(data));
        #endregion

        using var vertexStream = buffer.Lock(0, Marshal.SizeOf(typeof(T)) * data.Length, LockFlags.None);
        vertexStream.WriteRange(data);
        buffer.Unlock();
    }

    /// <summary>
    /// Fills the <see cref="VertexBuffer"/> of a <see cref="Mesh"/> with data.
    /// </summary>
    /// <typeparam name="T">The vertex format used in the buffer.</typeparam>
    /// <param name="mesh">The <see cref="Mesh"/> containing the buffer to write to.</param>
    /// <param name="data">The vertexes to fill the buffer with.</param>
    public static void WriteVertexBuffer<T>(this Mesh mesh, T[] data) where T : struct
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        if (data == null) throw new ArgumentNullException(nameof(data));
        #endregion

        using var vertexStream = mesh.LockVertexBuffer(LockFlags.None);
        vertexStream.WriteRange(data);
        mesh.UnlockVertexBuffer();
    }

    /// <summary>
    /// Creates an <see cref="IndexBuffer"/> and fills it with data.
    /// </summary>
    /// <param name="device">The device to create the buffer on.</param>
    /// <param name="data">The values to fill the buffer with.</param>
    /// <returns>The newly created and filled <see cref="IndexBuffer"/>.</returns>
    public static IndexBuffer CreateIndexBuffer(this Device device, short[] data)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (data == null) throw new ArgumentNullException(nameof(data));
        #endregion

        var buffer = new IndexBuffer(device, sizeof(short) * data.Length, Usage.WriteOnly, Pool.Managed, true);
        buffer.Write(data);
        return buffer;
    }

    /// <summary>
    /// Creates an <see cref="IndexBuffer"/> and fills it with data.
    /// </summary>
    /// <param name="device">The device to create the buffer on.</param>
    /// <param name="data">The values to fill the buffer with.</param>
    /// <returns>The newly created and filled <see cref="IndexBuffer"/>.</returns>
    public static IndexBuffer CreateIndexBuffer(this Device device, int[] data)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (data == null) throw new ArgumentNullException(nameof(data));
        #endregion

        var buffer = new IndexBuffer(device, sizeof(int) * data.Length, Usage.WriteOnly, Pool.Managed, false);
        buffer.Write(data);
        return buffer;
    }

    /// <summary>
    /// Copies the content of an <see cref="IndexBuffer"/> to an array of 16-bit values.
    /// </summary>
    /// <param name="buffer">The buffer to read from.</param>
    /// <remarks>Warning! No check is performed to ensure the buffer actually uses 16-bit values.</remarks>
    public static short[] Read16Bit(this IndexBuffer buffer)
    {
        #region Sanity checks
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        #endregion

        var indexStream = buffer.Lock(0, buffer.Description.SizeInBytes, LockFlags.ReadOnly);
        int indexCount = buffer.Description.SizeInBytes / sizeof(short);
        short[]? ret = indexStream.ReadRange<short>(indexCount);
        buffer.Unlock();
        indexStream.Dispose();

        return ret;
    }

    /// <summary>
    /// Copies the content of an <see cref="IndexBuffer"/> to an array of 32-bit values.
    /// </summary>
    /// <param name="buffer">The buffer to read from.</param>
    /// <remarks>Warning! No check is performed to ensure the buffer actually uses 32-bit values.</remarks>
    public static int[] Read32Bit(this IndexBuffer buffer)
    {
        #region Sanity checks
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        #endregion

        var indexStream = buffer.Lock(0, buffer.Description.SizeInBytes, LockFlags.ReadOnly);
        int indexCount = buffer.Description.SizeInBytes / sizeof(int);
        int[] ret = indexStream.ReadRange<int>(indexCount);
        buffer.Unlock();
        indexStream.Dispose();

        return ret;
    }

    /// <summary>
    /// Copies the content of the <see cref="IndexBuffer"/> of a <see cref="Mesh"/> to an array of 32-bit values (16-bit values are automatically converted).
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/> containing the buffer to read from.</param>
    public static int[] ReadIndexBuffer(this Mesh mesh)
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        #endregion

        int indexCount = mesh.FaceCount * 3;
        int[] ret;
        using var indexStream = mesh.LockIndexBuffer(LockFlags.ReadOnly);
        if ((mesh.CreationOptions).HasFlag(MeshFlags.Use32Bit))
        { // 32-bit values
            ret = indexStream.ReadRange<int>(indexCount);
        }
        else
        { // 16-bit values
            ret = new int[indexCount];
            short[]? temp = indexStream.ReadRange<short>(indexCount);
            for (int i = 0; i < indexCount; i++)
                ret[i] = temp[i];
        }
        mesh.UnlockIndexBuffer();

        return ret;
    }

    /// <summary>
    /// Fills an <see cref="IndexBuffer"/> with 16-bit values.
    /// </summary>
    /// <param name="buffer">The buffer to write to.</param>
    /// <param name="data">The values to fill the buffer with.</param>
    /// <remarks>Warning! No check is performed to ensure the buffer actually uses 16-bit values.</remarks>
    public static void Write(this IndexBuffer buffer, short[] data)
    {
        #region Sanity checks
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (data == null) throw new ArgumentNullException(nameof(data));
        #endregion

        using var indexStream = buffer.Lock(0, sizeof(short) * data.Length, LockFlags.None);
        indexStream.WriteRange(data);
        buffer.Unlock();
    }

    /// <summary>
    /// Fills an <see cref="IndexBuffer"/> with 32-bit values.
    /// </summary>
    /// <param name="buffer">The buffer to write to.</param>
    /// <param name="data">The values to fill the buffer with.</param>
    /// <remarks>Warning! No check is performed to ensure the buffer actually uses 32-bit values.</remarks>
    public static void Write(this IndexBuffer buffer, int[] data)
    {
        #region Sanity checks
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (data == null) throw new ArgumentNullException(nameof(data));
        #endregion

        using var indexStream = buffer.Lock(0, sizeof(int) * data.Length, LockFlags.None);
        indexStream.WriteRange(data);
        buffer.Unlock();
    }

    /// <summary>
    /// Fills the <see cref="IndexBuffer"/> of a <see cref="Mesh"/> with 16-bit values.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/> containing the buffer to write to.</param>
    /// <param name="data">The values to fill the buffer with.</param>
    public static void WriteIndexBuffer(this Mesh mesh, short[] data)
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        if (data == null) throw new ArgumentNullException(nameof(data));
        if ((mesh.CreationOptions).HasFlag(MeshFlags.Use32Bit)) throw new ArgumentException(Resources.MeshIndexBufferNot16bit, nameof(mesh));
        #endregion

        using var indexStream = mesh.LockIndexBuffer(LockFlags.None);
        indexStream.WriteRange(data);
        mesh.UnlockIndexBuffer();
    }

    /// <summary>
    /// Fills the <see cref="IndexBuffer"/> of a <see cref="Mesh"/> with 32-bit values.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/> containing the buffer to write to.</param>
    /// <param name="data">The values to fill the buffer with.</param>
    public static void WriteIndexBuffer(this Mesh mesh, int[] data)
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (!(mesh.CreationOptions).HasFlag(MeshFlags.Use32Bit)) throw new ArgumentException(Resources.MeshIndexBufferNot32bit, nameof(mesh));
        #endregion

        using var indexStream = mesh.LockIndexBuffer(LockFlags.None);
        indexStream.WriteRange(data);
        mesh.UnlockIndexBuffer();
    }

    /// <summary>
    /// Copies the content of the attribute buffer of a <see cref="Mesh"/> to an array of 32-bit values.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/> containing the buffer to read from.</param>
    public static int[] ReadAttributeBuffer(this Mesh mesh)
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        #endregion

        using var attributeStream = mesh.LockAttributeBuffer(LockFlags.ReadOnly);
        int[] ret = attributeStream.ReadRange<int>(mesh.FaceCount);
        mesh.UnlockAttributeBuffer();
        return ret;
    }

    /// <summary>
    /// Fills the attribute buffer of a <see cref="Mesh"/> with 32-bit values.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/> containing the buffer to write to.</param>
    /// <param name="data">The values to fill the buffer with.</param>
    public static void WriteAttributeBuffer(this Mesh mesh, int[] data)
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        if (data == null) throw new ArgumentNullException(nameof(data));
        #endregion

        using var attributeStream = mesh.LockAttributeBuffer(LockFlags.None);
        attributeStream.WriteRange(data);
        mesh.UnlockAttributeBuffer();
    }

    /// <summary>
    /// Gets all points contained within a <see cref="VertexBuffer"/>.
    /// </summary>
    /// <param name="buffer">The <see cref="VertexBuffer"/> to get points from.</param>
    /// <param name="vertexCount">The total number of vertex contained within <paramref name="buffer"/>.</param>
    /// <returns>An array of points defined by the object.</returns>
    public static Vector3[] GetPoints(this VertexBuffer buffer, int vertexCount)
    {
        #region Sanity checks
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        #endregion

        var vertexStream = buffer.Lock(0, buffer.Description.SizeInBytes, LockFlags.ReadOnly);
        var points = D3DX.GetVectors(vertexStream, vertexCount, buffer.Description.SizeInBytes / vertexCount);
        buffer.Unlock();
        vertexStream.Dispose();

        return points;
    }

    /// <summary>
    /// Gets all points contained within a <see cref="Mesh"/>.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/> to get points from.</param>
    /// <returns>An array of points defined by the object.</returns>
    public static Vector3[] GetPoints(this Mesh mesh)
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        #endregion

        var vertexStream = mesh.LockVertexBuffer(LockFlags.ReadOnly);
        var points = D3DX.GetVectors(vertexStream, mesh.VertexCount, mesh.VertexFormat);
        mesh.UnlockVertexBuffer();
        vertexStream.Dispose();

        return points;
    }
}
