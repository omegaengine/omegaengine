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
using System.Linq;
using System.Windows.Forms;
using NanoByte.Common.Controls;
using OmegaEngine.Foundation.Geometry;

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
    /// The number of pixels a touch may move to still be considered a tap.
    /// </summary>
    public int TapAccuracy { get; set; } = 10;

    /// <summary>
    /// The sensitivity of touch gestures. The higher the value, the faster the movement.
    /// </summary>
    public double TouchSensitivity { get; set; } = 1;

    /// <summary>
    /// The sensitivity of pinch-to-zoom gestures. The higher the value, the faster the zoom.
    /// </summary>
    public double ZoomSensitivity { get; set; } = 0.1;

    /// <summary>
    /// Starts monitoring and processing Touch events received by a specific control.
    /// </summary>
    /// <param name="control">The control receiving the touch events.</param>
    public TouchInputProvider(ITouchControl control)
    {
        _control = control ?? throw new ArgumentNullException(nameof(control));

        // Start tracking input events
        _control.TouchDown += TouchDown;
        _control.TouchMove += TouchMove;
        _control.TouchUp += TouchUp;
    }

    /// <summary>Tracks information about an active touch point.</summary>
    private class TouchPoint
    {
        public int ID { get; }
        public Point OriginalLocation { get; }
        public Point CurrentLocation { get; set; }
        public bool IsPrimary { get; }

        public TouchPoint(int id, Point location, bool isPrimary)
        {
            ID = id;
            OriginalLocation = location;
            CurrentLocation = location;
            IsPrimary = isPrimary;
        }
    }

    /// <summary>All currently active touch points.</summary>
    private readonly Dictionary<int, TouchPoint> _activeTouches = new();

    /// <summary>The distance between the first two touch points when they started.</summary>
    private double _initialDistance;

    /// <summary>The angle between the first two touch points when they started.</summary>
    private double _initialAngle;

    /// <summary>The center point between the first two touch points when they started.</summary>
    private Point _initialCenter;

    private void TouchDown(object sender, TouchEventArgs e)
    {
        if (!HasReceivers || e.Palm) return;

        var location = new Point(e.LocationX, e.LocationY);
        var touch = new TouchPoint(e.ID, location, e.Primary);
        _activeTouches[e.ID] = touch;

        // Initialize multi-touch gesture tracking when second finger touches
        if (_activeTouches.Count == 2)
        {
            var touches = _activeTouches.Values.ToList();
            _initialDistance = GetDistance(touches[0].CurrentLocation, touches[1].CurrentLocation);
            _initialAngle = GetAngle(touches[0].CurrentLocation, touches[1].CurrentLocation);
            _initialCenter = GetCenter(touches[0].CurrentLocation, touches[1].CurrentLocation);
        }
    }

    private void TouchMove(object sender, TouchEventArgs e)
    {
        if (!HasReceivers || e.Palm || !_activeTouches.ContainsKey(e.ID)) return;

        var touch = _activeTouches[e.ID];
        var oldLocation = touch.CurrentLocation;
        touch.CurrentLocation = new Point(e.LocationX, e.LocationY);

        if (_activeTouches.Count == 1)
        {
            // Single-finger: hover or drag
            OnHover(touch.CurrentLocation);

            // Check if this is a drag (moved beyond tap accuracy)
            var totalDelta = new Size(
                touch.CurrentLocation.X - touch.OriginalLocation.X,
                touch.CurrentLocation.Y - touch.OriginalLocation.Y);

            if (Math.Abs(totalDelta.Width) > TapAccuracy || Math.Abs(totalDelta.Height) > TapAccuracy)
            {
                // Treat single-finger drag as panning
                var delta = new Size(
                    touch.CurrentLocation.X - oldLocation.X,
                    touch.CurrentLocation.Y - oldLocation.Y);

                OnNavigate(
                    translation: new(
                        TouchSensitivity * -delta.Width,
                        TouchSensitivity * delta.Height,
                        0),
                    rotation: new());
            }
        }
        else if (_activeTouches.Count == 2)
        {
            // Two-finger: pinch-to-zoom, pan, and rotate
            var touches = _activeTouches.Values.ToList();
            var currentDistance = GetDistance(touches[0].CurrentLocation, touches[1].CurrentLocation);
            var currentAngle = GetAngle(touches[0].CurrentLocation, touches[1].CurrentLocation);
            var currentCenter = GetCenter(touches[0].CurrentLocation, touches[1].CurrentLocation);

            // Calculate zoom based on pinch distance change
            var zoomDelta = (currentDistance - _initialDistance) * TouchSensitivity * ZoomSensitivity;

            // Calculate rotation based on angle change
            var rotationDelta = (currentAngle - _initialAngle) * TouchSensitivity;

            // Calculate pan based on center point movement
            var panDelta = new Size(
                currentCenter.X - _initialCenter.X,
                currentCenter.Y - _initialCenter.Y);

            OnNavigate(
                translation: new(
                    TouchSensitivity * -panDelta.Width,
                    TouchSensitivity * panDelta.Height,
                    zoomDelta),
                rotation: new(0, 0, -rotationDelta));

            // Update initial values for continuous gesture (creates smooth relative movement)
            _initialDistance = currentDistance;
            _initialAngle = currentAngle;
            _initialCenter = currentCenter;
        }
    }

    private void TouchUp(object sender, TouchEventArgs e)
    {
        if (!HasReceivers || !_activeTouches.ContainsKey(e.ID)) return;

        var touch = _activeTouches[e.ID];

        // Check if this was a tap (didn't move much) and was the only active touch
        if (_activeTouches.Count == 1)
        {
            var totalDelta = new Size(
                touch.CurrentLocation.X - touch.OriginalLocation.X,
                touch.CurrentLocation.Y - touch.OriginalLocation.Y);

            if (Math.Abs(totalDelta.Width) <= TapAccuracy && Math.Abs(totalDelta.Height) <= TapAccuracy)
            {
                // Treat as a click
                OnClick(new MouseEventArgs(MouseButtons.Left, 1, touch.CurrentLocation.X, touch.CurrentLocation.Y, 0));
            }
        }

        _activeTouches.Remove(e.ID);

        // Reset multi-touch state when dropping below 2 fingers
        if (_activeTouches.Count < 2)
        {
            _initialDistance = 0;
            _initialAngle = 0;
            _initialCenter = Point.Empty;
        }
        // Re-initialize for remaining touch if we had 2+ and now have exactly 2
        else if (_activeTouches.Count == 2)
        {
            var touches = _activeTouches.Values.ToList();
            _initialDistance = GetDistance(touches[0].CurrentLocation, touches[1].CurrentLocation);
            _initialAngle = GetAngle(touches[0].CurrentLocation, touches[1].CurrentLocation);
            _initialCenter = GetCenter(touches[0].CurrentLocation, touches[1].CurrentLocation);
        }
    }

    /// <summary>
    /// Calculates the distance between two points.
    /// </summary>
    private static double GetDistance(Point p1, Point p2)
    {
        var dx = p2.X - p1.X;
        var dy = p2.Y - p1.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Calculates the angle (in degrees) between two points.
    /// </summary>
    private static double GetAngle(Point p1, Point p2)
    {
        var dx = p2.X - p1.X;
        var dy = p2.Y - p1.Y;
        return Math.Atan2(dy, dx) * 180.0 / Math.PI;
    }

    /// <summary>
    /// Calculates the center point between two points.
    /// </summary>
    private static Point GetCenter(Point p1, Point p2)
        => new((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        { // This block will only be executed on manual disposal, not by Garbage Collection
            // Stop tracking input events
            _control.TouchDown -= TouchDown;
            _control.TouchMove -= TouchMove;
            _control.TouchUp -= TouchUp;
        }
    }
}
