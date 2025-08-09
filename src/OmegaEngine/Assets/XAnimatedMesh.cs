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
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Foundation.Storage;

namespace OmegaEngine.Assets;

/// <summary>
/// An animated mesh loaded from an .X file.
/// </summary>
/// <seealso cref="AnimatedModel"/>
public class XAnimatedMesh : XMesh
{
    #region Properties
    ///// <summary>
    ///// The animation hierarchy of the mesh
    ///// </summary>
    //public AnimationFrame MeshFrame { get; private set; }
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
    protected XAnimatedMesh(Engine engine, Stream stream, string meshName) : base(engine, stream, meshName)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        #endregion

        try
        {
            // Load frame hierarchy
            // ToDo: Parse Mesh file
            //MeshFrame = Mesh.LoadHierarchy(stream, MeshFlags.Managed, engine.Device, new AnimationAllocation(), null);

            // Replace bounding bodies from static mesh with data considering the animation hierarchy
            // ToDo: Calculate bounding sphere
            //BoundingSphere = BufferHelper.ComputeBoundingSphere(MeshFrame.FrameHierarchy);
            //BoundingBox = new BoundingBox(); // No bounding box available for animation hierarchies
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
    public new static XAnimatedMesh Get(Engine engine, string id)
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
            using (new TimedLogEvent("Loading animated mesh: " + id))
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
                Log.Info("Disposing " + this);
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }
    #endregion
}
