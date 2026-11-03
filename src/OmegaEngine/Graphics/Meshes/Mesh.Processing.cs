/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Direct3D9;
using Vortice.Mathematics;

namespace OmegaEngine.Graphics.Meshes;

partial class Mesh
{
    #region Element access
    private int GetFloat3Offset(DeclarationUsage usage)
        => _declaration.Find(usage) is {Type: DeclarationType.Float3} element
            ? element.Offset
            : throw new InvalidOperationException($"The vertex declaration has no {usage} element of type {DeclarationType.Float3}.");

    private Vector3 ReadVector3(int vertex, int offset)
        => MemoryMarshal.Read<Vector3>(_vertexes.AsSpan(vertex * BytesPerVertex + offset));

    private void WriteVector3(int vertex, int offset, Vector3 value)
        => MemoryMarshal.Write(_vertexes.AsSpan(vertex * BytesPerVertex + offset), in value);

    private static Vector3 NormalizeOrZero(Vector3 vector)
        => vector == default ? default : Vector3.Normalize(vector);

    /// <summary>
    /// Calculates the angle at each corner of a face, used to weight the face's contribution to per-vertex vectors.
    /// </summary>
    private static (float, float, float) GetCornerAngles(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        static float Angle(Vector3 a, Vector3 b)
        {
            a = NormalizeOrZero(a);
            b = NormalizeOrZero(b);
            return a == default || b == default ? 0 : MathF.Acos(Math.Clamp(Vector3.Dot(a, b), -1, 1));
        }

        return (Angle(p1 - p0, p2 - p0), Angle(p2 - p1, p0 - p1), Angle(p0 - p2, p1 - p2));
    }
    #endregion

    #region Normals
    /// <summary>
    /// Computes vertex normals by averaging the normals of the faces using them, weighted by the angle of each face at the vertex.
    /// </summary>
    /// <exception cref="InvalidOperationException">The vertex declaration has no normals.</exception>
    public void ComputeNormals()
    {
        int normalOffset = GetFloat3Offset(DeclarationUsage.Normal);

        var normals = new Vector3[VertexCount];
        for (int face = 0; face < FaceCount; face++)
        {
            int i0 = _indexes[face * 3], i1 = _indexes[face * 3 + 1], i2 = _indexes[face * 3 + 2];
            Vector3 p0 = GetPosition(i0), p1 = GetPosition(i1), p2 = GetPosition(i2);

            var faceNormal = NormalizeOrZero(Vector3.Cross(p1 - p0, p2 - p0));
            var (a0, a1, a2) = GetCornerAngles(p0, p1, p2);
            normals[i0] += faceNormal * a0;
            normals[i1] += faceNormal * a1;
            normals[i2] += faceNormal * a2;
        }

        for (int i = 0; i < normals.Length; i++)
            WriteVector3(i, normalOffset, NormalizeOrZero(normals[i]));
        _vertexesDirty = true;
    }
    #endregion

    #region Tangents
    /// <summary>
    /// Computes tangent and binormal vectors, i.e. the directions in which the first set of texture coordinates increase.
    /// </summary>
    /// <param name="mergeSeams">Treat vertexes at the same position with the same normal as one, even if their texture coordinates differ.
    /// Avoids visible seams along texture borders on smooth surfaces.</param>
    /// <exception cref="InvalidOperationException">The vertex declaration has no tangents, binormals or 2D texture coordinates.</exception>
    public void ComputeTangents(bool mergeSeams = false)
    {
        int tangentOffset = GetFloat3Offset(DeclarationUsage.Tangent);
        int binormalOffset = GetFloat3Offset(DeclarationUsage.Binormal);
        int texCoordOffset = _declaration.Find(DeclarationUsage.TextureCoordinate) is {Type: DeclarationType.Float2} texCoord
            ? texCoord.Offset
            : throw new InvalidOperationException("The vertex declaration has no 2D texture coordinates.");
        int? normalOffset = _declaration.Find(DeclarationUsage.Normal) is {Type: DeclarationType.Float3} normal ? normal.Offset : null;

        Vector3 GetNormal(int vertex) => normalOffset is {} offset ? ReadVector3(vertex, offset) : default;

        // Map each vertex to the group whose tangents it shares
        var groups = new int[VertexCount];
        if (mergeSeams)
        {
            var groupIds = new Dictionary<(Vector3, Vector3), int>();
            for (int i = 0; i < groups.Length; i++)
            {
                var key = (GetPosition(i), GetNormal(i));
                if (!groupIds.TryGetValue(key, out groups[i]))
                    groupIds.Add(key, groups[i] = i);
            }
        }
        else
        {
            for (int i = 0; i < groups.Length; i++) groups[i] = i;
        }

        var tangents = new Vector3[VertexCount];
        var binormals = new Vector3[VertexCount];
        for (int face = 0; face < FaceCount; face++)
        {
            int i0 = _indexes[face * 3], i1 = _indexes[face * 3 + 1], i2 = _indexes[face * 3 + 2];
            Vector3 p0 = GetPosition(i0), p1 = GetPosition(i1), p2 = GetPosition(i2);
            var uv0 = MemoryMarshal.Read<Vector2>(_vertexes.AsSpan(i0 * BytesPerVertex + texCoordOffset));
            var uv1 = MemoryMarshal.Read<Vector2>(_vertexes.AsSpan(i1 * BytesPerVertex + texCoordOffset));
            var uv2 = MemoryMarshal.Read<Vector2>(_vertexes.AsSpan(i2 * BytesPerVertex + texCoordOffset));

            Vector3 edge1 = p1 - p0, edge2 = p2 - p0;
            Vector2 delta1 = uv1 - uv0, delta2 = uv2 - uv0;
            float determinant = delta1.X * delta2.Y - delta2.X * delta1.Y;
            if (MathF.Abs(determinant) < 1e-12f) continue; // Degenerate texture mapping, no defined tangent direction

            var tangent = NormalizeOrZero((edge1 * delta2.Y - edge2 * delta1.Y) / determinant);
            var binormal = NormalizeOrZero((edge2 * delta1.X - edge1 * delta2.X) / determinant);

            var (a0, a1, a2) = GetCornerAngles(p0, p1, p2);
            tangents[groups[i0]] += tangent * a0;
            tangents[groups[i1]] += tangent * a1;
            tangents[groups[i2]] += tangent * a2;
            binormals[groups[i0]] += binormal * a0;
            binormals[groups[i1]] += binormal * a1;
            binormals[groups[i2]] += binormal * a2;
        }

        for (int i = 0; i < VertexCount; i++)
        {
            var n = NormalizeOrZero(GetNormal(i));

            // Gram-Schmidt orthogonalization against the normal (and the tangent for the binormal)
            var t = tangents[groups[i]];
            t = NormalizeOrZero(t - n * Vector3.Dot(n, t));
            var b = binormals[groups[i]];
            b = NormalizeOrZero(b - n * Vector3.Dot(n, b) - t * Vector3.Dot(t, b));

            WriteVector3(i, tangentOffset, t);
            WriteVector3(i, binormalOffset, b);
        }
        _vertexesDirty = true;
    }
    #endregion

    #region Weld
    /// <summary>
    /// Merges vertexes that are equal, allowing for slight differences in position and normal.
    /// </summary>
    /// <param name="positionEpsilon">The maximum difference per position component for vertexes to be merged.</param>
    /// <param name="normalEpsilon">The maximum difference per normal component for vertexes to be merged.</param>
    /// <remarks>All other vertex elements must match exactly. Removes the merged vertexes, so the vertex order changes.</remarks>
    public void WeldVertexes(float positionEpsilon, float normalEpsilon)
    {
        int? normalOffset = _declaration.Find(DeclarationUsage.Normal) is {Type: DeclarationType.Float3} normal ? normal.Offset : null;

        // Bytes that must match exactly (everything except position and normal)
        var exactBytes = new bool[BytesPerVertex];
        Array.Fill(exactBytes, true);
        Array.Fill(exactBytes, false, _positionOffset, 12);
        if (normalOffset is {} offset) Array.Fill(exactBytes, false, offset, 12);

        bool Matches(int a, int b)
        {
            if (!Vector3Near(GetPosition(a), GetPosition(b), positionEpsilon)) return false;
            if (normalOffset is {} n && !Vector3Near(ReadVector3(a, n), ReadVector3(b, n), normalEpsilon)) return false;

            var spanA = _vertexes.AsSpan(a * BytesPerVertex, BytesPerVertex);
            var spanB = _vertexes.AsSpan(b * BytesPerVertex, BytesPerVertex);
            for (int i = 0; i < BytesPerVertex; i++)
                if (exactBytes[i] && spanA[i] != spanB[i]) return false;
            return true;
        }

        // Spatial hash, so only vertexes in neighboring cells need to be compared
        float cellSize = Math.Max(positionEpsilon, 1e-6f);
        (long, long, long) Cell(Vector3 p) => ((long)MathF.Floor(p.X / cellSize), (long)MathF.Floor(p.Y / cellSize), (long)MathF.Floor(p.Z / cellSize));
        var cells = new Dictionary<(long, long, long), List<int>>();

        var remap = new int[VertexCount];
        var kept = new List<int>();
        for (int i = 0; i < VertexCount; i++)
        {
            var (x, y, z) = Cell(GetPosition(i));
            remap[i] = -1;
            for (long dx = -1; dx <= 1 && remap[i] < 0; dx++)
            for (long dy = -1; dy <= 1 && remap[i] < 0; dy++)
            for (long dz = -1; dz <= 1 && remap[i] < 0; dz++)
            {
                if (!cells.TryGetValue((x + dx, y + dy, z + dz), out var candidates)) continue;
                foreach (int candidate in candidates)
                {
                    if (Matches(kept[candidate], i))
                    {
                        remap[i] = candidate;
                        break;
                    }
                }
            }

            if (remap[i] < 0)
            {
                remap[i] = kept.Count;
                kept.Add(i);
                if (!cells.TryGetValue((x, y, z), out var cell)) cells.Add((x, y, z), cell = []);
                cell.Add(remap[i]);
            }
        }

        if (kept.Count == VertexCount) return;
        ReorderVertexes(kept.ToArray(), remap);
    }

    private static bool Vector3Near(Vector3 a, Vector3 b, float epsilon)
        => MathF.Abs(a.X - b.X) <= epsilon && MathF.Abs(a.Y - b.Y) <= epsilon && MathF.Abs(a.Z - b.Z) <= epsilon;

    /// <summary>
    /// Replaces the vertexes with a selection of the existing ones.
    /// </summary>
    /// <param name="order">The old index of each new vertex.</param>
    /// <param name="remap">The new index of each old vertex.</param>
    private void ReorderVertexes(int[] order, int[] remap)
    {
        var vertexes = new byte[order.Length * BytesPerVertex];
        for (int i = 0; i < order.Length; i++)
            _vertexes.AsSpan(order[i] * BytesPerVertex, BytesPerVertex).CopyTo(vertexes.AsSpan(i * BytesPerVertex));
        _vertexes = vertexes;

        for (int i = 0; i < _indexes.Length; i++)
            _indexes[i] = remap[_indexes[i]];

        _vertexesDirty = _indexesDirty = true;
        _subsets = null;
    }
    #endregion

    #region Optimize
    /// <summary>
    /// Reorders the faces and vertexes of the mesh to improve rendering performance.
    /// </summary>
    /// <remarks>Sorts the faces by subset, optimizes their order for the GPU's vertex cache and sorts the vertexes by first use.</remarks>
    public void Optimize()
    {
        // Sort faces by subset and optimize the order within each subset for the vertex cache
        var faceOrder = Enumerable.Range(0, FaceCount).OrderBy(face => _attributes[face]).ToArray();
        int start = 0;
        while (start < faceOrder.Length)
        {
            int end = start;
            while (end < faceOrder.Length && _attributes[faceOrder[end]] == _attributes[faceOrder[start]]) end++;
            VertexCacheOptimizer.Optimize(_indexes, faceOrder.AsSpan(start, end - start));
            start = end;
        }

        var indexes = new int[_indexes.Length];
        var attributes = new int[_attributes.Length];
        for (int i = 0; i < faceOrder.Length; i++)
        {
            _indexes.AsSpan(faceOrder[i] * 3, 3).CopyTo(indexes.AsSpan(i * 3));
            attributes[i] = _attributes[faceOrder[i]];
        }
        _indexes = indexes;
        _attributes = attributes;

        // Sort vertexes by first use, keeping unused ones at the end
        var remap = new int[VertexCount];
        Array.Fill(remap, -1);
        var order = new List<int>(VertexCount);
        foreach (int index in _indexes)
        {
            if (remap[index] >= 0) continue;
            remap[index] = order.Count;
            order.Add(index);
        }
        for (int i = 0; i < remap.Length; i++)
        {
            if (remap[i] >= 0) continue;
            remap[i] = order.Count;
            order.Add(i);
        }
        ReorderVertexes(order.ToArray(), remap);
    }
    #endregion

    #region Intersect
    /// <summary>
    /// Determines whether a ray intersects the mesh.
    /// </summary>
    /// <param name="ray">The ray in mesh space.</param>
    /// <param name="distance">The distance from the ray's origin to the closest intersection, in multiples of the ray's direction vector.</param>
    /// <param name="faceIndex">The index of the closest face that was hit.</param>
    /// <returns><c>true</c> if the ray hits any face; <c>false</c> otherwise.</returns>
    public bool Intersects(Ray ray, out float distance, out int faceIndex)
    {
        distance = float.PositiveInfinity;
        faceIndex = -1;
        for (int face = 0; face < FaceCount; face++)
        {
            if (ray.Intersects(GetPosition(_indexes[face * 3]), GetPosition(_indexes[face * 3 + 1]), GetPosition(_indexes[face * 3 + 2]), out float faceDistance)
             && faceDistance < distance)
            {
                distance = faceDistance;
                faceIndex = face;
            }
        }

        if (faceIndex >= 0) return true;
        distance = 0;
        return false;
    }
    #endregion
}
