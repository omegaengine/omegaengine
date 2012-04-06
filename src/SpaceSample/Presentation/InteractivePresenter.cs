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
using Common.Values;
using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Input;
using EngineRenderable = OmegaEngine.Graphics.Renderables;

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
                // ToDo: Implement

                // Remove the outline from the screen
                _selectionRectangle = null;
            }
            else
            { // Add a selection outline to the screen
                _selectionRectangle = area;
            }
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

            // ToDo: Optimize performance by using .SetMany()

            switch (e.Button)
            {
                case MouseButtons.Left:
                    // Remove all previous selections unless the user wants to accumulate selections
                    if (!accumulate) _selectedPositionables.Clear();

                    // Action: Left-click on entity to select it
                    var pickedEntity = GetWorld(pickedObject) as World.Entity;
                    if (pickedEntity != null)
                    {
                        // Toggle entries when accumulating
                        if (accumulate && _selectedPositionables.Contains(pickedEntity)) _selectedPositionables.Remove(pickedEntity);
                        else _selectedPositionables.Add(pickedEntity);
                    }
                    break;

                case MouseButtons.Right:
                    // ToDo: Implement
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
