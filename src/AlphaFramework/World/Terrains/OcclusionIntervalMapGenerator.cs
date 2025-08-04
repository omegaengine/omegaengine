/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NanoByte.Common;
using NanoByte.Common.Tasks;
using OmegaEngine.Values;
using SlimDX;
using Resources = AlphaFramework.World.Properties.Resources;

namespace AlphaFramework.World.Terrains;

/// <summary>
/// Generates an occlusion interval map from a height map for a <see cref="ITerrain"/> as a background task.
/// </summary>
/// <seealso cref="ITerrain.OcclusionIntervalMap"/>
public class OcclusionIntervalMapGenerator : TaskBase
{
    #region Variables
    private readonly ByteGrid _heightMap;
    private readonly Vector3[] _rayDirections = new Vector3[256];

    private ByteVector4Grid _result;

    /// <summary>
    /// Returns the calculated occlusion end map array once the calculation is complete.
    /// </summary>
    /// <remarks>A light rise angle is the minimum vertical angle (0 = 0°, 255 = 90°) which a directional light must achieve to be not occluded.</remarks>
    /// <exception cref="InvalidOperationException">The calculation is not complete yet.</exception>
    public ByteVector4Grid Result
    {
        get
        {
            if (State != TaskState.Complete) throw new InvalidOperationException(Resources.CalculationNotComplete);

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
    public OcclusionIntervalMapGenerator(ByteGrid heightMap, float stretchH = 1, float stretchV = 1, double lightSourceInclination = 90)
    {
        _heightMap = heightMap ?? throw new ArgumentNullException(nameof(heightMap));

        // Pre-calculate all ray directions
        lightSourceInclination = lightSourceInclination.DegreeToRadian();
        for (int i = 0; i < _rayDirections.Length; i++)
            _rayDirections[i] = GetRayDirection(lightSourceInclination, (byte)i, stretchH, stretchV);

        UnitsTotal = heightMap.Width * heightMap.Height;
    }

    private static Vector3 GetRayDirection(double inclination, byte angle, float stretchH, float stretchV)
    {
        var direction = VectorMath.UnitVector(inclination, angle.ByteToAngle());
        direction.X /= stretchH;
        direction.Z /= stretchH;
        direction.Y /= stretchV;

        // Normalize on the XZ plane
        direction /= (float)Math.Sqrt(direction.X * direction.X + direction.Z * direction.Z);

        return direction;
    }

    /// <summary>
    /// Prepares to calculate an occlusion interval map for the height-map of a <see cref="ITerrain"/>.
    /// </summary>
    /// <param name="terrain">The <see cref="ITerrain"/> providing the height-map. The height-map is not cloned and must not be modified during calculation!</param>
    /// <param name="lightSourceInclination">The angle of inclination of the sun's path away from the horizon in degrees.</param>
    /// <returns>The newly crated occlusion interval map generator.</returns>
    /// <remarks>The results are not automatically written back to <paramref name="terrain"/>.</remarks>
    public static OcclusionIntervalMapGenerator FromTerrain(ITerrain terrain, double lightSourceInclination)
    {
        #region Sanity checks
        if (terrain == null) throw new ArgumentNullException(nameof(terrain));
        if (!terrain.DataLoaded) throw new InvalidOperationException(Resources.TerrainDataNotLoaded);
        #endregion

        return new(terrain.HeightMap, terrain.Size.StretchH, terrain.Size.StretchV, lightSourceInclination);
    }
    #endregion

    //--------------------//

    #region Thread code
    public ParallelOptions ParallelOptions { get; } = new();

    /// <inheritdoc/>
    public override string Name => Resources.CalculatingShadows;

    /// <inheritdoc/>
    public override bool CanCancel => true;

    /// <inheritdoc/>
    protected override bool UnitsByte => false;

    /// <inheritdoc/>
    protected override void Execute()
    {
        _result = new(_heightMap.Width, _heightMap.Height);

        State = TaskState.Data;

        var progressLock = new object();
        Parallel.For(0, _heightMap.Width, ParallelOptions, x =>
        {
            for (int y = 0; y < _heightMap.Height; y++)
                _result[x, y] = GetOcclusionVector(x, y);

            lock (progressLock) UnitsProcessed += _heightMap.Height;
        }/*, CancellationToken*/);

        State = TaskState.Complete;
    }
    #endregion

    #region Calculation
    // NOTE: Recycles List<> to reduce pressure on garbage collector
    [ThreadStatic]
    private static List<byte> _boundaries;

    private ByteVector4 GetOcclusionVector(int x, int y)
    {
        if (_boundaries == null) _boundaries = [];
        else _boundaries.Clear();

        CalculateBoundaries(x, y);
        while (_boundaries.Count < 4) _boundaries.Add(255);
        if (_boundaries.Count % 2 == 1) _boundaries.Add(255);
        while (_boundaries.Count > 4) RemoveShortestInterval();

        return new(_boundaries[0], _boundaries[1], _boundaries[2], _boundaries[3]);
    }

    private void CalculateBoundaries(int x, int y)
    {
        bool occluded = true;
        byte angle = 0;
        while (true)
        {
            angle = occluded ? FindNextUnoccluded(x, y, angle) : FindNextOccluded(x, y, angle);
            _boundaries.Add(angle);
            if (angle >= 255) break;

            angle++;
            occluded = !occluded;
        }
    }

    private static void RemoveShortestInterval()
    {
        int shortestIndex = 0;
        int shortestDistance = _boundaries[1] - _boundaries[0];
        for (int i = 1; i < _boundaries.Count - 1; i++)
        {
            int distance = _boundaries[i + 1] - _boundaries[i];
            if (distance < shortestDistance)
            {
                shortestIndex = i;
                shortestDistance = distance;
            }
        }

        _boundaries.RemoveAt(shortestIndex + 1);
        _boundaries.RemoveAt(shortestIndex);
    }

    private byte FindNextOccluded(int x, int y, byte startAngle)
    {
        for (byte angle = startAngle; angle < 255; angle++)
            if (IsOccluded(x, y, angle)) return angle;
        return 255;
    }

    private byte FindNextUnoccluded(int x, int y, byte startAngle)
    {
        for (byte angle = startAngle; angle < 255; angle++)
            if (!IsOccluded(x, y, angle)) return angle;
        return 255;
    }

    private bool IsOccluded(int startX, int startY, byte angle)
    {
        var rayStep = _rayDirections[angle];
        var startPoint = new Vector3(startX, SampleHeightMap(startX, startY), startY) + rayStep;

        for (Vector3 rayPoint = startPoint; _heightMap.IsInRange(rayPoint.X, rayPoint.Z); rayPoint += rayStep)
        {
            float height = SampleHeightMap(rayPoint.X, rayPoint.Z);
            if (height > rayPoint.Y) return true;
        }
        return false;
    }

    private float SampleHeightMap(float x, float y)
    {
        return _heightMap.SampledRead(x, y);
    }
    #endregion
}