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
using System.Runtime.InteropServices;
using Vortice.Direct3D9;

namespace OmegaEngine.Graphics.Meshes;

/// <summary>
/// A triangle mesh made up of vertexes, indexes and an attribute buffer assigning each face to a subset.
/// </summary>
/// <remarks>
/// The data is kept in system memory, where the CPU can read and modify it at any time.
/// It is uploaded to <see cref="Pool.Default"/> buffers when it is first drawn and again whenever it was modified in the meantime.
/// </remarks>
public sealed partial class Mesh : IDisposable
{
    private readonly VertexElement[] _declaration;
    private byte[] _vertexes;
    private int[] _indexes;
    private int[] _attributes;

    /// <summary>
    /// The device the mesh is drawn on.
    /// </summary>
    public IDirect3DDevice9 Device { get; }

    /// <summary>
    /// The size of a single vertex in bytes.
    /// </summary>
    public int BytesPerVertex { get; }

    /// <summary>
    /// The fixed-function vertex format of the mesh; <see cref="VertexFormat.None"/> if its vertex declaration has no equivalent.
    /// </summary>
    public VertexFormat VertexFormat { get; }

    /// <summary>
    /// The number of vertexes in the mesh.
    /// </summary>
    public int VertexCount => _vertexes.Length / BytesPerVertex;

    /// <summary>
    /// The number of faces (triangles) in the mesh.
    /// </summary>
    public int FaceCount => _indexes.Length / 3;

    /// <summary>
    /// The number of subsets in the mesh, i.e. the highest attribute value plus one.
    /// </summary>
    public int SubsetCount
    {
        get
        {
            int count = 0;
            foreach (int attribute in _attributes)
                if (attribute >= count) count = attribute + 1;
            return count;
        }
    }

    #region Constructor
    /// <summary>
    /// Creates an empty mesh with a vertex declaration.
    /// </summary>
    /// <param name="device">The device to draw the mesh on.</param>
    /// <param name="faceCount">The number of faces (triangles).</param>
    /// <param name="vertexCount">The number of vertexes.</param>
    /// <param name="declaration">The vertex declaration, optionally including the <see cref="VertexElement.VertexDeclarationEnd"/> marker.</param>
    public Mesh(IDirect3DDevice9 device, int faceCount, int vertexCount, VertexElement[] declaration)
    {
        #region Sanity checks
        if (faceCount < 0) throw new ArgumentOutOfRangeException(nameof(faceCount));
        if (vertexCount < 0) throw new ArgumentOutOfRangeException(nameof(vertexCount));
        if (declaration == null) throw new ArgumentNullException(nameof(declaration));
        #endregion

        Device = device ?? throw new ArgumentNullException(nameof(device));

        _declaration = declaration.Length > 0 && declaration[^1].Stream == VertexElement.VertexDeclarationEnd.Stream
            ? (VertexElement[])declaration.Clone()
            : [..declaration, VertexElement.VertexDeclarationEnd];
        BytesPerVertex = _declaration.GetVertexSize();
        if (BytesPerVertex == 0) throw new ArgumentException("The vertex declaration is empty.", nameof(declaration));
        VertexFormat = _declaration.ToFormat();
        _positionOffset = _declaration.Find(DeclarationUsage.Position) is {Type: DeclarationType.Float3} position ? position.Offset : 0;

        _vertexes = new byte[vertexCount * BytesPerVertex];
        _indexes = new int[faceCount * 3];
        _attributes = new int[faceCount];
    }

    /// <summary>
    /// Creates an empty mesh with a fixed-function vertex format.
    /// </summary>
    /// <param name="device">The device to draw the mesh on.</param>
    /// <param name="faceCount">The number of faces (triangles).</param>
    /// <param name="vertexCount">The number of vertexes.</param>
    /// <param name="format">The fixed-function vertex format.</param>
    public Mesh(IDirect3DDevice9 device, int faceCount, int vertexCount, VertexFormat format)
        : this(device, faceCount, vertexCount, VertexElements.FromFormat(format))
    {}
    #endregion

    #region Declaration
    /// <summary>
    /// Gets the vertex declaration of the mesh, including the <see cref="VertexElement.VertexDeclarationEnd"/> marker.
    /// </summary>
    public VertexElement[] GetDeclaration() => (VertexElement[])_declaration.Clone();

    /// <summary>
    /// Creates a copy of the mesh with a different vertex declaration.
    /// </summary>
    /// <param name="declaration">The vertex declaration of the copy.</param>
    /// <remarks>Copies vertex elements with matching usage and usage index. Elements of the new declaration with no match are left zero.</remarks>
    public Mesh Clone(VertexElement[] declaration)
    {
        var clone = new Mesh(Device, FaceCount, VertexCount, declaration);
        _indexes.CopyTo(clone._indexes, 0);
        _attributes.CopyTo(clone._attributes, 0);

        var copies = new List<(int Source, int Target, int Size)>();
        foreach (var target in clone._declaration)
        {
            if (target.Stream != 0 || _declaration.Find(target.Usage, target.UsageIndex) is not {} source) continue;
            if (source.Type == target.Type)
                copies.Add((source.Offset, target.Offset, source.Type.GetSize()));
            else if (IsFloat(source.Type) && IsFloat(target.Type))
                copies.Add((source.Offset, target.Offset, Math.Min(source.Type.GetSize(), target.Type.GetSize())));
        }

        for (int i = 0; i < VertexCount; i++)
        {
            foreach (var (source, target, size) in copies)
            {
                _vertexes.AsSpan(i * BytesPerVertex + source, size)
                         .CopyTo(clone._vertexes.AsSpan(i * clone.BytesPerVertex + target, size));
            }
        }

        return clone;
    }

    private static bool IsFloat(DeclarationType type)
        => type is DeclarationType.Float1 or DeclarationType.Float2 or DeclarationType.Float3 or DeclarationType.Float4;

    /// <summary>
    /// Creates a copy of the mesh with a different fixed-function vertex format.
    /// </summary>
    public Mesh Clone(VertexFormat format) => Clone(VertexElements.FromFormat(format));

    /// <summary>
    /// Creates an exact copy of the mesh.
    /// </summary>
    public Mesh Clone() => Clone(_declaration);
    #endregion

    #region Data access
    /// <summary>
    /// The raw vertex data. Call <see cref="InvalidateVertexes"/> after modifying it.
    /// </summary>
    internal Span<byte> VertexData => _vertexes;

    /// <summary>
    /// Marks the vertexes as modified, so they are uploaded again before the next draw.
    /// </summary>
    internal void InvalidateVertexes() => _vertexesDirty = true;

    /// <summary>
    /// Copies the vertexes to an array of structs.
    /// </summary>
    /// <typeparam name="T">A struct matching the vertex declaration.</typeparam>
    /// <exception cref="ArgumentException"><typeparamref name="T"/> does not have the size of a vertex.</exception>
    public T[] ReadVertexBuffer<T>() where T : unmanaged
        => MemoryMarshal.Cast<byte, T>(_vertexes.AsSpan(0, VertexCount * CheckVertexSize<T>())).ToArray();

    /// <summary>
    /// Replaces the vertexes.
    /// </summary>
    /// <typeparam name="T">A struct matching the vertex declaration.</typeparam>
    /// <param name="data">Exactly <see cref="VertexCount"/> vertexes.</param>
    /// <exception cref="ArgumentException"><typeparamref name="T"/> does not have the size of a vertex or <paramref name="data"/> does not have the right length.</exception>
    public void WriteVertexBuffer<T>(T[] data) where T : unmanaged
    {
        #region Sanity checks
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Length != VertexCount) throw new ArgumentException($"Expected {VertexCount} vertexes, got {data.Length}.", nameof(data));
        #endregion

        CheckVertexSize<T>();
        MemoryMarshal.AsBytes(data.AsSpan()).CopyTo(_vertexes);
        _vertexesDirty = true;
    }

    private unsafe int CheckVertexSize<T>() where T : unmanaged
    {
        if (sizeof(T) != BytesPerVertex) throw new ArgumentException($"{typeof(T).Name} has {sizeof(T)} bytes, but the vertexes have {BytesPerVertex} bytes.");
        return sizeof(T);
    }

    /// <summary>
    /// Copies the indexes (three per face) to an array.
    /// </summary>
    public int[] ReadIndexBuffer() => (int[])_indexes.Clone();

    /// <summary>
    /// Replaces the indexes.
    /// </summary>
    /// <param name="data">Exactly three indexes per face.</param>
    public void WriteIndexBuffer(int[] data)
    {
        #region Sanity checks
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Length != _indexes.Length) throw new ArgumentException($"Expected {_indexes.Length} indexes, got {data.Length}.", nameof(data));
        #endregion

        data.CopyTo(_indexes, 0);
        _indexesDirty = true;
    }

    /// <summary>
    /// Replaces the indexes with 16-bit values.
    /// </summary>
    /// <param name="data">Exactly three indexes per face. Interpreted as unsigned values.</param>
    public void WriteIndexBuffer(short[] data)
    {
        #region Sanity checks
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Length != _indexes.Length) throw new ArgumentException($"Expected {_indexes.Length} indexes, got {data.Length}.", nameof(data));
        #endregion

        for (int i = 0; i < data.Length; i++)
            _indexes[i] = (ushort)data[i];
        _indexesDirty = true;
    }

    /// <summary>
    /// Copies the attribute buffer (the subset of each face) to an array.
    /// </summary>
    public int[] ReadAttributeBuffer() => (int[])_attributes.Clone();

    /// <summary>
    /// Replaces the attribute buffer.
    /// </summary>
    /// <param name="data">The subset of each face.</param>
    public void WriteAttributeBuffer(int[] data)
    {
        #region Sanity checks
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Length != _attributes.Length) throw new ArgumentException($"Expected {_attributes.Length} attributes, got {data.Length}.", nameof(data));
        #endregion

        data.CopyTo(_attributes, 0);
        _subsets = null;
    }

    /// <summary>
    /// Gets the position of each vertex.
    /// </summary>
    public Vector3[] GetPoints()
    {
        var points = new Vector3[VertexCount];
        for (int i = 0; i < points.Length; i++)
            points[i] = GetPosition(i);
        return points;
    }

    /// <summary>
    /// Gets the positions of the vertexes used by a specific subset.
    /// </summary>
    /// <param name="subset">The subset to get points for.</param>
    /// <returns>The corner positions of all faces in the subset; an empty array if the subset has no faces.</returns>
    public Vector3[] GetPoints(int subset)
    {
        #region Sanity checks
        if (subset < 0) throw new ArgumentOutOfRangeException(nameof(subset), "Subset must be non-negative.");
        #endregion

        var points = new List<Vector3>();
        for (int face = 0; face < FaceCount; face++)
        {
            if (_attributes[face] != subset) continue;
            for (int corner = 0; corner < 3; corner++)
                points.Add(GetPosition(_indexes[face * 3 + corner]));
        }
        return points.ToArray();
    }

    /// <summary>The offset of the position within a vertex.</summary>
    private readonly int _positionOffset;

    /// <summary>
    /// Reads the position of a vertex.
    /// </summary>
    private Vector3 GetPosition(int vertex)
        => MemoryMarshal.Read<Vector3>(_vertexes.AsSpan(vertex * BytesPerVertex + _positionOffset));
    #endregion
}
