/*
 * Copyright 2006-2012 Bastian Eicher
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
using Common.Utils;

namespace Presentation
{
    /// <summary>
    /// Describes a square or circle terrain area that is to be modified.
    /// </summary>
    public struct TerrainBrush
    {
        /// <summary>
        /// The length of the sqaure or the diameter of the circle.
        /// </summary>
        public readonly int Size;

        /// <summary>
        /// <see langword="true"/> if this is a circle, <see langword="false"/> if this is a sqaure.
        /// </summary>
        public readonly bool Circle;

        /// <summary>
        /// Creates a new modification area.
        /// </summary>
        /// <param name="size">The length of the sqaure or the diameter of the circle.</param>
        /// <param name="circle"><see langword="true"/> if this is a circle, <see langword="false"/> if this is a sqaure.</param>
        public TerrainBrush(int size, bool circle)
        {
            Size = size;
            Circle = circle;
        }

        /// <summary>
        /// Checks whether specific 2D coordinates (relative to the top-left corner) lie within this modification area.
        /// </summary>
        public bool Contains(int x, int y)
        {
            if (Circle)
            {
                double radius = Size / 2.0;
                double distanceX = x - radius + 0.5;
                double distanceY = y - radius + 0.5;
                return (distanceX * distanceX + distanceY * distanceY <= radius * radius); // Euclidian norm
            }
            else return x >= 0 && x < Size && y >= 0 && y < Size; // Maximum norm
        }

        /// <summary>
        /// Provides a scaling factor depending on how close specific 2D coordinates (relative to the top-left corner) lie to the modification area center.
        /// </summary>
        public double Factor(int x, int y)
        {
            double radius = Size / 2.0;
            double distanceX = x - radius + 0.5;
            double distanceY = y - radius + 0.5;
            if (Circle)
            {
                double distance = Math.Sqrt(distanceX * distanceX + distanceY * distanceY); // Euclidian norm
                return MathUtils.InterpolateTrigonometric(distance / radius, 1, 0); // Smooth bell shape
            }
            else
            {
                double distance = Math.Max(Math.Abs(distanceX), Math.Abs(distanceY)); // Maximum norm
                return MathUtils.InterpolateTrigonometric(distance / radius * 2, 1, 1, 0); // Flat top with smooth slopes
            }
        }
    }
}
