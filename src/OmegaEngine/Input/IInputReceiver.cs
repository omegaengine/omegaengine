﻿/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using System.Windows.Forms;

namespace OmegaEngine.Input;

/// <summary>
/// An object that receives input procesed into navigational commands from an <see cref="InputProvider"/>.
/// </summary>
public interface IInputReceiver
{
    /// <summary>
    /// Called when the user changes the view perspective.
    /// </summary>
    /// <param name="pan">Horizontal XY-panning in pixels.</param>
    /// <param name="rotation">Horizontal rotation in pixels.</param>
    /// <param name="zoom">Vertical zooming in pixels. Greater than 0 to zoom in; less than 0 to zoom out.</param>
    void PerspectiveChange(Point pan, int rotation, int zoom);

    /// <summary>
    /// Called when the user is hovering above a point on the screen.
    /// </summary>
    /// <param name="target">The point the user is hovering over in pixels.</param>
    void Hover(Point target);

    /// <summary>
    /// Called when the user is selecting an area on the screen.
    /// </summary>
    /// <param name="area">The selected area in pixels. The bottom-right corner is always the last point selected by the user, therefore the box may be inverted.</param>
    /// <param name="accumulate"><c>true</c> when the user wants the new selection to be added to the old one.</param>
    /// <param name="done">True when the user has finished his selection (e.g. released the mouse).</param>
    void AreaSelection(Rectangle area, bool accumulate, bool done);

    /// <summary>
    /// Called when the user clicked something (not dragged!).
    /// </summary>
    /// <param name="e">The original event arguments from the click.</param>
    /// <param name="accumulate"><c>true</c> when the user wants the action to have an accumulative effect (usually for selections).</param>
    void Click(MouseEventArgs e, bool accumulate);

    /// <summary>
    /// Called when the user double-clicked something.
    /// </summary>
    /// <param name="e">The original event arguments from the click.</param>
    void DoubleClick(MouseEventArgs e);
}
