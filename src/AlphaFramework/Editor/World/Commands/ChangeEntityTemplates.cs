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
using AlphaFramework.World.Templates;
using NanoByte.Common.Undo;

namespace AlphaFramework.Editor.World.Commands
{
    /// <summary>
    /// Changes the <see cref="ITemplated.TemplateName"/> property of one or more <see cref="EntityBase{TCoordinates,TTemplate}"/>s.
    /// </summary>
    public class ChangeEntityTemplates : SimpleCommand
    {
        #region Variables
        private readonly List<ITemplated> _entities;
        private readonly string[] _oldTemplates;
        private readonly string _newTemplates;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command for changing the <see cref="EntityTemplateBase{TSelf}"/> of one or more <see cref="EntityBase{TCoordinates,TTemplate}"/>s.
        /// </summary>
        /// <param name="entities">The <see cref="EntityBase{TCoordinates,TTemplate}"/>s to modify.</param>
        /// <param name="template">The name of the new <see cref="EntityTemplateBase{TSelf}"/> to set.</param>
        public ChangeEntityTemplates(IEnumerable<ITemplated> entities, string template)
        {
            #region Sanity checks
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            if (template == null) throw new ArgumentNullException(nameof(template));
            #endregion

            // Create local defensive copy of entities
            _entities = new(entities);

            // Backup the old template names
            _oldTemplates = new string[_entities.Count];
            for (int i = 0; i < _entities.Count; i++)
                _oldTemplates[i] = _entities[i].TemplateName;

            _newTemplates = template;
        }
        #endregion

        //--------------------//

        #region Undo / Redo
        /// <inheritdoc/>
        protected override void OnExecute()
        {
            foreach (var entity in _entities)
                entity.TemplateName = _newTemplates;
        }

        /// <inheritdoc/>
        protected override void OnUndo()
        {
            for (int i = 0; i < _entities.Count; i++)
                _entities[i].TemplateName = _oldTemplates[i];
        }
        #endregion
    }
}
