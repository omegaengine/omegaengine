/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using NanoByte.Common;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Shaders;
using OmegaEngine.Graphics.VertexDecl;
using SlimDX;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Renderables;

/// <summary>
/// Displays a multi-textured terrain
/// </summary>
public partial class Terrain : Model
{
    #region Variables
    private SurfaceShader[] _subsetShaders;
    private BoundingBox[] _subsetBoundingBoxes, _subsetWorldBoundingBoxes;
    private float _blockSize;

    private readonly int[] _indexBuffer;
    private readonly Vector3[] _vertexBuffer;
    #endregion

    #region Properties

    #region Flags
    /// <summary>
    /// Use/support lighting when rendering this terrain?
    /// </summary>
    [Description("Use/support lighting when rendering this terrain?"), Category("Appearance")]
    public bool Lighting { get; }

    /// <summary>
    /// The size of the terrain in game units
    /// </summary>
    [Description("The size of the terrain in game units"), Category("Layout")]
    public Size Size { get; private set; }

    /// <summary>
    /// A factor by which the terrain is horizontally stretched
    /// </summary>
    [Description("A factor by which the terrain is horizontally stretched"), Category("Layout")]
    public float StretchH { get; private set; }

    /// <summary>
    /// A factor by which the terrain is vertically stretched
    /// </summary>
    [Description(" A factor by which the terrain is vertically stretched"), Category("Layout")]
    public float StretchV { get; private set; }
    #endregion

    #region Transform results
    /// <inheritdoc/>
    protected override void RecalcWorldTransform()
    {
        base.RecalcWorldTransform();

        // Transform subset-specific bounding bodies into world space
        if (_subsetBoundingBoxes != null)
        {
            _subsetWorldBoundingBoxes = new BoundingBox[_subsetBoundingBoxes.Length];
            for (int i = 0; i < NumberSubsets; i++)
                _subsetWorldBoundingBoxes[i] = _subsetBoundingBoxes[i].Transform(WorldTransform);
        }
    }
    #endregion

    #endregion

    #region Constructor
    /// <summary>
    /// Internal helper constructor
    /// </summary>
    /// <param name="mesh">The mesh use for rendering</param>
    /// <param name="material">The material to use for rendering the terrain</param>
    /// <param name="lighting">Use/support lighting when rendering this terrain?</param>
    protected Terrain(Mesh mesh, XMaterial material, bool lighting) : base(mesh, material)
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        #endregion

        SurfaceEffect = SurfaceEffect.Shader;

        Lighting = lighting;

        #region Copy index and vertex buffer content
        // Copy buffers to RAM for fast position lookups
        using (new TimedLogEvent("Copy index and vertex buffer content"))
        {
            _indexBuffer = BufferHelper.ReadIndexBuffer(mesh);

            // Get the vertex positions from the VertexBuffer
            if (lighting) // Different vertex formats
            {
                var verts = BufferHelper.ReadVertexBuffer<PositionNormalMultiTextured>(mesh);
                _vertexBuffer = new Vector3[verts.Length];
                for (int i = 0; i < verts.Length; i++)
                    _vertexBuffer[i] = verts[i].Position;
            }
            else
            {
                var verts = BufferHelper.ReadVertexBuffer<PositionMultiTextured>(mesh);
                _vertexBuffer = new Vector3[verts.Length];
                for (int i = 0; i < verts.Length; i++)
                    _vertexBuffer[i] = verts[i].Position;
            }
        }
        #endregion
    }
    #endregion

    #region Static access
    /// <summary>
    /// Creates a new terrain from a height-map and a texture-map
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to create the terrain in</param>
    /// <param name="size">The size of the terrain</param>
    /// <param name="stretchH">A factor by which all horizontal distances are multiplied</param>
    /// <param name="stretchV">A factor by which all vertical distances are multiplied</param>
    /// <param name="heightMap">The height values of the terrain in a 2D array.
    ///   Grid size = Terrain size</param>
    /// <param name="occlusionIntervalMap">The angles at which the global light source occlusion begins and ends.
    ///   Grid size = Terrain size; may be <c>null</c> for no shadowing</param>
    /// <param name="textureMap">The texture values of the terrain in a 2D array.
    ///   Grid size = Terrain size / 3</param>
    /// <param name="textures">An array with a maximum of 16 texture names associated to <paramref name="textureMap"/></param>
    /// <param name="lighting">Shall this mesh be prepared for lighting? (calculate normal vectors, make shaders support lighting, ...)</param>
    /// <param name="blockSize">How many points in X and Y direction shall one block for culling be?</param>
    /// <returns>The newly created terrain</returns>
    /// <exception cref="FileNotFoundException">One of the specified texture files could not be found.</exception>
    /// <exception cref="IOException">There was an error reading one of the texture files.</exception>
    /// <exception cref="UnauthorizedAccessException">Read access to one of the texture files is not permitted.</exception>
    /// <exception cref="InvalidDataException">One of the texture files does not contain a valid texture.</exception>
    public static Terrain Create(Engine engine, Size size, float stretchH, float stretchV, ByteGrid heightMap, NibbleGrid textureMap, string[] textures, ByteVector4Grid? occlusionIntervalMap, bool lighting, int blockSize)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        if (heightMap == null) throw new ArgumentNullException(nameof(heightMap));
        if (textureMap == null) throw new ArgumentNullException(nameof(textureMap));
        if (textures == null) throw new ArgumentNullException(nameof(textures));
        #endregion

        if (TerrainShader.MinShaderModel > engine.Capabilities.MaxShaderModel)
            throw new NotSupportedException(Resources.NotSupportedShader);

        // Generate mesh with subsets and bounding bodies
        var terrain = new Terrain(
            BuildMesh(engine, size, stretchH, stretchV, heightMap, textureMap, occlusionIntervalMap, lighting, blockSize, out var subsetShaders, out var subsetBoundingBoxes),
            BuildMaterial(engine, textures),
            lighting)
        {
            // Set properties here to keep constructor nice and simple
            Size = size, StretchH = stretchH, StretchV = stretchV,
            _blockSize = blockSize,
            _subsetBoundingBoxes = subsetBoundingBoxes,
            _subsetShaders = subsetShaders,
            NumberSubsets = subsetBoundingBoxes.Length
        };

        return terrain;
    }
    #endregion

    //--------------------//

    #region Modify color
    /// <summary>
    /// Modifies the color of a part of the terrain.
    /// </summary>
    /// <param name="start">The top-left index of the area to modify.</param>
    /// <param name="partialColorMap">A 2D array containing the new color values - array size specifies size of the area to modify.</param>
    /// <remarks>Cannot be called when <see cref="Lighting"/> is <c>false</c>, because coloring uses the lighting subsystem.</remarks>
    public void ModifyColor(Point start, Color[,] partialColorMap)
    {
        #region Sanity checks
        if (!Lighting) throw new InvalidOperationException(Resources.NoModifyTerrainColorWithoutLighting);
        if (partialColorMap == null) throw new ArgumentNullException(nameof(partialColorMap));
        #endregion

        var modifyArea = new Rectangle(start, new(partialColorMap.GetLength(0), partialColorMap.GetLength(1)));
        var verts = BufferHelper.ReadVertexBuffer<PositionNormalMultiTextured>(Mesh);

        // Verts may no longer be in their original order (mesh optimized)
        for (int i = 0; i < verts.Length; i++)
        {
            // Determine original index of vertex
            var index = new Point(
                (int)(verts[i].Position.X / StretchH),
                (int)(-verts[i].Position.Z / StretchH));

            // Check if vertex is within the target area
            if (modifyArea.Contains(index))
                verts[i].Color = partialColorMap[index.X - start.X, index.Y - start.Y];
        }

        BufferHelper.WriteVertexBuffer(Mesh, verts);
    }
    #endregion

    #region Modify height
    /// <summary>
    /// Modifies the height of a part of the terrain.
    /// </summary>
    /// <param name="start">The top-left index of the area to modify.</param>
    /// <param name="partialHeightMap">A 2D array containing the new height values; array size specifies size of the area to modify.</param>
    /// <remarks>
    /// Cannot be called when <see cref="Lighting"/> is <c>true</c>, because normals are not updated.
    /// Invalidates all internal <see cref="BoundingBox"/>es.
    /// </remarks>
    public void ModifyHeight(Point start, byte[,] partialHeightMap)
    {
        #region Sanity checks
        if (Lighting) throw new InvalidOperationException(Resources.NoModifyTerrainHeightWithLighting);
        if (partialHeightMap == null) throw new ArgumentNullException(nameof(partialHeightMap));
        #endregion

        var modifyArea = new Rectangle(start, new(partialHeightMap.GetLength(0), partialHeightMap.GetLength(1)));
        var verts = BufferHelper.ReadVertexBuffer<PositionMultiTextured>(Mesh);

        // Verts may no longer be in their original order (mesh optimized)
        for (int i = 0; i < verts.Length; i++)
        {
            // Determine original index of vertex
            var index = new Point(
                (int)(verts[i].Position.X / StretchH),
                (int)(-verts[i].Position.Z / StretchH));

            // Check if vertex is within the target area
            if (modifyArea.Contains(index))
                verts[i].Position.Y = partialHeightMap[index.X - start.X, index.Y - start.Y] * StretchV;
        }

        BufferHelper.WriteVertexBuffer(Mesh, verts);

        // Invalidate old bounding boxes
        // ToDo: Update bounding boxes instead
        _subsetBoundingBoxes = null;
        _subsetWorldBoundingBoxes = null;
    }
    #endregion

    #region Render
    /// <inheritdoc/>
    internal override void Render(Camera camera, GetLights? getLights = null)
    {
        #region Sanity checks
        if (getLights == null) throw new ArgumentNullException(nameof(getLights));
        #endregion

        // Rendering this without a shader isn't possible (non-standard FVF)
        if (SurfaceEffect < SurfaceEffect.Shader) SurfaceEffect = SurfaceEffect.Shader;

        // Note: Doesn't call base methods
        PrepareRender();
        Engine.State.WorldTransform = WorldTransform;

        // Update bounding bodies
        if (WorldTransformDirty) RecalcWorldTransform();

        for (int i = 0; i < NumberSubsets; i++) RenderSubset(i, camera, getLights);
    }

    private void RenderSubset(int i, Camera camera, GetLights lights)
    {
        // Frustum culling with the bouding box
        if (_subsetWorldBoundingBoxes != null && !camera.InFrustum(_subsetWorldBoundingBoxes[i])) return;

        using (new ProfilerEvent(() => "Subset " + i))
        {
            Action renderSubset = () => Mesh.DrawSubset(i);

            if (SurfaceEffect >= SurfaceEffect.Glow)
            {
                // The terrain will always appear completely black on the glow/shadow map
                using (new ProfilerEvent(() => "Apply black " + _subsetShaders[i]))
                    _subsetShaders[i].Apply(renderSubset, XMaterial.DefaultMaterial, camera);
            }
            else
            {
                // Apply the normal terrain shader
                if (_subsetShaders[i] != null) SurfaceShader = _subsetShaders[i];
                XMaterial currentMaterial = i < Materials.Length ? Materials[i] : Materials[0];

                // Handle lights for fixed-function or shader rendering
                Vector3 boxCenter = (_subsetBoundingBoxes == null ? new() : _subsetBoundingBoxes[i].Minimum + (_subsetBoundingBoxes[i].Maximum - _subsetBoundingBoxes[i].Minimum) * 0.5f);

                var effectiveLights = (SurfaceEffect == SurfaceEffect.Plain)
                    ? []
                    : lights(Position + boxCenter, _blockSize * StretchH * (float)(Math.Sqrt(2) / 2));
                RenderHelper(renderSubset, currentMaterial, camera, effectiveLights);
            }
        }

        // Only allow the visualization of bounding bodies in normal view
        if (DrawBoundingBox && _subsetWorldBoundingBoxes != null && SurfaceEffect < SurfaceEffect.Glow)
            Engine.DrawBoundingBox(_subsetWorldBoundingBoxes[i]);
    }
    #endregion

    #region Picking
    /// <inheritdoc/>
    protected override bool IntersectsBounding(Ray ray)
    {
        // Since the terrain is usually very big, assume its bounding body is everywhere
        return true;
    }

    protected virtual Vector3 GetFacePosition(int faceIndex, float u, float v)
    {
        #region Sanity checks
        if ((faceIndex >= Mesh.FaceCount) || (faceIndex < 0)) throw new ArgumentOutOfRangeException(nameof(faceIndex));
        #endregion

        // Get the corner positions of the face
        Vector3 pos0 = _vertexBuffer[_indexBuffer[faceIndex * 3]];
        Vector3 pos1 = _vertexBuffer[_indexBuffer[faceIndex * 3 + 1]];
        Vector3 pos2 = _vertexBuffer[_indexBuffer[faceIndex * 3 + 2]];

        // Use position 0 as the origin and move towards position 1 (scaled by U) and position 2 (scaled by V)
        return (pos0 + (pos1 - pos0) * u + (pos2 - pos0) * v);
    }
    #endregion
}
