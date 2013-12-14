/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using Common.Undo;
using TemplateWorld;
using TemplateWorld.Positionables;

namespace AlphaEditor.World.Commands
{
    /// <summary>
    /// Adds/removes one or more <see cref="Positionable{TCoordinates}"/>ies to/from a <see cref="UniverseBase{TCoordinates}"/>.
    /// </summary>
    public abstract class AddRemovePositionables<TCoordinates> : SimpleCommand
        where TCoordinates : struct
    {
        #region Variables
        private readonly UniverseBase<TCoordinates> _universe;

        // Note: Use List<> instead of Array, because the size of the incoming IEnumerable<> will be unkown
        private readonly List<Positionable<TCoordinates>> _positionables;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command for adding/removing one or more <see cref="Positionable{TCoordinates}"/>ies to/from a <see cref="UniverseBase{TCoordinates}"/>.
        /// </summary>
        /// <param name="universe">The <see cref="UniverseBase{TCoordinates}"/> to add to / remove from.</param>
        /// <param name="positionables">The <see cref="Positionable{TCoordinates}"/>s to add/remove.</param>
        protected AddRemovePositionables(UniverseBase<TCoordinates> universe, IEnumerable<Positionable<TCoordinates>> positionables)
        {
            #region Sanity checks
            if (universe == null) throw new ArgumentNullException("universe");
            if (positionables == null) throw new ArgumentNullException("positionables");
            #endregion

            _universe = universe;

            // Create local defensive copy of entities
            _positionables = new List<Positionable<TCoordinates>>(positionables);
        }
        #endregion

        //--------------------//

        #region Add/remove helpers
        /// <summary>
        /// Removes the entities from the universe
        /// </summary>
        protected void AddPositionables()
        {
            foreach (var positionable in _positionables)
                _universe.Positionables.Add(positionable);
        }

        /// <summary>
        /// Adds the entities to the universe
        /// </summary>
        protected void RemovePositionables()
        {
            foreach (var positionable in _positionables)
                _universe.Positionables.Remove(positionable);
        }
        #endregion
    }
}
