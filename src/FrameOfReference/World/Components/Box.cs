/*
 * Copyright 2006-2014 Bastian Eicher
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
using System.Drawing;
using System.Xml.Serialization;
using AlphaFramework.World.Components;
using Common.Values;
using SlimDX;

namespace FrameOfReference.World.Components
{
    /// <summary>
    /// Collision-detection using an axis-aligned box.
    /// </summary>
    public class Box : Collision<Vector2>
    {
        /// <summary>
        /// The lower left corner of the box (originating from the body's position).
        /// </summary>
        [DefaultValue(typeof(Vector2), "0; 0"), Description("The lower left corner of the box (originating from the body's position).")]
        public Vector2 Minimum { get; set; }

        /// <summary>
        /// The upper right corner of the box (originating from the body's position).
        /// </summary>
        [DefaultValue(typeof(Vector2), "0; 0"), Description("The upper right corner of the box (originating from the body's position).")]
        public Vector2 Maximum { get; set; }

        /// <summary>
        /// The area covered by this box.
        /// </summary>
        [Browsable(false), XmlIgnore]
        public RectangleF Area { get { return RectangleF.FromLTRB(Minimum.X, Minimum.Y, Maximum.X, Maximum.Y); } }

        /// <summary>
        /// Determines whether a certain point lies within the box.
        /// </summary>
        /// <param name="point">The point to check for collision in entity space.</param>
        /// <param name="rotation">How the box shall be rotated before performing the collision test.</param>
        /// <returns><see langword="true"/> if <paramref name="point"/> does collide with the box, <see langword="false"/> otherwise.</returns>
        public override bool CollisionTest(Vector2 point, float rotation)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (rotation == 0)
            {
                // Perform simple test if no rotation is to be performed
                return (point.X >= Minimum.X && point.X <= Maximum.X &&
                        point.Y >= Minimum.Y && point.Y <= Maximum.Y);
            }
            // Empty boxes can never intersect
            if (Minimum == Maximum) return false;

            // Perform rotation before intersection test
            var rotatedBox = new Quadrangle(Area).Rotate(rotation);
            return rotatedBox.IntersectWith(point);
        }

        /// <summary>
        /// Determines whether a certain area lies within the box.
        /// </summary>
        /// <param name="area">The area to check for collision in entity space.</param>
        /// <param name="rotation">How the box shall be rotated before performing the collision test.</param>
        /// <returns><see langword="true"/> if <paramref name="area"/> does collide with the box, <see langword="false"/>.</returns>
        public override bool CollisionTest(Quadrangle area, float rotation)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (rotation == 0)
            {
                // Perform simple test if no rotation is to be performed
                return area.IntersectWith(Area);
            }

            // Perform rotation before intersection test
            var rotatedBox = new Quadrangle(Area).Rotate(rotation);
            return rotatedBox.IntersectWith(area);
        }
    }
}
