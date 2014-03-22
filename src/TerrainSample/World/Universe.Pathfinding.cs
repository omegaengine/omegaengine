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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AlphaFramework.World;
using AlphaFramework.World.Paths;
using AlphaFramework.World.Positionables;
using AlphaFramework.World.Terrains;
using Common;
using Common.Utils;
using SlimDX;
using TerrainSample.World.Positionables;

namespace TerrainSample.World
{
    partial class Universe
    {
        private int _maxTraversableSlope = 10;

        /// <summary>
        /// The maximum slope the <see cref="IPathfinder{TCoordinates}"/> considers traversable.
        /// </summary>
        [DefaultValue(10), Category("Gameplay"), Description("The maximum slope the Pathfinder considers traversable.")]
        public int MaxTraversableSlope { get { return _maxTraversableSlope; } set { Math.Abs(value).To(ref _maxTraversableSlope, SetupPathfinding); } }

        #region Setup
        /// <summary>
        /// Initializes the <see cref="UniverseBase{TCoordinates}.Pathfinder"/> engine.
        /// </summary>
        /// <remarks>Is usually called automatically when needed.</remarks>
        public void SetupPathfinding()
        {
            if (Terrain == null) return;

            var obstructionMap = new bool[Terrain.Size.X, Terrain.Size.Y];
            MarkUntraversableWaters(obstructionMap);
            Terrain.MarkUntraversableSlopes(obstructionMap, _maxTraversableSlope);
            Pathfinder = new SimplePathfinder(obstructionMap);

            RecalcPaths();
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

        /// <summary>
        /// Recalculates all cached pathfinding results.
        /// </summary>
        private void RecalcPaths()
        {
            foreach (var entity in Positionables.OfType<Entity>())
            {
                var pathLeader = entity.PathControl as PathLeader<Vector2>;
                if (pathLeader != null)
                    PathfindEntity(entity, pathLeader.Target);
            }

            Update(0);
        }
        #endregion

        /// <summary>
        /// Makes an <see cref="Entity"/> move towards a <paramref name="target"/> using pathfinding.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void PathfindEntity(Entity entity, Vector2 target)
        {
            #region Sanity checks
            if (entity == null) throw new ArgumentNullException("entity");
            #endregion

            if (Pathfinder == null)
            {
                Log.Warn("Pathfinder not initialized");
                return;
            }

            // Get path and cancel if none was found
            var pathNodes = Pathfinder.FindPath(GetScaledPosition(entity.Position), GetScaledPosition(target));
            if (pathNodes == null)
            {
                entity.PathControl = null;
                return;
            }

            // Store path data in entity
            var pathLeader = new PathLeader<Vector2> {ID = 0, Target = target};
            foreach (var node in pathNodes)
                pathLeader.PathNodes.Enqueue(node * Terrain.Size.StretchH);
            entity.PathControl = pathLeader;
        }

        /// <summary>
        /// Applies <see cref="TerrainSize"/> scaling to a position.
        /// </summary>
        private Vector2 GetScaledPosition(Vector2 position)
        {
            return position * (1.0f / Terrain.Size.StretchH);
        }
    }
}
