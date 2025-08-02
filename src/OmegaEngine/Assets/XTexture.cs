/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using NanoByte.Common;
using NanoByte.Common.Storage;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics;
using OmegaEngine.Storage;

namespace OmegaEngine.Assets
{
    /// <summary>
    /// A texture loaded from one of DirectX's natively supported image formats (PNG, JPG, DDS, ...).
    /// </summary>
    public class XTexture : Asset, ITextureProvider
    {
        #region Properties
        /// <summary>
        /// The <see cref="SlimDX.Direct3D9.Texture"/> inside this asset. 
        /// </summary>
        public Texture Texture { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Loads a texture from an image file.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> providing rendering capabilities.</param>
        /// <param name="stream">The image file to load the texture from.</param>
        /// <exception cref="InvalidDataException"><paramref name="stream"/> does not contain a texture file.</exception>
        /// <remarks>This should only be called by <see cref="Get"/> to prevent unnecessary duplicates.</remarks>
        protected XTexture(Engine engine, Stream stream)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            #endregion

            try
            {
                Texture = Texture.FromStream(engine.Device, stream,
                    D3DX.Default, D3DX.Default, 0, Usage.None, Format.Unknown, Pool.Managed,
                    Filter.Default, Filter.Default, 0);
            }
                #region Sanity checks
            catch (Direct3D9Exception ex)
            {
                throw new InvalidDataException(ex.Message, ex);
            }
            #endregion
        }
        #endregion

        #region Static access
        /// <summary>
        /// Returns a cached <see cref="XTexture"/> or creates a new one if the requested ID is not cached.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> providing the cache and rendering capabilities.</param>
        /// <param name="id">The ID of the asset to be returned.</param>
        /// <param name="meshTexture">Shall the texture be loaded from the meshes directory instead of the textures directory?</param>
        /// <returns>The requested asset; <c>null</c> if <paramref name="id"/> was empty.</returns>
        /// <exception cref="FileNotFoundException">The specified file could not be found.</exception>
        /// <exception cref="IOException">There was an error reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">The file does not contain a valid texture.</exception>
        /// <remarks>Remember to call <see cref="CacheManager.Clean"/> when done, otherwise this object will never be released.</remarks>
        public static XTexture Get(Engine engine, string id, bool meshTexture = false)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            if (string.IsNullOrEmpty(id)) return null;
            #endregion

            // Try to find existing asset in cache
            string type = meshTexture ? "Meshes" : "Textures";
            id = FileUtils.UnifySlashes(id);
            string fullID = Path.Combine(type, id);
            var data = engine.Cache.GetAsset<XTexture>(fullID);

            // Load from file if not in cache
            if (data == null)
            {
                using (new TimedLogEvent((meshTexture ? "Loading mesh texture: " : "Loading texture: ") + id))
                using (var stream = ContentManager.GetFileStream(type, id))
                    data = new(engine, stream) {Name = fullID};
                engine.Cache.AddAsset(data);
            }

            return data;
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Convert a <see cref="XTexture"/> into its contained <see cref="SlimDX.Direct3D9.Texture"/>.
        /// </summary>
        public static implicit operator Texture(XTexture xTexture)
        {
            return xTexture?.Texture;
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
                    Log.Info("Disposing " + this);
                    Texture?.Dispose();
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
