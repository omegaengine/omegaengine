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

namespace OmegaEngine.Input;

/// <summary>
/// Calls a callback delegate whenever any kind of input is received.
/// </summary>
public class UpdateReceiver : IInputReceiver
{
    private readonly Action _update;

    /// <summary>
    /// Creates a new update receiver.
    /// </summary>
    /// <param name="update">The callback delegate to be called when any kind of input is received.</param>
    public UpdateReceiver(Action update)
    {
        _update = update ?? throw new ArgumentNullException(nameof(update));
    }

    /// <inheritdoc/>
    public void PerspectiveChange(Point pan, int rotation, int zoom)
    {
        _update();
    }

    /// <inheritdoc/>
    public void Hover(Point target)
    {
        _update();
    }

    /// <inheritdoc/>
    public void AreaSelection(Rectangle area, bool accumulate, bool done)
    {
        _update();
    }

    /// <inheritdoc/>
    public void Click(MouseEventArgs e, bool accumulate)
    {
        _update();
    }

    /// <inheritdoc/>
    public void DoubleClick(MouseEventArgs e)
    {
        _update();
    }
}