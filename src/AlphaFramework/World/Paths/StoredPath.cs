/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using AlphaFramework.World.Positionables;

namespace AlphaFramework.World.Paths;

/// <summary>
/// Stores a path calculated by <see cref="IPathfinder{TCoordinates}"/>.
/// </summary>
/// <typeparam name="TCoordinates">Data type for storing position coordinates of objects in the game world.</typeparam>
/// <seealso cref="EntityBase{TCoordinates,TTemplate}.CurrentPath"/>
public class StoredPath<TCoordinates> : ICloneable
    where TCoordinates : struct
{
    /// <summary>
    /// The final target of the pathfinding.
    /// </summary>
    public TCoordinates Target { get; set; }

    /// <summary>
    /// The path to walk.
    /// </summary>
    public Queue<TCoordinates> PathNodes { get; } = new();

    /// <inheritdoc/>
    public override string ToString() => GetType().Name;

    #region Clone
    /// <summary>
    /// Creates a shallow copy of this <see cref="StoredPath{TCoordinates}"/>
    /// </summary>
    /// <returns>The cloned <see cref="StoredPath{TCoordinates}"/>.</returns>
    public StoredPath<TCoordinates> Clone()
    {
        // Perform initial shallow copy
        return (StoredPath<TCoordinates>)MemberwiseClone();
    }

    object ICloneable.Clone() => Clone();
    #endregion
}
