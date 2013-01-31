/*
 * Copyright 2006-2013 Bastian Eicher
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
using Common.Undo;
using SlimDX;
using World;

namespace AlphaEditor.World.Commands
{
    /// <summary>
    /// Moves one or more <see cref="Positionable"/>s.
    /// </summary>
    public class MovePositionables : SimpleCommand
    {
        #region Variables
        // Note: Use List<> instead of Array, because the size of the incoming IEnumerable<> will be unkown
        private readonly List<Positionable> _positionables = new List<Positionable>();

        private readonly Vector2[] _oldPositions;
        private readonly Vector2 _newPosition;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command for moving one or more <see cref="Positionable"/>s.
        /// </summary>
        /// <param name="positionables">The <see cref="Positionable"/>s to be moved.</param>
        /// <param name="target">The terrain position to move the entities to.</param>
        public MovePositionables(IEnumerable<Positionable> positionables, Vector2 target)
        {
            #region Sanity checks
            if (positionables == null) throw new ArgumentNullException("positionables");
            #endregion

            // Create local defensive copy of entities
            _positionables = new List<Positionable>(positionables);

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
        /// Set the changed <see cref="Positionable.Position"/>s.
        /// </summary>
        protected override void OnExecute()
        {
            // ToDo: Perform grid-alignment
            foreach (Positionable positionable in _positionables)
                positionable.Position = _newPosition;
        }

        /// <summary>
        /// Restore the original <see cref="Positionable.Position"/>s.
        /// </summary>
        protected override void OnUndo()
        {
            for (int i = 0; i < _positionables.Count; i++)
                _positionables[i].Position = _oldPositions[i];
        }
        #endregion
    }
}
