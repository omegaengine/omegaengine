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
using Common.Undo;
using Common.Values;
using SlimDX;
using TerrainSample.Presentation;
using TerrainSample.World.Terrains;

namespace TerrainSample.Editor.World.TerrainModifiers
{
    /// <summary>
    /// Abstract base class for interactivley modifying a <see cref="Terrain"/>.
    /// </summary>
    internal abstract class Base
    {
        /// <summary>Used to collect data as it was before the modifications.</summary>
        protected readonly ExpandableRectangleArray<byte> OldData = new ExpandableRectangleArray<byte>();

        /// <summary>Used to collect data as it is after the modifcations.</summary>
        protected readonly ExpandableRectangleArray<byte> NewData = new ExpandableRectangleArray<byte>();

        /// <summary>The <see cref="Terrain"/> to modify.</summary>
        protected readonly Terrain Terrain;

        /// <summary>
        /// Creates a new <see cref="Terrain"/> modifier.
        /// </summary>
        /// <param name="terrain">The <see cref="Terrain"/> to modify.</param>
        protected Base(Terrain terrain)
        {
            #region Sanity checks
            if (terrain == null) throw new ArgumentNullException("terrain");
            #endregion

            Terrain = terrain;
        }

        /// <summary>
        /// Applies and accumulates a modification to the <see cref="Terrain"/>.
        /// </summary>
        /// <param name="terrainCoords">The center coordinates of the area to modify in world space.</param>
        /// <param name="brush">The shape and size of the area around <paramref name="terrainCoords"/> to modify.</param>
        public abstract void Apply(Vector2 terrainCoords, TerrainBrush brush);

        /// <summary>
        /// Creates a pre-executed undo command representing the accumulated <see cref="Apply"/> calls to this instance.
        /// </summary>
        public abstract IUndoCommand GetCommand();
    }
}
