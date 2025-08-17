/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using NanoByte.Common;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Audio;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Assets;

/// <summary>
/// Data loaded from a file and cached for use by one or more <see cref="Renderable"/>s, <see cref="Sound"/>s, etc..
/// </summary>
/// <seealso cref="CacheManager.GetAsset{T}"/>
public abstract class Asset : IReferenceCount, IDisposable, INamed, IComparable<Asset>
{
    #region Properties
    /// <summary>
    /// The filename
    /// </summary>
    // Mark as read only for PropertyGrid, since direct renames would confuse NamedCollection<T>
    [ReadOnly(true)]
    public string Name { get; set; } = null!;

    public override string ToString() => $"{GetType().Name}: {Name}";

    /// <summary>
    /// How many <see cref="Renderable"/>s use this asset
    /// </summary>
    /// <remarks>When this hits zero you can call <see cref="Dispose()"/></remarks>
    [Browsable(false)]
    public int ReferenceCount { get; private set; }
    #endregion

    //--------------------//

    #region Reference control
    /// <summary>
    /// Increments the <see cref="ReferenceCount"/> by one.
    /// </summary>
    public virtual void HoldReference()
    {
        ReferenceCount++;
    }

    /// <summary>
    /// Decrements the <see cref="ReferenceCount"/> by one.
    /// </summary>
    public virtual void ReleaseReference()
    {
        ReferenceCount--;

        #region Sanity checks
        if (ReferenceCount < 0)
            throw new InvalidOperationException(string.Format(Resources.ReleasedTooOften, this));
        #endregion
    }
    #endregion

    //--------------------//

    #region Comparison
    int IComparable<Asset>.CompareTo(Asset? other)
    {
        return (other == null) ? 0 : string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
    }
    #endregion

    #region Dispose
    /// <summary>
    /// Was this asset already disposed?
    /// </summary>
    [Browsable(false)]
    public bool Disposed { get; private set; }

    /// <summary>
    /// Disposes the internal DirectX resources of this asset.
    /// </summary>
    /// <remarks>Will be automatically called by <see cref="CacheManager.Clean"/> if <see cref="ReferenceCount"/> is zero.</remarks>
    public void Dispose()
    {
        if (Disposed) return;
        Dispose(true);
        GC.SuppressFinalize(this);
        Disposed = true;
    }

    /// <inheritdoc/>
    ~Asset()
    {
        Dispose(false);
    }

    /// <summary>
    /// To be called by <see cref="IDisposable.Dispose"/> and the object destructor.
    /// </summary>
    /// <param name="disposing"><c>true</c> if called manually and not by the garbage collector.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        { // This block will only be executed on Garbage Collection, not by manual disposal
            Log.Error($"Forgot to call Dispose on {this}");
#if DEBUG
            throw new InvalidOperationException($"Forgot to call Dispose on {this}");
#endif
        }
    }
    #endregion
}
