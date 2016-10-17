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
using NanoByte.Common.Values;

namespace OmegaEngine.Input
{
    /// <summary>
    /// Processes mouse events into higher-level navigational commands.
    /// </summary>
    /// <remarks>
    ///   <para>Dragging with the left mouse button allows <see cref="IInputReceiver.AreaSelection"/>.</para>
    ///   <para>Dragging with the right mouse button allows panning.</para>
    ///   <para>Dragging with the middle mouse button allows rotating and zooming.</para>
    ///   <para>Clicks and double-clicks are passed through.</para>
    /// </remarks>
    public class MouseInputProvider : InputProvider
    {
        #region Constants
        /// <summary>
        /// The number of pixels the mouse may move while pressed to still be considered a click.
        /// </summary>
        public const int ClickAccuracy = 10;
        #endregion

        #region Variables
        /// <summary>The control receiving the mouse events.</summary>
        private readonly Control _control;

        /// <summary>The original location of the mouse when the button was pressed.</summary>
        private Point _origMouseLoc;

        /// <summary>The location of the mouse the last time <see cref="Control.MouseMove"/> was raised.</summary>
        private Point _lastMouseLoc;

        /// <summary>The distance the mouse cursor has traveled since the button was pressed.</summary>
        private Size _totalMouseDelta;

        /// <summary>Was a <see cref="MouseDown"/> event received? If not, other mouse input should be ignored, since the event most likely was suppressed on purpose (handled by somebody else).</summary>
        private bool _pressed;

        /// <summary>Has the mouse been moved enough to decide this is no longer a click, but dragging?</summary>
        /// <remarks>This is required since <see cref="Control.Click"/> cannot be used to reliably detect clicks on render targets.</remarks>
        private bool _moving;

        /// <summary>Freeze (and hide) the mouse cursor for infinite panning.</summary>
        private bool _cursorFrozen;

        /// <summary>Don't execute <see cref="MouseMove"/>.</summary>
        private bool _ignoreMouseMove;
        #endregion

        #region Properties
        /// <summary>
        /// Invert the mouse axes.
        /// </summary>
        public bool InvertMouse { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Starts monitoring and processing mouse events receieved by a specififc control.
        /// </summary>
        /// <param name="control">The control receiving the mouse events.</param>
        public MouseInputProvider(Control control)
        {
            #region Sanity checks
            if (control == null) throw new ArgumentNullException(nameof(control));
            #endregion

            _control = control;

            // Start tracking input events
            _control.MouseDown += MouseDown;
            _control.MouseMove += MouseMove;
            _control.MouseUp += MouseUp;
            _control.MouseWheel += MouseWheel;
            _control.MouseDoubleClick += MouseDoubleClick;
            // Note: _control.MouseClick is useless since on a render target without any child controls even drags would be considered clicks
        }
        #endregion

        //--------------------//

        #region Event handlers
        private void MouseDown(object sender, MouseEventArgs e)
        {
            if (!HasReceivers) return;

            _pressed = true;

            // Cancle active selections when additional become pressed
            if (_moving)
            {
                bool accumulate = Control.ModifierKeys.HasFlag(Keys.Control);
                OnAreaSelection(new Rectangle(_origMouseLoc, _totalMouseDelta), accumulate, done: true);
                _moving = false;
            }

            // Remember the mouse cursor position when the button was pressed
            _lastMouseLoc = _origMouseLoc = e.Location;

            // Clean up
            _totalMouseDelta = default(Size);
            UpdateCursorFreezing();
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (_ignoreMouseMove || !HasReceivers) return;
            OnHover(e.Location);
            if (!_pressed) return;

            #region Delta
            // Calculate movement delta
            var delta = new Point(e.X - _lastMouseLoc.X, e.Y - _lastMouseLoc.Y);

            if (Control.MouseButtons != MouseButtons.None)
            {
                // Track total mouse movement while buttons are pressed
                _totalMouseDelta += (Size)delta;

                // Once the cursor has been moved far enough away from the origin, a click interpretation is longer possible, only selection or panning
                if (Math.Abs(_totalMouseDelta.Width) > ClickAccuracy || Math.Abs(_totalMouseDelta.Height) > ClickAccuracy)
                {
                    _moving = true;
                    UpdateCursorFreezing();
                }
            }
            #endregion

            bool accumulate = Control.ModifierKeys.HasFlag(Keys.Control);

            #region Events
            switch (Control.MouseButtons)
            {
                case MouseButtons.Left:
                    if (_moving)
                    { // The mouse moved more than a click, so this is an active selection
                        OnAreaSelection(new Rectangle(_origMouseLoc, _totalMouseDelta), accumulate);
                    }
                    break;

                case MouseButtons.Right:
                    if (_moving)
                    { // The mouse moved more than a click, so this is an active pan
                        // Linear panning (possibly inverted), no rotation, no zoom
                        OnPerspectiveChange(InvertMouse ? new Point(-delta.X, -delta.Y) : delta, 0, 0);
                    }
                    break;

                case MouseButtons.Middle:
                case MouseButtons.Left | MouseButtons.Right:
                    // No panning, linear rotation (possibly inverted), exponential zoom
                    OnPerspectiveChange(new Point(), InvertMouse ? -delta.X : delta.X, delta.Y);
                    break;
            }
            #endregion

            #region Infinte panning
            if (_cursorFrozen)
            {
                // Prevent infinte recursion
                _ignoreMouseMove = true;

                // Snap back to original position
                Cursor.Position = Cursor.Position - new Size(delta);
                Application.DoEvents();

                _ignoreMouseMove = false;
            }
            else _lastMouseLoc = e.Location;
            #endregion

            Application.DoEvents();
        }

        private void MouseUp(object sender, MouseEventArgs e)
        {
            if (!_pressed || !HasReceivers) return;
            _pressed = false;

            bool accumulate = Control.ModifierKeys.HasFlag(Keys.Control);

            // Check if only the left mouse-button was pressed and now released
            if (e.Button == MouseButtons.Left && Control.MouseButtons == MouseButtons.None)
            {
                if (_moving)
                { // The mouse moved more than a click, so this is a completed selection
                    OnAreaSelection(new Rectangle(_origMouseLoc, _totalMouseDelta), accumulate, done: true);
                }
                else
                { // The mouse didn't move more than a click, so this is a click
                    OnClick(e, accumulate);
                }
            }

            // Check if only the right mouse-button was pressed and now released
            if (e.Button == MouseButtons.Right && Control.MouseButtons == MouseButtons.None)
            {
                if (!_moving)
                { // The mouse moved more than a click, so this is a click
                    OnClick(e, /*ToDo*/true);
                }
            }

            // Clean up
            _origMouseLoc = default(Point);
            _totalMouseDelta = default(Size);
            _moving = false;
            UpdateCursorFreezing();
        }

        private void MouseWheel(object sender, MouseEventArgs e)
        {
            if (!HasReceivers) return;

            // No panning, no rotation, exponential zoom
            OnPerspectiveChange(new Point(), 0, e.Delta / -4);
        }

        private void MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OnDoubleClick(e);
        }
        #endregion

        #region Cursor hiding
        /// <summary>Freeze/unfreeze and Hide/show and the cursor as appropriate</summary>
        private void UpdateCursorFreezing()
        {
            if ((Control.MouseButtons == MouseButtons.Right && _moving) || // Right button pressed and moving
                (Control.MouseButtons == MouseButtons.Middle || Control.MouseButtons == (MouseButtons.Left | MouseButtons.Right))) // Middle button or left and right button pressed
            {
                // Hide and freeze the cursor if it isn't already frozen
                if (!_cursorFrozen)
                {
                    Cursor.Hide();
                    _cursorFrozen = true;
                }
            }
            else
            {
                // Show and unfreeze the cursor if it is currently frozen
                if (_cursorFrozen)
                {
                    Cursor.Show();
                    _cursorFrozen = false;
                }
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
                _control.MouseDown -= MouseDown;
                _control.MouseMove -= MouseMove;
                _control.MouseUp -= MouseUp;
                _control.MouseWheel -= MouseWheel;
                _control.MouseDoubleClick -= MouseDoubleClick;
            }
        }
        #endregion
    }
}
