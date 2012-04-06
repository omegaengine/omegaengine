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
using System.Diagnostics.CodeAnalysis;
using Common.Tasks;
using Common.Utils;
using World.Properties;

#if NETFX4
using System.Threading.Tasks;
#endif

namespace World
{
    /// <summary>
    /// Generates light-angle maps from a height map for a <see cref="Terrain"/> as a background task.
    /// </summary>
    /// <seealso cref="Terrain.LightRiseAngleMap"/>
    /// <seealso cref="Terrain.LightSetAngleMap"/>
    public class LightAngleMapGenerator : ThreadTask
    {
        #region Variables
        private readonly TerrainSize _size;
        private readonly byte[,] _heightMap;
        #endregion

        #region Properties
        /// <inheritdoc />
        public override string Name { get { return Resources.GeneratingLingleAngleMaps; } }

        private byte[,] _lightRiseAngleMap;

        /// <summary>
        /// Returns the calculated light rise angle-map array once <see cref="ITask.State"/> has reached <see cref="TaskState.Complete"/>.
        /// </summary>
        /// <remarks>A light rise angles is the minimum vertical angle (0 = 0°, 255 = 90°) which a directional light must achieve to be not occluded.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="ITask.State"/> is not <see cref="TaskState.Complete"/>.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "For performance reasons this property provides direct access to the underlying array without any cloning involved")]
        public byte[,] LightRiseAngleMap
        {
            get
            {
                lock (StateLock)
                {
                    if (State != TaskState.Complete) throw new InvalidOperationException(Resources.CalculationNotComplete);
                }

                return _lightRiseAngleMap;
            }
        }

        private byte[,] _lightSetAngleMap;

        /// <summary>
        /// Returns the calculated light set angle-map array once <see cref="ITask.State"/> has reached <see cref="TaskState.Complete"/>.
        /// </summary>
        /// <remarks>A light rise set is the maximum vertical angle (0 = 90°, 255 = 180°) which a directional light must not exceed to be not occluded.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="ITask.State"/> is not <see cref="TaskState.Complete"/>.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "For performance reasons this property provides direct access to the underlying array without any cloning involved")]
        public byte[,] LightSetAngleMap
        {
            get
            {
                lock (StateLock)
                {
                    if (State != TaskState.Complete) throw new InvalidOperationException(Resources.CalculationNotComplete);
                }

                return _lightSetAngleMap;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares to calculate light-angle maps for a height-map.
        /// </summary>
        /// <param name="size">The size of the terrain represented by the height-map.</param>
        /// <param name="heightMap">The height-map data. This is not cloned and must not be modified during calculation!</param>
        public LightAngleMapGenerator(TerrainSize size, byte[,] heightMap)
        {
            #region Sanity chekcs
            if (heightMap == null) throw new ArgumentNullException("heightMap");
            if (heightMap.GetUpperBound(0) + 1 != size.X || heightMap.GetUpperBound(1) + 1 != size.Y)
                throw new ArgumentException(Resources.HeightMapSizeEqualTerrain, "heightMap");
            #endregion

            _size = size;
            _heightMap = heightMap;

#if !NETFX4
            BytesTotal = size.X * size.Y;
#endif
        }
        #endregion

        #region Factory methods
        /// <summary>
        /// Prepares to calculate light-angle maps the height-map of a <see cref="Terrain"/>.
        /// </summary>
        /// <param name="terrain">The <see cref="Terrain"/> providing the height-map. The height-map is not cloned and must not be modified during calculation!</param>
        /// <returns>The newly crated light-angle map generator.</returns>
        /// <remarks>The results are not automatically written back to <paramref name="terrain"/>.</remarks>
        public static LightAngleMapGenerator FromTerrain(Terrain terrain)
        {
            #region Sanity checks
            if (terrain == null) throw new ArgumentNullException("terrain");
            #endregion

            return new LightAngleMapGenerator(terrain.Size, terrain.HeightMap);
        }
        #endregion

        //--------------------//

        #region Thread code
        /// <inheritdoc />
        protected override void RunTask()
        {
            // Create new angle-map arrays
            _lightRiseAngleMap = new byte[_size.X,_size.Y];
            _lightSetAngleMap = new byte[_size.X,_size.Y];

            lock (StateLock) State = TaskState.Data;

            // Iterate through each degree of longitude (lines from east-to-west)
#if NETFX4
            Parallel.For(0, _size.Y, y =>
#else
            for (int y = 0; y < _size.Y; y++)
#endif
            {
                // The west-most stays lit until the light-source fully sets (nothing west of it to throw a shadow)
                _lightSetAngleMap[0, y] = 255;

                // Iterate through all points along the line from west to east
                for (int x1 = 0; x1 < _size.X; x1++)
                {
                    // Start off assuming there is nothing casting a shadow
                    byte value = 255;

                    // Iterate through all points west of the current one
                    for (int x2 = 0; x2 < x1; x2++)
                    {
                        // Draw a line between the two points to determine the angle above which a shadow is cast for setting light sources
                        byte newValue = SetAngleToByte(GetLightAngle(x1, x2, y));
                        value = Math.Min(value, newValue);
                    }
                    _lightSetAngleMap[x1, y] = value;

                    // Iterate through all points east of the current one
                    value = 0;
                    for (int x2 = x1; x2 < _size.X; x2++)
                    {
                        // Draw a line between the two points to determine the angle below which a shadow is cast for rising light sources
                        byte newValue = RiseAngleToByte(GetLightAngle(x1, x2, y));
                        value = Math.Max(value, newValue);
                    }
                    _lightRiseAngleMap[x1, y] = value;
                }

#if !NETFX4
                BytesProcessed += _size.Y;
#endif
                if (CancelRequest.WaitOne(0)) throw new OperationCanceledException();
            }
#if NETFX4
            );
#endif

            lock (StateLock) State = TaskState.Complete;
        }
        #endregion

        #region Calculation helpers
        /// <summary>
        /// Calculates the angle between to points along the same Y-axis (degree of longitude) on the height-map.
        /// </summary>
        /// <param name="x1">The X-coordinate of the first point.</param>
        /// <param name="x2">The X-coordinate of the second point.</param>
        /// <param name="y">The shared Y-coordinate.</param>
        /// <returns>An angle in radians moving counter-clockwise assuming <paramref name="x2"/> is larger than <paramref name="x1"/>.</returns>
        private double GetLightAngle(int x1, int x2, int y)
        {
            int xDist = x2 - x1;
            int heightDist = MathUtils.Clamp(_heightMap[x2, y] - _heightMap[x1, y], 0, 255);
            return Math.Atan2(heightDist * _size.StretchV, xDist * _size.StretchH);
        }

        /// <summary>
        /// Converts the <paramref name="angle"/> to a byte value as follows: 0° = 0, 90° = 255
        /// </summary>
        private static byte RiseAngleToByte(double angle)
        {
            angle = MathUtils.Clamp(angle, 0, Math.PI / 2);
            return (byte)(angle / (Math.PI / 2) * 255);
        }

        /// <summary>
        /// Converts the <paramref name="angle"/> to a byte value as follows: 90° = 0, 180° = 255
        /// </summary>
        private static byte SetAngleToByte(double angle)
        {
            angle = MathUtils.Clamp(angle, Math.PI / 2, Math.PI);
            return (byte)((angle / (Math.PI / 2) - 1) * 255);
        }
        #endregion
    }
}
