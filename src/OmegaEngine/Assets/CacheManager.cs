/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using NanoByte.Common;
using NanoByte.Common.Collections;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Assets;

/// <summary>
/// Keeps a cache of <see cref="Asset"/>s that have been loaded and provides type-safe access to them.
/// </summary>
/// <seealso cref="Engine.Cache"/>
public sealed class CacheManager : IDisposable
{
    #region Variables
    private readonly NamedCollection<Asset> _assetCache = [];
    #endregion

    #region Constructor
    internal CacheManager()
    {}
    #endregion

    //--------------------//

    #region Add asset
    /// <summary>
    /// Adds an <see cref="Asset"/> to the cache.
    /// </summary>
    /// <param name="asset">The <see cref="Asset"/> to add.</param>
    internal void AddAsset(Asset asset)
    {
        _assetCache.Add(asset);
    }
    #endregion

    #region Get asset
    /// <summary>
    /// Tries to retrieve an <see cref="Asset"/> from cache.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Asset"/> to get.</typeparam>
    /// <param name="name">The name (full ID) of the <see cref="Asset"/> to get.</param>
    /// <returns>The <see cref="Asset"/> if found, <c>null</c> otherwise.</returns>
    /// <exception cref="InvalidOperationException">A different type of asset with this <paramref name="name"/> was found instead.</exception>
    internal T? GetAsset<T>(string name) where T : Asset
    {
        if (!_assetCache.Contains(name)) return null;
        return _assetCache[name] as T
            ?? throw new System.IO.InvalidDataException(Resources.WrongAssetType + name);
    }
    #endregion

    #region Clean cache
    /// <summary>
    /// Removes all <see cref="Asset"/>s from the cache that have no references any more.
    /// </summary>
    public void Clean()
    {
        using (new ProfilerEvent("Cleaning asset cache"))
        {
            // Build a list of elements to remove
            var pendingRemove = new LinkedList<Asset>();
            foreach (var asset in _assetCache.Where(x => x.ReferenceCount == 0))
            {
                asset.Dispose();
                pendingRemove.AddLast(asset);
            }

            // Remove the elements one-by-one
            foreach (var asset in pendingRemove) _assetCache.Remove(asset);
        }
    }
    #endregion

    //--------------------//

    #region Dispose
    /// <summary>
    /// Calls <see cref="Clean"/> and then checks if any <see cref="Asset"/> was not released.
    /// </summary>
    public void Dispose()
    {
        Clean();

        // Check if any cache asset was not released
        foreach (var asset in _assetCache)
        {
            asset.Dispose();

            Log.Error($"References were not properly released for {asset}");
#if DEBUG
            throw new InvalidOperationException($"References were not properly released for {asset}");
#endif
        }

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    ~CacheManager()
    {
        // This block will only be executed on Garbage Collection, not by manual disposal
        Log.Error($"Forgot to call Dispose on {this}");
#if DEBUG
        throw new InvalidOperationException($"Forgot to call Dispose on {this}");
#endif
    }
    #endregion
}
