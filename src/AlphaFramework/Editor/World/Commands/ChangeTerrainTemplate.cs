/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using AlphaFramework.World.Templates;
using AlphaFramework.World.Terrains;
using Common.Undo;

namespace AlphaFramework.Editor.World.Commands
{
    /// <summary>
    /// Changes a terrain <see cref="Template{TSelf}"/> on a <see cref="Terrain{TTemplate}"/>s.
    /// </summary>
    public class ChangeTerrainTemplate<TTemplate> : SimpleCommand where TTemplate : Template<TTemplate>
    {
        #region Variables
        private readonly Terrain<TTemplate> _terrain;
        private readonly int _templateIndex;
        private string _oldTemplateName;
        private readonly string _newTemplateName;
        private readonly Action _refreshHandler;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command for changing a <see cref="Template{TSelf}"/> on a <see cref="Terrain{TTemplate}"/>.
        /// </summary>
        /// <param name="terrain">The <see cref="Terrain{TTemplate}"/> to modify.</param>
        /// <param name="templateIndex">The index in <see cref="Terrain{TTemplate}.Templates"/> to set.</param>
        /// <param name="templateName">The name of the new <typeparamref name="TTemplate"/> to set.</param>
        /// <param name="refreshHandler">Called when the <see cref="Terrain{TTemplate}"/> needs to be reset.</param>
        public ChangeTerrainTemplate(Terrain<TTemplate> terrain, int templateIndex, string templateName, Action refreshHandler)
        {
            _terrain = terrain;
            _templateIndex = templateIndex;
            _newTemplateName = templateName;
            _refreshHandler = refreshHandler;
        }
        #endregion

        //--------------------//

        #region Undo / Redo
        /// <inheritdoc />
        protected override void OnExecute()
        {
            _oldTemplateName = (_terrain.Templates[_templateIndex] == null) ? null : _terrain.Templates[_templateIndex].Name;
            _terrain.Templates[_templateIndex] = string.IsNullOrEmpty(_newTemplateName) ? null : Template<TTemplate>.All[_newTemplateName];
            _refreshHandler();
        }

        /// <inheritdoc />
        protected override void OnUndo()
        {
            _terrain.Templates[_templateIndex] = string.IsNullOrEmpty(_oldTemplateName) ? null : Template<TTemplate>.All[_oldTemplateName];
            _refreshHandler();
        }
        #endregion
    }
}
