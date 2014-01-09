/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
        /// Requests a path for a player character.
        /// </summary>
        /// <param name="start">The starting coordinates.</param>
        /// <param name="target">The end coordinates.</param>
        /// <returns>A list of points forming a path; <see langword="null"/> if no path was found.</returns>
        IEnumerable<TCoordinates> FindPathPlayer(TCoordinates start, TCoordinates target);
    }
}
