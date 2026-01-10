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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AlphaFramework.World;
using AlphaFramework.World.Components;
using AlphaFramework.World.Paths;
using AlphaFramework.World.Positionables;
using AlphaFramework.World.Terrains;
using FrameOfReference.World.Positionables;
using NanoByte.Common;
using SlimDX;

namespace FrameOfReference.World;

partial class Universe
{
    /// <summary>
    /// The pathfinding engine used to navigate <see cref="Positionables"/>.
    /// </summary>
    [Browsable(false)]
    private IPathfinder<Vector2>? _pathfinder;

    private int _maxTraversableSlope = 10;

    /// <summary>
    /// The maximum slope the <see cref="IPathfinder{TCoordinates}"/> considers traversable.
    /// </summary>
    [DefaultValue(10), Category("Gameplay"), Description("The maximum slope the Pathfinder considers traversable.")]
    public int MaxTraversableSlope { get => _maxTraversableSlope; set => Math.Abs(value).To(ref _maxTraversableSlope, ResetPathfinding); }

    #region Initialize
    /// <summary>
    /// Initializes the <see cref="IPathfinder{TCoordinates}"/> engine.
    /// </summary>
    [MemberNotNull(nameof(_pathfinder))]
    private void InitializePathfinding()
    {
        var obstructionMap = new bool[Terrain.Size.X, Terrain.Size.Y];
        MarkUntraversableWaters(obstructionMap);
        Terrain.MarkUntraversableSlopes(obstructionMap, MaxTraversableSlope);
        MarkUnmoveableEntities(obstructionMap);

        _pathfinder = new SimplePathfinder(obstructionMap);
    }

    /// <summary>
    /// Sets the <see cref="IPathfinder{TCoordinates}"/> engine to <c>null</c>.
    /// </summary>
    public void ResetPathfinding()
    {
        _pathfinder = null;
    }

    /// <summary>
    /// Marks all nodes blocked by <see cref="Entity"/>s with no <see cref="Movement"/>.
    /// </summary>
    private void MarkUnmoveableEntities(bool[,] obstructionMap)
    {
        #region Sanity checks
        if (Terrain == null) throw new InvalidOperationException("Terrain data not loaded.");
        #endregion

        using var _ = new TimedLogEvent("Pathfinding: Marking unmovable entities");

        int lengthX = obstructionMap.GetLength(0);
        int lengthY = obstructionMap.GetLength(1);

        Positionables.OfType<Entity>().Where(x => x.IsObstacle).AsParallel().ForAll(entity =>
        {
            for (int x = 0; x < lengthX; x++)
            for (int y = 0; y < lengthY; y++)
            {
                if (entity.CollisionTest(new Vector2(x, y) * Terrain.Size.StretchH))
                    obstructionMap[x, y] = true;
            }
        });
    }

    /// <summary>
    /// Marks all nodes under <see cref="Water"/> deeper than <see cref="Water.TraversableDepth"/>
    /// </summary>
    private void MarkUntraversableWaters(bool[,] obstructionMap)
    {
        #region Sanity checks
        if (Terrain is not {DataLoaded: true}) throw new InvalidOperationException("Terrain data not loaded.");
        #endregion

        using var _ = new TimedLogEvent("Pathfinding: Marking untraversable waters");

        Positionables.OfType<Water>().AsParallel().ForAll(water =>
        {
            int xStart = (int)Math.Floor(water.Position.X / Terrain.Size.StretchH);
            int xEnd = (int)Math.Ceiling((water.Position.X + water.Size.X) / Terrain.Size.StretchH);
            int yStart = (int)Math.Floor(water.Position.Y / Terrain.Size.StretchH);
            int yEnd = (int)Math.Ceiling((water.Position.Y + water.Size.Y) / Terrain.Size.StretchH);

            if (xEnd > Terrain.Size.X - 1) xEnd = Terrain.Size.X - 1;
            if (yEnd > Terrain.Size.Y - 1) yEnd = Terrain.Size.Y - 1;

            for (int x = xStart; x <= xEnd; x++)
            for (int y = yStart; y <= yEnd; y++)
            {
                if (Terrain.HeightMap[x, y] * Terrain.Size.StretchV < water.Height - water.TraversableDepth)
                    obstructionMap[x, y] = true;
            }
        });
    }
    #endregion

    #region Movement
    /// <summary>
    /// Makes an <see cref="Entity"/> move towards a <paramref name="target"/> using pathfinding.
    /// </summary>
    private void StartMoving(Entity entity, Vector2 target)
    {
        #region Sanity checks
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (Terrain == null) throw new InvalidOperationException("Terrain data not loaded.");
        #endregion

        if (entity.TemplateData?.Movement == null)
        {
            Log.Warn($"{entity} has no Movement component");
            return;
        }
        if (Terrain == null)
        {
            Log.Warn("Terrain data not loaded.");
            return;
        }
        if (_pathfinder == null)
        {
            Log.Warn("Initializing pathfinding just-in-time");
            InitializePathfinding();
        }

        if (entity.Position == target) return;
        if (entity.CurrentPath != null && entity.CurrentPath.Target == target && entity.CurrentPath.PathNodes.Count != 0) return;

        using var _ = new TimedLogEvent($"Calculating path from {entity.Position} to {target}");

        // Get path and cancel if none was found
        var pathNodes = _pathfinder.FindPath(GetScaledPosition(entity.Position), GetScaledPosition(target));
        if (pathNodes == null)
        {
            entity.CurrentPath = null;
            return;
        }

        // Store path data in entity
        var pathLeader = new StoredPath<Vector2> {Target = target};
        foreach (var node in pathNodes)
            pathLeader.PathNodes.Enqueue(node * Terrain.Size.StretchH);
        entity.CurrentPath = pathLeader;
    }

    /// <summary>
    /// Applies <see cref="TerrainSize"/> scaling to a position.
    /// </summary>
    private Vector2 GetScaledPosition(Vector2 position)
    {
        #region Sanity checks
        if (Terrain == null) throw new InvalidOperationException("Terrain data not loaded.");
        #endregion

        return position * (1.0f / Terrain.Size.StretchH);
    }
    #endregion

    #region Waypoints
    /// <summary>
    /// Moves <see cref="Waypoint"/> from <see cref="CoordinateUniverse{TCoordinates}.Positionables"/> to <see cref="Entity.Waypoints"/>.
    /// Call to prepare for gameplay.
    /// </summary>
    private void WrapWaypoints()
    {
        var toRemove = new List<Waypoint>();
        foreach (var waypoint in Positionables.OfType<Waypoint>().OrderBy(x => x.ActivationTime))
        {
            if (waypoint.TargetEntity != null && GetEntity(waypoint.TargetEntity) is {} entity)
            {
                entity.Waypoints.Add(waypoint);
                toRemove.Add(waypoint);
            }
        }
        foreach (var waypoint in toRemove) Positionables.Remove(waypoint);
    }

    /// <summary>
    /// Moves <see cref="Waypoint"/> from <see cref="Entity.Waypoints"/> to <see cref="CoordinateUniverse{TCoordinates}.Positionables"/>.
    /// Call to prepare for editing.
    /// </summary>
    public void UnwrapWaypoints()
    {
        foreach (var entity in Positionables.OfType<Entity>().ToList())
        {
            Positionables.AddMany(entity.Waypoints);
            entity.Waypoints.Clear();
        }
    }

    /// <summary>
    /// Handles <see cref="Entity.Waypoints"/>.
    /// </summary>
    private void HandleWaypoints(Entity entity, double elapsedGameTime)
    {
        int index = entity.GetCurrentWaypointIndex(GameTime);

        if (index != - 1)
        {
            var waypoint = entity.Waypoints[index];

            if (elapsedGameTime > 0)
            {
                if (index == entity.ActiveWaypointIndex)
                {
                    if (entity.CurrentPath == null)
                    {
                        RecordArrivalTime(entity, waypoint);
                        entity.ActiveWaypointIndex = -1;
                    }
                }
                else RecordOriginPosition(entity, waypoint);
            }

            StartMoving(entity,
                target: (elapsedGameTime >= 0) ? waypoint.Position : waypoint.OriginPosition);
        }

        entity.ActiveWaypointIndex = index;
    }

    private void RecordArrivalTime(Entity entity, Waypoint waypoint)
    {
        if (waypoint.ArrivalTimeSpecified) return;

        waypoint.ArrivalTime = GameTime;
        Log.Info($"Recorded arrival time for {entity.Name} at {waypoint.Name}: {GameTime}");
    }

    private static void RecordOriginPosition(Entity entity, Waypoint waypoint)
    {
        if (waypoint.OriginPositionSpecified) return;

        waypoint.OriginPosition = entity.Position;
        Log.Info($"Recorded origin position for {entity.Name} towards {waypoint.Name}: {entity.Position}");
    }
    #endregion
}
