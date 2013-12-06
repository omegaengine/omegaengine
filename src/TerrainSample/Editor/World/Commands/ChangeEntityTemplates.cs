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
    /// Changes the <see cref="Entity{TCoordinates}.TemplateName"/> property of one or more <see cref="Entity{TCoordinates}"/>s.
    /// </summary>
    public class ChangeEntityTemplates : SimpleCommand
    {
        #region Variables
        private readonly List<Entity<Vector2>> _entities;
        private readonly string[] _oldTemplates;
        private readonly string _newTemplates;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command for changing the <see cref="EntityTemplate"/> of one or more <see cref="Entity{TCoordinates}"/>s.
        /// </summary>
        /// <param name="entities">The <see cref="Entity{TCoordinates}"/>s to modify.</param>
        /// <param name="template">The name of the new <see cref="EntityTemplate"/> to set.</param>
        public ChangeEntityTemplates(IEnumerable<Entity<Vector2>> entities, string template)
        {
            #region Sanity checks
            if (entities == null) throw new ArgumentNullException("entities");
            if (template == null) throw new ArgumentNullException("template");
            #endregion

            // Create local defensive copy of entities
            _entities = new List<Entity<Vector2>>(entities);

            // Backup the old class names
            _oldTemplates = new string[_entities.Count];
            for (int i = 0; i < _entities.Count; i++)
                _oldTemplates[i] = _entities[i].TemplateName;

            _newTemplates = template;
        }
        #endregion

        //--------------------//

        #region Undo / Redo
        /// <inheritdoc />
        protected override void OnExecute()
        {
            foreach (var entity in _entities)
                entity.TemplateName = _newTemplates;
        }

        /// <inheritdoc />
        protected override void OnUndo()
        {
            for (int i = 0; i < _entities.Count; i++)
                _entities[i].TemplateName = _oldTemplates[i];
        }
        #endregion
    }
}
