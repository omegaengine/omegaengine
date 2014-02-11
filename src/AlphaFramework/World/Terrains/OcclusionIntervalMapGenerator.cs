/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using Common;
using Common.Tasks;
using Common.Utils;
using Common.Values;
using Resources = AlphaFramework.World.Properties.Resources;

namespace AlphaFramework.World.Terrains
{
    /// <summary>
    /// Generates an occlusion interval map from a height map for a <see cref="ITerrain"/> as a background task.
    /// </summary>
    /// <seealso cref="ITerrain.OcclusionIntervalMap"/>
    public class OcclusionIntervalMapGenerator : ThreadTask
    {
        #region Variables
        private readonly ByteGrid _heightMap;
        private readonly float _lightSourceInclination;
        private readonly float _stretchH;
        private readonly float _stretchV;
        #endregion

        #region Properties
        /// <inheritdoc />
        public override string Name { get { return Resources.CalculatingShadows; } }

        /// <inheritdoc />
        public override bool UnitsByte { get { return false; } }

        /// <inheritdoc />
        public override bool CanCancel { get { return Parallel.ThreadsCount == 1; } }

        private ByteVector4Grid _result;

        /// <summary>
        /// Returns the calculated occlusion end map array once <see cref="ITask.State"/> has reached <see cref="TaskState.Complete"/>.
        /// </summary>
        /// <remarks>A light rise angles is the minimum vertical angle (0 = 0°, 255 = 90°) which a directional light must achieve to be not occluded.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="ITask.State"/> is not <see cref="TaskState.Complete"/>.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "For performance reasons this property provides direct access to the underlying array without any cloning involved")]
        public ByteVector4Grid Result
        {
            get
            {
                lock (StateLock)
                {
                    if (State != TaskState.Complete) throw new InvalidOperationException(Resources.CalculationNotComplete);
                }

                return _result;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares to calculate an occlusion interval map for a height-map.
        /// </summary>
        /// <param name="heightMap">The height-map data. This is not cloned and must not be modified during calculation!</param>
        /// <param name="stretchH">A factor by which the terrain is horizontally stretched.</param>
        /// <param name="stretchV">A factor by which the terrain is vertically stretched.</param>
        /// <param name="lightSourceInclination">The angle of inclination of the sun's path away from the horizon in degrees.</param>
        public OcclusionIntervalMapGenerator(ByteGrid heightMap, float stretchH = 1, float stretchV = 1, float lightSourceInclination = 90)
        {
            #region Sanity chekcs
            if (heightMap == null) throw new ArgumentNullException("heightMap");
            #endregion

            _heightMap = heightMap;
            _lightSourceInclination = lightSourceInclination.DegreeToRadian();
            _stretchH = stretchH;
            _stretchV = stretchV;

            UnitsTotal = heightMap.Width * heightMap.Height;
        }

        /// <summary>
        /// Prepares to calculate an occlusion interval map for the height-map of a <see cref="ITerrain"/>.
        /// </summary>
        /// <param name="terrain">The <see cref="ITerrain"/> providing the height-map. The height-map is not cloned and must not be modified during calculation!</param>
        /// <param name="lightSourceInclination">The angle of inclination of the sun's path away from the zenith in degrees.</param>
        /// <returns>The newly crated occlusion interval map generator.</returns>
        /// <remarks>The results are not automatically written back to <paramref name="terrain"/>.</remarks>
        public static OcclusionIntervalMapGenerator FromTerrain(ITerrain terrain, float lightSourceInclination)
        {
            #region Sanity checks
            if (terrain == null) throw new ArgumentNullException("terrain");
            #endregion

            return new OcclusionIntervalMapGenerator(terrain.HeightMap, terrain.Size.StretchH, terrain.Size.StretchV, lightSourceInclination);
        }
        #endregion

        //--------------------//

        #region Thread code
        /// <inheritdoc />
        protected override void RunTask()
        {
            _result = new ByteVector4Grid(_heightMap.Width, _heightMap.Height);

            lock (StateLock) State = TaskState.Data;

            // Iterate through each degree of longitude (lines from west to east)
            Parallel.For(0, _heightMap.Height, y =>
            {
                // Iterate through all points along the line from west to east
                for (int x = 0; x < _heightMap.Width; x++)
                    _result[x, y] = GetOcclusionAngles(x, y);

                lock (StateLock) UnitsProcessed += _heightMap.Width;
                if (Parallel.ThreadsCount == 1 && CancelRequest.WaitOne(0)) throw new OperationCanceledException();
            });

            lock (StateLock) State = TaskState.Complete;
        }
        #endregion

        #region Calculation
        private ByteVector4 GetOcclusionAngles(int x, int y)
        {
            return new ByteVector4(GetRiseAngleByte(x, y), GetSetAngleByte(x, y), 255, 255);
        }

        private byte GetRiseAngleByte(int x1, int y)
        {
            // Iterate through all points east of the current one
            byte value2 = 0;
            for (int x = x1; x < _heightMap.Width; x++)
            {
                // Draw a line between the two points to determine the angle below which a shadow is cast for rising light sources
                byte newValue = GetAngle(x1, x, y).AngleToByte();
                value2 = Math.Max(value2, newValue);
            }
            return value2;
        }

        private byte GetSetAngleByte(int x1, int y)
        {
            // Iterate through all points west of the current one
            byte value1 = 255;
            for (int x = 0; x < x1; x++)
            {
                // Draw a line between the two points to determine the angle above which a shadow is cast for setting light sources
                byte newValue = GetAngle(x1, x, y).AngleToByte();
                value1 = Math.Min(value1, newValue);
            }
            return value1;
        }

        /// <summary>
        /// Calculates the angle between two points along the same Y-axis (degree of longitude) on the height-map.
        /// </summary>
        /// <param name="x1">The X-coordinate of the first point.</param>
        /// <param name="x2">The X-coordinate of the second point.</param>
        /// <param name="y">The shared Y-coordinate.</param>
        /// <returns>An angle in radians moving counter-clockwise assuming <paramref name="x2"/> is larger than <paramref name="x1"/>.</returns>
        private double GetAngle(int x1, int x2, int y)
        {
            int xDist = x2 - x1;
            float heightDist = (_heightMap[x2, y] - _heightMap[x1, y]).Clamp(0, 255);
            return Math.Atan2(heightDist * _stretchV, xDist * _stretchH);
        }
        #endregion
    }
}
