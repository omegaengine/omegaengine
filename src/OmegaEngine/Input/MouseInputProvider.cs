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
    public double WheelSensitivity { get; set; } = 0.1;

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
        _control.LostFocus += (_, _) => ForceReleaseCursor();
        // Note: _control.MouseClick is useless since on a render target without any child controls even drags would be considered clicks
    }

    /// <summary>The original location of the mouse when the button was pressed.</summary>
    private Point _origMouseLoc;

    /// <summary>The location of the mouse the last time <see cref="Control.MouseMove"/> was raised.</summary>
    private Point _lastMouseLoc;

    /// <summary>The distance the mouse cursor has traveled since the button was pressed.</summary>
    private Size _totalMouseDelta;

    private MouseButtons _pressedButtons;
    private bool _isDragging;
    private MouseAction? _activeAction;

    private bool _cursorCaptured;
    private Point _cursorCapturePos;
    private bool _ignoreMouseMove;

    private void CaptureCursor()
    {
        if (_cursorCaptured) return;

        _cursorCapturePos = Cursor.Position;
        Cursor.Hide();
        _cursorCaptured = true;
    }

    private void ReleaseCursor()
    {
        if (!_cursorCaptured) return;

        Cursor.Position = _cursorCapturePos;
        Cursor.Show();
        _cursorCaptured = false;
    }

    private void ForceReleaseCursor()
    {
        _isDragging = false;
        _activeAction = null;
        _pressedButtons = MouseButtons.None;
        ReleaseCursor();
    }

    private void MouseDown(object sender, MouseEventArgs e)
    {
        if (!HasReceivers) return;

        _pressedButtons |= e.Button;
        _activeAction = _pressedButtons switch
        {
            MouseButtons.Left => Scheme.LeftDrag,
            MouseButtons.Right => Scheme.RightDrag,
            MouseButtons.Middle or MouseButtons.Left | MouseButtons.Right => Scheme.MiddleDrag,
            _ => null
        };

        _origMouseLoc = _lastMouseLoc = e.Location;
        _totalMouseDelta = default;
        _isDragging = false;
    }

    private void MouseMove(object sender, MouseEventArgs e)
    {
        if (_ignoreMouseMove || !HasReceivers) return;
        OnHover(e.Location);

        if (_pressedButtons == MouseButtons.None)
        {
            _lastMouseLoc = e.Location;
            return;
        }

        var delta = new Size(e.X - _lastMouseLoc.X, e.Y - _lastMouseLoc.Y);
        _totalMouseDelta += delta;

        if (!_isDragging &&
            (Math.Abs(_totalMouseDelta.Width) > ClickAccuracy ||
             Math.Abs(_totalMouseDelta.Height) > ClickAccuracy))
        {
            _isDragging = true;

            if (_activeAction is MouseNavigation { CaptureCursor: true })
                CaptureCursor();
        }

        switch (_activeAction)
        {
            case MouseAreaSelection when _isDragging:
                OnAreaSelection(new(_origMouseLoc, _totalMouseDelta), accumulate: Control.ModifierKeys.HasFlag(Keys.Control));
                break;

            case MouseNavigation nav when _isDragging:
                ApplyNavigation(nav, delta);
                break;
        }

        if (_cursorCaptured)
        {
            // Prevent infinite recursion
            _ignoreMouseMove = true;

            // Snap back to original position
            Cursor.Position -= delta;

            _ignoreMouseMove = false;
        }
        else _lastMouseLoc = e.Location;
    }

    private void MouseUp(object sender, MouseEventArgs e)
    {
        if (!HasReceivers) return;

        _pressedButtons &= ~e.Button;
        bool accumulate = Control.ModifierKeys.HasFlag(Keys.Control);

        if (e.Button == MouseButtons.Left && _pressedButtons == MouseButtons.None)
        {
            if (_isDragging)
            {
                OnAreaSelection(new(_origMouseLoc, _totalMouseDelta), accumulate, done: true);
            }
            else
            { // The mouse didn't move more than a click, so this is a click
                OnClick(e, accumulate);
            }
        }

        if (e.Button == MouseButtons.Right && _pressedButtons == MouseButtons.None && !_isDragging) OnClick(e, accumulate: true);

        if (_pressedButtons == MouseButtons.None)
        {
            _isDragging = false;
            _activeAction = null;
            _totalMouseDelta = default;
            ReleaseCursor();
        }
    }

    private void ApplyNavigation(MouseNavigation nav, Size delta)
    {
        var translation = new DoubleVector3();
        var rotation = new DoubleVector3();

        ApplyAxis(nav.X, delta.Width);
        ApplyAxis(nav.Y, delta.Height);

        OnNavigate(translation, rotation);

        void ApplyAxis(MouseNavigationAxis axis, int value)
        {
            double v = InvertMouse ? -value : value;
            if (nav.ViewportScaling)
                v *= 100f / Math.Max(_control.ClientSize.Width, _control.ClientSize.Height);

            switch (axis)
            {
                case TranslationX:
                    translation.X += CursorSensitivity * -v;
                    break;
                case TranslationY:
                    translation.Y += CursorSensitivity * +v;
                    break;
                case TranslationZ:
                    translation.Z += WheelSensitivity * -v;
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
            translation: new(0, 0, WheelSensitivity * e.Delta),
            rotation: new());

    private void MouseDoubleClick(object sender, MouseEventArgs e)
        => OnDoubleClick(e);
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        { // This block will only be executed on manual disposal, not by Garbage Collection
            ForceReleaseCursor();

            // Stop tracking input events
            _control.MouseDown -= MouseDown;
            _control.MouseMove -= MouseMove;
            _control.MouseUp -= MouseUp;
            _control.MouseWheel -= MouseWheel;
            _control.MouseDoubleClick -= MouseDoubleClick;
        }
    }
}
