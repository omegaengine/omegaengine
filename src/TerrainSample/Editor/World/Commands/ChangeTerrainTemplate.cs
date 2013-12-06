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
using Common.Undo;
using World;
using World.Templates;
using World.Terrains;

namespace AlphaEditor.World.Commands
{
    /// <summary>
    /// Changes a <see cref="TerrainTemplate"/> of a <see cref="Terrain"/>s.
    /// </summary>
    public class ChangeTerrainTemplate : SimpleCommand
    {
        #region Variables
        private readonly Terrain _terrain;
        private readonly int _templateIndex;
        private string _oldTemplateName;
        private readonly string _newTemplateName;
        private readonly Action _refreshHandler;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command for changing a <see cref="TerrainTemplate"/> in a <see cref="Terrain"/>.
        /// </summary>
        /// <param name="terrain">The <see cref="Terrain"/> to modify.</param>
        /// <param name="templateIndex">The index in <see cref="Terrain.Templates"/> to set.</param>
        /// <param name="templateName">The name of the new <see cref="TerrainTemplate"/> to set.</param>
        /// <param name="refreshHandler">Called when the <see cref="Terrain"/> needs to be reset.</param>
        public ChangeTerrainTemplate(Terrain terrain, int templateIndex, string templateName, Action refreshHandler)
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
            _terrain.Templates[_templateIndex] = string.IsNullOrEmpty(_newTemplateName) ? null : TemplateManager.GetTerrainTemplate(_newTemplateName);
            _refreshHandler();
        }

        /// <inheritdoc />
        protected override void OnUndo()
        {
            _terrain.Templates[_templateIndex] = string.IsNullOrEmpty(_oldTemplateName) ? null : TemplateManager.GetTerrainTemplate(_oldTemplateName);
            _refreshHandler();
        }
        #endregion
    }
}
