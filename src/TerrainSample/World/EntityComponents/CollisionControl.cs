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
using Common.Values;
using SlimDX;
using World.Positionables;
using World.Templates;

namespace World.EntityComponents
{
    /// <summary>
    /// Controls how <see cref="Entity{TCoordinates}"/>s occupy space around them.
    /// </summary>
    /// <seealso cref="EntityTemplate.CollisionControl"/>
    public abstract class CollisionControl<TCoordinates> : ICloneable
        where TCoordinates : struct
    {
        /// <inheritdoc/>
        public override string ToString()
        {
            return GetType().Name;
        }

        #region Collision test
        /// <summary>
        /// Determines whether a a certain point lies within the collision body.
        /// </summary>
        /// <param name="point">The point to check for collision in entity space.</param>
        /// <param name="rotation">How the collision body shall be rotated before performing the collision test.</param>
        /// <returns><see langword="true"/> if <paramref name="point"/> does collide with the body, <see langword="false"/> otherwise.</returns>
        internal abstract bool CollisionTest(TCoordinates point, float rotation);

        /// <summary>
        /// Determines whether a certain area lies within the collision body.
        /// </summary>
        /// <param name="area">The area to check for collision in entity space.</param>
        /// <param name="rotation">How the collision body shall be rotated before performing the collision test.</param>
        /// <returns><see langword="true"/> if <paramref name="area"/> does collide with the body, <see langword="false"/> otherwise.</returns>
        internal abstract bool CollisionTest(Quadrangle area, float rotation);
        #endregion

        #region Path finding
        /// <summary>
        /// Returns a list of positions that outline this collision body.
        /// </summary>
        /// <param name="rotation">How the collision body shall be rotated before performing the outline calculation.</param>
        /// <returns>Positions in entity space for use by the pathfinding system.</returns>
        internal abstract TCoordinates[] GetPathFindingOutline(float rotation);
        #endregion

        #region Clone
        /// <summary>
        /// Creates a copy of this <see cref="CollisionControl{TCoordinates}"/>.
        /// </summary>
        /// <returns>The cloned <see cref="CollisionControl{TCoordinates}"/>.</returns>
        public CollisionControl<TCoordinates> Clone()
        {
            // Perform initial shallow copy
            return (CollisionControl<TCoordinates>)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}
