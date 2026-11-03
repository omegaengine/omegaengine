/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using Vortice.Direct3D9;

namespace OmegaEngine.Graphics.Meshes;

partial class Mesh
{
    private IDirect3DVertexDeclaration9? _vertexDeclaration;
    private IDirect3DVertexBuffer9? _vertexBuffer;
    private IDirect3DIndexBuffer9? _indexBuffer;
    private bool _vertexesDirty = true, _indexesDirty = true;

    /// <summary>Set once the vertexes were modified after their first upload, to use a buffer optimized for frequent updates from then on.</summary>
    private bool _dynamicVertexes;

    /// <summary>A contiguous run of faces belonging to the same subset.</summary>
    private readonly record struct FaceRange(int StartFace, int FaceCount, int MinVertex, int VertexSpan);

    /// <summary>The face ranges making up each subset; <c>null</c> if they need to be determined again.</summary>
    private FaceRange[][]? _subsets;

    /// <summary>
    /// The vertex buffer used for drawing; <c>null</c> if the mesh was not drawn yet.
    /// </summary>
    internal IDirect3DVertexBuffer9? VertexBuffer => _vertexBuffer;

    /// <summary>
    /// The index buffer used for drawing; <c>null</c> if the mesh was not drawn yet.
    /// </summary>
    internal IDirect3DIndexBuffer9? IndexBuffer => _indexBuffer;

    /// <summary>
    /// Draws all faces of a subset.
    /// </summary>
    /// <param name="subset">The subset to draw.</param>
    /// <remarks>Uploads the mesh to the GPU first if it has not been yet or was modified since.</remarks>
    public void DrawSubset(int subset)
    {
        if (IsDisposed) throw new ObjectDisposedException(nameof(Mesh));

        _subsets ??= BuildSubsets();
        if (subset < 0 || subset >= _subsets.Length || _subsets[subset].Length == 0) return;

        Upload();
        Device.VertexDeclaration = _vertexDeclaration;
        Device.SetStreamSource(0, _vertexBuffer, 0, (uint)BytesPerVertex);
        Device.Indices = _indexBuffer;
        foreach (var range in _subsets[subset])
            Device.DrawIndexedPrimitive(PrimitiveType.TriangleList, 0, (uint)range.MinVertex, (uint)range.VertexSpan, (uint)(range.StartFace * 3), (uint)range.FaceCount);
    }

    /// <summary>
    /// Groups runs of consecutive faces by subset.
    /// </summary>
    private FaceRange[][] BuildSubsets()
    {
        var subsets = new List<FaceRange>[SubsetCount];
        for (int i = 0; i < subsets.Length; i++) subsets[i] = [];

        int face = 0;
        while (face < FaceCount)
        {
            int subset = _attributes[face], start = face;
            int minVertex = int.MaxValue, maxVertex = -1;
            for (; face < FaceCount && _attributes[face] == subset; face++)
            {
                for (int corner = 0; corner < 3; corner++)
                {
                    int index = _indexes[face * 3 + corner];
                    minVertex = Math.Min(minVertex, index);
                    maxVertex = Math.Max(maxVertex, index);
                }
            }

            if (subset >= 0) subsets[subset].Add(new(start, face - start, minVertex, maxVertex - minVertex + 1));
        }

        return Array.ConvertAll(subsets, x => x.ToArray());
    }

    /// <summary>
    /// Creates the GPU resources or updates them with modified data.
    /// </summary>
    private unsafe void Upload()
    {
        _vertexDeclaration ??= Device.CreateVertexDeclaration(_declaration);

        if (_vertexesDirty || _vertexBuffer == null)
        {
            if (_vertexBuffer != null && (!_dynamicVertexes || _vertexBuffer.Description.SizeInBytes != _vertexes.Length))
            {
                _vertexBuffer.Dispose();
                _vertexBuffer = null;
                _dynamicVertexes = true;
            }
            _vertexBuffer ??= Device.CreateVertexBuffer((uint)_vertexes.Length, _dynamicVertexes ? Usage.Dynamic | Usage.WriteOnly : Usage.WriteOnly, VertexFormat.None, Pool.Default);

            var target = _vertexBuffer.LockToPointer(0, (uint)_vertexes.Length, _dynamicVertexes ? LockFlags.Discard : LockFlags.None);
            try
            {
                _vertexes.CopyTo(new Span<byte>((void*)target, _vertexes.Length));
            }
            finally
            {
                _vertexBuffer.Unlock();
            }
            _vertexesDirty = false;
        }

        if (_indexesDirty || _indexBuffer == null)
        {
            bool sixteenBit = VertexCount <= ushort.MaxValue + 1;
            int size = _indexes.Length * (sixteenBit ? sizeof(ushort) : sizeof(int));
            if (_indexBuffer != null && _indexBuffer.Description.SizeInBytes != size)
            {
                _indexBuffer.Dispose();
                _indexBuffer = null;
            }
            _indexBuffer ??= Device.CreateIndexBuffer((uint)size, Usage.WriteOnly, sixteenBit, Pool.Default);

            var target = _indexBuffer.LockToPointer(0, (uint)size, LockFlags.None);
            try
            {
                if (sixteenBit)
                {
                    var span = new Span<ushort>((void*)target, _indexes.Length);
                    for (int i = 0; i < _indexes.Length; i++) span[i] = (ushort)_indexes[i];
                }
                else _indexes.CopyTo(new Span<int>((void*)target, _indexes.Length));
            }
            finally
            {
                _indexBuffer.Unlock();
            }
            _indexesDirty = false;
        }
    }

    #region Dispose
    /// <summary>
    /// Has this mesh been disposed?
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Releases the GPU resources. The mesh cannot be drawn afterwards.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed) return;

        _vertexDeclaration?.Dispose();
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        IsDisposed = true;
    }
    #endregion
}
