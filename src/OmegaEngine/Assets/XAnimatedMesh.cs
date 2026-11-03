/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using NanoByte.Common.Storage;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Meshes;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Foundation.Storage;
using Vortice.Mathematics;

namespace OmegaEngine.Assets;

/// <summary>
/// An animated mesh loaded from a model file (.X, .FBX, .glTF, ...).
/// </summary>
/// <seealso cref="AnimatedModel"/>
public class XAnimatedMesh : Asset
{
    #region Properties
    /// <summary>
    /// The mesh containers making up the hierarchy, in load order.
    /// </summary>
    internal IReadOnlyList<ContainerData> Containers { get; private set; } = [];

    /// <summary>The number of frames (bones) in the flattened hierarchy.</summary>
    internal int FrameCount { get; private set; }

    /// <summary>The index of each frame's parent in the flattened hierarchy; <c>-1</c> for the root.</summary>
    internal int[] FrameParents { get; private set; } = [];

    /// <summary>Each frame's local transformation matrix in the bind pose.</summary>
    internal Matrix4x4[] FrameBindLocal { get; private set; } = [];

    /// <summary>The decomposed scale of each frame's bind pose (for animation blending).</summary>
    internal Vector3[] FrameBindScale { get; private set; } = [];

    /// <summary>The decomposed rotation of each frame's bind pose (for animation blending).</summary>
    internal Quaternion[] FrameBindRotation { get; private set; } = [];

    /// <summary>The decomposed translation of each frame's bind pose (for animation blending).</summary>
    internal Vector3[] FrameBindTranslation { get; private set; } = [];

    /// <summary>Whether each frame's bind pose could be decomposed into scale/rotation/translation.</summary>
    internal bool[] FrameCanBlend { get; private set; } = [];

    /// <summary>The animations, shared and read-only; sampled per-instance.</summary>
    internal ImmutableArray<AnimationClip> AnimationSets { get; private set; } = [];

    /// <summary>
    /// A bounding sphere surrounding the mesh in its bind pose.
    /// </summary>
    public BoundingSphere? BoundingSphere { get; private set; }

    /// <summary>
    /// The names of the animations (animation sets) available in this mesh.
    /// </summary>
    public ImmutableArray<string> AnimationSetNames { get; private set; } = [];
    #endregion

    #region Constructor
    /// <summary>
    /// Loads an animated mesh from a model file.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> providing rendering capabilities.</param>
    /// <param name="id">The ID of the model file in the <c>Meshes</c> content directory. This is also used for finding associated textures.</param>
    /// <exception cref="InvalidDataException">The file does not contain a valid animated mesh.</exception>
    /// <remarks>This should only be called from within <see cref="Get"/> to prevent unnecessary duplicates.</remarks>
    protected XAnimatedMesh(Engine engine, string id)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        if (id == null) throw new ArgumentNullException(nameof(id));
        #endregion

        try
        {
            var hierarchy = MeshImporter.LoadHierarchy(engine.Device, id);

            // Assign the containers right away, so Dispose() cleans them up if anything below fails
            Containers = hierarchy.Containers.Select(x => new ContainerData {WorkMesh = x.Mesh, SkinInfo = x.Skin, FrameIndex = x.FrameIndex}).ToList();

            BuildSkeleton(engine, id, hierarchy);

            // Use the bounding sphere of the bind pose (no bounding box for animated meshes)
            BoundingSphere = hierarchy.BoundingSphere.Radius < 0.01 ? null : hierarchy.BoundingSphere;

            AnimationSets = [..hierarchy.Animations];
            AnimationSetNames = [..hierarchy.Animations.Select(x => x.Name)];
        }
        #region Error handling
        catch (Exception)
        {
            // Since private objects may already have been created at this point, a proper cleanup is needed
            Dispose();
            throw;
        }
        #endregion
    }

    /// <summary>
    /// Stores the flattened frame hierarchy (parents-before-children), decomposes each bind pose,
    /// loads the materials of the mesh containers and resolves their bones to frame indexes.
    /// </summary>
    private void BuildSkeleton(Engine engine, string meshName, ImportedHierarchy hierarchy)
    {
        var frames = hierarchy.Frames;
        var indexByName = new Dictionary<string, int>();
        for (int i = 0; i < frames.Count; i++)
        {
            if (!string.IsNullOrEmpty(frames[i].Name)) indexByName[frames[i].Name] = i;
        }

        FrameCount = frames.Count;
        FrameParents = frames.Select(x => x.Parent).ToArray();
        FrameBindLocal = new Matrix4x4[FrameCount];
        FrameBindScale = new Vector3[FrameCount];
        FrameBindRotation = new Quaternion[FrameCount];
        FrameBindTranslation = new Vector3[FrameCount];
        FrameCanBlend = new bool[FrameCount];
        for (int i = 0; i < FrameCount; i++)
        {
            var local = frames[i].Transform;
            FrameBindLocal[i] = local;
            FrameCanBlend[i] = Matrix4x4.Decompose(local, out FrameBindScale[i], out FrameBindRotation[i], out FrameBindTranslation[i]);
        }

        for (int i = 0; i < Containers.Count; i++)
        {
            var data = Containers[i];
            var xMaterials = XMesh.LoadMaterials(engine, meshName, [hierarchy.Containers[i].Material], out _);
            data.Materials = xMaterials.IsDefaultOrEmpty ? [XMaterial.Default] : [..xMaterials];
            data.NumberSubsets = 1;

            // Resolve bone -> frame index now that all frames are known (done once at load, not per frame)
            if (data.SkinInfo is {} skinInfo)
            {
                data.BoneOffsets = skinInfo.BoneOffsets.ToArray();
                data.BoneNames = skinInfo.BoneNames.ToArray();
                data.BoneFrameIndices = data.BoneNames.Select(name => indexByName.TryGetValue(name, out int frameIndex) ? frameIndex : 0).ToArray();
            }
        }
    }
    #endregion

    #region Static access
    /// <summary>
    /// Returns a cached <see cref="XAnimatedMesh"/> or creates a new one if the requested ID is not cached.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> providing the cache and rendering capabilities.</param>
    /// <param name="id">The ID of the asset to be returned.</param>
    /// <returns>The requested asset; <c>null</c> if <paramref name="id"/> was empty.</returns>
    /// <exception cref="FileNotFoundException">The specified file could not be found.</exception>
    /// <exception cref="IOException">There was an error reading the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
    /// <exception cref="InvalidDataException">The file does not contain a valid animated mesh.</exception>
    /// <remarks>Remember to call <see cref="CacheManager.Clean"/> when done, otherwise this object will never be released.</remarks>
    [return: NotNullIfNotNull(nameof(id))]
    public static XAnimatedMesh? Get(Engine engine, string? id)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        if (string.IsNullOrEmpty(id)) return null;
        #endregion

        // Try to find existing asset in cache
        const string type = "Meshes";
        id = id.ToNativePath();
        string fullID = Path.Combine(type, id);
        var data = engine.Cache.GetAsset<XAnimatedMesh>(fullID);

        // Load from file if not in cache
        if (data == null)
        {
            if (!ContentManager.FileExists(type, id)) throw new FileNotFoundException($"Mesh file not found: {id}", id);

            using (new TimedLogEvent($"Loading animated mesh: {id}"))
                data = new(engine, id) {Name = fullID};
            engine.Cache.AddAsset(data);
        }

        return data;
    }
    #endregion

    //--------------------//

    #region Dispose
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
            { // This block will only be executed on manual disposal, not by Garbage Collection
                Log.Info($"Disposing {this}");
                AnimationSets = [];
                foreach (var container in Containers) container.WorkMesh.Dispose();
                Containers = [];
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }
    #endregion
}
