/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using SharpGen.Runtime;
using Vortice.Direct3D9;

namespace OmegaEngine.D3DX;

/// <summary>
/// Wraps an <c>ID3DXSkinInfo</c>: the bones of a skinned mesh, their bind pose and the vertices they influence.
/// </summary>
internal sealed unsafe class SkinInfo : ComObject
{
    #region vtable
    // ID3DXSkinInfo (d3dx9mesh.h)
    private const int
        VtSetBoneInfluence = 3,
        VtGetNumBones = 9,
        VtSetBoneName = 14,
        VtGetBoneName = 15,
        VtSetBoneOffsetMatrix = 16,
        VtGetBoneOffsetMatrix = 17,
        VtUpdateSkinnedMesh = 24;
    #endregion

    internal SkinInfo(nint nativePointer) : base(nativePointer)
    {}

    /// <summary>
    /// Creates skinning information without any bone influences.
    /// </summary>
    /// <param name="vertexCount">The number of vertices of the mesh to be skinned.</param>
    /// <param name="format">The vertex format of the mesh to be skinned.</param>
    /// <param name="boneCount">The number of bones.</param>
    public SkinInfo(int vertexCount, VertexFormat format, int boneCount)
        : this(Create(vertexCount, format, boneCount))
    {}

    private static nint Create(int vertexCount, VertexFormat format, int boneCount)
    {
        nint skinInfo;
        new Result(D3DX9Native.D3DXCreateSkinInfoFVF((uint)vertexCount, (uint)format, (uint)boneCount, &skinInfo)).CheckError();
        return skinInfo;
    }

    /// <summary>
    /// The number of bones.
    /// </summary>
    public int BoneCount => (int)((delegate* unmanaged[Stdcall]<nint, uint>)this[VtGetNumBones])(NativePointer);

    /// <summary>
    /// Gets the name of a bone, which matches the name of the <see cref="Frame"/> it is attached to.
    /// </summary>
    public string GetBoneName(int bone)
        => Marshal.PtrToStringAnsi(((delegate* unmanaged[Stdcall]<nint, uint, nint>)this[VtGetBoneName])(NativePointer, (uint)bone)) ?? "";

    /// <summary>
    /// Sets the name of a bone.
    /// </summary>
    public void SetBoneName(int bone, string name)
    {
        nint pName = Marshal.StringToHGlobalAnsi(name);
        try
        {
            Check(((delegate* unmanaged[Stdcall]<nint, uint, nint, int>)this[VtSetBoneName])(NativePointer, (uint)bone, pName));
        }
        finally
        {
            Marshal.FreeHGlobal(pName);
        }
    }

    /// <summary>
    /// Gets the offset matrix of a bone, which transforms from mesh space to the bone's space in the bind pose.
    /// </summary>
    public Matrix4x4 GetBoneOffsetMatrix(int bone)
    {
        var matrix = ((delegate* unmanaged[Stdcall]<nint, uint, Matrix4x4*>)this[VtGetBoneOffsetMatrix])(NativePointer, (uint)bone);
        return matrix == null ? Matrix4x4.Identity : *matrix;
    }

    /// <summary>
    /// Sets the offset matrix of a bone.
    /// </summary>
    public void SetBoneOffsetMatrix(int bone, Matrix4x4 matrix)
        => Check(((delegate* unmanaged[Stdcall]<nint, uint, Matrix4x4*, int>)this[VtSetBoneOffsetMatrix])(NativePointer, (uint)bone, &matrix));

    /// <summary>
    /// Sets the vertices a bone influences and by how much.
    /// </summary>
    public void SetBoneInfluence(int bone, uint[] vertices, float[] weights)
    {
        #region Sanity checks
        if (vertices == null) throw new ArgumentNullException(nameof(vertices));
        if (weights == null) throw new ArgumentNullException(nameof(weights));
        if (vertices.Length != weights.Length) throw new ArgumentException("Each vertex needs exactly one weight.", nameof(weights));
        #endregion

        fixed (uint* pVertices = vertices)
        fixed (float* pWeights = weights)
            Check(((delegate* unmanaged[Stdcall]<nint, uint, uint, uint*, float*, int>)this[VtSetBoneInfluence])(NativePointer, (uint)bone, (uint)vertices.Length, pVertices, pWeights));
    }

    /// <summary>
    /// Applies software skinning: transforms the source vertices by the weighted bone transforms and writes them to the destination.
    /// </summary>
    /// <param name="boneTransforms">The transform of each bone, including its offset matrix.</param>
    /// <param name="boneInvTransposeTransforms">The inverse transpose of each bone transform, used for normals; may be <c>null</c> to use <paramref name="boneTransforms"/>.</param>
    /// <param name="sourceVertices">A pointer to the vertices in their bind pose, e.g. from <see cref="Mesh.LockVertexBuffer"/>.</param>
    /// <param name="destinationVertices">A pointer to the vertices to write, with the same layout as <paramref name="sourceVertices"/>.</param>
    public void UpdateSkinnedMesh(Matrix4x4[] boneTransforms, Matrix4x4[]? boneInvTransposeTransforms, nint sourceVertices, nint destinationVertices)
    {
        #region Sanity checks
        if (boneTransforms == null) throw new ArgumentNullException(nameof(boneTransforms));
        if (boneTransforms.Length < BoneCount) throw new ArgumentException("There must be a transform for each bone.", nameof(boneTransforms));
        if (boneInvTransposeTransforms != null && boneInvTransposeTransforms.Length < BoneCount) throw new ArgumentException("There must be a transform for each bone.", nameof(boneInvTransposeTransforms));
        #endregion

        fixed (Matrix4x4* pBoneTransforms = boneTransforms)
        fixed (Matrix4x4* pBoneInvTransposeTransforms = boneInvTransposeTransforms)
            Check(((delegate* unmanaged[Stdcall]<nint, Matrix4x4*, Matrix4x4*, nint, nint, int>)this[VtUpdateSkinnedMesh])(NativePointer, pBoneTransforms, pBoneInvTransposeTransforms, sourceVertices, destinationVertices));
    }

    private static void Check(int hr) => new Result(hr).CheckError();
}
