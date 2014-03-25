/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;
using Common;
using Common.Values;
using SlimDX;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics
{
    /// <summary>
    /// Helper methods for creating, reading from and writing to <see cref="VertexBuffer"/>s and <see cref="IndexBuffer"/>s.
    /// </summary>
    internal static class BufferHelper
    {
        #region Create VB
        /// <summary>
        /// Creates a <see cref="VertexBuffer"/> and fills it with data.
        /// </summary>
        /// <typeparam name="T">The vertex format used in the buffer.</typeparam>
        /// <param name="device">The device to create the buffer on.</param>
        /// <param name="data">The vertexes to fill the buffer with.</param>
        /// <param name="format">The fixed-function vertex format.</param>
        /// <returns>The newly created and filled <see cref="VertexBuffer"/>.</returns>
        internal static VertexBuffer CreateVertexBuffer<T>(Device device, T[] data, VertexFormat format) where T : struct
        {
            #region Sanity checks
            if (device == null) throw new ArgumentNullException("device");
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            var buffer = new VertexBuffer(device, Marshal.SizeOf(typeof(T)) * data.Length, Usage.WriteOnly, format, Pool.Managed);
            WriteVertexBuffer(buffer, data);

            return buffer;
        }
        #endregion

        #region Read VB
        /// <summary>
        /// Copies the content of a <see cref="VertexBuffer"/> to an array of structs.
        /// </summary>
        /// <typeparam name="T">The vertex format used in the buffer.</typeparam>
        /// <param name="buffer">The buffer to read from.</param>
        /// <returns>The content of the <paramref name="buffer"/>.</returns>
        internal static T[] ReadVertexBuffer<T>(VertexBuffer buffer) where T : struct
        {
            #region Sanity checks
            if (buffer == null) throw new ArgumentNullException("buffer");
            #endregion

            DataStream vertexStream = buffer.Lock(0, buffer.Description.SizeInBytes, LockFlags.ReadOnly);
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
        internal static T[] ReadVertexBuffer<T>(Mesh mesh) where T : struct
        {
            #region Sanity checks
            if (mesh == null) throw new ArgumentNullException("mesh");
            #endregion

            DataStream vertexStream = mesh.LockVertexBuffer(LockFlags.ReadOnly);
            var ret = vertexStream.ReadRange<T>(mesh.VertexCount);
            mesh.UnlockVertexBuffer();
            vertexStream.Dispose();

            return ret;
        }
        #endregion

        #region Write VB
        /// <summary>
        /// Fills a <see cref="VertexBuffer"/> with data.
        /// </summary>
        /// <typeparam name="T">The vertex format used in the buffer.</typeparam>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="data">The vertexes to fill the buffer with.</param>
        internal static void WriteVertexBuffer<T>(VertexBuffer buffer, T[] data) where T : struct
        {
            #region Sanity checks
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            using (DataStream vertexStream = buffer.Lock(0, Marshal.SizeOf(typeof(T)) * data.Length, LockFlags.None))
            {
                vertexStream.WriteRange(data);
                buffer.Unlock();
            }
        }

        /// <summary>
        /// Fills the <see cref="VertexBuffer"/> of a <see cref="Mesh"/> with data.
        /// </summary>
        /// <typeparam name="T">The vertex format used in the buffer.</typeparam>
        /// <param name="mesh">The <see cref="Mesh"/> containing the buffer to write to.</param>
        /// <param name="data">The vertexes to fill the buffer with.</param>
        internal static void WriteVertexBuffer<T>(Mesh mesh, T[] data) where T : struct
        {
            #region Sanity checks
            if (mesh == null) throw new ArgumentNullException("mesh");
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            using (DataStream vertexStream = mesh.LockVertexBuffer(LockFlags.None))
            {
                vertexStream.WriteRange(data);
                mesh.UnlockVertexBuffer();
            }
        }
        #endregion

        //--------------------//

        #region Create IB
        /// <summary>
        /// Creates an <see cref="IndexBuffer"/> and fills it with data.
        /// </summary>
        /// <param name="device">The device to create the buffer on.</param>
        /// <param name="data">The values to fill the buffer with.</param>
        /// <returns>The newly created and filled <see cref="IndexBuffer"/>.</returns>
        internal static IndexBuffer CreateIndexBuffer(Device device, short[] data)
        {
            #region Sanity checks
            if (device == null) throw new ArgumentNullException("device");
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            var buffer = new IndexBuffer(device, sizeof(short) * data.Length, Usage.WriteOnly, Pool.Managed, true);
            WriteIndexBuffer(buffer, data);

            return buffer;
        }

        /// <summary>
        /// Creates an <see cref="IndexBuffer"/> and fills it with data.
        /// </summary>
        /// <param name="device">The device to create the buffer on.</param>
        /// <param name="data">The values to fill the buffer with.</param>
        /// <returns>The newly created and filled <see cref="IndexBuffer"/>.</returns>
        internal static IndexBuffer CreateIndexBuffer(Device device, int[] data)
        {
            #region Sanity checks
            if (device == null) throw new ArgumentNullException("device");
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            var buffer = new IndexBuffer(device, sizeof(int) * data.Length, Usage.WriteOnly, Pool.Managed, false);
            WriteIndexBuffer(buffer, data);

            return buffer;
        }
        #endregion

        #region Read IB
        /// <summary>
        /// Copies the content of an <see cref="IndexBuffer"/> to an array of 16-bit values.
        /// </summary>
        /// <param name="buffer">The buffer to read from.</param>
        /// <remarks>Warning! No check is performed to ensure the buffer actually uses 16-bit values.</remarks>
        internal static short[] Read16BitIndexBuffer(IndexBuffer buffer)
        {
            #region Sanity checks
            if (buffer == null) throw new ArgumentNullException("buffer");
            #endregion

            DataStream indexStream = buffer.Lock(0, buffer.Description.SizeInBytes, LockFlags.ReadOnly);
            int indexCount = buffer.Description.SizeInBytes / sizeof(short);
            var ret = indexStream.ReadRange<short>(indexCount);
            buffer.Unlock();
            indexStream.Dispose();

            return ret;
        }

        /// <summary>
        /// Copies the content of an <see cref="IndexBuffer"/> to an array of 32-bit values.
        /// </summary>
        /// <param name="buffer">The buffer to read from.</param>
        /// <remarks>Warning! No check is performed to ensure the buffer actually uses 32-bit values.</remarks>
        internal static int[] Read32BitIndexBuffer(IndexBuffer buffer)
        {
            #region Sanity checks
            if (buffer == null) throw new ArgumentNullException("buffer");
            #endregion

            DataStream indexStream = buffer.Lock(0, buffer.Description.SizeInBytes, LockFlags.ReadOnly);
            int indexCount = buffer.Description.SizeInBytes / sizeof(int);
            var ret = indexStream.ReadRange<int>(indexCount);
            buffer.Unlock();
            indexStream.Dispose();

            return ret;
        }

        /// <summary>
        /// Copies the content of the <see cref="IndexBuffer"/> of a <see cref="Mesh"/> to an array of 32-bit values (16-bit values are automatically converted).
        /// </summary>
        /// <param name="mesh">The <see cref="Mesh"/> containing the buffer to read from.</param>
        internal static int[] ReadIndexBuffer(Mesh mesh)
        {
            #region Sanity checks
            if (mesh == null) throw new ArgumentNullException("mesh");
            #endregion

            int indexCount = mesh.FaceCount * 3;
            int[] ret;
            using (DataStream indexStream = mesh.LockIndexBuffer(LockFlags.ReadOnly))
            {
                if ((mesh.CreationOptions).HasFlag(MeshFlags.Use32Bit))
                { // 32-bit values
                    ret = indexStream.ReadRange<int>(indexCount);
                }
                else
                { // 16-bit values
                    ret = new int[indexCount];
                    var temp = indexStream.ReadRange<short>(indexCount);
                    for (int i = 0; i < indexCount; i++)
                        ret[i] = temp[i];
                }
                mesh.UnlockIndexBuffer();
            }

            return ret;
        }
        #endregion

        #region Write IB
        /// <summary>
        /// Fills an <see cref="IndexBuffer"/> with 16-bit values.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="data">The values to fill the buffer with.</param>
        /// <remarks>Warning! No check is performed to ensure the buffer actually uses 16-bit values.</remarks>
        internal static void WriteIndexBuffer(IndexBuffer buffer, short[] data)
        {
            #region Sanity checks
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            using (DataStream indexStream = buffer.Lock(0, sizeof(short) * data.Length, LockFlags.None))
            {
                indexStream.WriteRange(data);
                buffer.Unlock();
            }
        }

        /// <summary>
        /// Fills an <see cref="IndexBuffer"/> with 32-bit values.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="data">The values to fill the buffer with.</param>
        /// <remarks>Warning! No check is performed to ensure the buffer actually uses 32-bit values.</remarks>
        internal static void WriteIndexBuffer(IndexBuffer buffer, int[] data)
        {
            #region Sanity checks
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            using (DataStream indexStream = buffer.Lock(0, sizeof(int) * data.Length, LockFlags.None))
            {
                indexStream.WriteRange(data);
                buffer.Unlock();
            }
        }

        /// <summary>
        /// Fills the <see cref="IndexBuffer"/> of a <see cref="Mesh"/> with 16-bit values.
        /// </summary>
        /// <param name="mesh">The <see cref="Mesh"/> containing the buffer to write to.</param>
        /// <param name="data">The values to fill the buffer with.</param>
        internal static void WriteIndexBuffer(Mesh mesh, short[] data)
        {
            #region Sanity checks
            if (mesh == null) throw new ArgumentNullException("mesh");
            if (data == null) throw new ArgumentNullException("data");
            if ((mesh.CreationOptions).HasFlag(MeshFlags.Use32Bit)) throw new ArgumentException(Resources.MeshIndexBufferNot16bit, "mesh");
            #endregion

            using (DataStream indexStream = mesh.LockIndexBuffer(LockFlags.None))
            {
                indexStream.WriteRange(data);
                mesh.UnlockIndexBuffer();
            }
        }

        /// <summary>
        /// Fills the <see cref="IndexBuffer"/> of a <see cref="Mesh"/> with 32-bit values.
        /// </summary>
        /// <param name="mesh">The <see cref="Mesh"/> containing the buffer to write to.</param>
        /// <param name="data">The values to fill the buffer with.</param>
        internal static void WriteIndexBuffer(Mesh mesh, int[] data)
        {
            #region Sanity checks
            if (mesh == null) throw new ArgumentNullException("mesh");
            if (data == null) throw new ArgumentNullException("data");
            if (!(mesh.CreationOptions).HasFlag(MeshFlags.Use32Bit)) throw new ArgumentException(Resources.MeshIndexBufferNot32bit, "mesh");
            #endregion

            using (DataStream indexStream = mesh.LockIndexBuffer(LockFlags.None))
            {
                indexStream.WriteRange(data);
                mesh.UnlockIndexBuffer();
            }
        }
        #endregion

        //--------------------//

        #region Bounding box
        /// <summary>
        /// Generates a <see cref="BoundingBox"/> that completely contains all points within a <see cref="VertexBuffer"/>.
        /// </summary>
        /// <param name="vb">The <see cref="VertexBuffer"/> to be contained within the <see cref="BoundingBox"/>.</param>
        /// <param name="vertexCount">The total number of vertex contained within <paramref name="vb"/>.</param>
        public static BoundingBox ComputeBoundingBox(VertexBuffer vb, int vertexCount)
        {
            return BoundingBox.FromPoints(GetPoints(vb, vertexCount));
        }

        /// <summary>
        /// Generates a <see cref="BoundingBox"/> that completely contains all points within a <see cref="Mesh"/>.
        /// </summary>
        /// <param name="mesh">The <see cref="Mesh"/> to be contained within the <see cref="BoundingBox"/>.</param>
        public static BoundingBox ComputeBoundingBox(Mesh mesh)
        {
            return BoundingBox.FromPoints(GetPoints(mesh));
        }
        #endregion

        #region Bounding sphere
        /// <summary>
        /// Generates a <see cref="BoundingSphere"/> that completely contains all points within a <see cref="VertexBuffer"/>.
        /// </summary>
        /// <param name="vb">The <see cref="VertexBuffer"/> to be contained within the <see cref="BoundingSphere"/>.</param>
        /// <param name="vertexCount">The total number of vertex contained within <paramref name="vb"/>.</param>
        public static BoundingSphere ComputeBoundingSphere(VertexBuffer vb, int vertexCount)
        {
            return BoundingSphere.FromPoints(GetPoints(vb, vertexCount));
        }

        /// <summary>
        /// Generates a <see cref="BoundingSphere"/> that completely contains all points within a <see cref="Mesh"/>.
        /// </summary>
        /// <param name="mesh">The <see cref="Mesh"/> to be contained within the <see cref="BoundingSphere"/>.</param>
        public static BoundingSphere ComputeBoundingSphere(Mesh mesh)
        {
            return BoundingSphere.FromPoints(GetPoints(mesh));
        }
        #endregion

        #region Bounding body helpers
        /// <summary>
        /// Gets all points contained within a <see cref="VertexBuffer"/>.
        /// </summary>
        /// <param name="buffer">The <see cref="VertexBuffer"/> to get points from.</param>
        /// <param name="vertexCount">The total number of vertex contained within <paramref name="buffer"/>.</param>
        /// <returns>An array of points defined by the object.</returns>
        private static Vector3[] GetPoints(VertexBuffer buffer, int vertexCount)
        {
            DataStream vertexStream = buffer.Lock(0, buffer.Description.SizeInBytes, LockFlags.ReadOnly);
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
        private static Vector3[] GetPoints(Mesh mesh)
        {
            DataStream vertexStream = mesh.LockVertexBuffer(LockFlags.ReadOnly);
            var points = D3DX.GetVectors(vertexStream, mesh.VertexCount, mesh.VertexFormat);
            mesh.UnlockVertexBuffer();
            vertexStream.Dispose();

            return points;
        }
        #endregion
    }
}
