/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Windows.Forms;
using NanoByte.Common.Controls;
using OmegaEngine.Input;
using SlimDX.Direct3D9;

namespace OmegaEngine
{
    /// <summary>
    /// A <see cref="Panel"/> that automatically provides an <see cref="OmegaEngine.Engine"/> instance for rendering on it, an optional timer-driver render loop, input handling, etc.
    /// </summary>
    [Description("A panel that automatically provides an Engine instance for rendering on it, an optional timer-driver render loop, input handling, etc.")]
    public class RenderPanel : TouchPanel
    {
        #region Variables
        private readonly Timer _renderTimer = new Timer {Interval = 33};
        #endregion

        #region Properties
        /// <summary>
        /// The <see cref="OmegaEngine.Engine"/> used to render graphics onto this panel.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Engine Engine { get; private set; }

        /// <summary>
        /// A default <see cref="Input.KeyboardInputProvider"/> hooked up to the <see cref="Panel"/>.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public KeyboardInputProvider KeyboardInputProvider { get; private set; }

        /// <summary>
        /// A default <see cref="Input.MouseInputProvider"/> hooked up to the <see cref="Panel"/>.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MouseInputProvider MouseInputProvider { get; private set; }

        /// <summary>
        /// A default <see cref="Input.TouchInputProvider"/> hooked up to the <see cref="Panel"/>.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TouchInputProvider TouchInputProvider { get; private set; }

        /// <summary>
        /// When set to <see langword="true"/> <see cref="OmegaEngine.Engine.Render()"/> is automatically called in regular intervals.
        /// </summary>
        /// <seealso cref="AutoRenderInterval"/>
        [DefaultValue(false), Category("Behavior"), Description("When set to true Engine.Render() is automatically called in regular intervals.")]
        public bool AutoRender { get { return _renderTimer.Enabled; } set { _renderTimer.Enabled = value; } }

        /// <summary>
        /// The interval in milliseconds in which <see cref="OmegaEngine.Engine.Render()"/> is automatically called.
        /// </summary>
        /// <seealso cref="AutoRender"/>
        [DefaultValue(33), Category("Behavior"), Description("The interval in milliseconds in which Engine.Render() is automatically called.")]
        public int AutoRenderInterval { get { return _renderTimer.Interval; } set { _renderTimer.Interval = value; } }
        #endregion

        #region Constructor
        public RenderPanel()
        {
            _renderTimer.Tick += delegate { if (Engine != null) Engine.Render(); };

            // Constantly steal focus so the scrool wheel will work
            MouseMove += delegate { Focus(); };
        }
        #endregion

        //--------------------//

        #region Event hooks
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Engine != null) Engine.Render();
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);

            if (Engine != null) Engine.Config = new EngineConfig {TargetSize = ClientSize};
        }
        #endregion

        //--------------------//

        #region Setup
        /// <summary>
        /// Initializes the <see cref="OmegaEngine.Engine"/> for rendering on this <see cref="Panel"/>.
        /// </summary>
        /// <returns>The newly initialized <see cref="Engine"/>.</returns>
        /// <exception cref="NotSupportedException">The graphics card does not meet the engine's minimum requirements.</exception>
        /// <exception cref="Direct3D9NotFoundException">Throw if required DirectX version is missing.</exception>
        /// <exception cref="Direct3DX9NotFoundException">Throw if required DirectX version is missing.</exception>
        /// <exception cref="Direct3D9Exception">internal errors occurred while intiliazing the graphics card.</exception>
        /// <exception cref="SlimDX.DirectSound.DirectSoundException">internal errors occurred while intiliazing the sound card.</exception>
        /// <remarks>Calling this multiple times will always return the same <see cref="OmegaEngine.Engine"/> instance.</remarks>
        public Engine Setup()
        {
            if (Engine == null)
            {
                Engine = new Engine(this, new EngineConfig {TargetSize = ClientSize});
                KeyboardInputProvider = new KeyboardInputProvider(this);
                MouseInputProvider = new MouseInputProvider(this);
                TouchInputProvider = new TouchInputProvider(this);
            }

            return Engine;
        }
        #endregion

        #region Input handling
        /// <summary>
        /// Calls <see cref="InputProvider.AddReceiver"/> for all default <see cref="InputProvider"/>s.
        /// </summary>
        /// <param name="receiver">The object to receive the commands.</param>
        public void AddInputReceiver(IInputReceiver receiver)
        {
            KeyboardInputProvider.AddReceiver(receiver);
            MouseInputProvider.AddReceiver(receiver);
            TouchInputProvider.AddReceiver(receiver);
        }

        /// <summary>
        /// Calls <see cref="InputProvider.RemoveReceiver"/> for all default <see cref="InputProvider"/>s.
        /// </summary>
        /// <param name="receiver">The object to no longer receive the commands.</param>
        public void RemoveInputReceiver(IInputReceiver receiver)
        {
            KeyboardInputProvider.RemoveReceiver(receiver);
            MouseInputProvider.RemoveReceiver(receiver);
            TouchInputProvider.RemoveReceiver(receiver);
        }
        #endregion

        //--------------------//

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            try
            {
                _renderTimer.Dispose();

                if (Engine != null) Engine.Dispose();
                if (TouchInputProvider != null) TouchInputProvider.Dispose();
                if (MouseInputProvider != null) MouseInputProvider.Dispose();
                if (KeyboardInputProvider != null) KeyboardInputProvider.Dispose();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion
    }
}
