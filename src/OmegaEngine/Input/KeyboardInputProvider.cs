/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OmegaEngine.Foundation.Geometry;

namespace OmegaEngine.Input;

/// <summary>
/// Processes keyboard events into higher-level navigational commands.
/// </summary>
public class KeyboardInputProvider : InputProvider
{
    /// <summary>
    /// The rate at which the keyboard events are repeated when a button is held down.
    /// </summary>
    public TimeSpan KeyRepetitionRate
    {
        get => TimeSpan.FromMilliseconds(_timerKeyboard.Interval);
        set => _timerKeyboard.Interval = (int)value.TotalMilliseconds;
    }

    /// <summary>
    /// The number of translation units to apply per key press event.
    /// </summary>
    public double TranslationFactor { get; set; } = 0.01;

    /// <summary>
    /// The number of rotation degrees to apply per key press event.
    /// </summary>
    public double RotationFactor { get; set; } = 1;

    /// <summary>The control receiving the keyboard events.</summary>
    private readonly Control _control;

    /// <summary>A timer that continuously raises events while keys are kept pressed.</summary>
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

    /// <summary>All keys that are currently pressed.</summary>
    private readonly HashSet<Keys> _pressedKeys = [];

    private void KeyDown(object sender, KeyEventArgs e)
    {
        // Only trigger for alpha-numeric and arrow keys
        if (e.KeyCode is >= Keys.A and <= Keys.Z
            or >= Keys.D0 and <= Keys.D9
            or >= Keys.Left and <= Keys.Down)
        {
            _pressedKeys.Add(e.KeyCode);
            _timerKeyboard.Enabled = true;
        }
    }

    private void Tick(object sender, EventArgs e)
    {
        var translation = new DoubleVector3();
        var rotation = new DoubleVector3();

        foreach (var key in _pressedKeys)
        {
            translation += key switch
            {
                Keys.W => new(0, 0, 1),
                Keys.S => new(0, 0, -1),
                Keys.A => new(-1, 0, 0),
                Keys.D => new(1, 0, 0),
                _ => new DoubleVector3()
            };

            rotation += key switch
            {
                Keys.Q => new(0, 0, -1),
                Keys.E => new(0, 0, 1),
                Keys.Up => new (0, 1, 0),
                Keys.Down => new(0, -1, 0),
                Keys.Right => new(-1, 0, 0),
                Keys.Left => new (1, 0, 0),
                _ => new DoubleVector3()
            };
        }

        OnNavigate(
            translation: TranslationFactor * translation,
            rotation: RotationFactor * rotation);
    }

    private void KeyUp(object sender, KeyEventArgs e)
    {
        _pressedKeys.Remove(e.KeyCode);
        _timerKeyboard.Enabled = _pressedKeys.Count > 0;
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
