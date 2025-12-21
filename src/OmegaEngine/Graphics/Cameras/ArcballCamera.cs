/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Drawing.Design;
using NanoByte.Common;
using OmegaEngine.Foundation.Design;
using OmegaEngine.Foundation.Geometry;
using SlimDX;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Cameras;

/// <summary>
/// A camera that can be rotated around a specific point in space.
/// </summary>
/// <param name="minRadius">The minimum radius allowed. Also used as the initial radius.</param>
/// <param name="maxRadius">The maximum radius allowed.</param>
public sealed class ArcballCamera(double minRadius = 50, double maxRadius = 100)
    : ZoomCamera(minRadius, maxRadius)
{
    private double _yaw;

    /// <summary>
    /// The clockwise horizontal rotation of the camera around the target in degrees.
    /// </summary>
    [Description("The clockwise horizontal rotation of the camera around the target in degrees.."), Category("Layout")]
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
    public double Yaw
    {
        get => _yaw.RadianToDegree();
        set
        {
            #region Sanity checks
            if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            #endregion

            value.DegreeToRadian()
                 .Modulo(2 * Math.PI)
                 .To(ref _yaw, ref ViewDirty, ref ViewFrustumDirty);
        }
    }

    private double _pitch;

    /// <summary>
    /// The vertical rotation upwards of the camera around the target in degrees.
    /// </summary>
    [Description("The vertical rotation upwards of the camera around the target in degrees."), Category("Layout")]
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
    public double Pitch
    {
        get => _pitch.RadianToDegree();
        set
        {
            #region Sanity checks
            if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            #endregion

            PreventGimbalLock(value.DegreeToRadian().Modulo(2 * Math.PI))
               .To(ref _pitch, ref ViewDirty, ref ViewFrustumDirty);
        }
    }

    private const double
        QuarterCircle = Math.PI / 2,
        ThreeQuarterCircle = Math.PI * 3 / 2,
        Epsilon = 0.000001;

    private static double PreventGimbalLock(double value)
        => value switch
        {
            > QuarterCircle - Epsilon and <= QuarterCircle => QuarterCircle + Epsilon,
            > QuarterCircle and < QuarterCircle + Epsilon => QuarterCircle - Epsilon,
            > ThreeQuarterCircle - Epsilon and <= ThreeQuarterCircle => ThreeQuarterCircle + Epsilon,
            > ThreeQuarterCircle and < ThreeQuarterCircle + Epsilon => ThreeQuarterCircle - Epsilon,
            _ => value
        };

    private bool IsUpsideDown => _pitch is > QuarterCircle and < ThreeQuarterCircle;

    private double _roll;

    /// <summary>
    /// The counter-clockwise roll along the view direction in degrees.
    /// </summary>
    [Description("The counter-clockwise roll along the view direction in degrees."), Category("Layout")]
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
    public double Roll
    {
        get => _roll.RadianToDegree();
        set
        {
            #region Sanity checks
            if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            #endregion

            value.DegreeToRadian()
                 .Modulo(2 * Math.PI)
                 .To(ref _roll, ref ViewDirty, ref ViewFrustumDirty);
        }
    }

    private static readonly DoubleVector3 _defaultWorldUp = new(0, 1, 0);
    private DoubleVector3 _worldUp = _defaultWorldUp;

    /// <summary>
    /// A unit vector describing the direction considered up in the world.
    /// </summary>
    [Description("A unit vector describing the direction considered up in the world."), Category("Layout")]
    public DoubleVector3 WorldUp
    {
        get => _worldUp;
        set => value.Normalize().To(ref _worldUp, ref ViewDirty, ref ViewFrustumDirty);
    }

    /// <summary>
    /// Controls the sensitivity of movement.
    /// </summary>
    [FloatRange(0, 100), Description("Controls the sensitivity of movement."), Category("Behavior")]
    [Editor(typeof(SliderEditor), typeof(UITypeEditor))]
    public double MovementSensitivity { get; set; } = 0.01;

    /// <inheritdoc/>
    public override void Navigate(DoubleVector3 translation, DoubleVector3 rotation)
    {
        var viewDir = (Target - PositionCached).Normalize();

        Target += Radius * MovementSensitivity *
                  new DoubleVector3(
                          translation.X * -Math.Cos(_yaw),
                          IsUpsideDown ? -translation.Y : translation.Y,
                          translation.X * Math.Sin(_yaw))
                     .AdjustReference(from: _defaultWorldUp, to: _worldUp)
                     .RotateAroundAxis(viewDir, -_roll);

        Yaw += rotation.X;
        Pitch -= rotation.Y;
        Roll += rotation.Z;

        base.Navigate(translation, rotation);
    }

    /// <inheritdoc/>
    protected override void UpdateView()
    {
        if (!ViewDirty) return;

        var viewDirection = new DoubleVector3(
                -Math.Cos(_pitch) * Math.Sin(_yaw),
                -Math.Sin(_pitch),
                -Math.Cos(_pitch) * Math.Cos(_yaw))
           .AdjustReference(from: _defaultWorldUp, to: _worldUp);
        PositionCached = Target - Radius * viewDirection;

        Up = (Vector3)(IsUpsideDown ? -1 * _worldUp : _worldUp).RotateAroundAxis(viewDirection, -_roll);

        base.UpdateView();
    }
}
