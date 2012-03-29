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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Input;
using SlimDX;
using EngineRenderable = OmegaEngine.Graphics.Renderables;
using Common;
using Common.Values;

namespace Presentation
{
    /// <summary>
    /// Handles the visual representation of <see cref="World"/> content where the user can manually control the perspective
    /// </summary>
    public abstract partial class InteractivePresenter : Presenter, IInputReceiver
    {
        #region Variables
        // Note: Using custom collection-class to allow external update-notification
        private readonly World.PositionableCollection _selectedPositionables = new World.PositionableCollection();

        /// <summary>An outline to show on the screen</summary>
        private Rectangle? _selectionRectangle;

        private Asset[] _preCachedAssets;
        #endregion

        #region Properties
        /// <summary>
        /// The <see cref="World.Positionable"/>s the user has selected with the mouse
        /// </summary>
        public World.PositionableCollection SelectedPositionables { get { return _selectedPositionables; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new interactive presenter
        /// </summary>
        /// <param name="engine">The engine to use for rendering</param>
        /// <param name="universe">The universe to display</param>
        protected InteractivePresenter(Engine engine, World.Universe universe) : base(engine, universe)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (universe == null) throw new ArgumentNullException("universe");
            #endregion

            // Add selection highlighting hooks
            engine.ExtraRender += DrawSelectionOutline;

            _selectedPositionables.Added += AddSelectedPositionable;
            _selectedPositionables.Removing += RemoveSelectedPositionable;
        }
        #endregion

        //--------------------//

        #region Initialize
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
        }
        #endregion

        //--------------------//

        #region Perspective change
        /// <inheritdoc/>
        public void PerspectiveChange(Point pan, int rotation, int zoom)
        {
            // Adapt panning speed based on view frustum size
            float panFactor = 1.0f / Math.Max(Engine.RenderSize.Width, Engine.RenderSize.Height);

            View.Camera.PerspectiveChange(pan.X * panFactor, pan.Y * panFactor, rotation / 2.0f, (float)Math.Pow(1.1, zoom / 15.0));
        }
        #endregion

        #region Hover
        /// <inheritdoc/>
        public virtual void Hover(Point target)
        {}
        #endregion

        #region Area selection
        /// <inheritdoc/>
        public virtual void AreaSelection(Rectangle area, bool accumulate, bool done)
        {
            if (done)
            {
                // Handle inverted rectangles and project to terrain
                var terrainArea = GetTerrainArea(Rectangle.FromLTRB(
                    (area.Left < area.Right) ? area.Left : area.Right,
                    (area.Top < area.Bottom) ? area.Top : area.Bottom,
                    (area.Left < area.Right) ? area.Right : area.Left,
                    (area.Top < area.Bottom) ? area.Bottom : area.Top));

                // ToDo: Optimize performance by using .SetMany()

                // Remove all previous selections unless the user wants to accumulate selections
                if (!accumulate) _selectedPositionables.Clear();

                // Check each entity in World if it is positioned on top of the selection area
                foreach (var entity in Universe.Positionables.Entities)
                {
                    if (entity.CollisionTest(terrainArea))
                    {
                        // Toggle entries when accumulating
                        if (accumulate && _selectedPositionables.Contains(entity)) _selectedPositionables.Remove(entity);
                        else _selectedPositionables.Add(entity);
                    }
                }

                // Remove the outline from the screen
                _selectionRectangle = null;
            }
            else
            { // Add a selection outline to the screen
                _selectionRectangle = area;
            }
        }

        /// <summary>
        /// Projects a 2D screen rectangle on to the <see cref="Presenter.Terrain"/>, forming a convex quadrangle.
        /// </summary>
        private Quadrangle GetTerrainArea(Rectangle area)
        {
            Vector2 topLeftCoord, bottomLeftCoord, bottomRightCoord, topRightCoord;
            using (new TimedLogEvent("Calculating terrain coordinates for picking"))
            {
                DoubleVector3 topLeftPoint;
                if (!Terrain.Intersects(View.PickingRay(new Point(area.Left, area.Top)), out topLeftPoint)) return new Quadrangle();
                topLeftCoord = World.Terrain.ToWorldCoords(topLeftPoint);

                DoubleVector3 bottomLeftPoint;
                if (!Terrain.Intersects(View.PickingRay(new Point(area.Left, area.Bottom)), out bottomLeftPoint)) return new Quadrangle();
                bottomLeftCoord = World.Terrain.ToWorldCoords(bottomLeftPoint);

                DoubleVector3 bottomRightPoint;
                if (!Terrain.Intersects(View.PickingRay(new Point(area.Right, area.Bottom)), out bottomRightPoint)) return new Quadrangle();
                bottomRightCoord = World.Terrain.ToWorldCoords(bottomRightPoint);

                DoubleVector3 topRightPoint;
                if (!Terrain.Intersects(View.PickingRay(new Point(area.Right, area.Top)), out topRightPoint)) return new Quadrangle();
                topRightCoord = World.Terrain.ToWorldCoords(topRightPoint);
            }

            var terrainArea = new Quadrangle(topLeftCoord, bottomLeftCoord, bottomRightCoord, topRightCoord);
            return terrainArea;
        }
        #endregion

        #region Click
        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", Justification = "There are no dangerous operations in this event handler")]
        public virtual void Click(MouseEventArgs e, bool accumulate)
        {
            #region Sanity checks
            if (e == null) throw new ArgumentNullException("e");
            #endregion

            // Determine the Engine object the user clicked on
            DoubleVector3 intersectPosition;
            var pickedObject = View.Pick(e.Location, out intersectPosition);
            if (pickedObject == null) return;
            bool pickedTerrain = pickedObject is EngineRenderable.Terrain;

            // ToDo: Optimize performance by using .SetMany()

            switch (e.Button)
            {
                case MouseButtons.Left:
                    // Remove all previous selections unless the user wants to accumulate selections
                    if (!accumulate) _selectedPositionables.Clear();

                    if (pickedTerrain)
                    { // Action: Left-click on terrain to select one nearby entity
                        foreach (var entity in Universe.Positionables.Entities)
                        {
                            // Check each entity in World if it is positioned at the selection point
                            if (entity.CollisionTest(World.Terrain.ToWorldCoords(intersectPosition)))
                            {
                                // Toggle entries when accumulating
                                if (accumulate && _selectedPositionables.Contains(entity)) _selectedPositionables.Remove(entity);
                                else _selectedPositionables.Add(entity);

                                // Stop after first hit (multi-selection only when dragging mouse)
                                break;
                            }
                        }
                    }
                    else
                    { // Action: Left-click on entity to select it
                        World.Positionable pickedEntity = GetWorld(pickedObject);
                        if (pickedEntity != null)
                        {
                            // Toggle entries when accumulating
                            if (accumulate && _selectedPositionables.Contains(pickedEntity)) _selectedPositionables.Remove(pickedEntity);
                            else _selectedPositionables.Add(pickedEntity);
                        }
                    }
                    break;

                case MouseButtons.Right:
                    if (_selectedPositionables.Count != 0 && pickedTerrain)
                    { // Action: Right-click on terrain to move
                        // Depending on the actual presenter type this may invoke path-finding or teleportation
                        MovePositionables(_selectedPositionables, World.Terrain.ToWorldCoords(intersectPosition));
                    }
                    break;
            }
        }
        #endregion

        #region Double click
        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", Justification = "There are no dangerous operations in this event handler")]
        public virtual void DoubleClick(MouseEventArgs e)
        {
            #region Sanity checks
            if (e == null) throw new ArgumentNullException("e");
            #endregion

            // Determine the Engine object the user double-clicked on
            DoubleVector3 intersectPosition;
            var pickedObject = View.Pick(e.Location, out intersectPosition);

            // Action: Double-click on entity to select and focus camera
            if (pickedObject != null && !(pickedObject is EngineRenderable.Terrain) && !(View.Camera is CinematicCamera)) /* Each swing must complete before the next one can start */
            {
                var newState = new World.CameraState
                {
                    Name = View.Camera.Name,
                    Position = World.Terrain.ToWorldCoords(pickedObject.Position),
                    Radius = pickedObject.WorldBoundingSphere.HasValue ? pickedObject.WorldBoundingSphere.Value.Radius * 2.5f : 50,
                };

                // Perform the animation
                View.SwingCameraTo(CreateCamera(newState), 1);
            }
        }
        #endregion

        //--------------------//

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
        /// Moves one or more <see cref="World.Positionable"/>s to a new position.
        /// </summary>
        /// <param name="positionables">The <see cref="World.Positionable"/>s to be moved.</param>
        /// <param name="target">The terrain position to move the <paramref name="positionables"/> to.</param>
        protected virtual void MovePositionables(World.PositionableCollection positionables, Vector2 target)
        {
            #region Sanity checks
            if (positionables == null) throw new ArgumentNullException("positionables");
            #endregion

            foreach (var entity in positionables.Entities)
            {
                // Start path-finding if this entity can move
                if (entity.TemplateData.MovementControl != null)
                    Universe.MoveEntity(entity, target);
                else
                    Log.Warn(entity + " is unable to move");
            }
        }
        #endregion

        //--------------------//

        #region Dispose
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            try
            {
                // Remove selection highlighting hooks
                Engine.ExtraRender -= DrawSelectionOutline;
                _selectedPositionables.Clear(); // Trigger any remaining "Removing" hooks
                _selectedPositionables.Added -= AddSelectedPositionable;
                _selectedPositionables.Removing -= RemoveSelectedPositionable;

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
    }
}
