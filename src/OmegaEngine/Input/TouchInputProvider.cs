/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using NanoByte.Common.Controls.Touch;
using OmegaEngine.Foundation.Geometry;
using static OmegaEngine.Input.NavigationAxis;

namespace OmegaEngine.Input;

/// <summary>
/// Processes touch events into higher-level navigational commands.
/// </summary>
/// <remarks>Complex manipulations with combined panning, rotating and zooming are possible.</remarks>
public class TouchInputProvider : InputProvider
{
    /// <summary>
    /// Controls which touch gesture does what.
    /// </summary>
    public TouchInputScheme Scheme { get; set; } = TouchInputScheme.Scene;

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

        var translation = new DoubleVector3();
        var rotation = new DoubleVector3();

        if (Scheme.Pan is {} pan)
        {
            ApplyAxis(pan.X, PanSensitivity * e.Delta.TranslationX);
            ApplyAxis(pan.Y, PanSensitivity * e.Delta.TranslationY);
        }
        if (Scheme.Pinch is {} pinch) ApplyAxis(pinch, ZoomSensitivity * (e.Delta.Scale - 1));
        if (Scheme.Twist is {} twist) ApplyAxis(twist, RotationSensitivity * (e.Delta.Rotation * 180.0 / Math.PI));

        OnNavigate(translation, rotation);

        void ApplyAxis(NavigationAxis axis, double v)
        {
            switch (axis)
            {
                case TranslationX:
                    translation.X += -v;
                    break;
                case TranslationY:
                    translation.Y += +v;
                    break;
                case TranslationZ:
                    translation.Z += +v;
                    break;
                case RotationX:
                    rotation.X += +v;
                    break;
                case RotationY:
                    rotation.Y += -v;
                    break;
                case RotationZ:
                    rotation.Z += -v;
                    break;
            }
        }
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
