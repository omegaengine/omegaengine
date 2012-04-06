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
using Hanoi.Logic;
using OmegaEngine;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Input;
using View = OmegaEngine.Graphics.View;

namespace Hanoi.Presentation
{
    /// <summary>
    /// Handles the visual representation of World content in the Engine
    /// </summary>
    public abstract class Presenter : IDisposable, IInputReceiver
    {
        #region Variables
        protected readonly Engine Engine;
        #endregion

        #region Properties
        /// <summary>
        /// The engine view used to display the <see cref="Scene"/>
        /// </summary>
        public View View { get; protected set; }

        /// <summary>
        /// The scene as it is used by the engine
        /// </summary>
        public Scene Scene { get; protected set; }

        /// <summary>
        /// The universe data for this scene
        /// </summary>
        public Universe Universe { get; protected set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new presentator
        /// </summary>
        /// <param name="engine">The engine to use for rendering</param>
        /// <param name="universe">The universe to display</param>
        protected Presenter(Engine engine, Universe universe)
        {
            Engine = engine;
            Universe = universe;
        }
        #endregion

        //--------------------//

        #region Initialize
        /// <summary>
        /// Hook the presentator's views into the engine's render system
        /// </summary>
        public void HookIn()
        {
            Engine.Views.Add(View);
        }

        /// <summary>
        /// Hook the presentator's views out of the engine's render system
        /// </summary>
        public void HookOut()
        {
            Engine.Views.Remove(View);
        }
        #endregion

        #region Input receiver
        /// <inheritdoc/>
        public void PerspectiveChange(Point pan, int rotation, int zoom)
        {
            // Adapt panning speed based on view frustum size
            float panFactor = 1.0f / Math.Max(Engine.RenderSize.Width, Engine.RenderSize.Height);

            View.Camera.PerspectiveChange(pan.X * panFactor, pan.Y * panFactor, rotation / 2.0f, (float)Math.Pow(1.1, zoom / 15.0));
        }

        /// <inheritdoc/>
        public virtual void Hover(Point target)
        {}

        /// <inheritdoc/>
        public virtual void AreaSelection(Rectangle area, bool accumulate, bool done)
        {
            // Ignore
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", Justification = "There are no dangerous operations in this event handler")]
        public virtual void Click(MouseEventArgs e, bool accumulate)
        {
            // Ignore
        }

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
            if (pickedObject != null && !(pickedObject is OmegaEngine.Graphics.Renderables.Terrain) && !(View.Camera is CinematicCamera)) /* Each swing must complete before the next one can start */
            {
                // Perform the animation
                View.SwingCameraTo(new TrackCamera(10, 1000)
                {
                    NearClip = 1,
                    FarClip = 1e+6f,
                    Radius = 50,
                    HorizontalRotation = 90,
                    Target = pickedObject.Position
                }, 1);
            }
        }
        #endregion

        //--------------------//

        #region Dispose
        /// <summary>
        /// Releases unmanaged resources for this presentator
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Auto-dispose destructor
        ~Presenter()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            View.Dispose();

            if (Scene != null) Scene.Dispose();
        }
        #endregion
    }
}
