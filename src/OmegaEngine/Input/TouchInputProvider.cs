/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using NanoByte.Common.Controls.Touch;

namespace OmegaEngine.Input;

/// <summary>
/// Processes touch events into higher-level navigational commands.
/// </summary>
/// <remarks>Complex manipulations with combined panning, rotating and zooming are possible.</remarks>
public class TouchInputProvider : InputProvider
{
    /// <summary>The control receiving the touch events.</summary>
    private readonly ITouchControl _control;

    /// <summary>
    /// Starts monitoring and processing Touch events received by a specific control.
    /// </summary>
    /// <param name="control">The control receiving the touch events.</param>
    public TouchInputProvider(ITouchControl control)
    {
        _control = control ?? throw new ArgumentNullException(nameof(control));
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {}
}
