/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Numerics;
using Vortice.Direct3D9;
using OmegaEngine.D3DX;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics;

/// <summary>
/// Helper methods for creating, reading from and writing to <see cref="IDirect3DVertexBuffer9"/>s, <see cref="IDirect3DIndexBuffer9"/>s and attribute buffers.
/// </summary>
public static class BufferUtils
{
    /// <summary>
    /// Replaces the pool flags in mesh creation options with <see cref="Pool.SystemMemory"/>.
    /// </summary>
    public static MeshFlags ToSystemMemory(this MeshFlags options)
        => (options & ~(MeshFlags.Managed | MeshFlags.Dynamic | MeshFlags.WriteOnly)) | MeshFlags.SystemMemory;

    /// <summary>
    /// Strips pool and write-only flags from mesh creation options, so a clone ends up in the lockable <see cref="Pool.Default"/> pool.
    /// </summary>
    /// <remarks>All other flags, most importantly <see cref="MeshFlags.Use32Bit"/>, are preserved.</remarks>
    public static MeshFlags ToDefaultPool(this MeshFlags options)
        => options & ~(MeshFlags.Managed | MeshFlags.SystemMemory | MeshFlags.Dynamic | MeshFlags.WriteOnly);

    /// <summary>
    /// Creates a copy of a <see cref="Mesh"/> in system memory, where the CPU can read and modify it at full speed.
    /// </summary>
    public static Mesh ToSystemMemory(this Mesh mesh)
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        #endregion

        return mesh.Clone(mesh.Device, mesh.CreationOptions.ToSystemMemory(), mesh.GetDeclaration());
    }

    /// <summary>
    /// Creates a copy of a <see cref="Mesh"/> in <see cref="Pool.Default"/>, ready for rendering.
    /// </summary>
    /// <param name="mesh">The finished system memory mesh to publish.</param>
    /// <param name="writeOnly">Add <see cref="MeshFlags.WriteOnly"/> for the fastest possible rendering. Prevents any CPU read-back.</param>
    public static Mesh ToDefaultPool(this Mesh mesh, bool writeOnly = true)
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        #endregion

        var options = mesh.CreationOptions.ToDefaultPool();
        if (writeOnly) options |= MeshFlags.WriteOnly;
        return mesh.Clone(mesh.Device, options, mesh.GetDeclaration());
    }

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
    /// Copies the content of the <see cref="IDirect3DVertexBuffer9"/> of a <see cref="Mesh"/> to an array of structs.
    /// </summary>
    /// <typeparam name="T">The vertex format used in the buffer.</typeparam>
    /// <param name="mesh">The <see cref="Mesh"/> containing the buffer to read from.</param>
    /// <returns>The content of the buffer in <paramref name="mesh"/>.</returns>
    public static unsafe T[] ReadVertexBuffer<T>(this Mesh mesh) where T : unmanaged
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        #endregion

        var data = mesh.LockVertexBuffer(LockFlags.ReadOnly);
        try
        {
            return new ReadOnlySpan<T>((void*)data, mesh.VertexCount).ToArray();
        }
        finally
        {
            mesh.UnlockVertexBuffer();
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
    /// Fills the <see cref="IDirect3DVertexBuffer9"/> of a <see cref="Mesh"/> with data.
    /// </summary>
    /// <typeparam name="T">The vertex format used in the buffer.</typeparam>
    /// <param name="mesh">The <see cref="Mesh"/> containing the buffer to write to.</param>
    /// <param name="data">The vertexes to fill the buffer with.</param>
    public static unsafe void WriteVertexBuffer<T>(this Mesh mesh, T[] data) where T : unmanaged
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        if (data == null) throw new ArgumentNullException(nameof(data));
        #endregion

        var target = mesh.LockVertexBuffer(LockFlags.None);
        try
        {
            data.CopyTo(new Span<T>((void*)target, data.Length));
        }
        finally
        {
            mesh.UnlockVertexBuffer();
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
    /// Copies the content of the <see cref="IDirect3DIndexBuffer9"/> of a <see cref="Mesh"/> to an array of 32-bit values (16-bit values are automatically converted).
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/> containing the buffer to read from.</param>
    public static unsafe int[] ReadIndexBuffer(this Mesh mesh)
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        #endregion

        int indexCount = mesh.FaceCount * 3;
        var data = mesh.LockIndexBuffer(LockFlags.ReadOnly);
        try
        {
            if (mesh.CreationOptions.HasFlag(MeshFlags.Use32Bit))
            { // 32-bit values
                return new ReadOnlySpan<int>((void*)data, indexCount).ToArray();
            }
            else
            { // 16-bit values
                var source = new ReadOnlySpan<ushort>((void*)data, indexCount); // 16-bit indexes are unsigned, so avoid sign-extending values above 32767
                var ret = new int[indexCount];
                for (int i = 0; i < indexCount; i++)
                    ret[i] = source[i];
                return ret;
            }
        }
        finally
        {
            mesh.UnlockIndexBuffer();
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
    /// Fills the <see cref="IDirect3DIndexBuffer9"/> of a <see cref="Mesh"/> with 16-bit values.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/> containing the buffer to write to.</param>
    /// <param name="data">The values to fill the buffer with.</param>
    public static unsafe void WriteIndexBuffer(this Mesh mesh, short[] data)
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (mesh.CreationOptions.HasFlag(MeshFlags.Use32Bit)) throw new ArgumentException(Resources.MeshIndexBufferNot16bit, nameof(mesh));
        #endregion

        var target = mesh.LockIndexBuffer(LockFlags.None);
        try
        {
            data.CopyTo(new Span<short>((void*)target, data.Length));
        }
        finally
        {
            mesh.UnlockIndexBuffer();
        }
    }

    /// <summary>
    /// Fills the <see cref="IDirect3DIndexBuffer9"/> of a <see cref="Mesh"/> with 32-bit values.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/> containing the buffer to write to.</param>
    /// <param name="data">The values to fill the buffer with.</param>
    public static unsafe void WriteIndexBuffer(this Mesh mesh, int[] data)
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (!mesh.CreationOptions.HasFlag(MeshFlags.Use32Bit)) throw new ArgumentException(Resources.MeshIndexBufferNot32bit, nameof(mesh));
        #endregion

        var target = mesh.LockIndexBuffer(LockFlags.None);
        try
        {
            data.CopyTo(new Span<int>((void*)target, data.Length));
        }
        finally
        {
            mesh.UnlockIndexBuffer();
        }
    }

    /// <summary>
    /// Copies the content of the attribute buffer of a <see cref="Mesh"/> to an array of 32-bit values.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/> containing the buffer to read from.</param>
    public static unsafe int[] ReadAttributeBuffer(this Mesh mesh)
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        #endregion

        var data = mesh.LockAttributeBuffer(LockFlags.ReadOnly);
        try
        {
            return new ReadOnlySpan<int>((void*)data, mesh.FaceCount).ToArray();
        }
        finally
        {
            mesh.UnlockAttributeBuffer();
        }
    }

    /// <summary>
    /// Fills the attribute buffer of a <see cref="Mesh"/> with 32-bit values.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/> containing the buffer to write to.</param>
    /// <param name="data">The values to fill the buffer with.</param>
    public static unsafe void WriteAttributeBuffer(this Mesh mesh, int[] data)
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        if (data == null) throw new ArgumentNullException(nameof(data));
        #endregion

        var target = mesh.LockAttributeBuffer(LockFlags.None);
        try
        {
            data.CopyTo(new Span<int>((void*)target, data.Length));
        }
        finally
        {
            mesh.UnlockAttributeBuffer();
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
    /// Gets all points contained within a <see cref="Mesh"/>.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/> to get points from.</param>
    /// <returns>An array of points defined by the object.</returns>
    public static Vector3[] GetPoints(this Mesh mesh)
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        #endregion

        var data = mesh.LockVertexBuffer(LockFlags.ReadOnly);
        try
        {
            return GetPoints(data, mesh.VertexCount, mesh.BytesPerVertex); // Use the vertex stride rather than the vertex format, because meshes with a custom vertex declaration have no equivalent FVF
        }
        finally
        {
            mesh.UnlockVertexBuffer();
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

    /// <summary>
    /// Gets all points contained within a specific subset of a <see cref="Mesh"/>.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/> to get points from.</param>
    /// <param name="subset">The subset to get points for.</param>
    /// <returns>An array of points defined by the subset. Returns an empty array if the subset has no faces.</returns>
    public static unsafe Vector3[] GetPoints(this Mesh mesh, int subset)
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        if (subset < 0) throw new ArgumentOutOfRangeException(nameof(subset), "Subset must be non-negative.");
        #endregion

        // Read mesh data to determine which vertices belong to this subset
        int[] attributes = mesh.ReadAttributeBuffer();
        int[] indices = mesh.ReadIndexBuffer();

        // Collect vertex indices for this subset (may contain duplicates from shared vertices)
        var vertexIndices = new List<int>();
        for (int faceIndex = 0; faceIndex < mesh.FaceCount; faceIndex++)
        {
            if (attributes[faceIndex] == subset)
            {
                // Add the three vertices of this face (triangle)
                int baseIndex = faceIndex * 3;
                vertexIndices.Add(indices[baseIndex]);
                vertexIndices.Add(indices[baseIndex + 1]);
                vertexIndices.Add(indices[baseIndex + 2]);
            }
        }
        if (vertexIndices.Count == 0) return [];

        // Extract vertex positions
        var positions = new Vector3[vertexIndices.Count];
        int vertexCount = mesh.VertexCount, stride = mesh.BytesPerVertex;
        var data = (byte*)mesh.LockVertexBuffer(LockFlags.ReadOnly);
        try
        {
            for (int i = 0; i < vertexIndices.Count; i++)
            {
                int index = vertexIndices[i];
                if (index < 0 || index >= vertexCount)
                    throw new InvalidOperationException($"Vertex index {index} is out of range [0, {vertexCount})");

                // Read the first 3 floats as the position (standard vertex format)
                positions[i] = *(Vector3*)(data + index * stride);
            }
        }
        finally
        {
            mesh.UnlockVertexBuffer();
        }

        return positions;
    }
}
