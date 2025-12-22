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

namespace OmegaEngine.Input;

/// <summary>
/// Calls a callback delegate whenever any kind of input is received.
/// </summary>
/// <param name="update">The callback delegate to be called when any kind of input is received.</param>
public class ActionReceiver(Action update) : IInputReceiver
{
    /// <inheritdoc/>
    public void Navigate(DoubleVector3 translation, DoubleVector3 rotation) => update();

    /// <inheritdoc/>
    public void Hover(Point target) => update();

    /// <inheritdoc/>
    public void AreaSelection(Rectangle area, bool accumulate, bool done) => update();

    /// <inheritdoc/>
    public void Click(MouseEventArgs e, bool accumulate) => update();

    /// <inheritdoc/>
    public void DoubleClick(MouseEventArgs e) => update();
}
