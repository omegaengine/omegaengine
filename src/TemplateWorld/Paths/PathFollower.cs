﻿/*
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

using TemplateWorld.Positionables;

namespace TemplateWorld.Paths
{
    /// <summary>.
    /// A following member in a pathfinding group (follows <see cref="PathLeader{TCoordinates}"/>).
    /// </summary>
    /// <typeparam name="TCoordinates">Data type for storing position coordinates of objects in the game world.</typeparam>
    /// <seealso cref="EntityBase{TSelf,TCoordinates,TTemplate}.PathControl"/>
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