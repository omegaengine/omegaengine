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

using Common.Values;

namespace World.Terrains
{
    /// <summary>
    /// Contains positioning and orientation information for a specific point on a <see cref="Terrain"/>
    /// </summary>
    /// <seealso cref="Terrain.ToEngineCoords"/>
    public struct TerrainPoint
    {
        #region Properties
        private readonly DoubleVector3 _position;

        /// <summary>
        /// The 3D-position of the point in engine coordinates.
        /// </summary>
        public DoubleVector3 Position { get { return _position; } }

        private readonly float _angleX;

        /// <summary>
        /// The angle of the point along the x-axis in radians.
        /// </summary>
        public float AngleX { get { return _angleX; } }

        private readonly float _angleY;

        /// <summary>
        /// The angle of the point along the y-axis in radians
        /// </summary>
        public float AngleY { get { return _angleY; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new terrain point information struct.
        /// </summary>
        /// <param name="position">The 3D-position of the point in engine coordinates.</param>
        /// <param name="angleX">The angle of the point along the x-axis in radians.</param>
        /// <param name="angleY">The angle of the point along the y-axis in radians.</param>
        internal TerrainPoint(DoubleVector3 position, float angleX, float angleY)
        {
            _position = position;
            _angleX = angleX;
            _angleY = angleY;
        }
        #endregion
    }
}
