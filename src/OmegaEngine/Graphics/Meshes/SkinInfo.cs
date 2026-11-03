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
/// The bones of a skinned mesh, their bind pose and the vertexes they influence.
/// </summary>
internal sealed class SkinInfo
{
    /// <summary>
    /// The name of each bone, matching the name of the frame it is attached to.
    /// </summary>
    public IReadOnlyList<string> BoneNames { get; }

    /// <summary>
    /// The offset matrix of each bone, which transforms from mesh space to the bone's space in the bind pose.
    /// </summary>
    public IReadOnlyList<Matrix4x4> BoneOffsets { get; }

    /// <summary>
    /// The number of bones.
    /// </summary>
    public int BoneCount => BoneNames.Count;

    // The influences of each vertex: _influenceStart[v] to _influenceStart[v + 1] index into _influenceBones/_influenceWeights
    private readonly int[] _influenceStart;
    private readonly int[] _influenceBones;
    private readonly float[] _influenceWeights;

    /// <summary>
    /// Creates skinning information.
    /// </summary>
    /// <param name="vertexCount">The number of vertexes of the mesh to be skinned.</param>
    /// <param name="boneNames">The name of each bone.</param>
    /// <param name="boneOffsets">The offset matrix of each bone.</param>
    /// <param name="influences">The vertexes each bone influences and by how much.</param>
    public SkinInfo(int vertexCount, IReadOnlyList<string> boneNames, IReadOnlyList<Matrix4x4> boneOffsets, IEnumerable<(int Bone, int Vertex, float Weight)> influences)
    {
        #region Sanity checks
        if (boneNames == null) throw new ArgumentNullException(nameof(boneNames));
        if (boneOffsets == null) throw new ArgumentNullException(nameof(boneOffsets));
        if (influences == null) throw new ArgumentNullException(nameof(influences));
        if (boneNames.Count != boneOffsets.Count) throw new ArgumentException("Each bone needs exactly one offset matrix.", nameof(boneOffsets));
        #endregion

        BoneNames = boneNames;
        BoneOffsets = boneOffsets;

        var perVertex = new List<(int Bone, float Weight)>[vertexCount];
        int total = 0;
        foreach (var (bone, vertex, weight) in influences)
        {
            if (bone < 0 || bone >= boneNames.Count) throw new ArgumentOutOfRangeException(nameof(influences), $"Bone {bone} does not exist.");
            if (vertex < 0 || vertex >= vertexCount) throw new ArgumentOutOfRangeException(nameof(influences), $"Vertex {vertex} does not exist.");
            (perVertex[vertex] ??= []).Add((bone, weight));
            total++;
        }

        _influenceStart = new int[vertexCount + 1];
        _influenceBones = new int[total];
        _influenceWeights = new float[total];
        int next = 0;
        for (int vertex = 0; vertex < vertexCount; vertex++)
        {
            _influenceStart[vertex] = next;
            if (perVertex[vertex] is {} list)
            {
                foreach (var (bone, weight) in list)
                {
                    _influenceBones[next] = bone;
                    _influenceWeights[next] = weight;
                    next++;
                }
            }
        }
        _influenceStart[vertexCount] = next;
    }

    /// <summary>
    /// Applies software skinning: transforms the source vertexes by the weighted bone transforms and writes them to the destination.
    /// </summary>
    /// <param name="boneTransforms">The transform of each bone, including its offset matrix.</param>
    /// <param name="boneInvTransposeTransforms">The inverse transpose of each bone transform, used for normals, tangents and binormals; may be <c>null</c> to use <paramref name="boneTransforms"/>.</param>
    /// <param name="source">The mesh in its bind pose.</param>
    /// <param name="destination">The mesh to write the skinned vertexes to, with the same vertex declaration as <paramref name="source"/>.</param>
    /// <remarks>Vertexes not influenced by any bone are left unchanged.</remarks>
    public void UpdateSkinnedMesh(Matrix4x4[] boneTransforms, Matrix4x4[]? boneInvTransposeTransforms, Mesh source, Mesh destination)
    {
        #region Sanity checks
        if (boneTransforms == null) throw new ArgumentNullException(nameof(boneTransforms));
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (destination == null) throw new ArgumentNullException(nameof(destination));
        if (boneTransforms.Length < BoneCount) throw new ArgumentException("There must be a transform for each bone.", nameof(boneTransforms));
        if (boneInvTransposeTransforms != null && boneInvTransposeTransforms.Length < BoneCount) throw new ArgumentException("There must be a transform for each bone.", nameof(boneInvTransposeTransforms));
        if (source.VertexCount != _influenceStart.Length - 1 || destination.VertexCount != source.VertexCount || destination.BytesPerVertex != source.BytesPerVertex)
            throw new ArgumentException("The meshes do not match the skinning information.");
        #endregion

        boneInvTransposeTransforms ??= boneTransforms;

        var declaration = source.GetDeclaration();
        int positionOffset = declaration.Find(DeclarationUsage.Position) is {Type: DeclarationType.Float3} position
            ? position.Offset
            : throw new InvalidOperationException("The vertex declaration has no 3D position.");
        var directionOffsets = new List<int>();
        foreach (var usage in new[] {DeclarationUsage.Normal, DeclarationUsage.Tangent, DeclarationUsage.Binormal})
        {
            if (declaration.Find(usage) is {Type: DeclarationType.Float3} element)
                directionOffsets.Add(element.Offset);
        }

        ReadOnlySpan<byte> sourceData = source.VertexData;
        var destinationData = destination.VertexData;
        int stride = source.BytesPerVertex;
        for (int vertex = 0; vertex < _influenceStart.Length - 1; vertex++)
        {
            int start = _influenceStart[vertex], end = _influenceStart[vertex + 1];
            if (start == end) continue;

            var sourceVertex = sourceData.Slice(vertex * stride, stride);
            var destinationVertex = destinationData.Slice(vertex * stride, stride);

            var p = MemoryMarshal.Read<Vector3>(sourceVertex[positionOffset..]);
            var skinned = Vector3.Zero;
            for (int i = start; i < end; i++)
                skinned += Vector3.Transform(p, boneTransforms[_influenceBones[i]]) * _influenceWeights[i];
            MemoryMarshal.Write(destinationVertex[positionOffset..], in skinned);

            foreach (int offset in directionOffsets)
            {
                var direction = MemoryMarshal.Read<Vector3>(sourceVertex[offset..]);
                var skinnedDirection = Vector3.Zero;
                for (int i = start; i < end; i++)
                    skinnedDirection += Vector3.TransformNormal(direction, boneInvTransposeTransforms[_influenceBones[i]]) * _influenceWeights[i];
                MemoryMarshal.Write(destinationVertex[offset..], in skinnedDirection);
            }
        }

        destination.InvalidateVertexes();
    }
}
