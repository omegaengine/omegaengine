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
using OmegaEngine.Foundation.Geometry;
using static OmegaEngine.Input.MouseNavigationAxis;

namespace OmegaEngine.Input;

/// <summary>
/// Processes mouse events into higher-level navigational commands.
/// </summary>
public class MouseInputProvider : InputProvider
{
    /// <summary>
    /// Controls which mouse button does what.
    /// </summary>
    public MouseInputScheme Scheme { get; set; } = MouseInputScheme.Scene;

    /// <summary>
    /// The number of pixels the mouse may move while pressed to still be considered a click.
    /// </summary>
    public int ClickAccuracy { get; set; } = 10;

    /// <summary>
    /// Invert the mouse axes.
    /// </summary>
    public bool InvertMouse { get; set; }

    /// <summary>
    /// The sensitivity of the mouse cursor. The higher the value, the faster the movement.
    /// </summary>
    public double CursorSensitivity { get; set; } = 1;

    /// <summary>
    /// The sensitivity of the mouse wheel. The higher the value, the faster the movement.
    /// </summary>
    public double WheelSensitivity { get; set; } = 0.001;

    /// <summary>The control receiving the mouse events.</summary>
    private readonly Control _control;

    /// <summary>
    /// Starts monitoring and processing mouse events received by a specific control.
    /// </summary>
    /// <param name="control">The control receiving the mouse events.</param>
    public MouseInputProvider(Control control)
    {
        _control = control ?? throw new ArgumentNullException(nameof(control));

        // Start tracking input events
        _control.MouseDown += MouseDown;
        _control.MouseMove += MouseMove;
        _control.MouseUp += MouseUp;
        _control.MouseWheel += MouseWheel;
        _control.MouseDoubleClick += MouseDoubleClick;
        // Note: _control.MouseClick is useless since on a render target without any child controls even drags would be considered clicks
    }

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

    /// <summary>The action currently active due to mouse dragging.</summary>
    private MouseAction? _activeAction;

    private void MouseDown(object sender, MouseEventArgs e)
    {
        if (!HasReceivers) return;

        _pressed = true;
        _activeAction = Control.MouseButtons switch
        {
            MouseButtons.Left => Scheme.LeftDrag,
            MouseButtons.Right => Scheme.RightDrag,
            MouseButtons.Middle or MouseButtons.Left | MouseButtons.Right => Scheme.MiddleDrag,
            _ => null
        };

        if (_moving && _activeAction is not MouseAreaSelection)
        {
            bool accumulate = Control.ModifierKeys.HasFlag(Keys.Control);
            OnAreaSelection(new(_origMouseLoc, _totalMouseDelta), accumulate, done: true);
            _moving = false;
        }

        // Remember the mouse cursor position when the button was pressed
        _lastMouseLoc = _origMouseLoc = e.Location;

        // Clean up
        _totalMouseDelta = default;
        UpdateCursorCapture();
    }

    /// <summary>Don't execute <see cref="MouseMove"/>.</summary>
    private bool _ignoreMouseMove;

    /// <summary>Is the cursor currently captured?</summary>
    private bool _cursorCaptured;

    private void MouseMove(object sender, MouseEventArgs e)
    {
        if (_ignoreMouseMove || !HasReceivers) return;
        OnHover(e.Location);
        if (!_pressed) return;

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
                UpdateCursorCapture();
            }
        }

        switch (_activeAction)
        {
            case MouseAreaSelection when _moving:
                OnAreaSelection(new(_origMouseLoc, _totalMouseDelta), accumulate: Control.ModifierKeys.HasFlag(Keys.Control));
                break;

            case MouseNavigation nav:
                ApplyNavigation(nav, delta);
                break;
        }

        if (_cursorCaptured)
        {
            // Prevent infinite recursion
            _ignoreMouseMove = true;

            // Snap back to original position
            Cursor.Position -= new Size(delta);
            Application.DoEvents();

            _ignoreMouseMove = false;
        }
        else _lastMouseLoc = e.Location;

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
                OnAreaSelection(new(_origMouseLoc, _totalMouseDelta), accumulate, done: true);
            }
            else
            { // The mouse didn't move more than a click, so this is a click
                OnClick(e, accumulate);
            }
        }

        if (e.Button == MouseButtons.Right && Control.MouseButtons == MouseButtons.None && !_moving)
        {
            OnClick(e, accumulate: true);
        }

        // Clean up
        _origMouseLoc = default;
        _totalMouseDelta = default;
        _moving = false;
        _activeAction = null;
        UpdateCursorCapture();
    }

    private void ApplyNavigation(MouseNavigation nav, Point delta)
    {
        double screenScale = 1.0 / Math.Max(_control.ClientSize.Width, _control.ClientSize.Height);

        var translation = new DoubleVector3();
        var rotation = new DoubleVector3();

        ApplyAxis(nav.X, delta.X);
        ApplyAxis(nav.Y, delta.Y);

        OnNavigate(translation, rotation);

        void ApplyAxis(MouseNavigationAxis axis, int value)
        {
            int v = InvertMouse ? -value : value;
            switch (axis)
            {
                case TranslationX:
                    translation.X += CursorSensitivity * screenScale * -v;
                    break;
                case TranslationY:
                    translation.Y += CursorSensitivity * screenScale * +v;
                    break;
                case TranslationZ:
                    translation.Z += WheelSensitivity * +v;
                    break;
                case RotationX:
                    rotation.X += CursorSensitivity * +v;
                    break;
                case RotationY:
                    rotation.Y += CursorSensitivity * -v;
                    break;
                case RotationZ:
                    rotation.Z += CursorSensitivity * +v;
                    break;
            }
        }
    }

    private void MouseWheel(object sender, MouseEventArgs e)
        => OnNavigate(
            translation: new DoubleVector3(0, 0, WheelSensitivity * e.Delta),
            rotation: new DoubleVector3());

    private void MouseDoubleClick(object sender, MouseEventArgs e)
        => OnDoubleClick(e);

    private void UpdateCursorCapture()
    {
        bool shouldCapture = _moving && _activeAction is MouseNavigation {CaptureCursor: true};

        if (shouldCapture && !_cursorCaptured)
        {
            Cursor.Hide();
            _cursorCaptured = true;
        }
        else if (!shouldCapture && _cursorCaptured)
        {
            Cursor.Show();
            _cursorCaptured = false;
        }
    }

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
}
