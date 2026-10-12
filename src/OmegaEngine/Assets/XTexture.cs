/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using NanoByte.Common.Storage;
using Vortice.Direct3D9;
using SharpGen.Runtime;
using OmegaEngine.Graphics;
using OmegaEngine.Foundation.Storage;

namespace OmegaEngine.Assets;

/// <summary>
/// A texture loaded from an image file (PNG, JPG, DDS, ...).
/// </summary>
public class XTexture : Asset, ITextureProvider
{
    #region Properties
    /// <summary>
    /// The <see cref="IDirect3DTexture9"/> inside this asset.
    /// </summary>
    public IDirect3DTexture9 Texture { get; }
    #endregion

    #region Constructor
    /// <summary>
    /// Loads a texture from an image file.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> providing rendering capabilities.</param>
    /// <param name="stream">The image file to load the texture from.</param>
    /// <param name="srgb">Does the file hold sRGB-encoded color data (rather than data such as a normal, height or specular map)?</param>
    /// <exception cref="InvalidDataException"><paramref name="stream"/> does not contain a texture file.</exception>
    /// <remarks>This should only be called by <see cref="Get"/> to prevent unnecessary duplicates.</remarks>
    protected XTexture(Engine engine, Stream stream, bool srgb = true)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        #endregion

        try
        {
            try
            {
                // Color textures must be filtered in linear space when generating mip-maps, otherwise gamma-encoded values get averaged
                Texture = TextureLoader.FromStream(engine.Device, stream, mipmaps: true, srgb);
            }
            catch (SharpGenException ex) when (stream.CanSeek)
            {
                Log.Warn("Loading texture with mip-maps failed; retrying without mip-map generation", ex);
                stream.Position = 0;
                Texture = TextureLoader.FromStream(engine.Device, stream, mipmaps: false);
            }
        }
        #region Error handling
        catch (Exception ex) when (ex is SharpGenException or InvalidDataException)
        {
            GC.SuppressFinalize(this); // Prevent "Forgot to call Dispose" error
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
    /// <param name="srgb">Does the file hold sRGB-encoded color data (rather than data such as a normal, height or specular map)?</param>
    /// <returns>The requested asset; <c>null</c> if <paramref name="id"/> was empty.</returns>
    /// <exception cref="FileNotFoundException">The specified file could not be found.</exception>
    /// <exception cref="IOException">There was an error reading the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
    /// <exception cref="InvalidDataException">The file does not contain a valid texture.</exception>
    /// <remarks>Remember to call <see cref="CacheManager.Clean"/> when done, otherwise this object will never be released.</remarks>
    [return: NotNullIfNotNull(nameof(id))]
    public static XTexture? Get(Engine engine, string? id, bool meshTexture = false, bool srgb = true)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        #endregion

        if (string.IsNullOrEmpty(id)) return null;

        // Try to find existing asset in cache
        string type = meshTexture ? "Meshes" : "Textures";
        id = id.ToNativePath();
        string fullID = Path.Combine(type, id);
        var data = engine.Cache.GetAsset<XTexture>(fullID);

        // Load from file if not in cache
        if (data == null)
        {
            using (new TimedLogEvent((meshTexture ? "Loading mesh texture: " : "Loading texture: ") + id))
            using (var stream = ContentManager.GetFileStream(type, id))
                data = new(engine, stream, srgb) {Name = fullID};
            engine.Cache.AddAsset(data);
        }

        return data;
    }
    #endregion

    #region Conversion
    /// <summary>
    /// Convert a <see cref="XTexture"/> into its contained <see cref="IDirect3DTexture9"/>.
    /// </summary>
    public static implicit operator IDirect3DTexture9?(XTexture? xTexture) => xTexture?.Texture;
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
