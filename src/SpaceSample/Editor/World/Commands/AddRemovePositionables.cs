using System.Collections.Generic;
using Common.Undo;
using World;

namespace AlphaEditor.World.Commands
{
    /// <summary>
    /// Command that adds/removes one or more <see cref="Positionable"/>ies to/from a <see cref="Universe"/>
    /// </summary>
    internal abstract class AddRemovePositionables : SimpleCommand
    {
        #region Variables
        private readonly Universe _universe;

        // Note: Use List<> instead of Array, because the size of the incoming IEnumerable<> will be unkown
        private readonly List<Positionable> _positionables;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command for adding/removing one or more <see cref="Positionable"/>ies to/from a <see cref="Universe"/>
        /// </summary>
        /// <param name="universe">The <see cref="Universe"/> to add to / remove from</param>
        /// <param name="entities">The <see cref="Positionable"/>s to add/remove</param>
        protected AddRemovePositionables(Universe universe, IEnumerable<Positionable> entities)
        {
            _universe = universe;

            // Create local defensive copy of entities
            _positionables = new List<Positionable>(entities);
        }
        #endregion

        //--------------------//

        #region Add/remove helpers
        /// <summary>
        /// Removes the entities from the universe
        /// </summary>
        protected void AddPositionables()
        {
            foreach (Positionable positionable in _positionables)
                _universe.Positionables.Add(positionable);
        }

        /// <summary>
        /// Adds the entities to the universe
        /// </summary>
        protected void RemovePositionables()
        {
            foreach (Positionable positionable in _positionables)
                _universe.Positionables.Remove(positionable);
        }
        #endregion
    }
}
