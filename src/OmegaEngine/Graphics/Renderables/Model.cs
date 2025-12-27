/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.LightSources;
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
    private XMesh _asset;

    /// <summary>The mesh object to use for rendering; never <c>null</c>.</summary>
    protected readonly Mesh Mesh;

    /// <summary>
    /// An array of materials used to render this mesh
    /// </summary>
    public readonly XMaterial[] Materials;

    /// <summary>
    /// Sets the first diffuse texture of the first material.
    /// </summary>
    /// <param name="texture">The texture to set; <c>null</c> for no texture.</param>
    public void SetTexture(ITextureProvider texture)
    {
        Materials[0].DiffuseMaps[0] = texture;
    }

    /// <summary>True if the <see cref="Mesh"/> is created or owned by this class and therefore should also be disposed by it.</summary>
    private readonly bool _ownMesh;

    /// <summary>True if the <see cref="Materials"/> are not owned by <see cref="_asset"/> and therefor need to be released by <see cref="OnDispose"/>.</summary>
    private readonly bool _separateMaterials;
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

    #region Internal texture
    /// <summary>
    /// Creates a new model based upon a <see cref="XMesh"/>, using its internal material data if available.
    /// </summary>
    /// <param name="mesh">The <see cref="XMesh"/> providing the mesh data.</param>
    /// <remarks>Calling <see cref="IDisposable.Dispose"/> will not dispose the <paramref name="mesh"/>. This is handled by the <see cref="CacheManager"/>.</remarks>
    public Model(XMesh mesh)
    {
        _asset = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _asset.HoldReference();

        // Get mesh from asset
        Mesh = mesh.Mesh;

        // Get materials from asset
        Materials = mesh.Materials;
        NumberSubsets = Materials.Length;

        // Get bounding bodies
        BoundingSphere = mesh.BoundingSphere;
        BoundingBox = mesh.BoundingBox;
    }
    #endregion

    #region External texture
    /// <summary>
    /// Creates a new model based upon a <see cref="XMesh"/>, using an external texture and a plain white material.
    /// </summary>
    /// <param name="mesh">The <see cref="XMesh"/> providing the mesh data.</param>
    /// <param name="materials">The materials to use for rendering the model.</param>
    /// <remarks>Calling <see cref="IDisposable.Dispose"/> will call <see cref="IReferenceCount.ReleaseReference"/> on <paramref name="mesh"/> and <paramref name="materials"/>.</remarks>
    public Model(XMesh mesh, params XMaterial[] materials)
    {
        _asset = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _asset.HoldReference();

        // Get mesh from asset
        Mesh = mesh.Mesh;

        // Get separate materials
        Materials = materials ?? [];
        NumberSubsets = Materials.Length;
        foreach (var material in Materials) material.HoldReference();
        _separateMaterials = true;

        // Get bounding bodies
        BoundingSphere = mesh.BoundingSphere;
        BoundingBox = mesh.BoundingBox;
    }
    #endregion

    #region Custom mesh
    /// <summary>
    /// Creates a new model based upon a custom mesh.
    /// </summary>
    /// <param name="mesh">The mesh to render. Normals should be calculated before-hand if they will be used (e.g. by <see cref="SurfaceShader"/>s).</param>
    /// <param name="materials">The materials to use for rendering the model.</param>
    /// <remarks>Calling <see cref="IDisposable.Dispose"/> will call <see cref="IDisposable.Dispose"/> on <paramref name="mesh"/> and <see cref="IReferenceCount.ReleaseReference"/> on <paramref name="materials"/>.</remarks>
    public Model(Mesh mesh, params XMaterial[] materials)
    {
        // Get custom mesh
        Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _ownMesh = true;

        // Get separate materials
        Materials = materials ?? [];
        NumberSubsets = Materials.Length;
        foreach (var material in Materials) material.HoldReference();
        _separateMaterials = true;
    }
    #endregion

    #endregion

    //--------------------//

    #region Render
    /// <inheritdoc/>
    internal override void Render(Camera camera, GetLights? getLights = null)
    {
        base.Render(camera, getLights);
        Engine.State.WorldTransform = WorldTransform;

        var effectiveLights = (SurfaceEffect == SurfaceEffect.Plain || getLights == null)
            ? []
            : getLights(Position, BoundingSphere?.Radius ?? 0);
        for (int i = 0; i < NumberSubsets; i++) RenderSubset(i, camera, effectiveLights);
    }

    protected void RenderSubset(int i, Camera camera, LightSource[] lights)
    {
        using (new ProfilerEvent(() => $"Subset {i}"))
        {
            // Load the subset-material (default to first one, if the subset has no own)
            XMaterial currentMaterial = i < Materials.Length ? Materials[i] : Materials[0];

            RenderHelper(() => Mesh.DrawSubset(i), currentMaterial, camera, lights);
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
            distance = float.MaxValue;
            return false;
        }

        // Transform the world space picking ray into entity space
        ray = new(
            Vector3.TransformCoordinate(ray.Position, InverseWorldTransform),
            // Do not normalize so that ray length remains the same
            Vector3.TransformNormal(ray.Direction, InverseWorldTransform));

        // Check for mesh intersection
        bool result = Mesh.Intersects(ray, out distance, out int _, out _);
        return result;
    }
    #endregion

    //--------------------//

    #region Dispose
    /// <inheritdoc/>
    protected override void OnDispose()
    {
        try
        {
            if (_ownMesh) Mesh?.Dispose();

            if (_asset != null)
            {
                _asset.ReleaseReference();
                _asset = null;
            }

            if (_separateMaterials)
                foreach (var material in Materials) material.ReleaseReference();
        }
        finally
        {
            base.OnDispose();
        }
    }
    #endregion
}
