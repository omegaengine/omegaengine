using System.Collections.Generic;
using World;

namespace AlphaEditor.World.Commands
{
    /// <summary>
    /// Command that adds one or more <see cref="Positionable"/>ies to a <see cref="Universe"/>
    /// </summary>
    internal class AddPositionables : AddRemovePositionables
    {
        #region Constructor
        /// <summary>
        /// Creates a new command for adding one or more <see cref="Positionable"/>ies to a <see cref="Universe"/>
        /// </summary>
        /// <param name="universe">The <see cref="Universe"/> to add to</param>
        /// <param name="entities">The <see cref="Positionable"/>ies to add</param>
        internal AddPositionables(Universe universe, IEnumerable<Positionable> entities) : base(universe, entities)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <summary>
        /// Adds the <see cref="Positionable"/> to the <see cref="Universe"/>
        /// </summary>
        protected override void OnExecute()
        {
            AddPositionables();
        }
        #endregion

        #region Undo
        /// <summary>
        /// Removes the <see cref="Positionable"/> from the <see cref="Universe"/> again
        /// </summary>
        protected override void OnUndo()
        {
            RemovePositionables();
        }
        #endregion
    }
}
