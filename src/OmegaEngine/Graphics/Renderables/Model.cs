/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Linq;
using NanoByte.Common;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Shaders;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.Renderables;

/// <summary>
/// A model (stored as a Direct3DX <see cref="Mesh"/> with one or more subsets). Handle
/// </summary>
/// <remarks>No custom <see cref="PrimitiveType"/>s. Use <see cref="VertexGroup"/> for that.</remarks>
public partial class Model : PositionableRenderable
{
    #region Variables
    /// <summary>A reference to the asset providing the data for this model.</summary>
    private XMesh? _asset;

    /// <summary>The mesh object to use for rendering.</summary>
    protected readonly Mesh Mesh;

    /// <summary>
    /// An array of materials used to render this mesh
    /// </summary>
    protected readonly XMaterial[] Materials;
    #endregion

    #region Properties
    /// <summary>
    /// The numbers of vertexes in this model
    /// </summary>
    [Description("The numbers of vertexes in this model"), Category("Design")]
    public int VertexCount => Mesh.VertexCount;

    /// <summary>
    /// The numbers of subsets in this model
    /// </summary>
    [Description("The numbers of subsets in this model"), Category("Design")]
    public int NumberSubsets { get; protected set; }
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new model based upon a <see cref="XMesh"/>, using its internal material data if available.
    /// </summary>
    /// <param name="mesh">The <see cref="XMesh"/> providing the mesh and material data.</param>
    /// <remarks>Calling <see cref="IDisposable.Dispose"/> will not dispose the <paramref name="mesh"/>. This is handled by the <see cref="CacheManager"/>.</remarks>
    public Model(XMesh mesh) : this(mesh, mesh.Materials.ToArray())
    {}

    /// <summary>
    /// Creates a new model based upon a <see cref="XMesh"/>, using custom material data.
    /// </summary>
    /// <param name="mesh">The <see cref="XMesh"/> providing the mesh data.</param>
    /// <param name="materials">The materials to use for rendering the model.</param>
    /// <exception cref="ArgumentException">The number of <paramref name="materials"/> provided does not match the number of materials required by the <paramref name="mesh"/>.</exception>
    /// <remarks>Calling <see cref="IDisposable.Dispose"/> will call <see cref="IReferenceCount.ReleaseReference"/> on <paramref name="mesh"/> and <paramref name="materials"/>.</remarks>
    public Model(XMesh mesh, params XMaterial[] materials) : this(mesh.Mesh, materials)
    {
        #region Sanity checks
        if (mesh.Materials.Length != materials.Length)
            throw new ArgumentException($"Mesh requires {mesh.Materials.Length} materials, but {materials.Length} materials were provided");
        #endregion

        _asset = mesh;
        _asset.HoldReference();

        BoundingSphere = mesh.BoundingSphere;
        BoundingBox = mesh.BoundingBox;
        SubsetBoundingBoxes = mesh.SubsetBoundingBoxes;
        SubsetBoundingSpheres = mesh.SubsetBoundingSpheres;
    }

    /// <summary>
    /// Creates a new model based upon a custom mesh.
    /// </summary>
    /// <param name="mesh">The mesh to render. Normals should be calculated before-hand if they will be used (e.g. by <see cref="SurfaceShader"/>s).</param>
    /// <param name="materials">The materials to use for rendering the model.</param>
    /// <remarks>Calling <see cref="IDisposable.Dispose"/> will call <see cref="IDisposable.Dispose"/> on <paramref name="mesh"/> and <see cref="IReferenceCount.ReleaseReference"/> on <paramref name="materials"/>.</remarks>
    public Model(Mesh mesh, params XMaterial[] materials)
    {
        Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));

        Materials = materials;
        NumberSubsets = Materials.Length;
        foreach (var material in Materials) material.HoldReference();
    }
    #endregion

    //--------------------//

    #region Bounding bodies
    private BoundingSphere[]? _subsetBoundingSpheres, _subsetWorldBoundingSpheresCached;
    private bool _subsetWorldBoundingSpheresDirty = true;

    /// <summary>
    /// Per-subset bounding spheres in entity space.
    /// </summary>
    protected BoundingSphere[]? SubsetBoundingSpheres
    {
        get => _subsetBoundingSpheres;
        set => value.To(ref _subsetBoundingSpheres, () => _subsetWorldBoundingSpheresDirty = true);
    }

    /// <summary>
    /// Per-subset bounding spheres in floating world space.
    /// </summary>
    protected BoundingSphere[]? SubsetWorldBoundingSpheres
    {
        get
        {
            RecalcWorldTransform();
            if (_subsetWorldBoundingSpheresDirty)
            {
                if (SubsetBoundingSpheres == null) _subsetWorldBoundingSpheresCached = null;
                else
                {
                    _subsetWorldBoundingSpheresCached ??= new BoundingSphere[SubsetBoundingSpheres.Length];
                    for (int i = 0; i < _subsetWorldBoundingSpheresCached.Length; i++)
                        _subsetWorldBoundingSpheresCached[i] = SubsetBoundingSpheres[i].Transform(WorldTransformWithoutForcedPerspectiveCached);
                }
                _subsetWorldBoundingSpheresDirty = false;
            }
            return _subsetWorldBoundingSpheresCached;
        }
    }

    private BoundingBox[]? _subsetBoundingBoxes, _subsetWorldBoundingBoxesCached;
    private bool _subsetWorldBoundingBoxesDirty = true;

    /// <summary>
    /// Per-subset bounding boxes in entity space.
    /// </summary>
    protected BoundingBox[]? SubsetBoundingBoxes
    {
        get => _subsetBoundingBoxes;
        set => value.To(ref _subsetBoundingBoxes, () => _subsetWorldBoundingBoxesDirty = true);
    }

    /// <summary>
    /// Per-subset bounding boxes in floating world space.
    /// </summary>
    protected BoundingBox[]? SubsetWorldBoundingBoxes
    {
        get
        {
            RecalcWorldTransform();
            if (_subsetWorldBoundingBoxesDirty)
            {
                if (SubsetBoundingBoxes == null) _subsetWorldBoundingBoxesCached = null;
                else
                {
                    _subsetWorldBoundingBoxesCached ??= new BoundingBox[SubsetBoundingBoxes.Length];
                    for (int i = 0; i < _subsetWorldBoundingBoxesCached.Length; i++)
                        _subsetWorldBoundingBoxesCached[i] = SubsetBoundingBoxes[i].Transform(WorldTransformWithoutForcedPerspectiveCached);
                }
                _subsetWorldBoundingBoxesDirty = false;
            }
            return _subsetWorldBoundingBoxesCached;
        }
    }

    /// <inheritdoc/>
    protected override void RecalcWorldTransform()
    {
        if (!WorldTransformDirty) return;

        base.RecalcWorldTransform();

        // Mark subset world bounding bodies as dirty instead of recalculating immediately
        _subsetWorldBoundingSpheresDirty = true;
        _subsetWorldBoundingBoxesDirty = true;
    }
    #endregion

    #region Render
    /// <inheritdoc/>
    internal override void Render(Camera camera, GetEffectiveLights? getEffectiveLights = null)
    {
        base.Render(camera, getEffectiveLights);

        Engine.State.WorldTransform = WorldTransform;

        for (int i = 0; i < NumberSubsets; i++)
        {
            var boundingSphere = SubsetWorldBoundingSpheres?[i];
            var boundingBox = SubsetWorldBoundingBoxes?[i];

            // Per-subset frustum culling
            {
                bool ignoreFarClip = ForcedPerspectiveDistance != null;
                if (boundingSphere is {} sphere && !camera.InFrustum(sphere, ignoreFarClip)) continue;
                if (boundingBox is {} box && !camera.InFrustum(box, ignoreFarClip)) continue;
            }

            RenderSubset(i, camera, getEffectiveLights);

            // Draw per-subset bounding bodies
            if (SurfaceEffect < SurfaceEffect.Glow)
            {
                if (DrawBoundingSphere && boundingSphere is {} sphere) Engine.DrawBoundingSphere(sphere);
                if (DrawBoundingBox && boundingBox is {} box) Engine.DrawBoundingBox(box);
            }
        }
    }

    protected virtual void RenderSubset(int i, Camera camera, GetEffectiveLights? getEffectiveLights)
    {
        using (new ProfilerEvent(() => $"Subset {i}"))
        {
            // Load the subset-material (default to first one, if the subset has no own)
            XMaterial currentMaterial = i < Materials.Length ? Materials[i] : Materials[0];

            var effectiveLights = (SurfaceEffect == SurfaceEffect.Plain || getEffectiveLights == null)
                ? []
                : getEffectiveLights(SubsetWorldBoundingSpheres?[i] ?? GetWorldBoundingSphereOrPosition(), ShadowReceiver);

            RenderHelper(() => Mesh.DrawSubset(i), currentMaterial, camera, effectiveLights);
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

        // Check for mesh intersection
        return Mesh.Intersects(ray, out distance, out int _, out _);
    }
    #endregion

    //--------------------//

    #region Dispose
    /// <inheritdoc/>
    protected override void OnDispose()
    {
        try
        {
            if (_asset == null) Mesh.Dispose();
            else
            {
                _asset.ReleaseReference();
                _asset = null;
            }

            foreach (var material in Materials) material.ReleaseReference();
        }
        finally
        {
            base.OnDispose();
        }
    }
    #endregion
}

