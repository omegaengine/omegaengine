/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using AlphaFramework.World.Positionables;

namespace AlphaFramework.World.Paths
{
    /// <summary>.
    /// A following member in a pathfinding group (follows <see cref="PathLeader{TCoordinates}"/>).
    /// </summary>
    /// <typeparam name="TCoordinates">Data type for storing position coordinates of objects in the game world.</typeparam>
    /// <seealso cref="EntityBase{TCoordinates,TTemplate}.PathControl"/>
    public class PathFollower<TCoordinates> : PathControl<TCoordinates>
        where TCoordinates : struct
    {
        /// <summary>
        /// Stores the <see cref="PathLeader{TCoordinates}.ID"/>.
        /// </summary>
        public int LeaderID { get; set; }

        /// <summary>
        /// The relative position to the <see cref="PathLeader{TCoordinates}"/>.
        /// </summary>
        public TCoordinates Position { get; set; }
    }
}
