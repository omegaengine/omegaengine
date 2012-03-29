using System.Collections.Generic;
using Common.Undo;
using World;

namespace AlphaEditor.World.Commands
{
    /// <summary>
    /// Command that changes the <see cref="Entity.TemplateName"/> property of one or more <see cref="Entity"/>s
    /// </summary>
    public class ChangeEntityTemplates : SimpleCommand
    {
        #region Variables
        private readonly List<Entity> _enti;
        private readonly string[] _oldTemplates;
        private readonly string _newTemplates;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command for changing the <see cref="EntityTemplate"/> of one or more <see cref="Entity"/>s.
        /// </summary>
        /// <param name="entities">The <see cref="Entity"/>s to be modified</param>
        /// <param name="Template">The name of the new <see cref="EntityTemplate"/> to set</param>
        public ChangeEntityTemplates(IEnumerable<Entity> entities, string Template)
        {
            // Create local defensive copy of entities
            _enti = new List<Entity>(entities);

            // Backup the old class names
            _oldTemplates = new string[_enti.Count];
            for (int i = 0; i < _enti.Count; i++)
                _oldTemplates[i] = _enti[i].TemplateName;

            _newTemplates = Template;
        }
        #endregion

        //--------------------//

        #region Undo / Redo
        /// <inheritdoc />
        protected override void OnExecute()
        {
            foreach (Entity entity in _enti)
                entity.TemplateName = _newTemplates;
        }

        /// <inheritdoc />
        protected override void OnUndo()
        {
            for (int i = 0; i < _enti.Count; i++)
                _enti[i].TemplateName = _oldTemplates[i];
        }
        #endregion
    }
}
