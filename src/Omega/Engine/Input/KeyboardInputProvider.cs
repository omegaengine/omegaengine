/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using System.Windows.Forms;

namespace OmegaEngine.Input
{
    /// <summary>
    /// Processes keyboard events into higher-level navigational commands.
    /// </summary>
    /// <remarks>
    ///   <para>Pressing the left and right arrow keys allows rotating.</para>
    ///   <para>Pressing the up and down arrow keys allows zooming.</para>
    /// </remarks>
    public class KeyboardInputProvider : InputProvider
    {
        #region Variables
        /// <summary>The key on the keyboard that is currently pressed.</summary>
        private Keys _pressedKey = Keys.None;

        /// <summary>The control receiving the keyboard events.</summary>
        private readonly Control _control;

        /// <summary>A timer that continuously raises events while a key is kept pressed.</summary>
        private readonly Timer _timerKeyboard = new Timer {Interval = 10};
        #endregion

        #region Constructor
        /// <summary>
        /// Starts monitoring and processing keyboard events receieved by a specififc control.
        /// </summary>
        /// <param name="control">The control receiving the keyboard events.</param>
        public KeyboardInputProvider(Control control)
        {
            #region Sanity checks
            if (control == null) throw new ArgumentNullException("control");
            #endregion

            _control = control;

            // Start tracking input events
            _control.KeyDown += KeyDown;
            _control.KeyUp += KeyUp;

            _timerKeyboard.Tick += Tick;
        }
        #endregion

        //--------------------//

        #region Event handlers
        private void KeyDown(object sender, KeyEventArgs e)
        {
            // Only process one key at a time
            if (_pressedKey != Keys.None) return;

            // Otherwise only trigger for alpha-numeric and arrow keys
            if ((e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z) ||
                (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) ||
                (e.KeyCode >= Keys.Left && e.KeyCode <= Keys.Down))
            {
                _pressedKey = e.KeyCode;
                _timerKeyboard.Enabled = true;
            }
        }

        private void Tick(object sender, EventArgs e)
        {
            switch (_pressedKey)
            {
                case Keys.Up:
                    OnPerspectiveChange(new Point(), 0, 7);
                    break;
                case Keys.Down:
                    OnPerspectiveChange(new Point(), 0, -7);
                    break;
                case Keys.Right:
                    OnPerspectiveChange(new Point(), 7, 0);
                    break;
                case Keys.Left:
                    OnPerspectiveChange(new Point(), -7, 0);
                    break;
            }
        }

        private void KeyUp(object sender, KeyEventArgs e)
        {
            if (_pressedKey == e.KeyCode)
            {
                _timerKeyboard.Enabled = false;
                _pressedKey = Keys.None;
            }
        }
        #endregion

        //--------------------//

        #region Dispose
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            { // This block will only be executed on manual disposal, not by Garbage Collection
                // Stop tracking input events
                _control.KeyDown -= KeyDown;
                _control.KeyUp -= KeyUp;

                _timerKeyboard.Tick -= Tick;
            }
        }
        #endregion
    }
}
