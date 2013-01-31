/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using OmegaEngine;
using OmegaEngine.Graphics.Cameras;

namespace AlphaEditor.Graphics
{
    /// <summary>
    /// Abstract base class for editing particle system presets
    /// </summary>
    public partial class ParticleSystemEditor : UndoCommandTab
    {
        #region Variables
        /// <summary>The <see cref="OmegaEngine.Engine"/> used to render the particle system preview.</summary>
        protected Engine Engine;

        /// <summary>
        /// The camera used by the presenter
        /// </summary>
        protected readonly TrackCamera Camera = new TrackCamera(50, 2000) {Radius = 400};
        #endregion

        #region Constructor
        /// <inheritdoc/>
        protected ParticleSystemEditor()
        {
            InitializeComponent();

            MouseWheel += ParticleSystemEditor_MouseWheel;
        }
        #endregion

        //--------------------//

        #region Render control
        /// <summary>
        /// Creates an <see cref="EngineConfig"/> for rendering in this tab
        /// </summary>
        protected EngineConfig BuildEngineConfig()
        {
            return new EngineConfig {TargetSize = panelRender.ClientSize};
        }

        private void panelRender_Paint(object sender, PaintEventArgs e)
        {
            if (Engine != null) Engine.Render();
        }

        private void timerRender_Tick(object sender, EventArgs e)
        {
            timerRender.Enabled = false; // Prevent multiple ticks from accumulating
            if (Visible && Engine != null) Engine.Render();
            timerRender.Enabled = true;
        }

        private void panelRender_Resize(object sender, EventArgs e)
        {
            if (Engine != null && !Engine.Disposed && Size != new Size())
                Engine.EngineConfig = BuildEngineConfig();
        }
        #endregion

        #region Mouse control
        private Point _lastMouseLoc;

        private void panelRender_MouseMove(object sender, MouseEventArgs e)
        {
            // Make sure mouse wheel will work
            panelRender.Focus();

            var delta = new Point(e.X - _lastMouseLoc.X, e.Y - _lastMouseLoc.Y);
            //if (Settings.Current.Controls.InvertMouse)
            //{
            //    delta.X = -delta.X;
            //    delta.Y = -delta.Y;
            //}

            if (Engine != null)
            {
                switch (MouseButtons)
                {
                    case MouseButtons.Middle:
                    case MouseButtons.Left | MouseButtons.Right:
                        Camera.HorizontalRotation += delta.X / 2.0;
                        Camera.Radius *= Math.Pow(1.1, delta.Y / 15f);
                        Engine.Render();
                        break;
                    case MouseButtons.Right:
                        Camera.HorizontalRotation += delta.X / 2.0;
                        Camera.VerticalRotation += delta.Y / 2.0;
                        Engine.Render();
                        break;
                }
            }

            _lastMouseLoc = new Point(e.X, e.Y);
        }

        private void ParticleSystemEditor_MouseWheel(object sender, MouseEventArgs e)
        {
            if (Engine == null) return;

            Camera.Radius *= Math.Pow(1.1, e.Delta / -60f);
            Engine.Render();
        }
        #endregion
    }
}
