/*
 * Copyright 2006-2012 Bastian Eicher
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

using System.Collections.Generic;
using Common.Utils;
using SlimDX;
using EngineRenderable = OmegaEngine.Graphics.Renderables;

namespace Presentation
{
    /*
     * This file provides helper methods for visualizing selections.
     * 
     * The method AddSelectedPositionable() is to be called for every newly selected Positionable.
     * It internally uses AddSelectedEntity() to create the actual visual representations in OmegaEngine.
     * _worldToEngine is maintained to allow forward lookups, which are needed for updating and removing.
     * 
     * The method RemoveSelectedPositionable() is to be called once for every newly unselected Positionable.
     * It internally uses RemoveSelectedEntity() to remove the visual representations from OmegaEngine.
     * 
     * AddSelectedPositionable() sets up hooks to call RemoveSelectedEntity() and AddSelectedEntity() when changes are made that require an entirly new rendering object (new entity template).
     * 
     * The existing RemovePositionable() is extended to remove any associated selection visualizations along with the entities.
     * The existing UpdatePositionable() is extended to update the positions of any associated selection visualizations along with the entities.
     */

    partial class InteractivePresenter
    {
        #region Variables
        /// <summary>1:1 association of <see cref="World.Entity"/> to selection highlighting <see cref="EngineRenderable.PositionableRenderable"/>.</summary>
        private readonly Dictionary<World.Entity, EngineRenderable.PositionableRenderable> _worldToEngine = new Dictionary<World.Entity, EngineRenderable.PositionableRenderable>();
        #endregion

        //--------------------//

        #region Remove positionables
        /// <inheritdoc />
        protected override void RemovePositionable(World.Positionable positionable)
        {
            // Entities that are no longer in the Universe can't be selected
            SelectedPositionables.Remove(positionable);

            base.RemovePositionable(positionable);
        }
        #endregion

        #region Update positionables
        /// <inheritdoc />
        protected override void UpdatePositionable(World.Positionable positionable)
        {
            base.UpdatePositionable(positionable);

            // Only continue if the positionable is a entity that has a selection highlighting associated to it
            var entity = positionable as World.Entity;
            if (entity == null) return;
            EngineRenderable.PositionableRenderable selection;
            if (!_worldToEngine.TryGetValue(entity, out selection)) return;

            // Update the posistion and rotation of the selection highlighting
            selection.Position = GetEngine(entity.TemplateData.RenderControls[0]).Position;
            selection.Rotation = Quaternion.RotationYawPitchRoll(MathUtils.DegreeToRadian(entity.Rotation), 0, 0);
        }
        #endregion
    }
}
