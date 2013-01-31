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
using Resources = World.Properties.Resources;

namespace World
{
    partial class Terrain
    {
        #region Camera height
        /// <summary>
        /// Gets the camera target height for a certain point on the <see cref="Terrain"/>.
        /// </summary>
        /// <param name="coordinates">The engine coordinates in world space of the point to get information for; the Y-component is ignored.</param>
        /// <returns>The height in engine units</returns>
        /// <remarks>Unlike <see cref="ToEngineCoords"/> this function returns smooth values ignoring the polygonal nature of the rendered <see cref="Terrain"/>.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the coordinates lie outside the range of the <see cref="Terrain"/>.</exception>
        public double GetCameraHeight(DoubleVector3 coordinates)
        {
            // Engine +X = World X
            // Engine -Z = World Y
            var unstrechedCoords = new Vector2((float)coordinates.X / _size.StretchH, -(float)coordinates.Z / _size.StretchH);

            #region Sanity checks
            if (unstrechedCoords.X > HeightMap.GetLength(0) - 1 || unstrechedCoords.Y > HeightMap.GetLength(1) - 1 ||
                unstrechedCoords.X < 0 || unstrechedCoords.Y < 0)
            {
                // Prevent panning outside of the terrain
                throw new ArgumentOutOfRangeException("coordinates");
            }
            #endregion

            #region Snap values
            // Snap X values to bounding whole values
            var xPos0 = (int)Math.Floor(unstrechedCoords.X);
            var xPos1 = (int)Math.Ceiling(unstrechedCoords.X);
            float xPosDiff = unstrechedCoords.X - xPos0;

            // Snap Y values to bounding whole values
            var yPos0 = (int)Math.Floor(unstrechedCoords.Y);
            var yPos1 = (int)Math.Ceiling(unstrechedCoords.Y);
            float yPosDiff = unstrechedCoords.Y - yPos0;
            #endregion

            // Calculate weighted average of surrounding four points
            return _size.StretchV *
                (HeightMap[xPos0, yPos0] * (1 - xPosDiff) * (1 - yPosDiff) +
                    HeightMap[xPos0, yPos1] * (1 - xPosDiff) * yPosDiff +
                    HeightMap[xPos1, yPos0] * xPosDiff * (1 - yPosDiff) +
                    HeightMap[xPos1, yPos1] * xPosDiff * yPosDiff);
        }
        #endregion

        #region Coordinate conversion
        /// <summary>
        /// Converts a position in world coordinates to the engine entity space coordinate system.
        /// </summary>
        /// <param name="coordinates">The coordinates of the point in engine world space to get information for.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the coordinates lie outside the range of the <see cref="Terrain"/>.</exception>
        /// <remarks>Unlike <see cref="GetCameraHeight"/> this function returns potentially jerky values, respecting the polygonal nature of the rendered <see cref="Terrain"/>.</remarks>
        public TerrainPoint ToEngineCoords(Vector2 coordinates)
        {
            // Note: This is only required for lookups in the height-map, not for actually unstretching the coordinates to be returned
            Vector2 unstrechedCoords = coordinates * (1 / _size.StretchH);

            #region Sanity checks
            if (unstrechedCoords.X > HeightMap.GetLength(0) - 1 || unstrechedCoords.Y > HeightMap.GetLength(1) - 1 ||
                unstrechedCoords.X < 0 || unstrechedCoords.Y < 0)
                throw new ArgumentOutOfRangeException("coordinates", Resources.CoordinatesNotInRange);
            #endregion

            // ToDo: Use helper method

            #region Snap values
            // Snap X values to bounding whole values
            var xPos0 = (int)Math.Floor(unstrechedCoords.X);
            var xPos1 = (int)Math.Ceiling(unstrechedCoords.X);
            float xPosDiff = unstrechedCoords.X - xPos0;

            // Snap Y values to bounding whole values
            var yPos0 = (int)Math.Floor(unstrechedCoords.Y);
            var yPos1 = (int)Math.Ceiling(unstrechedCoords.Y);
            float yPosDiff = unstrechedCoords.Y - yPos0;
            #endregion

            #region Maths code
            float height, xHeightDiff, yHeightDiff;
            // Determine one which of the two possible triangles of the map square the point is located
            if (xPosDiff + yPosDiff < 1)
            {
                // Top left triangle
                xHeightDiff = HeightMap[xPos1, yPos0] - HeightMap[xPos0, yPos0];
                yHeightDiff = HeightMap[xPos0, yPos1] - HeightMap[xPos0, yPos0];
                height = HeightMap[xPos0, yPos0] +
                    ((xHeightDiff * xPosDiff) + (yHeightDiff * yPosDiff));
            }
            else
            {
                // Bottom right triangle
                xHeightDiff = HeightMap[xPos1, yPos1] - HeightMap[xPos0, yPos1];
                yHeightDiff = HeightMap[xPos1, yPos1] - HeightMap[xPos1, yPos0];
                height = HeightMap[xPos1, yPos1] -
                    ((xHeightDiff * (1 - xPosDiff)) + (yHeightDiff * (1 - yPosDiff)));
            }

            // Apply vertical stretch factor
            xHeightDiff *= _size.StretchV;
            yHeightDiff *= _size.StretchV;
            height *= _size.StretchV;
            #endregion

            // World X = Engine +X
            // World Y = Engine -Z
            // World height = Engine +Y
            var pointPosition = new DoubleVector3(coordinates.X, height, -coordinates.Y);
            return new TerrainPoint(pointPosition,
                (float)Math.Atan(yHeightDiff / _size.StretchH),
                (float)Math.Atan(xHeightDiff / _size.StretchH));
        }

        /// <summary>
        /// Converts a position in the engine entity space coordinate system to world coordinates.
        /// </summary>
        /// <param name="coordinates">The coordinates in engine entity space to convert.</param>
        /// <returns>The world coordinates.</returns>
        public static Vector2 ToWorldCoords(DoubleVector3 coordinates)
        {
            // Engine X = World +X
            // Engine Z = World -Y
            return new Vector2((float)coordinates.X, (float)-coordinates.Z);
        }
        #endregion
    }
}
