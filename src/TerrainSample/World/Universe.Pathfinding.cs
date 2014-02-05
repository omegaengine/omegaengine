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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AlphaFramework.World.Paths;
using AlphaFramework.World.Terrains;
using SlimDX;
using TerrainSample.World.Positionables;

namespace TerrainSample.World
{
    partial class Universe
    {
        /// <summary>
        /// Recalculates all cached pathfinding results.
        /// </summary>
        /// <remarks>This needs to be called when new obstacles have appeared or when a savegame was loaded (which does not store paths).</remarks>
        public void RecalcPaths()
        {
            foreach (var entity in Positionables.OfType<Entity>())
            {
                var pathLeader = entity.PathControl as PathLeader<Vector2>;
                if (pathLeader != null)
                    PathfindEntity(entity, pathLeader.Target);
            }
        }

        /// <summary>
        /// Makes an <see cref="Entity"/> move towards a <paramref name="target"/> using pathfinding.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void PathfindEntity(Entity entity, Vector2 target)
        {
            #region Sanity checks
            if (entity == null) throw new ArgumentNullException("entity");
            #endregion

            // Get path and cancel if none was found
            var pathNodes = Terrain.Pathfinder.FindPathPlayer(GetScaledPosition(entity.Position), GetScaledPosition(target));
            if (pathNodes == null)
            {
                entity.PathControl = null;
                return;
            }

            // Store path data in entity
            var pathLeader = new PathLeader<Vector2> {ID = 0, Target = target};
            foreach (var node in pathNodes)
                pathLeader.PathNodes.Push(node * Terrain.Size.StretchH);
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
