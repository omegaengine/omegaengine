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
    /// <summary>
    /// A factor used to scale panning (drag) input.
    /// </summary>
    public double PanSensitivity { get; set; } = 0.2;

    /// <summary>
    /// A factor used to scale zoom (pinch) input.
    /// </summary>
    public double ZoomSensitivity { get; set; } = 40;

    /// <summary>
    /// A factor used to scale rotation (twist) input.
    /// </summary>
    public double RotationSensitivity { get; set; } = 1;

    /// <summary>The control receiving the touch events.</summary>
    private readonly ITouchControl _control;

    /// <summary>
    /// Starts monitoring and processing Touch events received by a specific control.
    /// </summary>
    /// <param name="control">The control receiving the touch events.</param>
    public TouchInputProvider(ITouchControl control)
    {
        _control = control ?? throw new ArgumentNullException(nameof(control));

        // Start tracking input events
        _control.ManipulationUpdated += ManipulationUpdated;
    }

    private void ManipulationUpdated(object sender, ManipulationEventArgs e)
    {
        if (!HasReceivers) return;

        var d = e.Delta;
        OnNavigate(
            translation: new(PanSensitivity * -d.TranslationX, PanSensitivity * d.TranslationY, ZoomSensitivity * (d.Scale - 1)),
            rotation: new(0, 0, RotationSensitivity * -(d.Rotation * 180.0 / Math.PI)));
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        { // This block will only be executed on manual disposal, not by Garbage Collection
            // Stop tracking input events
            _control.ManipulationUpdated -= ManipulationUpdated;
        }
    }
}
