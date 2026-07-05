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
using NanoByte.Common;
using NanoByte.Common.Storage;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Foundation.Storage;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Assets;

/// <summary>
/// An animated mesh loaded from an .X file.
/// </summary>
/// <seealso cref="AnimatedModel"/>
public class XAnimatedMesh : Asset
{
    #region Variables
    private AnimationAllocation? _allocation;
    private Frame? _rootFrame;
    private AnimationController? _controller;
    #endregion

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
    internal Matrix[] FrameBindLocal { get; private set; } = [];

    /// <summary>The decomposed scale of each frame's bind pose (for animation blending).</summary>
    internal Vector3[] FrameBindScale { get; private set; } = [];

    /// <summary>The decomposed rotation of each frame's bind pose (for animation blending).</summary>
    internal Quaternion[] FrameBindRotation { get; private set; } = [];

    /// <summary>The decomposed translation of each frame's bind pose (for animation blending).</summary>
    internal Vector3[] FrameBindTranslation { get; private set; } = [];

    /// <summary>Whether each frame's bind pose could be decomposed into scale/rotation/translation.</summary>
    internal bool[] FrameCanBlend { get; private set; } = [];

    /// <summary>The animation sets, shared and read-only; sampled per-instance via <see cref="AnimationSet.GetTransformation"/>.</summary>
    internal ImmutableArray<AnimationSet> AnimationSets { get; private set; } = [];

    /// <summary>For each animation set, maps an animation index to the frame index it animates.</summary>
    internal int[][] AnimationFrameMaps { get; private set; } = [];

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
    /// Loads an animated mesh from an .X file.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> providing rendering capabilities.</param>
    /// <param name="stream">The .X file to load the mesh from.</param>
    /// <param name="meshName">The name of the mesh. This is used for finding associated textures.</param>
    /// <exception cref="InvalidDataException"><paramref name="stream"/> does not contain a valid animated mesh.</exception>
    /// <remarks>This should only be called from within <see cref="Get"/> to prevent unnecessary duplicates.</remarks>
    protected XAnimatedMesh(Engine engine, Stream stream, string meshName)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        #endregion

        try
        {
            // Load the frame hierarchy and animation controller
            _allocation = new AnimationAllocation(engine, meshName);
            AnimationController controller;
            try
            {
                _rootFrame = Frame.LoadHierarchyFromX(engine.Device, stream, MeshFlags.Managed, _allocation, null, out controller);
            }
            catch (Direct3D9Exception ex) when (stream.CanSeek)
            {
                Log.Warn("LoadHierarchyFromX with MeshFlags.Managed failed; retrying with MeshFlags.SystemMemory", ex);
                stream.Position = 0;
                _allocation = new AnimationAllocation(engine, meshName);
                _rootFrame = Frame.LoadHierarchyFromX(engine.Device, stream, MeshFlags.SystemMemory, _allocation, null, out controller);
            }
            _controller = controller;

            Containers = _allocation.Containers;
            BuildSkeleton();

            // Compute bounding sphere from the bind pose (no bounding box for animated meshes)
            var boundingSphere = Frame.CalculateBoundingSphere(_rootFrame);
            BoundingSphere = boundingSphere.Radius < 0.01 ? null : boundingSphere;

            BuildAnimations();
        }
        #region Error handling
        catch (Direct3D9Exception ex)
        {
            Dispose();
            throw new InvalidDataException(ex.Message, ex);
        }
        catch (Exception)
        {
            // Since private objects have already been created at this point, a proper cleanup is needed
            Dispose();
            throw;
        }
        #endregion
    }

    /// <summary>
    /// Walks the frame hierarchy to flatten it into parallel arrays (parents-before-children), decomposes each bind pose,
    /// and resolves each mesh container and bone to a frame index.
    /// </summary>
    private void BuildSkeleton()
    {
        var frames = new List<Frame>();
        var parents = new List<int>();
        var indexByName = new Dictionary<string, int>();

        void Traverse(Frame? frame, int parentIndex)
        {
            while (frame != null)
            {
                int index = frames.Count;
                frames.Add(frame);
                parents.Add(parentIndex);
                if (!string.IsNullOrEmpty(frame.Name)) indexByName[frame.Name] = index;

                for (var container = frame.MeshContainer; container != null; container = container.NextMeshContainer)
                {
                    // Match by mesh reference (SlimDX caches Mesh COM wrappers by pointer; MeshContainer wrappers are not stable)
                    var mesh = container.MeshData?.Mesh;
                    if (mesh == null) continue;
                    foreach (var data in Containers)
                        if (data.WorkMesh == mesh) data.FrameIndex = index;
                }

                Traverse(frame.FirstChild, index);
                frame = frame.Sibling;
            }
        }

        Traverse(_rootFrame, -1);

        FrameCount = frames.Count;
        FrameParents = [..parents];
        FrameBindLocal = new Matrix[FrameCount];
        FrameBindScale = new Vector3[FrameCount];
        FrameBindRotation = new Quaternion[FrameCount];
        FrameBindTranslation = new Vector3[FrameCount];
        FrameCanBlend = new bool[FrameCount];
        for (int i = 0; i < FrameCount; i++)
        {
            var local = frames[i].TransformationMatrix;
            FrameBindLocal[i] = local;
            FrameCanBlend[i] = local.Decompose(out FrameBindScale[i], out FrameBindRotation[i], out FrameBindTranslation[i]);
        }

        // Resolve bone -> frame index now that all frames are known (done once at load, not per frame)
        foreach (var data in Containers)
        {
            if (data.SkinInfo == null) continue;
            data.BoneFrameIndices = new int[data.BoneNames.Length];
            for (int i = 0; i < data.BoneNames.Length; i++)
                data.BoneFrameIndices[i] = indexByName.TryGetValue(data.BoneNames[i], out int frameIndex) ? frameIndex : 0;
        }

        // Store the name -> index lookup for animation resolution
        _frameIndexByName = indexByName;
    }

    private Dictionary<string, int> _frameIndexByName = [];

    /// <summary>
    /// Caches the animation sets and, for each, a map from animation index to the frame index it animates.
    /// </summary>
    private void BuildAnimations()
    {
        if (_controller == null) return;

        int setCount = _controller.AnimationSetCount;
        var sets = ImmutableArray.CreateBuilder<AnimationSet>(setCount);
        var names = ImmutableArray.CreateBuilder<string>(setCount);
        AnimationFrameMaps = new int[setCount][];

        for (int s = 0; s < setCount; s++)
        {
            var set = _controller.GetAnimationSet<AnimationSet>(s);
            sets.Add(set);
            names.Add(set.Name);

            int animationCount = set.AnimationCount;
            var map = new int[animationCount];
            for (int a = 0; a < animationCount; a++)
                map[a] = _frameIndexByName.TryGetValue(set.GetAnimationName(a), out int frameIndex) ? frameIndex : -1;
            AnimationFrameMaps[s] = map;
        }

        AnimationSets = sets.MoveToImmutable();
        AnimationSetNames = names.MoveToImmutable();
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
            using (new TimedLogEvent($"Loading animated mesh: {id}"))
            using (var stream = ContentManager.GetFileStream(type, id))
                data = new(engine, stream, id) {Name = fullID};
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
                _controller?.Dispose();
                if (_rootFrame != null && _allocation != null)
                    Frame.DestroyHierarchy(_rootFrame, _allocation);
                _rootFrame = null;
                _controller = null;
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }
    #endregion
}
