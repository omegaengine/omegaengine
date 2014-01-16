/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Xml.Serialization;
using AlphaFramework.World.Positionables;

namespace AlphaFramework.World.Paths
{
    /// <summary>
    /// The leader in a pathfinding group (followed by <see cref="PathFollower{TCoordinates}"/>).
    /// </summary>
    /// <typeparam name="TCoordinates">Data type for storing position coordinates of objects in the game world.</typeparam>
    /// <seealso cref="EntityBase{TCoordinates,TTemplate}.PathControl"/>
    public class PathLeader<TCoordinates> : PathControl<TCoordinates>
        where TCoordinates : struct
    {
        /// <summary>
        /// The ID of the group leader.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The final target of the pathfinding.
        /// </summary>
        public TCoordinates Target { get; set; }

        // ToDo: Replace Stack with Queue, by turning PathFinder output around
        private readonly Stack<TCoordinates> _pathNodes = new Stack<TCoordinates>();

        /// <summary>
        /// The path to walk.
        /// </summary>
        /// <remarks>Is not serialized/stored, will be recalculated.</remarks>
        [XmlIgnore]
        public Stack<TCoordinates> PathNodes { get { return _pathNodes; } }
    }
}
