/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Text;
using Common;
using Common.Storage.SlimDX;
using Common.Utils;
using Common.Storage;
using OmegaEngine.Graphics;
using SlimDX;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics.Renderables;

namespace OmegaEngine.Assets
{
    /// <summary>
    /// A mesh loaded from an .X file.
    /// </summary>
    /// <seealso cref="Model"/>
    public class XMesh : Asset
    {
        #region Variables
        /// <summary>
        /// Materials (diffuse/normal/height/specular map, lighting settings) associated with the mesh
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly", Justification = "Only the array length is supposed to be read-only")]
        public readonly XMaterial[] Materials;
        #endregion

        #region Properties
        /// <summary>
        /// The mesh in DirectX format
        /// </summary>
        private readonly Mesh _mesh;

        public Mesh Mesh { get { return _mesh; } }

        /// <summary>
        /// A bounding sphere surrounding this mesh
        /// </summary>
        public BoundingSphere BoundingSphere { get; protected set; }

        /// <summary>
        /// A bounding box surrounding this mesh
        /// </summary>
        public BoundingBox BoundingBox { get; protected set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Loads a static mesh from an .X file.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> providing rendering capabilities.</param>
        /// <param name="stream">The .X file to load the mesh from.</param>
        /// <param name="meshName">The name of the mesh. This is used for finding associated textures.</param>
        /// <exception cref="InvalidDataException">Thrown if <paramref name="stream"/> does not contain a valid mesh.</exception>
        /// <remarks>This should only be called by <see cref="Get"/> to prevent unnecessary duplicates.</remarks>
        protected XMesh(Engine engine, Stream stream, string meshName)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            // Load mesh and materials

            try
            {
                _mesh = Mesh.FromStream(engine.Device, stream, MeshFlags.Managed);
            }
                #region Sanity checks
            catch (Direct3D9Exception ex)
            {
                throw new InvalidDataException(ex.Message, ex);
            }
            #endregion

            ExtendedMaterial[] extendedMaterials = Mesh.GetMaterials();
            EffectInstance[] effectInstances = Mesh.GetEffects();

            // Calculate bounding bodies before manipulating the mesh
            BoundingSphere = BufferHelper.ComputeBoundingSphere(Mesh);
            BoundingBox = BufferHelper.ComputeBoundingBox(Mesh);

            try
            {
                #region Extract material data
                bool needsTangents = false;
                if ((extendedMaterials != null) && (extendedMaterials.Length > 0))
                {
                    Materials = new XMaterial[extendedMaterials.Length];

                    // Store each material and texture
                    for (int i = 0; i < extendedMaterials.Length; i++)
                    {
                        Materials[i] = XMaterial.DefaultMaterial;

                        // Apply the mesh's material information
                        Material meshMaterial = extendedMaterials[i].MaterialD3D;
                        Materials[i].Diffuse = meshMaterial.Diffuse.ToColor();
                        Materials[i].Specular = meshMaterial.Specular.ToColor();
                        Materials[i].SpecularPower = meshMaterial.Power;
                        Materials[i].Emissive = meshMaterial.Emissive.ToColor();

                        // Search for texture file names in material
                        string textureFilename = extendedMaterials[i].TextureFileName;
                        if (!string.IsNullOrEmpty(textureFilename))
                        {
                            Materials[i].DiffuseMaps[0] = ShaderLoadHelper(engine, meshName, textureFilename);

                            #region Auto-detect extra texture maps
                            string baseFilename = Path.Combine(Path.GetDirectoryName(meshName) ?? "", Path.GetFileNameWithoutExtension(textureFilename));
                            string fileExt = Path.GetExtension(textureFilename);

                            // Normal map
                            string normalFilename = baseFilename + "_normal" + fileExt;
                            if (ContentManager.FileExists("Meshes", normalFilename))
                            {
                                Materials[i].NormalMap = XTexture.Get(engine, normalFilename, meshTexture: true);
                                needsTangents = true;
                            }

                            // Height map
                            string heightFilename = baseFilename + "_height" + fileExt;
                            if (ContentManager.FileExists("Meshes", heightFilename))
                            {
                                Materials[i].HeightMap = XTexture.Get(engine, heightFilename, meshTexture: true);
                                needsTangents = true;
                            }

                            // Specular map
                            string specularFilename = baseFilename + "_specular" + fileExt;
                            if (ContentManager.FileExists("Meshes", specularFilename))
                                Materials[i].SpecularMap = XTexture.Get(engine, specularFilename, meshTexture: true);

                            // Glow map (internally represented as emissive map)
                            string glowFilename = baseFilename + "_glow" + fileExt;
                            if (ContentManager.FileExists("Meshes", glowFilename))
                            {
                                Materials[i].EmissiveMap = XTexture.Get(engine, glowFilename, meshTexture: true);

                                // Automatically set emissive color if a glow map is used (since this color usually defaults to black)
                                if (Materials[i].Emissive == Color.FromArgb(255, 0, 0, 0)) Materials[i].Emissive = Color.White;
                            }
                            #endregion
                        }

                        #region Load extra texture maps from shader configuration
                        // Search for texture file names in shader effect if present
                        if ((effectInstances != null) && (i < effectInstances.Length))
                        {
                            EffectDefault[] parameters = effectInstances[i].Defaults;
                            foreach (EffectDefault param in parameters)
                            {
                                XTexture extraTexture = ShaderTextureHelper(engine, param, meshName, "diffuseTexture");
                                if (extraTexture != null) Materials[i].DiffuseMaps[0] = extraTexture;

                                extraTexture = ShaderTextureHelper(engine, param, meshName, "normalTexture");
                                if (extraTexture != null)
                                {
                                    Materials[i].NormalMap = extraTexture;
                                    needsTangents = true;
                                }

                                extraTexture = ShaderTextureHelper(engine, param, meshName, "heightTexture");
                                if (extraTexture != null)
                                {
                                    Materials[i].HeightMap = extraTexture;
                                    needsTangents = true;
                                }

                                extraTexture = ShaderTextureHelper(engine, param, meshName, "specularTexture");
                                if (extraTexture != null) Materials[i].SpecularMap = extraTexture;
                            }
                        }
                        #endregion
                    }

                    // Generate normals (plus tagents if normal/height maps are available)
                    if (needsTangents && engine.Capabilities.PerPixelEffects)
                        MeshHelper.GenerateNormalsAndTangents(engine.Device, ref _mesh, true);
                    else
                        MeshHelper.GenerateNormals(engine.Device, ref _mesh);
                }
                #endregion
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
        #endregion

        #region Static access
        /// <summary>
        /// Returns a cached <see cref="XMesh"/> or creates a new one if the requested ID is not cached.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> providing the cache and rendering capabilities.</param>
        /// <param name="id">The ID of the asset to be returned.</param>
        /// <returns>The requested asset; <see langword="null"/> if <paramref name="id"/> was empty.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the specified file could not be found.</exception>
        /// <exception cref="IOException">Thrown if there was an error reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the file does not contain a valid mesh.</exception>
        /// <remarks>Remember to call <see cref="CacheManager.Clean"/> when done, otherwise this object will never be released.</remarks>
        public static XMesh Get(Engine engine, string id)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (string.IsNullOrEmpty(id)) return null;
            #endregion

            // Try to find existing asset in cache
            const string type = "Meshes";
            id = FileUtils.UnifySlashes(id);
            string fullID = Path.Combine(type, id);
            var data = engine.Cache.GetAsset<XMesh>(fullID);

            // Load from file if not in cache
            if (data == null)
            {
                using (new TimedLogEvent("Loading mesh: " + id))
                using (var stream = ContentManager.GetFileStream(type, id))
                    data = new XMesh(engine, stream, id) {Name = fullID};
                engine.Cache.AddAsset(data);
            }

            return data;
        }
        #endregion

        //--------------------//

        #region Texture helpers
        /// <summary>
        /// Finds and loads a mesh texture
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the texture from</param>
        /// <param name="meshName">The name of the mesh to load a texture file for</param>
        /// <param name="textureID">The texture ID</param>
        /// <returns>The requested mesh texture</returns>
        private static XTexture ShaderLoadHelper(Engine engine, string meshName, string textureID)
        {
            // Determine the path the mesh was originally loaded from
            string meshPath = Path.GetDirectoryName(meshName);
            if (!string.IsNullOrEmpty(meshPath)) meshPath += Path.DirectorySeparatorChar;

            // Try to find the texture in the directory of its mesh first, then look in the generic mesh textures directory
            string id = Path.Combine("Meshes", textureID);
            return ContentManager.FileExists("Meshes", meshPath + textureID)
                ? XTexture.Get(engine, meshPath + textureID, meshTexture: true)
                : XTexture.Get(engine, id);
        }

        /// <summary>
        /// Extracts a texture reference from shader parameters
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the texture from</param>
        /// <param name="param">The shader parameter</param>
        /// <param name="meshName">The name of the mesh to load a texture file for</param>
        /// <param name="textureType">The type of texture to check the shader parameter for</param>
        /// <returns>The texture specified by the shader parameter or null if no texture was specified</returns>
        private static XTexture ShaderTextureHelper(Engine engine, EffectDefault param, string meshName, string textureType)
        {
            // Check if the parameter has the right name and contains a string
            if (StringUtils.EqualsIgnoreCase(param.ParameterName, textureType) && param.Type == EffectDefaultType.String)
            {
                // Read the string and trim away tailing nul-bytes and spaces
                string paramData = new StreamReader(param.Value, Encoding.ASCII).ReadToEnd().Trim('\0', ' ');

                // Attempt to load the file specified in the parameter
                return ShaderLoadHelper(engine, meshName, paramData);
            }
            return null;
        }
        #endregion

        #region Reference control
        /// <inheritdoc/>
        public override void HoldReference()
        {
            base.HoldReference();

            foreach (var material in Materials)
                material.HoldReference();
        }

        /// <inheritdoc/>
        public override void ReleaseReference()
        {
            base.ReleaseReference();

            foreach (var material in Materials)
                material.ReleaseReference();
        }
        #endregion

        //--------------------//

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (Disposed || _mesh == null) return; // Don't try to dispose more than once

            try
            {
                if (disposing)
                { // This block will only be executed on manual disposal, not by Garbage Collection
                    Log.Info("Disposing " + this);
                    Mesh.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion
    }
}
