/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Input;

namespace AlphaFramework.Editor.Graphics
{
    /// <summary>
    /// Abstract base class for editing particle system presets
    /// </summary>
    public partial class ParticleSystemEditor : UndoCommandTab, IInputReceiver
    {
        #region Variables
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
        }
        #endregion

        //--------------------//

        #region Render control
        private void timerRender_Tick(object sender, EventArgs e)
        {
            timerRender.Enabled = false; // Prevent multiple ticks from accumulating
            if (Visible) renderPanel.Engine?.Render();
            timerRender.Enabled = true;
        }
        #endregion

        #region View control
        protected override void OnInitialize() => renderPanel.AddInputReceiver(this);

        void IInputReceiver.PerspectiveChange(Point pan, int rotation, int zoom)
        {
            Camera.Radius *= (float)Math.Pow(1.1, zoom / 15.0);
            Camera.VerticalRotation += pan.Y;
            Camera.HorizontalRotation += (pan.X + rotation) / 2.0f;

            renderPanel.Engine.Render();
        }

        void IInputReceiver.Hover(Point target)
        {}

        void IInputReceiver.AreaSelection(Rectangle area, bool accumulate, bool done)
        {}

        void IInputReceiver.Click(MouseEventArgs e, bool accumulate)
        {}

        void IInputReceiver.DoubleClick(MouseEventArgs e)
        {}
        #endregion
    }
}
