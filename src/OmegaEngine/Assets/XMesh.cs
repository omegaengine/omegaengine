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
using NanoByte.Common.Storage;
using OmegaEngine.Graphics;
using Vortice.Mathematics;
using OmegaEngine.Graphics.Meshes;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Foundation.Storage;

namespace OmegaEngine.Assets;

/// <summary>
/// A mesh loaded from a model file (.X, .OBJ, .FBX, .glTF, ...).
/// </summary>
/// <seealso cref="Model"/>
public class XMesh : Asset
{
    /// <summary>
    /// Materials (diffuse/normal/height/specular map, lighting settings) associated with the mesh
    /// </summary>
    public ImmutableArray<XMaterial> Materials { get; }

    private readonly Mesh _mesh;

    /// <summary>
    /// The mesh, ready for rendering and intersection tests.
    /// </summary>
    public Mesh Mesh => _mesh;

    /// <summary>
    /// A bounding sphere surrounding this mesh
    /// </summary>
    public BoundingSphere? BoundingSphere { get; }

    /// <summary>
    /// A bounding box surrounding this mesh
    /// </summary>
    public BoundingBox? BoundingBox { get; }

    /// <summary>
    /// Per-subset bounding boxes in entity space (null for single-subset meshes)
    /// </summary>
    public BoundingBox[]? SubsetBoundingBoxes { get; }

    /// <summary>
    /// Per-subset bounding spheres in entity space (null for single-subset meshes)
    /// </summary>
    public BoundingSphere[]? SubsetBoundingSpheres { get; }

    /// <summary>
    /// Loads a static mesh from a model file.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> providing rendering capabilities.</param>
    /// <param name="id">The ID of the model file in the <c>Meshes</c> content directory. This is also used for finding associated textures.</param>
    /// <exception cref="InvalidDataException">The file does not contain a valid mesh.</exception>
    /// <remarks>This should only be called by <see cref="Get"/> to prevent unnecessary duplicates.</remarks>
    protected XMesh(Engine engine, string id)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        if (id == null) throw new ArgumentNullException(nameof(id));
        #endregion

        ImportedMaterial[] importedMaterials;
        try
        {
            (_mesh, importedMaterials) = MeshImporter.LoadStatic(engine.Device, id);
        }
        catch
        {
            GC.SuppressFinalize(this); // Prevent "Forgot to call Dispose" error
            throw;
        }

        if (importedMaterials.Length > 1)
        {
            (SubsetBoundingSpheres, SubsetBoundingBoxes) = CalculateSubsetBoundingBodies(importedMaterials.Length);

            BoundingSphere = SubsetBoundingSpheres?.Aggregate((a, b) => Vortice.Mathematics.BoundingSphere.CreateMerged(a, b));
            BoundingBox = SubsetBoundingBoxes?.Aggregate((a, b) => Vortice.Mathematics.BoundingBox.CreateMerged(a, b));
        }
        else
        {
            var points = Mesh.GetPoints();
            BoundingSphere = Vortice.Mathematics.BoundingSphere.CreateFromPoints(points);
            BoundingBox = Vortice.Mathematics.BoundingBox.CreateFromPoints(points);

            // Heuristic for discarding invalid bounding bodies
            if (BoundingSphere.Value.Radius < 0.01) BoundingSphere = null;
            if (BoundingBox.Value.Diagonal().Length() < 0.01) BoundingBox = null;
        }

        try
        {
            Materials = LoadMaterials(engine, id, importedMaterials, out bool needsTangents);

            // Generate normals (plus tangents if normal/height maps are available)
            if (!Materials.IsDefault)
            {
                // Keep the textures alive for as long as this mesh is cached, even if no Model currently uses them
                foreach (var material in Materials)
                    material.HoldReference();

                if (needsTangents)
                    TexturedMeshUtils.GenerateTBN(ref _mesh, weldVertexes: true);
                else
                    TexturedMeshUtils.GenerateNormals(ref _mesh);
            }
        }
        #region Error handling
        catch (Exception)
        {
            // Since private objects have already been created at this point, a proper cleanup is needed
            Dispose();
            throw;
        }
        #endregion
    }

    /// <summary>
    /// Builds <see cref="XMaterial"/>s from a mesh's materials, loading associated textures.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to load textures with.</param>
    /// <param name="meshName">The name of the mesh. This is used for finding associated textures.</param>
    /// <param name="importedMaterials">The materials loaded from the model file.</param>
    /// <param name="needsTangents">Set to <c>true</c> if any loaded material references a normal or height map and therefore requires tangent data.</param>
    /// <returns>The loaded materials; <c>default</c> if <paramref name="importedMaterials"/> is empty.</returns>
    internal static ImmutableArray<XMaterial> LoadMaterials(Engine engine, string meshName, IReadOnlyList<ImportedMaterial> importedMaterials, out bool needsTangents)
    {
        needsTangents = false;
        if (importedMaterials is not { Count: > 0 }) return default;

        var builder = ImmutableArray.CreateBuilder<XMaterial>(importedMaterials.Count);

        // Store each material and texture
        foreach (var importedMaterial in importedMaterials)
        {
            var material = importedMaterial.Material.ToXMaterial();

            // Search for texture file names in material
            string? textureFilename = importedMaterial.TextureFileName;
            if (!string.IsNullOrEmpty(textureFilename))
            {
                material.DiffuseMap = TextureLoadHelper(engine, meshName, textureFilename);

                string baseFilename = Path.Combine(Path.GetDirectoryName(meshName) ?? "", Path.GetFileNameWithoutExtension(textureFilename));
                string fileExt = Path.GetExtension(textureFilename);

                // Normal map
                string normalFilename = $"{baseFilename}_normal{fileExt}";
                if (ContentManager.FileExists("Meshes", normalFilename))
                {
                    material.NormalMap = XTexture.Get(engine, normalFilename, meshTexture: true, srgb: false);
                    needsTangents = true;
                }

                // Height map
                string heightFilename = $"{baseFilename}_height{fileExt}";
                if (ContentManager.FileExists("Meshes", heightFilename))
                {
                    material.HeightMap = XTexture.Get(engine, heightFilename, meshTexture: true, srgb: false);
                    needsTangents = true;
                }

                // Specular map
                string specularFilename = $"{baseFilename}_specular{fileExt}";
                if (ContentManager.FileExists("Meshes", specularFilename))
                    material.SpecularMap = XTexture.Get(engine, specularFilename, meshTexture: true, srgb: false);

                // Emissive map
                string emissiveFilename = $"{baseFilename}_emissive{fileExt}";
                if (ContentManager.FileExists("Meshes", emissiveFilename))
                    material.EmissiveMap = XTexture.Get(engine, emissiveFilename, meshTexture: true);

                // Glow map, doubling as the emissive map if there is no dedicated one
                string glowFilename = $"{baseFilename}_glow{fileExt}";
                if (ContentManager.FileExists("Meshes", glowFilename))
                {
                    material.GlowMap = XTexture.Get(engine, glowFilename, meshTexture: true);
                    material.EmissiveMap ??= material.GlowMap;
                }
            }

            builder.Add(material);
        }

        return builder.MoveToImmutable();
    }

    /// <summary>
    /// Returns a cached <see cref="XMesh"/> or creates a new one if the requested ID is not cached.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> providing the cache and rendering capabilities.</param>
    /// <param name="id">The ID of the asset to be returned.</param>
    /// <returns>The requested asset; <c>null</c> if <paramref name="id"/> was empty.</returns>
    /// <exception cref="FileNotFoundException">The specified file could not be found.</exception>
    /// <exception cref="IOException">There was an error reading the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
    /// <exception cref="InvalidDataException">The file does not contain a valid mesh.</exception>
    /// <remarks>Remember to call <see cref="CacheManager.Clean"/> when done, otherwise this object will never be released.</remarks>
    [return: NotNullIfNotNull(nameof(id))]
    public static XMesh? Get(Engine engine, string? id)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        if (string.IsNullOrEmpty(id)) return null;
        #endregion

        // Try to find existing asset in cache
        const string type = "Meshes";
        id = id.ToNativePath();
        string fullID = Path.Combine(type, id);
        var data = engine.Cache.GetAsset<XMesh>(fullID);

        // Load from file if not in cache
        if (data == null)
        {
            if (!ContentManager.FileExists(type, id)) throw new FileNotFoundException($"Mesh file not found: {id}", id);

            using (new TimedLogEvent($"Loading mesh: {id}"))
                data = new(engine, id) {Name = fullID};
            engine.Cache.AddAsset(data);
        }

        return data;
    }

    /// <summary>
    /// Finds and loads a mesh texture
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to load the texture from</param>
    /// <param name="meshName">The name of the mesh to load a texture file for</param>
    /// <param name="textureID">The texture ID</param>
    /// <param name="srgb">Does the file hold sRGB-encoded color data (rather than data such as a normal, height or specular map)?</param>
    /// <returns>The requested mesh texture</returns>
    private static XTexture? TextureLoadHelper(Engine engine, string meshName, string textureID, bool srgb = true)
    {
        // Determine the path the mesh was originally loaded from
        string meshPath = Path.GetDirectoryName(meshName);
        if (!string.IsNullOrEmpty(meshPath)) meshPath += Path.DirectorySeparatorChar;

        // Try to find the texture in the directory of its mesh first, then look in the generic mesh textures directory
        string id = Path.Combine("Meshes", textureID);
        return ContentManager.FileExists("Meshes", meshPath + textureID)
            ? XTexture.Get(engine, meshPath + textureID, meshTexture: true, srgb: srgb)
            : XTexture.Get(engine, id, srgb: srgb);
    }

    /// <summary>
    /// Calculates per-subset bounding boxes and spheres for multi-subset meshes.
    /// </summary>
    private (BoundingSphere[]?, BoundingBox[]?) CalculateSubsetBoundingBodies(int subsetCount)
    {
        using (new TimedLogEvent("Calculate per-subset bounding bodies"))
        {
            var subsetBoundingSpheres = new BoundingSphere[subsetCount];
            var subsetBoundingBoxes = new BoundingBox[subsetCount];
            for (int i = 0; i < subsetCount; i++)
            {
                var positions = Mesh.GetPoints(subset: i);
                if (positions.Length == 0) return (null, null);

                subsetBoundingSpheres[i] = Vortice.Mathematics.BoundingSphere.CreateFromPoints(positions);
                if (subsetBoundingSpheres[i].Radius < 0.01) return (null, null);

                subsetBoundingBoxes[i] = Vortice.Mathematics.BoundingBox.CreateFromPoints(positions);
                if (subsetBoundingBoxes[i].Diagonal().Length() < 0.01) return (null, null);
            }

            // Only use subset bounding bodies if they are all valid
            return (subsetBoundingSpheres, subsetBoundingBoxes);
        }
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
            { // This block will only be executed on manual disposal, not by Garbage Collection
                Log.Info($"Disposing {this}");
                _mesh.Dispose();

                if (!Materials.IsDefaultOrEmpty)
                {
                    foreach (var material in Materials)
                        material.ReleaseReference();
                }
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }
}
