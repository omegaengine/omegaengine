using System.Collections.Generic;
using World;

namespace AlphaEditor.World.Commands
{
    /// <summary>
    /// Command that removes one or more <see cref="Positionable"/>ies from a <see cref="Universe"/>
    /// </summary>
    internal class RemovePositionables : AddRemovePositionables
    {
        #region Constructor
        /// <summary>
        /// Creates a new command for removing one or more <see cref="Positionable"/>ies from a <see cref="Universe"/>
        /// </summary>
        /// <param name="universe">The <see cref="Universe"/> to remove from</param>
        /// <param name="entities">The <see cref="Positionable"/>ies to remove</param>
        internal RemovePositionables(Universe universe, IEnumerable<Positionable> entities) : base(universe, entities)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <summary>
        /// Removes the <see cref="Positionable"/> from the <see cref="Universe"/>
        /// </summary>
        protected override void OnExecute()
        {
            RemovePositionables();
        }
        #endregion

        #region Undo
        /// <summary>
        /// Adds the <see cref="Positionable"/> back to the <see cref="Universe"/>
        /// </summary>
        protected override void OnUndo()
        {
            AddPositionables();
        }
        #endregion
    }
}
