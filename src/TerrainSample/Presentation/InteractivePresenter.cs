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
using System.Drawing;
using System.Linq;
using AlphaFramework.World.Components;
using AlphaFramework.World.Positionables;
using Common;
using Common.Collections;
using Common.Dispatch;
using Common.Utils;
using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Input;
using SlimDX;
using TerrainSample.World;
using TerrainSample.World.Components;
using TerrainSample.World.Positionables;

namespace TerrainSample.Presentation
{
    /// <summary>
    /// Handles the visual representation of <see cref="World"/> content where the user can manually control the perspective
    /// </summary>
    public abstract partial class InteractivePresenter : Presenter, IInputReceiver
    {
        /// <summary>
        /// Creates a new interactive presenter
        /// </summary>
        /// <param name="engine">The engine to use for rendering</param>
        /// <param name="universe">The universe to display</param>
        protected InteractivePresenter(Engine engine, Universe universe) : base(engine, universe)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (universe == null) throw new ArgumentNullException("universe");
            #endregion

            // Add selection highlighting hooks
            engine.ExtraRender += DrawSelectionOutline;

            _selectionsSync = new ModelViewSync<Positionable<Vector2>, PositionableRenderable>(_selectedPositionables, Scene.Positionables);

            Universe.Positionables.Removed += OnPositionableRemoved;
        }

        #region Initialize
        private Asset[] _preCachedAssets;

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();

            if (_preCachedAssets == null)
            {
                // Preload selection highlighting meshes
                _preCachedAssets = new Asset[] {XMesh.Get(Engine, "Engine/Circle.x"), XMesh.Get(Engine, "Engine/Rectangle.x")};
                foreach (var asset in _preCachedAssets) asset.HoldReference();
            }

            _selectionsSync.Register<Entity, PositionableRenderable>(GetSelectionHighlighting, UpdateSelectionHighlighting);
            _selectionsSync.Initialize();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            try
            {
                Engine.ExtraRender -= DrawSelectionOutline;
                Universe.Positionables.Removed -= OnPositionableRemoved;

                _selectionsSync.Dispose();

                // Allow the cache management system to clean thes up later
                if (_preCachedAssets != null)
                {
                    foreach (var asset in _preCachedAssets)
                        asset.ReleaseReference();
                    _preCachedAssets = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion

        //--------------------//

        #region Selection highlighting
        private readonly MonitoredCollection<Positionable<Vector2>> _selectedPositionables = new MonitoredCollection<Positionable<Vector2>>();

        /// <summary>
        /// The <see cref="Positionable{TCoordinates}"/>s the user has selected with the mouse
        /// </summary>
        public MonitoredCollection<Positionable<Vector2>> SelectedPositionables { get { return _selectedPositionables; } }

        /// <summary>
        /// Maps between <see cref="SelectedPositionables"/> and selection highlighting.
        /// </summary>
        private readonly ModelViewSync<Positionable<Vector2>, PositionableRenderable> _selectionsSync;

        /// <summary>
        /// Adds the selection highlighting for a <see cref="EntityBase{TCoordinates,TTemplate}"/>
        /// </summary>
        /// <param name="entity">The <see cref="EntityBase{TCoordinates,TTemplate}"/> to add the selection highlighting for</param>
        private Model GetSelectionHighlighting(Entity entity)
        {
            var selectionHighlight = new PerTypeDispatcher<Collision<Vector2>, Model>(ignoreMissing: true)
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
            }.Dispatch(entity.TemplateData.Collision);
            if (selectionHighlight != null) selectionHighlight.Name = entity.Name + " Selection";
            return selectionHighlight;
        }

        private void UpdateSelectionHighlighting(Entity entity, PositionableRenderable representation)
        {
            // Update the position and rotation of the selection highlighting
            representation.Position = GetTerrainPosition(entity);
            representation.Rotation = Quaternion.RotationYawPitchRoll(entity.Rotation.DegreeToRadian(), 0, 0);
        }

        private void OnPositionableRemoved(Positionable<Vector2> positionable)
        {
            SelectedPositionables.Remove(positionable);
        }
        #endregion

        #region Draw selection outline
        /// <summary>
        /// Continuously draw a selection rectangle while the mouse button is pressed
        /// </summary>
        private void DrawSelectionOutline()
        {
            if (_selectionRectangle.HasValue)
                Engine.DrawRectangleOutline(_selectionRectangle.Value, Color.Black);
        }
        #endregion

        #region User movement
        /// <summary>
        /// Moves one or more <see cref="Positionable{TCoordinates}"/>s to a new position.
        /// </summary>
        /// <param name="positionables">The <see cref="Positionable{TCoordinates}"/>s to be moved.</param>
        /// <param name="target">The terrain position to move the <paramref name="positionables"/> to.</param>
        protected virtual void MovePositionables(IEnumerable<Positionable<Vector2>> positionables, Vector2 target)
        {
            #region Sanity checks
            if (positionables == null) throw new ArgumentNullException("positionables");
            #endregion

            foreach (var entity in positionables.OfType<Entity>())
            {
                // Start pathfinding if this entity can move
                if (entity.TemplateData.Movement != null)
                    Universe.MoveEntity(entity, target);
                else
                    Log.Warn(entity + " is unable to move");
            }
        }
        #endregion
    }
}
