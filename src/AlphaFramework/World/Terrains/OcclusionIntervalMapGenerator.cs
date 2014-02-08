/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using AlphaFramework.World.Properties;
using Common.Tasks;
using Common.Utils;

#if NETFX4
using System.Threading.Tasks;
#endif

namespace AlphaFramework.World.Terrains
{
    /// <summary>
    /// Generates an occlusion interval map from a height map for a <see cref="ITerrain"/> as a background task.
    /// </summary>
    /// <seealso cref="ITerrain.OcclusionEndMap"/>
    /// <seealso cref="ITerrain.OcclusionBeginMap"/>
    public class OcclusionIntervalMapGenerator : ThreadTask
    {
        #region Variables
        private readonly TerrainSize _size;
        private readonly byte[,] _heightMap;
        private readonly float _sunInclination;
        #endregion

        #region Properties
        /// <inheritdoc />
        public override string Name { get { return Resources.GeneratingOcclusionIntervalMaps ; } }

        /// <inheritdoc />
        public override bool UnitsByte { get { return false; } }

        private byte[,] _occlusionEndMap;

        /// <summary>
        /// Returns the calculated occlusion end map array once <see cref="ITask.State"/> has reached <see cref="TaskState.Complete"/>.
        /// </summary>
        /// <remarks>A light rise angles is the minimum vertical angle (0 = 0°, 255 = 90°) which a directional light must achieve to be not occluded.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="ITask.State"/> is not <see cref="TaskState.Complete"/>.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "For performance reasons this property provides direct access to the underlying array without any cloning involved")]
        public byte[,] OcclusionEndMap
        {
            get
            {
                lock (StateLock)
                {
                    if (State != TaskState.Complete) throw new InvalidOperationException(Resources.CalculationNotComplete);
                }

                return _occlusionEndMap;
            }
        }

        private byte[,] _occlusionBeginMap;

        /// <summary>
        /// Returns the calculated occlusion begin map array once <see cref="ITask.State"/> has reached <see cref="TaskState.Complete"/>.
        /// </summary>
        /// <remarks>A light rise set is the maximum vertical angle (0 = 90°, 255 = 180°) which a directional light must not exceed to be not occluded.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="ITask.State"/> is not <see cref="TaskState.Complete"/>.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "For performance reasons this property provides direct access to the underlying array without any cloning involved")]
        public byte[,] OcclusionBeginMap
        {
            get
            {
                lock (StateLock)
                {
                    if (State != TaskState.Complete) throw new InvalidOperationException(Resources.CalculationNotComplete);
                }

                return _occlusionBeginMap;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares to calculate an occlusion interval map for a height-map.
        /// </summary>
        /// <param name="size">The size of the terrain represented by the height-map.</param>
        /// <param name="heightMap">The height-map data. This is not cloned and must not be modified during calculation!</param>
        /// <param name="sunInclination">The angle of inclination of the sun's path away from the zenith in degrees.</param>
        public OcclusionIntervalMapGenerator(TerrainSize size, byte[,] heightMap, float sunInclination)
        {
            #region Sanity chekcs
            if (heightMap == null) throw new ArgumentNullException("heightMap");
            if (heightMap.GetUpperBound(0) + 1 != size.X || heightMap.GetUpperBound(1) + 1 != size.Y)
                throw new ArgumentException(Resources.HeightMapSizeEqualTerrain, "heightMap");
            #endregion

            _size = size;
            _heightMap = heightMap;
            _sunInclination = sunInclination;

#if !NETFX4
            UnitsTotal = size.X * size.Y;
#endif
        }
        #endregion

        #region Factory methods
        /// <summary>
        /// Prepares to calculate an occlusion interval map for the height-map of a <see cref="ITerrain"/>.
        /// </summary>
        /// <param name="terrain">The <see cref="ITerrain"/> providing the height-map. The height-map is not cloned and must not be modified during calculation!</param>
        /// <param name="sunInclination">The angle of inclination of the sun's path away from the zenith in degrees.</param>
        /// <returns>The newly crated occlusion interval map generator.</returns>
        /// <remarks>The results are not automatically written back to <paramref name="terrain"/>.</remarks>
        public static OcclusionIntervalMapGenerator FromTerrain(ITerrain terrain, float sunInclination)
        {
            #region Sanity checks
            if (terrain == null) throw new ArgumentNullException("terrain");
            #endregion

            return new OcclusionIntervalMapGenerator(terrain.Size, terrain.HeightMap, sunInclination);
        }
        #endregion

        //--------------------//

        #region Thread code
        /// <inheritdoc />
        protected override void RunTask()
        {
            _occlusionEndMap = new byte[_size.X, _size.Y];
            _occlusionBeginMap = new byte[_size.X, _size.Y];

            lock (StateLock) State = TaskState.Data;

            // Iterate through each degree of longitude (lines from west to east)
#if NETFX4
            Parallel.For(0, _size.Y, y =>
#else
            for (int y = 0; y < _size.Y; y++)
#endif
            {
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
                    _occlusionBeginMap[x1, y] = value;

                    // Iterate through all points east of the current one
                    value = 0;
                    for (int x2 = x1; x2 < _size.X; x2++)
                    {
                        // Draw a line between the two points to determine the angle below which a shadow is cast for rising light sources
                        byte newValue = RiseAngleToByte(GetLightAngle(x1, x2, y));
                        value = Math.Max(value, newValue);
                    }
                    _occlusionEndMap[x1, y] = value;
                }

#if !NETFX4
                UnitsProcessed += _size.Y;
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
            int heightDist = (_heightMap[x2, y] - _heightMap[x1, y]).Clamp(0, 255);
            return Math.Atan2(heightDist * _size.StretchV, xDist * _size.StretchH);
        }

        /// <summary>
        /// Converts the <paramref name="angle"/> to a byte value as follows: 0° = 0, 90° = 255
        /// </summary>
        private static byte RiseAngleToByte(double angle)
        {
            angle = angle.Clamp(0, Math.PI / 2);
            return (byte)(angle / (Math.PI / 2) * 255);
        }

        /// <summary>
        /// Converts the <paramref name="angle"/> to a byte value as follows: 90° = 0, 180° = 255
        /// </summary>
        private static byte SetAngleToByte(double angle)
        {
            angle = angle.Clamp(Math.PI / 2, Math.PI);
            return (byte)((angle / (Math.PI / 2) - 1) * 255);
        }
        #endregion
    }
}
