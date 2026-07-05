/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Cameras;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.Renderables;

/// <summary>
/// A mesh-based model with frame-hierarchy (skeletal) animation that can be rendered by the engine.
/// </summary>
public class AnimatedModel : PositionableRenderable
{
    #region Variables
    /// <summary>A reference to the asset providing the data for this model.</summary>
    private XAnimatedMesh? _asset;

    /// <summary>
    /// An array of all materials used to render this mesh (aggregated across all mesh containers, held for reference counting).
    /// </summary>
    protected readonly XMaterial[] Materials;

    /// <summary>Per-mesh-container render state for this instance.</summary>
    private sealed class ContainerInstance
    {
        public required ContainerData Data;
        public bool Skinned;

        /// <summary>The destination mesh drawn each frame (a per-instance clone for skinned containers, the shared source for rigid ones).</summary>
        public Mesh? Mesh;

        /// <summary>Scratch bone transforms; reused every frame to avoid allocations.</summary>
        public Matrix[] BoneTransforms = [];

        /// <summary>Scratch inverse-transpose bone transforms; reused every frame to avoid allocations.</summary>
        public Matrix[] BoneInvTranspose = [];
    }

    private readonly ContainerInstance[] _instances;

    // Per-instance animation scratch arrays (sized to the frame count of the shared skeleton)
    private readonly Matrix[] _combined, _local;
    private readonly Vector3[] _scale, _translation;
    private readonly Quaternion[] _rotation;

    /// <summary>Playback state of a single animation track.</summary>
    private struct Track
    {
        public int SetIndex;
        public double Time;
        public bool Loop;
    }

    private Track _track0 = new() {SetIndex = -1};
    private Track _track1 = new() {SetIndex = -1};
    private bool _blending;
    private double _blendElapsed, _blendDuration;
    #endregion

    #region Properties
    /// <summary>
    /// The numbers of subsets in this model
    /// </summary>
    [Description("The numbers of subsets in this model"), Category("Design")]
    public int NumberSubsets { get; protected set; }

    /// <summary>
    /// The speed at which animations are played back (1 = normal speed).
    /// </summary>
    [DefaultValue(1f), Description("The speed at which animations are played back (1 = normal speed)."), Category("Behavior")]
    public float Speed { get; set; } = 1;

    /// <summary>
    /// The names of the animations available in this model.
    /// </summary>
    public ImmutableArray<string> AnimationSetNames => _asset?.AnimationSetNames ?? [];

    /// <summary>
    /// The name of the animation currently playing on the primary track; <c>null</c> if none.
    /// </summary>
    public string? CurrentAnimation
        => _asset != null && _track0.SetIndex >= 0 && _track0.SetIndex < _asset.AnimationSetNames.Length
            ? _asset.AnimationSetNames[_track0.SetIndex]
            : null;
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new animated model based upon a cached animated mesh, using its internal material data.
    /// </summary>
    /// <param name="mesh">The animated mesh to use for rendering</param>
    public AnimatedModel(XAnimatedMesh mesh)
    {
        _asset = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _asset.HoldReference();

        Materials = mesh.Containers.SelectMany(c => c.Materials).ToArray();
        NumberSubsets = mesh.Containers.Sum(c => c.NumberSubsets);
        foreach (var material in Materials) material.HoldReference();

        // Get bounding bodies (no bounding box for animation hierarchies)
        BoundingSphere = mesh.BoundingSphere;

        // Allocate per-instance animation scratch arrays once
        int frameCount = mesh.FrameCount;
        _combined = new Matrix[frameCount];
        _local = new Matrix[frameCount];
        _scale = new Vector3[frameCount];
        _rotation = new Quaternion[frameCount];
        _translation = new Vector3[frameCount];

        _instances = mesh.Containers.Select(c => new ContainerInstance
        {
            Data = c,
            Skinned = c.SkinInfo != null,
            BoneTransforms = new Matrix[c.BoneOffsets.Length],
            BoneInvTranspose = new Matrix[c.BoneOffsets.Length]
        }).ToArray();

        // Play the first animation by default
        if (!mesh.AnimationSetNames.IsDefaultOrEmpty)
            _track0 = new Track {SetIndex = 0, Loop = true};
    }
    #endregion

    #region Engine
    /// <inheritdoc/>
    protected override void OnEngineSet()
    {
        base.OnEngineSet();

        // Skinned containers get a per-instance writable copy; rigid containers share the read-only source mesh
        foreach (var instance in _instances)
        {
            instance.Mesh = instance.Skinned
                ? instance.Data.WorkMesh.Clone(Engine.Device, MeshFlags.Managed, instance.Data.WorkMesh.GetDeclaration())
                : instance.Data.WorkMesh;
        }
    }
    #endregion

    //--------------------//

    #region Animation control
    /// <summary>
    /// Immediately starts playing an animation by name on the primary track.
    /// </summary>
    /// <param name="name">The name of the animation (see <see cref="AnimationSetNames"/>).</param>
    /// <param name="loop">Whether to loop the animation.</param>
    public void PlayAnimation(string name, bool loop = true)
    {
        int index = _asset?.AnimationSetNames.IndexOf(name) ?? -1;
        if (index >= 0) PlayAnimation(index, loop);
    }

    /// <summary>
    /// Immediately starts playing an animation by index on the primary track.
    /// </summary>
    /// <param name="index">The index of the animation (see <see cref="AnimationSetNames"/>).</param>
    /// <param name="loop">Whether to loop the animation.</param>
    public void PlayAnimation(int index, bool loop = true)
    {
        if (_asset == null || index < 0 || index >= _asset.AnimationSets.Length) return;
        _track0 = new Track {SetIndex = index, Loop = loop};
        _track1 = new Track {SetIndex = -1};
        _blending = false;
    }

    /// <summary>
    /// Smoothly blends from the currently playing animation to another one by name.
    /// </summary>
    /// <param name="name">The name of the animation to blend to (see <see cref="AnimationSetNames"/>).</param>
    /// <param name="duration">The duration of the transition in seconds.</param>
    public void CrossFadeTo(string name, double duration = 0.25)
    {
        int index = _asset?.AnimationSetNames.IndexOf(name) ?? -1;
        if (index >= 0) CrossFadeTo(index, duration);
    }

    /// <summary>
    /// Smoothly blends from the currently playing animation to another one by index.
    /// </summary>
    /// <param name="index">The index of the animation to blend to (see <see cref="AnimationSetNames"/>).</param>
    /// <param name="duration">The duration of the transition in seconds.</param>
    public void CrossFadeTo(int index, double duration = 0.25)
    {
        if (_asset == null || index < 0 || index >= _asset.AnimationSets.Length) return;
        if (_track0.SetIndex < 0)
        {
            PlayAnimation(index);
            return;
        }

        _track1 = new Track {SetIndex = index, Loop = true};
        _blending = true;
        _blendElapsed = 0;
        _blendDuration = duration;
    }
    #endregion

    #region Update
    /// <inheritdoc/>
    protected override void OnPreRender()
    {
        base.OnPreRender();

        // Only update once per frame, even when rendered in multiple views (RenderCount is still 0 on the first render of the frame)
        if (RenderCount > 0) return;
        if (_asset == null || _asset.FrameCount == 0) return;

        using (new ProfilerEvent("Update animation"))
        {
            BuildPose(Engine.LastFrameGameTime);
            UpdateSkinnedMeshes();
        }
    }

    /// <summary>
    /// Advances the animation tracks and computes the combined (model-space) transform of every frame.
    /// </summary>
    private void BuildPose(double elapsedTime)
    {
        var mesh = _asset!;
        int n = mesh.FrameCount;

        if (_track0.SetIndex >= 0) AdvanceTrack(ref _track0, elapsedTime);
        if (_blending && _track1.SetIndex >= 0) AdvanceTrack(ref _track1, elapsedTime);

        // Determine blend weight of the secondary track and finish the transition once complete
        float weight1 = 0;
        if (_blending && _track1.SetIndex >= 0)
        {
            _blendElapsed += elapsedTime;
            weight1 = _blendDuration <= 0 ? 1f : (float)Math.Min(1.0, _blendElapsed / _blendDuration);
            if (weight1 >= 1f)
            {
                _track0 = _track1;
                _track1 = new Track {SetIndex = -1};
                _blending = false;
                weight1 = 0;
            }
        }

        // Start from the bind pose
        Array.Copy(mesh.FrameBindScale, _scale, n);
        Array.Copy(mesh.FrameBindRotation, _rotation, n);
        Array.Copy(mesh.FrameBindTranslation, _translation, n);

        if (_track0.SetIndex >= 0) SampleTrack(_track0, replaceOnly: true, weight: 1f);
        if (weight1 > 0 && _track1.SetIndex >= 0) SampleTrack(_track1, replaceOnly: false, weight: weight1);

        // Compose local matrices from the sampled scale/rotation/translation
        for (int i = 0; i < n; i++)
        {
            _local[i] = mesh.FrameCanBlend[i]
                ? Matrix.Scaling(_scale[i]) * Matrix.RotationQuaternion(_rotation[i]) * Matrix.Translation(_translation[i])
                : mesh.FrameBindLocal[i];
        }

        // Accumulate combined transforms; parents always precede children in the flattened list
        for (int i = 0; i < n; i++)
        {
            int parent = mesh.FrameParents[i];
            _combined[i] = parent < 0 ? _local[i] : _local[i] * _combined[parent];
        }
    }

    private void AdvanceTrack(ref Track track, double elapsedTime)
    {
        track.Time += elapsedTime * Speed;
        if (!track.Loop)
        {
            double period = _asset!.AnimationSets[track.SetIndex].Period;
            if (track.Time > period) track.Time = period;
        }
    }

    /// <summary>
    /// Samples an animation track into the per-frame scale/rotation/translation scratch arrays.
    /// </summary>
    /// <param name="track">The track to sample.</param>
    /// <param name="replaceOnly">If <c>true</c>, animated frames overwrite the scratch values; if <c>false</c>, they are blended in using <paramref name="weight"/>.</param>
    /// <param name="weight">The blend weight of the track (only used when <paramref name="replaceOnly"/> is <c>false</c>).</param>
    private void SampleTrack(in Track track, bool replaceOnly, float weight)
    {
        var mesh = _asset!;
        var set = mesh.AnimationSets[track.SetIndex];
        int[] map = mesh.AnimationFrameMaps[track.SetIndex];

        double position = set.GetPeriodicPosition(track.Time);

        for (int a = 0; a < map.Length; a++)
        {
            int frame = map[a];
            if (frame < 0) continue;

            var output = set.GetTransformation(position, a);
            DecomposeOutput(output, frame, out var scale, out var rotation, out var translation);

            if (replaceOnly)
            {
                _scale[frame] = scale;
                _rotation[frame] = rotation;
                _translation[frame] = translation;
            }
            else
            {
                _scale[frame] = Vector3.Lerp(_scale[frame], scale, weight);
                _rotation[frame] = Quaternion.Slerp(_rotation[frame], rotation, weight);
                _translation[frame] = Vector3.Lerp(_translation[frame], translation, weight);
            }
        }
    }

    /// <summary>
    /// Extracts scale/rotation/translation from an <see cref="AnimationOutput"/>, falling back to the frame's bind pose for any component the output does not provide.
    /// </summary>
    private void DecomposeOutput(AnimationOutput output, int frame, out Vector3 scale, out Quaternion rotation, out Vector3 translation)
    {
        var mesh = _asset!;
        scale = mesh.FrameBindScale[frame];
        rotation = mesh.FrameBindRotation[frame];
        translation = mesh.FrameBindTranslation[frame];

        var flags = output.Flags;
        if ((flags & AnimationOutputFlags.Transformation) != 0)
            output.Transformation.Decompose(out scale, out rotation, out translation);
        else
        {
            if ((flags & AnimationOutputFlags.Scale) != 0) scale = output.Scaling;
            if ((flags & AnimationOutputFlags.Rotation) != 0) rotation = output.Rotation;
            if ((flags & AnimationOutputFlags.Translation) != 0) translation = output.Translation;
        }
    }

    private void UpdateSkinnedMeshes()
    {
        foreach (var instance in _instances)
        {
            if (!instance.Skinned || instance.Mesh == null) continue;
            var data = instance.Data;

            for (int b = 0; b < instance.BoneTransforms.Length; b++)
            {
                var transform = data.BoneOffsets[b] * _combined[data.BoneFrameIndices[b]];
                instance.BoneTransforms[b] = transform;
                instance.BoneInvTranspose[b] = Matrix.Transpose(Matrix.Invert(transform));
            }

            // Software skinning: read the bind-pose source vertices, write the deformed vertices into the per-instance copy
            var source = data.WorkMesh.LockVertexBuffer(LockFlags.ReadOnly);
            try
            {
                var destination = instance.Mesh.LockVertexBuffer(LockFlags.None);
                try
                {
                    data.SkinInfo!.UpdateSkinnedMesh(instance.BoneTransforms, instance.BoneInvTranspose, source, destination);
                }
                finally
                {
                    instance.Mesh.UnlockVertexBuffer();
                }
            }
            finally
            {
                data.WorkMesh.UnlockVertexBuffer();
            }
        }
    }
    #endregion

    #region Render
    /// <inheritdoc/>
    internal override void Render(Camera camera, GetEffectiveLights? getEffectiveLights = null)
    {
        base.Render(camera, getEffectiveLights);

        var effectiveLights = (SurfaceEffect == SurfaceEffect.Plain || getEffectiveLights == null)
            ? []
            : getEffectiveLights(GetWorldBoundingSphereOrPosition(), ShadowReceiver);

        foreach (var instance in _instances)
        {
            if (instance.Mesh is not {} mesh) continue;
            var data = instance.Data;

            // Skinned vertices are already in model space; rigid meshes are placed by their frame transform
            Engine.State.WorldTransform = instance.Skinned || data.FrameIndex < 0
                ? WorldTransform
                : _combined[data.FrameIndex] * WorldTransform;

            for (int i = 0; i < data.NumberSubsets; i++)
            {
                XMaterial material = i < data.Materials.Length ? data.Materials[i] : data.Materials[0];
                int subset = i;
                using (new ProfilerEvent(() => $"Subset {subset}"))
                    RenderHelper(() => mesh.DrawSubset(subset), material, camera, effectiveLights);
            }
        }
    }
    #endregion

    #region Intersect
    /// <inheritdoc/>
    public override bool Intersects(Ray ray, out float distance)
    {
        // Perform pre-checks with bounding bodies to filter out broad misses
        if (!IntersectsBounding(ray))
        {
            distance = float.PositiveInfinity;
            return false;
        }

        // Transform the world space picking ray into entity space
        ray = new(
            Vector3.TransformCoordinate(ray.Position, InverseWorldTransform),
            // Do not normalize so that ray length remains the same
            Vector3.TransformNormal(ray.Direction, InverseWorldTransform));

        bool hit = false;
        distance = float.PositiveInfinity;
        foreach (var instance in _instances)
        {
            if (instance.Mesh == null) continue;

            var testRay = ray;
            if (!instance.Skinned && instance.Data.FrameIndex >= 0)
            {
                // Undo the rigid frame transform to test against the mesh in its own space
                var inverse = Matrix.Invert(_combined[instance.Data.FrameIndex]);
                testRay = new(
                    Vector3.TransformCoordinate(ray.Position, inverse),
                    Vector3.TransformNormal(ray.Direction, inverse));
            }

            if (instance.Mesh.Intersects(testRay, out float d, out int _, out _) && d < distance)
            {
                distance = d;
                hit = true;
            }
        }
        return hit;
    }
    #endregion

    //--------------------//

    #region Dispose
    /// <inheritdoc/>
    protected override void OnDispose()
    {
        try
        {
            // Dispose the per-instance skinned mesh copies (rigid instances reference the shared source, disposed with the asset)
            foreach (var instance in _instances)
                if (instance.Skinned) instance.Mesh?.Dispose();

            foreach (var material in Materials) material.ReleaseReference();

            if (_asset != null)
            {
                _asset.ReleaseReference();
                _asset = null;
            }
        }
        finally
        {
            base.OnDispose();
        }
    }
    #endregion
}
