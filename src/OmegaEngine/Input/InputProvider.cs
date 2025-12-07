/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace OmegaEngine.Input;

/// <summary>
/// Processes events from an input device into higher-level navigational commands.
/// </summary>
public abstract class InputProvider : IDisposable
{
    private readonly ICollection<IInputReceiver> _receivers = new LinkedList<IInputReceiver>();

    /// <summary>
    /// Indicates whether this handler currently has <see cref="IInputReceiver"/> attached to it.
    /// </summary>
    public bool HasReceivers => _receivers.Count != 0;

    /// <summary>
    /// Adds an object that wishes to be notified about navigational commands that are triggered by input.
    /// </summary>
    /// <param name="receiver">The object to receive the commands.</param>
    public void AddReceiver(IInputReceiver receiver)
    {
        #region Sanity checks
        if (receiver == null) throw new ArgumentNullException(nameof(receiver));
        #endregion

        _receivers.Add(receiver);
    }

    /// <summary>
    /// Removes an object that no longer wishes to be notified about navigational commands.
    /// </summary>
    /// <param name="receiver">The object to no longer receive the commands.</param>
    public void RemoveReceiver(IInputReceiver receiver)
    {
        #region Sanity checks
        if (receiver == null) throw new ArgumentNullException(nameof(receiver));
        #endregion

        _receivers.Remove(receiver);
    }

    /// <summary>
    /// Raises all registered <see cref="IInputReceiver.PerspectiveChange"/>s.
    /// </summary>
    /// <param name="pan">Horizontal XY-panning in pixels.</param>
    /// <param name="rotation">Horizontal rotation in pixels.</param>
    /// <param name="zoom">Vertical zooming in pixels. Greater than 0 to zoom in; less than 0 to zoom out.</param>
    protected virtual void OnPerspectiveChange(Point pan, int rotation, int zoom)
    {
        foreach (var receiver in _receivers)
            receiver.PerspectiveChange(pan, rotation, zoom);
    }

    /// <summary>
    /// Raises all registered <see cref="IInputReceiver.AreaSelection"/>s.
    /// </summary>
    /// <param name="area">The selected area in pixels.</param>
    /// <param name="accumulate"><c>true</c> when the user wants the new selection to be added to the old one.</param>
    /// <param name="done">True when the user has finished his selection (e.g. released the mouse).</param>
    protected virtual void OnAreaSelection(Rectangle area, bool accumulate, bool done = false)
    {
        foreach (var receiver in _receivers)
            receiver.AreaSelection(area, accumulate, done);
    }

    /// <summary>
    /// Raises all registered <see cref="IInputReceiver.Hover"/>s.
    /// </summary>
    /// <param name="target">The point the user is hovering over in pixels.</param>
    protected virtual void OnHover(Point target)
    {
        foreach (var receiver in _receivers)
            receiver.Hover(target);
    }

    /// <summary>
    /// Raises all registered <see cref="IInputReceiver.Click"/>s.
    /// </summary>
    /// <param name="e">The original event arguments from the click.</param>
    /// <param name="accumulate"><c>true</c> when the user wants the action to have an accumulative effect (usually for selections).</param>
    protected virtual void OnClick(MouseEventArgs e, bool accumulate)
    {
        foreach (var receiver in _receivers)
            receiver.Click(e, accumulate);
    }

    /// <summary>
    /// Raises all registered <see cref="IInputReceiver.DoubleClick"/>s.
    /// </summary>
    /// <param name="e">The original event arguments from the click.</param>
    protected virtual void OnDoubleClick(MouseEventArgs e)
    {
        foreach (var receiver in _receivers)
            receiver.DoubleClick(e);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    ~InputProvider()
    {
        Dispose(false);
    }

    /// <summary>
    /// To be called by <see cref="IDisposable.Dispose"/> and the object destructor.
    /// </summary>
    /// <param name="disposing"><c>true</c> if called manually and not by the garbage collector.</param>
    protected abstract void Dispose(bool disposing);
}
