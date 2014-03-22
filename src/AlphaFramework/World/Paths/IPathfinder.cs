/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace AlphaFramework.World.Paths
{
    /// <summary>
    /// A strategy pattern interface for pathfinding algorithms.
    /// </summary>
    /// <typeparam name="TCoordinates">Data type for storing position coordinates of objects in the game world.</typeparam>
    public interface IPathfinder<TCoordinates>
        where TCoordinates : struct
    {
        /// <summary>
        /// Calculates a path from source to target coordinates.
        /// </summary>
        /// <param name="start">The starting coordinates.</param>
        /// <param name="target">The end coordinates.</param>
        /// <returns>A list of coordinates forming a path; <see langword="null"/> if no path was found.</returns>
        IEnumerable<TCoordinates> FindPath(TCoordinates start, TCoordinates target);
    }
}
