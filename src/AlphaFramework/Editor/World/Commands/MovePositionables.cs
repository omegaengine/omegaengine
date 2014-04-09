/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using AlphaFramework.World.Positionables;
using NanoByte.Common.Undo;
using SlimDX;

namespace AlphaFramework.Editor.World.Commands
{
    /// <summary>
    /// Moves one or more <see cref="Positionable{TCoordinates}"/>s.
    /// </summary>
    public class MovePositionables : SimpleCommand
    {
        #region Variables
        // Note: Use List<> instead of Array, because the size of the incoming IEnumerable<> will be unkown
        private readonly List<Positionable<Vector2>> _positionables = new List<Positionable<Vector2>>();

        private readonly Vector2[] _oldPositions;
        private readonly Vector2 _newPosition;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command for moving one or more <see cref="Positionable{TCoordinates}"/>s.
        /// </summary>
        /// <param name="positionables">The <see cref="Positionable{TCoordinates}"/>s to be moved.</param>
        /// <param name="target">The terrain position to move the entities to.</param>
        public MovePositionables(IEnumerable<Positionable<Vector2>> positionables, Vector2 target)
        {
            #region Sanity checks
            if (positionables == null) throw new ArgumentNullException("positionables");
            #endregion

            // Create local defensive copy of entities
            _positionables = new List<Positionable<Vector2>>(positionables);

            // Create array based on collection size to backup old positions
            _oldPositions = new Vector2[_positionables.Count];
            for (int i = 0; i < _positionables.Count; i++)
                _oldPositions[i] = _positionables[i].Position;

            _newPosition = target;
        }
        #endregion

        //--------------------//

        #region Undo / Redo
        /// <summary>
        /// Set the changed <see cref="Positionable{TCoordinates}.Position"/>s.
        /// </summary>
        protected override void OnExecute()
        {
            // ToDo: Perform grid-alignment
            foreach (var positionable in _positionables)
                positionable.Position = _newPosition;
        }

        /// <summary>
        /// Restore the original <see cref="Positionable{TCoordinates}.Position"/>s.
        /// </summary>
        protected override void OnUndo()
        {
            for (int i = 0; i < _positionables.Count; i++)
                _positionables[i].Position = _oldPositions[i];
        }
        #endregion
    }
}
