/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows.Forms;

namespace OmegaEngine.Input;

/// <summary>
/// Processes keyboard events into higher-level navigational commands.
/// </summary>
/// <remarks>
///   <para>Pressing the left and right arrow keys allows rotating.</para>
///   <para>Pressing the up and down arrow keys allows zooming.</para>
/// </remarks>
public class KeyboardInputProvider : InputProvider
{
    /// <summary>The key on the keyboard that is currently pressed.</summary>
    private Keys _pressedKey = Keys.None;

    /// <summary>The control receiving the keyboard events.</summary>
    private readonly Control _control;

    /// <summary>A timer that continuously raises events while a key is kept pressed.</summary>
    private readonly Timer _timerKeyboard = new() {Interval = 10};

    /// <summary>
    /// Starts monitoring and processing keyboard events received by a specific control.
    /// </summary>
    /// <param name="control">The control receiving the keyboard events.</param>
    public KeyboardInputProvider(Control control)
    {
        _control = control ?? throw new ArgumentNullException(nameof(control));

        // Start tracking input events
        _control.KeyDown += KeyDown;
        _control.KeyUp += KeyUp;

        _timerKeyboard.Tick += Tick;
    }

    private void KeyDown(object sender, KeyEventArgs e)
    {
        // Only process one key at a time
        if (_pressedKey != Keys.None) return;

        // Otherwise only trigger for alpha-numeric and arrow keys
        if (e.KeyCode is >= Keys.A and <= Keys.Z
            or >= Keys.D0 and <= Keys.D9
            or >= Keys.Left and <= Keys.Down)
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
                OnPerspectiveChange(new(), 0, 7);
                break;
            case Keys.Down:
                OnPerspectiveChange(new(), 0, -7);
                break;
            case Keys.Right:
                OnPerspectiveChange(new(), 7, 0);
                break;
            case Keys.Left:
                OnPerspectiveChange(new(), -7, 0);
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
}
