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

using System.ComponentModel;
using System.Xml.Serialization;
using Common.Values;
using SlimDX;

namespace World.EntityComponents
{
    /// <summary>
    /// Collision-detection using a simple uniform circle.
    /// </summary>
    /// <seealso cref="EntityTemplate.CollisionControl"/>
    public class Circle : CollisionControl<Vector2>
    {
        #region Properties
        /// <summary>
        /// The radius of the circle.
        /// </summary>
        [DefaultValue(0f), Description("The radius of the circle.")]
        [XmlAttribute]
        public float Radius { get; set; }
        #endregion

        //--------------------//

        #region Collision test
        /// <summary>
        /// Determines whether a certain point lies within a circle.
        /// </summary>
        /// <param name="point">The point to check for collision in entity space.</param>
        /// <param name="rotation">This is ignored for circles.</param>
        /// <returns><see langword="true"/> if the <paramref name="point"/> does collide with the circle, <see langword="false"/>.</returns>
        internal override bool CollisionTest(Vector2 point, float rotation)
        {
            // Empty or negative circles can never intersect
            if (Radius <= 0) return false;

            return point.Length() < Radius;
        }

        /// <summary>
        /// Determines whether a certain area lies within a circle.
        /// </summary>
        /// <param name="area">The area to check for collision in entity space.</param>
        /// <param name="rotation">This is ignored for circles.</param>
        /// <returns><see langword="true"/> if <paramref name="area"/> does collide with the circle, <see langword="false"/>.</returns>
        internal override bool CollisionTest(Quadrangle area, float rotation)
        {
            // Empty or negative circles can never intersect
            if (Radius <= 0) return false;

            // Shift area to the circle center as the origin
            return area.IntersectCircle(Radius);
        }
        #endregion

        #region Path finding
        /// <summary>
        /// Returns a list of positions that outline this circle.
        /// </summary>
        /// <param name="rotation">How the collision body shall be rotated before performing the outline calculation.</param>
        /// <returns>Positions in entity space for use by the pathfinding system.</returns>
        internal override Vector2[] GetPathFindingOutline(float rotation)
        {
            // ToDo: Implement
            return new Vector2[0];
        }
        #endregion
    }
}
