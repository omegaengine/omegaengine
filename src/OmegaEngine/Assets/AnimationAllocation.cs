/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using OmegaEngine.Graphics;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Assets;

/// <summary>
/// Per-<see cref="MeshContainer"/> data collected while loading an animated mesh hierarchy.
/// </summary>
/// <seealso cref="AnimationAllocation"/>
internal sealed class ContainerData
{
    /// <summary>The mesh in its bind pose. Shared/read-only; per-instance skinning writes into copies.</summary>
    public required Mesh WorkMesh;

    /// <summary>The skinning information; <c>null</c> for rigid (non-skinned) containers.</summary>
    public SkinInfo? SkinInfo;

    /// <summary>Materials (one per subset) loaded from the container.</summary>
    public XMaterial[] Materials = [XMaterial.Default];

    /// <summary>The number of subsets in <see cref="WorkMesh"/>.</summary>
    public int NumberSubsets = 1;

    /// <summary>The bone-offset (inverse bind pose) matrices, indexed by bone.</summary>
    public Matrix[] BoneOffsets = [];

    /// <summary>The name of the <see cref="Frame"/> each bone is attached to, indexed by bone.</summary>
    public string[] BoneNames = [];

    /// <summary>The index into the flattened frame list of the <see cref="Frame"/> each bone is attached to, indexed by bone. Resolved after loading.</summary>
    public int[] BoneFrameIndices = [];

    /// <summary>The index into the flattened frame list of the <see cref="Frame"/> this container is attached to. Resolved after loading.</summary>
    public int FrameIndex = -1;
}

/// <summary>
/// Allocates the frame hierarchy of an animated mesh while loading it via <see cref="Frame.LoadHierarchyFromX(Device, System.IO.Stream, MeshFlags, IAllocateHierarchy, ILoadUserData, out AnimationController)"/>.
/// </summary>
/// <param name="engine">The <see cref="Engine"/> used to load associated textures.</param>
/// <param name="meshName">The name of the mesh. Used for finding associated textures.</param>
/// <seealso cref="XAnimatedMesh"/>
public sealed class AnimationAllocation(Engine engine, string meshName) : IAllocateHierarchy
{
    private readonly List<ContainerData> _containers = [];

    /// <summary>All mesh containers created while loading, in creation order.</summary>
    internal IReadOnlyList<ContainerData> Containers => _containers;

    Frame IAllocateHierarchy.CreateFrame(string name)
        => new() {Name = name ?? "", TransformationMatrix = Matrix.Identity};

    MeshContainer IAllocateHierarchy.CreateMeshContainer(string name, MeshData meshData, ExtendedMaterial[] materials, EffectInstance[] effectInstances, int[] adjacency, SkinInfo skinInfo)
    {
        var mesh = meshData.Mesh;

        var xMaterials = XMesh.LoadMaterials(engine, meshName, materials, effectInstances, out _);
        var data = new ContainerData
        {
            WorkMesh = mesh,
            SkinInfo = skinInfo,
            Materials = xMaterials.IsDefaultOrEmpty ? [XMaterial.Default] : [..xMaterials],
            NumberSubsets = materials is {Length: > 0} ? materials.Length : 1
        };

        if (skinInfo != null)
        {
            int boneCount = skinInfo.BoneCount;
            data.BoneOffsets = new Matrix[boneCount];
            data.BoneNames = new string[boneCount];
            for (int i = 0; i < boneCount; i++)
            {
                data.BoneOffsets[i] = skinInfo.GetBoneOffsetMatrix(i);
                data.BoneNames[i] = skinInfo.GetBoneName(i);
            }
        }

        _containers.Add(data);
        return new MeshContainer {Name = name ?? "", MeshData = new MeshData(mesh), SkinInfo = skinInfo};
    }

    void IAllocateHierarchy.DestroyFrame(Frame frame) => frame?.Dispose();

    void IAllocateHierarchy.DestroyMeshContainer(MeshContainer container)
    {
        container.MeshData?.Mesh?.Dispose();
        container.SkinInfo?.Dispose();
        container.Dispose();
    }
}
