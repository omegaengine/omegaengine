/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
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
    private TerrainShader[]? _subsetShaders;

    private readonly int[] _indexBuffer;
    private readonly Vector3[] _vertexBuffer;

    /// <summary>The indexes of the faces belonging to each subset (block), used to narrow down intersection tests.</summary>
    private readonly int[][] _subsetFaces;
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
    public float StretchH { get; private init; }

    /// <summary>
    /// A factor by which the terrain is vertically stretched
    /// </summary>
    [Description(" A factor by which the terrain is vertically stretched"), Category("Layout")]
    public float StretchV { get; private init; }
    #endregion

    #endregion

    #region Constructor
    /// <summary>
    /// Internal helper constructor
    /// </summary>
    /// <param name="sourceMesh">The finished terrain mesh in <see cref="Pool.SystemMemory"/>. Disposed once its content has been copied and published.</param>
    /// <param name="material">The material to use for rendering the terrain</param>
    /// <param name="lighting">Use/support lighting when rendering this terrain?</param>
    /// <remarks>
    /// Unlike other <see cref="Model"/>s the terrain keeps no system memory copy of its mesh; it is by far the largest mesh in a scene.
    /// It is published without <see cref="MeshFlags.WriteOnly"/> instead, so <see cref="ModifyColor"/> and <see cref="ModifyHeight"/> keep working.
    /// </remarks>
    protected Terrain(Mesh sourceMesh, XMaterial material, bool lighting)
        : base((sourceMesh ?? throw new ArgumentNullException(nameof(sourceMesh))).ToDefaultPool(writeOnly: false), pickingMesh: null, [material])
    {
        Lighting = lighting;

        #region Copy index and vertex buffer content
        // Copy buffers to RAM for fast position lookups and intersection tests
        using (new TimedLogEvent("Copy index and vertex buffer content"))
        {
            _indexBuffer = sourceMesh.ReadIndexBuffer();

            // Get the vertex positions from the VertexBuffer
            if (lighting) // Different vertex formats
            {
                var verts = sourceMesh.ReadVertexBuffer<PositionNormalMultiTextured>();
                _vertexBuffer = new Vector3[verts.Length];
                for (int i = 0; i < verts.Length; i++)
                    _vertexBuffer[i] = verts[i].Position;
            }
            else
            {
                var verts = sourceMesh.ReadVertexBuffer<PositionMultiTextured>();
                _vertexBuffer = new Vector3[verts.Length];
                for (int i = 0; i < verts.Length; i++)
                    _vertexBuffer[i] = verts[i].Position;
            }

            _subsetFaces = GroupFacesBySubset(sourceMesh.ReadAttributeBuffer());
        }
        #endregion

        sourceMesh.Dispose();
    }

    /// <summary>
    /// Groups face indexes by the subset (block) they belong to, so <see cref="Intersects(Ray,out float)"/> can skip whole blocks.
    /// </summary>
    /// <param name="attributes">The subset each face belongs to, indexed by face.</param>
    private static int[][] GroupFacesBySubset(int[] attributes)
    {
        int subsetCount = 0;
        foreach (int subset in attributes)
            if (subset >= subsetCount) subsetCount = subset + 1;

        var faceCounts = new int[subsetCount];
        foreach (int subset in attributes) faceCounts[subset]++;

        var subsetFaces = new int[subsetCount][];
        for (int subset = 0; subset < subsetCount; subset++)
            subsetFaces[subset] = new int[faceCounts[subset]];

        var nextIndexes = new int[subsetCount];
        for (int face = 0; face < attributes.Length; face++)
        {
            int subset = attributes[face];
            subsetFaces[subset][nextIndexes[subset]++] = face;
        }

        return subsetFaces;
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
    public static Terrain Create(Engine engine, Size size, float stretchH, float stretchV, ByteGrid heightMap, NibbleGrid textureMap, string?[] textures, ByteVector4Grid? occlusionIntervalMap, bool lighting, int blockSize)
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
            Engine = engine,
            Size = size, StretchH = stretchH, StretchV = stretchV,
            SubsetBoundingBoxes = subsetBoundingBoxes,
            SubsetBoundingSpheres = subsetBoundingBoxes.Select(SlimDX.BoundingSphere.FromBox).ToArray(),
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
        var verts = Mesh.ReadVertexBuffer<PositionNormalMultiTextured>();

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

        Mesh.WriteVertexBuffer(verts);
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
        var verts = Mesh.ReadVertexBuffer<PositionMultiTextured>();

        // Verts may no longer be in their original order (mesh optimized)
        for (int i = 0; i < verts.Length; i++)
        {
            // Determine original index of vertex
            var index = new Point(
                (int)(verts[i].Position.X / StretchH),
                (int)(-verts[i].Position.Z / StretchH));

            // Check if vertex is within the target area
            if (modifyArea.Contains(index))
            {
                verts[i].Position.Y = partialHeightMap[index.X - start.X, index.Y - start.Y] * StretchV;

                // Keep the RAM copy used for picking in sync
                _vertexBuffer[i] = verts[i].Position;
            }
        }

        Mesh.WriteVertexBuffer(verts);

        // Invalidate old bounding bodies
        SubsetBoundingBoxes = null;
        SubsetBoundingSpheres = null;
    }
    #endregion

    #region Render
    protected override void RenderSubset(int i, Camera camera, GetEffectiveLights? getEffectiveLights)
    {
        // Rendering this without a shader isn't possible (non-standard FVF)
        if (SurfaceEffect < SurfaceEffect.Shader) SurfaceEffect = SurfaceEffect.Shader;
        if (_subsetShaders == null) return;
        var shader = _subsetShaders[i];

        using (new ProfilerEvent(() => $"Subset {i}"))
        {
            Action renderSubset = () => Mesh.DrawSubset(i);

            switch (SurfaceEffect)
            {
                case SurfaceEffect.Glow:
                    // The terrain will always appear completely black on the glow map
                    using (new ProfilerEvent(() => $"Apply black {shader}"))
                        shader.Apply(renderSubset, XMaterial.Default, camera);
                    break;

                case SurfaceEffect.Depth:
                    using (new ProfilerEvent(() => $"Apply depth {shader}"))
                    {
                        shader.RenderDepthOnly = true;
                        try
                        {
                            shader.Apply(renderSubset, XMaterial.Default, camera);
                        }
                        finally
                        {
                            shader.RenderDepthOnly = false;
                        }
                    }
                    break;

                default:
                    // Apply the regular terrain shader
                    if (_subsetShaders?[i] != null) SurfaceShader = shader;
                    XMaterial currentMaterial = i < Materials.Length ? Materials[i] : Materials[0];

                    var effectiveLights = getEffectiveLights == null
                        ? []
                        : getEffectiveLights(SubsetWorldBoundingSpheres?[i] ?? GetWorldBoundingSphereOrPosition(), shadowing: false);

                    RenderHelper(renderSubset, currentMaterial, camera, effectiveLights);
                    break;
            }
        }
    }
    #endregion

    #region Picking
    /// <inheritdoc/>
    protected override bool IntersectsBounding(Ray ray)
    {
        // Since the terrain is usually very big, assume its bounding body is everywhere
        return true;
    }

    /// <inheritdoc/>
    public override bool Intersects(Ray ray, out float distance)
    {
        // Transform the world space picking ray into entity space
        ray = new(
            Vector3.TransformCoordinate(ray.Position, InverseWorldTransform),
            // Do not normalize so that ray length remains the same
            Vector3.TransformNormal(ray.Direction, InverseWorldTransform));

        distance = float.PositiveInfinity;

        if (SubsetBoundingBoxes is not {} boundingBoxes)
        { // ModifyHeight() invalidated the per-block bounding boxes, so every face has to be tested
            for (int face = 0; face < _indexBuffer.Length / 3; face++)
                IntersectsFace(ray, face, ref distance);
            return !float.IsPositiveInfinity(distance);
        }

        #region Collect the blocks the ray passes through, nearest first
        var blocks = new List<(float Entry, int Subset)>();
        for (int subset = 0; subset < Math.Min(boundingBoxes.Length, _subsetFaces.Length); subset++)
        {
            if (SlimDX.BoundingBox.Intersects(boundingBoxes[subset], ray, out float entry))
                blocks.Add((entry, subset));
        }
        blocks.Sort((first, second) => first.Entry.CompareTo(second.Entry));
        #endregion

        foreach ((float entry, int subset) in blocks)
        {
            // Blocks are ordered by distance, so no later block can hold anything closer than what was already found
            if (entry > distance) break;

            foreach (int face in _subsetFaces[subset])
                IntersectsFace(ray, face, ref distance);
        }

        return !float.IsPositiveInfinity(distance);
    }

    /// <summary>
    /// Intersects a single face, keeping <paramref name="distance"/> at the closest hit found so far.
    /// </summary>
    private void IntersectsFace(Ray ray, int faceIndex, ref float distance)
    {
        if (Ray.Intersects(ray,
                _vertexBuffer[_indexBuffer[faceIndex * 3]],
                _vertexBuffer[_indexBuffer[faceIndex * 3 + 1]],
                _vertexBuffer[_indexBuffer[faceIndex * 3 + 2]],
                out float faceDistance)
         && faceDistance < distance) distance = faceDistance;
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
