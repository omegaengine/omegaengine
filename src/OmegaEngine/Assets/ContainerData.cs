/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Numerics;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Meshes;

namespace OmegaEngine.Assets;

/// <summary>
/// Per-mesh data collected while loading an animated mesh hierarchy.
/// </summary>
/// <seealso cref="XAnimatedMesh"/>
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
    public Matrix4x4[] BoneOffsets = [];

    /// <summary>The name of the frame each bone is attached to, indexed by bone.</summary>
    public string[] BoneNames = [];

    /// <summary>The index into the flattened frame list of the frame each bone is attached to, indexed by bone. Resolved after loading.</summary>
    public int[] BoneFrameIndices = [];

    /// <summary>The index into the flattened frame list of the frame this container is attached to.</summary>
    public int FrameIndex = -1;
}
