using System.Collections.Generic;
using Common.Undo;
using Common.Values;
using World;

namespace AlphaEditor.World.Commands
{
    /// <summary>
    /// An undo command that handles moved <see cref="Positionable"/>s
    /// </summary>
    public class MovePositionables : SimpleCommand
    {
        #region Variables
        // Note: Use List<> instead of Array, because the size of the incoming IEnumerable<> will be unkown
        private readonly List<Positionable> _positionables = new List<Positionable>();

        private readonly DoubleVector3[] _oldPositions;
        private readonly DoubleVector3 _newPosition;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the command after the <see cref="Positionable"/>s were first moved.
        /// </summary>
        /// <param name="positionables">The <see cref="Positionable"/>s to be moved.</param>
        /// <param name="target">The terrain position to move the entities to.</param>
        public MovePositionables(IEnumerable<Positionable> positionables, DoubleVector3 target)
        {
            // Create local defensive copy of entities
            _positionables = new List<Positionable>(positionables);

            // Create array based on collection size to backup old positions
            _oldPositions = new DoubleVector3[_positionables.Count];
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
