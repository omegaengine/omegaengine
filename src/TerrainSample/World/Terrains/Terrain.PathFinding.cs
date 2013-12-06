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

using System;
using System.Collections.Generic;
using System.Linq;
using LuaInterface;
using SlimDX;
using World.Pathfinding;
using World.Positionables;

namespace World.Terrains
{
    partial class Terrain
    {
        /// <summary>
        /// The pathfinding engine used on this <see cref="Terrain"/>.
        /// Is <see langword="null"/> until <see cref="SetupPathfinding"/> has been called.
        /// </summary>
        internal IPathfinder<Vector2> Pathfinder { get; private set; }

        /// <summary>
        /// Initializes the pathfinding engine.
        /// </summary>
        /// <param name="entities">The <see cref="Positionable{TCoordinates}"/>s to consider for obstacles.</param>
        /// <remarks>Is automatically called on first access to <see cref="TerrainUniverse.Terrain"/>.</remarks>
        [LuaHide]
        public void SetupPathfinding(IEnumerable<Positionable<Vector2>> entities)
        {
            #region Sanity checks
            if (entities == null) throw new ArgumentNullException("entities");
            #endregion

            var initMap = new bool[_size.X, _size.Y];
            foreach (var water in entities.OfType<Water>())
            {
                var xStart = (int)Math.Floor(water.Position.X / _size.StretchH);
                var xEnd = (int)Math.Ceiling((water.Position.X + water.Size.X) / _size.StretchH);
                var yStart = (int)Math.Floor(water.Position.Y / _size.StretchH);
                var yEnd = (int)Math.Ceiling((water.Position.Y + water.Size.Y) / _size.StretchH);

                if (xEnd > Size.X - 1) xEnd = Size.X - 1;
                if (yEnd > Size.Y - 1) yEnd = Size.Y - 1;

                for (int x = xStart; x <= xEnd; x++)
                {
                    for (int y = yStart; y <= yEnd; y++)
                        initMap[x, y] = (HeightMap[x, y] * Size.StretchV) < water.Height - water.TraversableDepth;
                }
            }

            Pathfinder = new SimplePathfinder(initMap);
        }
    }
}
