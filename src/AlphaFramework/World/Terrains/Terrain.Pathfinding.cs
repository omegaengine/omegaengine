/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using AlphaFramework.World.Paths;
using AlphaFramework.World.Positionables;
using LuaInterface;
using SlimDX;

namespace AlphaFramework.World.Terrains
{
    partial class Terrain<TTemplate>
    {
        /// <summary>
        /// The pathfinding engine used on this terrain.
        /// Is <see langword="null"/> until <see cref="SetupPathfinding"/> has been called.
        /// </summary>
        [XmlIgnore]
        public IPathfinder<Vector2> Pathfinder { get; private set; }

        /// <summary>
        /// Initializes the pathfinding engine.
        /// </summary>
        /// <param name="entities">The <see cref="Positionable{TCoordinates}"/>s to consider for obstacles.</param>
        /// <remarks>Is automatically called on first access to the terrain.</remarks>
        [LuaHide]
        public void SetupPathfinding(IEnumerable<Positionable<Vector2>> entities)
        {
            #region Sanity checks
            if (entities == null) throw new ArgumentNullException("entities");
            #endregion

            var blockedMap = new bool[_size.X, _size.Y];

            foreach (var water in entities.OfType<Water>())
            {
                var xStart = (int)Math.Floor(water.Position.X / _size.StretchH);
                var xEnd = (int)Math.Ceiling((water.Position.X + water.Size.X) / _size.StretchH);
                var yStart = (int)Math.Floor(water.Position.Y / _size.StretchH);
                var yEnd = (int)Math.Ceiling((water.Position.Y + water.Size.Y) / _size.StretchH);

                if (xEnd > Size.X - 1) xEnd = Size.X - 1;
                if (yEnd > Size.Y - 1) yEnd = Size.Y - 1;

                for (int x = xStart; x <= xEnd; x++)
                {
                    for (int y = yStart; y <= yEnd; y++)
                        blockedMap[x, y] = (HeightMap[x, y] * Size.StretchV) < water.Height - water.TraversableDepth;
                }
            }

            // TODO: Block on too steep terrain

            Pathfinder = new SimplePathfinder(blockedMap);
        }
    }
}
