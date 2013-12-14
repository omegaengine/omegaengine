/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System.Collections.Generic;
using System.Xml.Serialization;
using TemplateWorld.Positionables;

namespace TemplateWorld.Paths
{
    /// <summary>
    /// The leader in a pathfinding group (followed by <see cref="PathFollower{TCoordinates}"/>).
    /// </summary>
    /// <typeparam name="TCoordinates">Data type for storing position coordinates of objects in the game world.</typeparam>
    /// <seealso cref="EntityBase{TSelf,TCoordinates,TTemplate}.PathControl"/>
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
