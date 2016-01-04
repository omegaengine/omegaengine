/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using NanoByte.Common;

namespace AlphaFramework.World.Terrains
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
        /// <c>true</c> if this is a circle, <c>false</c> if this is a sqaure.
        /// </summary>
        public readonly bool Circle;

        /// <summary>
        /// Creates a new modification area.
        /// </summary>
        /// <param name="size">The length of the sqaure or the diameter of the circle.</param>
        /// <param name="circle"><c>true</c> if this is a circle, <c>false</c> if this is a sqaure.</param>
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
