/*
 * Copyright 2006-2014 Bastian Eicher
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
using AlphaFramework.World.EntityComponents;
using AlphaFramework.World.Positionables;
using Common.Collections;
using Common.Dispatch;
using Common.Utils;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Renderables;
using SlimDX;
using TerrainSample.World.EntityComponents;
using TerrainSample.World.Positionables;

namespace TerrainSample.Presentation
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
        /// <summary>1:1 association of <see cref="EntityBase{TCoordinates,TTemplate}"/> to selection highlighting <see cref="OmegaEngine.Graphics.Renderables.PositionableRenderable"/>.</summary>
        private readonly Dictionary<Entity, PositionableRenderable> _worldToEngine = new Dictionary<Entity, PositionableRenderable>();
        #endregion

        //--------------------//

        #region Add selected positionables
        /// <summary>
        /// Adds the selection highlighting for an <see cref="Positionable{TCoordinates}"/>
        /// </summary>
        /// <param name="positionable">The <see cref="Positionable{TCoordinates}"/> to add the selection highlighting for</param>
        private void AddSelectedPositionable(Positionable<Vector2> positionable)
        {
            new PerTypeDispatcher<Positionable<Vector2>>(ignoreMissing: true)
            {
                (Entity entity) =>
                {
                    AddSelectedEntity(entity);

                    // Remove and then re-add an entity to apply the new entity template
                    entity.ChangedRebuild += RemoveSelectedEntity;
                    entity.ChangedRebuild += AddSelectedEntity;
                }
            }.Dispatch(positionable);
        }

        /// <summary>
        /// Adds the selection highlighting for a <see cref="EntityBase{TCoordinates,TTemplate}"/>
        /// </summary>
        /// <param name="positionable">The <see cref="EntityBase{TCoordinates,TTemplate}"/> to add the selection highlighting for</param>
        /// <remarks>This is a helper method for <see cref="AddSelectedPositionable"/>.</remarks>
        private void AddSelectedEntity(Positionable<Vector2> positionable)
        {
            var entity = (Entity)positionable;

            var selectionHighlight = new PerTypeDispatcher<CollisionControl<Vector2>, Model>(ignoreMissing: true)
            {
                (Circle circle) =>
                {
                    // Create a circle around the entity based on the radius
                    var hightlight = new Model(XMesh.Get(Engine, "Engine/Circle.x"));
                    float scale = circle.Radius / 20 + 1;
                    hightlight.PreTransform = Matrix.Scaling(scale, 1, scale);
                    return hightlight;
                },
                (Box box) =>
                {
                    // Create a rectangle around the entity based on the box corners
                    var highlight = new Model(XMesh.Get(Engine, "Engine/Rectangle.x"));

                    // Determine the component-wise minimums and maxmimums and the absolute difference
                    var min = new Vector2(
                        Math.Min(box.Minimum.X, box.Maximum.X),
                        Math.Min(box.Minimum.Y, box.Maximum.Y));
                    var max = new Vector2(
                        Math.Max(box.Minimum.X, box.Maximum.X),
                        Math.Max(box.Minimum.Y, box.Maximum.Y));
                    var diff = max - min;

                    highlight.PreTransform = Matrix.Scaling(diff.X, 1, diff.Y) * Matrix.Translation(min.X, 0, -min.Y);
                    return highlight;
                }
            }.Dispatch(entity.TemplateData.CollisionControl);
            if (selectionHighlight == null) return;

            selectionHighlight.Name = entity.Name + " Selection";

            // Add the selection highlighting to a dictionary and the engine
            _worldToEngine.Add(entity, selectionHighlight);
            Scene.Positionables.Add(selectionHighlight);

            // Set up initial position and rotation
            UpdatePositionable(entity);
        }
        #endregion

        #region Remove selected positionables
        /// <summary>
        /// Removes the selection highlighting for an <see cref="Positionable{TCoordinates}"/>
        /// </summary>
        /// <param name="positionable">The <see cref="Positionable{TCoordinates}"/> to remove the selection highlighting for</param>
        private void RemoveSelectedPositionable(Positionable<Vector2> positionable)
        {
            new PerTypeDispatcher<Positionable<Vector2>>(ignoreMissing: true)
            {
                (Entity entity) =>
                {
                    RemoveSelectedEntity(entity);

                    // Unhook class update event
                    entity.ChangedRebuild -= RemoveSelectedEntity;
                    entity.ChangedRebuild -= AddSelectedEntity;
                }
            }.Dispatch(positionable);
        }

        /// <summary>
        /// Removes the selection highlighting for an <see cref="EntityBase{TCoordinates,TTemplate}"/>
        /// </summary>
        /// <param name="positionable">The <see cref="EntityBase{TCoordinates,TTemplate}"/> to remove the selection highlighting for</param>
        /// <remarks>This is a helper method for <see cref="RemoveSelectedPositionable"/>.</remarks>
        private void RemoveSelectedEntity(Positionable<Vector2> positionable)
        {
            var entity = (Entity)positionable;

            PositionableRenderable selection;
            if (!_worldToEngine.TryGetValue(entity, out selection)) return;

            // Remove the selection highlighting from the dictionary and the engine and dispose it
            _worldToEngine.Remove(entity);
            Scene.Positionables.Remove(selection);
            selection.Dispose();
        }
        #endregion

        //--------------------//

        #region Remove positionables
        /// <inheritdoc />
        protected override void RemovePositionable(Positionable<Vector2> positionable)
        {
            // Entities that are no longer in the Universe can't be selected
            SelectedPositionables.Remove(positionable);

            base.RemovePositionable(positionable);
        }
        #endregion

        #region Update positionables
        /// <inheritdoc />
        protected override void UpdatePositionable(Positionable<Vector2> positionable)
        {
            base.UpdatePositionable(positionable);

            new PerTypeDispatcher<Positionable<Vector2>>(ignoreMissing: true)
            {
                (Entity entity) =>
                {
                    PositionableRenderable selection;
                    if (!_worldToEngine.TryGetValue(entity, out selection)) return;

                    // Update the position and rotation of the selection highlighting
                    selection.Position = GetEngine(entity.TemplateData.RenderControls[0]).Position;
                    selection.Rotation = Quaternion.RotationYawPitchRoll(entity.Rotation.DegreeToRadian(), 0, 0);
                }
            }.Dispatch(positionable);
        }
        #endregion
    }
}
