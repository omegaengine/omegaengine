/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using AlphaFramework.World.Terrains;
using Common.Undo;
using Common.Values;
using SlimDX;

namespace AlphaFramework.Editor.World.TerrainModifiers
{
    /// <summary>
    /// Abstract base class for interactivley modifying a <see cref="Terrain"/>.
    /// </summary>
    public abstract class Base
    {
        /// <summary>Used to collect data as it was before the modifications.</summary>
        protected readonly ExpandableRectangleArray<byte> OldData = new ExpandableRectangleArray<byte>();

        /// <summary>Used to collect data as it is after the modifcations.</summary>
        protected readonly ExpandableRectangleArray<byte> NewData = new ExpandableRectangleArray<byte>();

        /// <summary>The <see cref="Terrain"/> to modify.</summary>
        protected readonly ITerrain Terrain;

        /// <summary>
        /// Creates a new <see cref="Terrain"/> modifier.
        /// </summary>
        /// <param name="terrain">The <see cref="Terrain"/> to modify.</param>
        protected Base(ITerrain terrain)
        {
            #region Sanity checks
            if (terrain == null) throw new ArgumentNullException("terrain");
            #endregion

            Terrain = terrain;
        }

        /// <summary>
        /// Applies and accumulates a modification to the <see cref="Terrain"/>.
        /// </summary>
        /// <param name="terrainCoords">The center coordinates of the area to modify in world space.</param>
        /// <param name="brush">The shape and size of the area around <paramref name="terrainCoords"/> to modify.</param>
        public abstract void Apply(Vector2 terrainCoords, TerrainBrush brush);

        /// <summary>
        /// Creates a pre-executed undo command representing the accumulated <see cref="Apply"/> calls to this instance.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public abstract IUndoCommand GetCommand();
    }
}
