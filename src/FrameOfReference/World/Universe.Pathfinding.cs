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
using System.Linq;
using AlphaFramework.World;
using AlphaFramework.World.Components;
using AlphaFramework.World.Paths;
using AlphaFramework.World.Positionables;
using AlphaFramework.World.Terrains;
using FrameOfReference.World.Positionables;
using NanoByte.Common;
using NanoByte.Common.Utils;
using SlimDX;

namespace FrameOfReference.World
{
    partial class Universe
    {
        private int _maxTraversableSlope = 10;

        /// <summary>
        /// The maximum slope the <see cref="IPathfinder{TCoordinates}"/> considers traversable.
        /// </summary>
        [DefaultValue(10), Category("Gameplay"), Description("The maximum slope the Pathfinder considers traversable.")]
        public int MaxTraversableSlope { get { return _maxTraversableSlope; } set { Math.Abs(value).To(ref _maxTraversableSlope, InitializePathfinding); } }

        #region Initialize
        /// <summary>
        /// Initializes the <see cref="UniverseBase{TCoordinates}.Pathfinder"/> engine.
        /// </summary>
        /// <remarks>Is usually called automatically when needed.</remarks>
        private void InitializePathfinding()
        {
            if (Terrain == null) return;

            var obstructionMap = new bool[Terrain.Size.X, Terrain.Size.Y];
            MarkUntraversableWaters(obstructionMap);
            Terrain.MarkUntraversableSlopes(obstructionMap, MaxTraversableSlope);
            MarkUnmoveableEntities(obstructionMap);
            Pathfinder = new SimplePathfinder(obstructionMap);
        }

        /// <summary>
        /// Marks all nodes blocked by <see cref="Entity"/>s with no <see cref="Movement"/>.
        /// </summary>
        private void MarkUnmoveableEntities(bool[,] obstructionMap)
        {
            foreach (var entity in Positionables.OfType<Entity>().Where(x => x.TemplateData.Movement == null && x.TemplateData.Collision != null))
            {
                for (int x = 0; x < obstructionMap.GetLength(0); x++)
                {
                    for (int y = 0; y < obstructionMap.GetLength(1); y++)
                        obstructionMap[x, y] |= entity.CollisionTest(new Vector2(x, y) * Terrain.Size.StretchH);
                }
            }
        }

        /// <summary>
        /// Marks all nodes under <see cref="Water"/> deeper than <see cref="Water.TraversableDepth"/>
        /// </summary>
        private void MarkUntraversableWaters(bool[,] obstructionMap)
        {
            foreach (var water in Positionables.OfType<Water>())
            {
                var xStart = (int)Math.Floor(water.Position.X / Terrain.Size.StretchH);
                var xEnd = (int)Math.Ceiling((water.Position.X + water.Size.X) / Terrain.Size.StretchH);
                var yStart = (int)Math.Floor(water.Position.Y / Terrain.Size.StretchH);
                var yEnd = (int)Math.Ceiling((water.Position.Y + water.Size.Y) / Terrain.Size.StretchH);

                if (xEnd > Terrain.Size.X - 1) xEnd = Terrain.Size.X - 1;
                if (yEnd > Terrain.Size.Y - 1) yEnd = Terrain.Size.Y - 1;

                for (int x = xStart; x <= xEnd; x++)
                {
                    for (int y = yStart; y <= yEnd; y++)
                        obstructionMap[x, y] |= (Terrain.HeightMap[x, y] * Terrain.Size.StretchV) < (water.Height - water.TraversableDepth);
                }
            }
        }
        #endregion

        #region Movement
        /// <summary>
        /// Makes an <see cref="Entity"/> move towards a <paramref name="target"/> using pathfinding.
        /// </summary>
        private void StartMoving(Entity entity, Vector2 target)
        {
            #region Sanity checks
            if (entity == null) throw new ArgumentNullException("entity");
            #endregion

            if (Pathfinder == null)
            {
                Log.Warn("Pathfinder not initialized");
                return;
            }
            if (entity.TemplateData.Movement == null)
            {
                Log.Warn(entity + " has no Movement component");
                return;
            }
            if (entity.Position == target) return;
            if (entity.CurrentPath != null && entity.CurrentPath.Target == target && entity.CurrentPath.PathNodes.Count != 0) return;

            using (new TimedLogEvent("Calculating path from " + entity.Position + " to " + target))
            {
                // Get path and cancel if none was found
                var pathNodes = Pathfinder.FindPath(GetScaledPosition(entity.Position), GetScaledPosition(target));
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
        }

        /// <summary>
        /// Applies <see cref="TerrainSize"/> scaling to a position.
        /// </summary>
        private Vector2 GetScaledPosition(Vector2 position)
        {
            return position * (1.0f / Terrain.Size.StretchH);
        }
        #endregion

        #region Waypoints
        /// <summary>
        /// Moves <see cref="Waypoint"/> from <see cref="UniverseBase{TCoordinates}.Positionables"/> to <see cref="Entity.Waypoints"/>.
        /// Call to prepare for gameplay.
        /// </summary>
        private void WrapWaypoints()
        {
            var toRemove = new List<Waypoint>();
            foreach (var waypoint in Positionables.OfType<Waypoint>().OrderBy(x => x.ActivationTime))
            {
                var entity = GetEntity(waypoint.TargetEntity);
                if (entity != null)
                {
                    entity.Waypoints.Add(waypoint);
                    toRemove.Add(waypoint);
                }
            }
            foreach (var waypoint in toRemove) Positionables.Remove(waypoint);
        }

        /// <summary>
        /// Moves <see cref="Waypoint"/> from <see cref="Entity.Waypoints"/> to <see cref="UniverseBase{TCoordinates}.Positionables"/>.
        /// Call to prepare for editing.
        /// </summary>
        public void UnwrapWaypoints()
        {
            foreach (var entity in Positionables.OfType<Entity>().ToList())
            {
                Positionables.AddMany(entity.Waypoints.Cast<Positionable<Vector2>>());
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
            Log.Info("Recorded arrival time for " + entity.Name + " at " + waypoint.Name + ": " + GameTime);
        }

        private static void RecordOriginPosition(Entity entity, Waypoint waypoint)
        {
            if (waypoint.OriginPositionSpecified) return;

            waypoint.OriginPosition = entity.Position;
            Log.Info("Recorded origin position for " + entity.Name + " towards " + waypoint.Name + ": " + entity.Position);
        }
        #endregion
    }
}
