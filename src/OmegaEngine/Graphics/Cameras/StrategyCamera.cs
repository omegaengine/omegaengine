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
using OmegaEngine.Properties;

namespace OmegaEngine.Graphics.Cameras;

/// <summary>
/// An RTS-style camera with a rotatable horizontal view and an automatic vertical angle.
/// </summary>
/// <param name="minRadius">The minimum radius allowed. Also used as the initial radius.</param>
/// <param name="maxRadius">The maximum radius allowed.</param>
/// <param name="minAngle">The minimum vertical angle in degrees. Effective when <see cref="ZoomCamera.Radius"/> is equal to <see cref="ZoomCamera.MinRadius"/>.</param>
/// <param name="maxAngle">The maximum vertical angle in degrees. Effective when <see cref="ZoomCamera.Radius"/> is equal to <see cref="ZoomCamera.MaxRadius"/>.</param>
/// <param name="heightController">This delegate is called to control the minimum height of the strategy camera based on its 2D coordinates.</param>
public class StrategyCamera(double minRadius, double maxRadius, float minAngle, float maxAngle, Func<DoubleVector3, double> heightController)
    : ZoomCamera(minRadius, maxRadius)
{
    /// <summary>
    /// The position the camera is looking at.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The coordinates lie outside the range of the height-controlling terrain.</exception>
    [Description("The position the camera is looking at."), Category("Layout")]
    public override DoubleVector3 Target
    {
        get => base.Target;
        set => base.Target = new(
            value.X,
            heightController(value), // Target object on the terrain's surface
            value.Z);
    }

    private double _horizontalRotation;

    /// <summary>
    /// The clockwise horizontal rotation around the target in degrees.
    /// </summary>
    [Description("The clockwise horizontal rotation around the target in degrees."), Category("Layout")]
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
    public double HorizontalRotation
    {
        get => _horizontalRotation.RadianToDegree();
        set
        {
            #region Sanity checks
            if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            #endregion

            value.DegreeToRadian()
                 .Modulo(2 * Math.PI)
                 .To(ref _horizontalRotation, ref ViewDirty, ref ViewFrustumDirty);
        }
    }

    private float _minAngle = minAngle.DegreeToRadian();

    /// <summary>
    /// The minimum vertical angle in degrees. Effective when <see cref="ZoomCamera.Radius"/> is equal to <see cref="ZoomCamera.MinRadius"/>.
    /// </summary>
    [Description("The minimum vertical angle in degrees. Effective when Radius is equal to MinRadius."), Category("Behavior")]
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
    public float MinAngle
    {
        get => _minAngle.RadianToDegree();
        set
        {
            #region Sanity checks
            if (float.IsInfinity(value) || float.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), Resources.ValueNotPositive);
            if (value >= 90) throw new ArgumentOutOfRangeException(nameof(value), Resources.AngleNotBelow90);
            #endregion

            value.DegreeToRadian().To(ref _minAngle, ref ViewDirty, ref ViewFrustumDirty);
        }
    }

    private float _maxAngle = maxAngle.DegreeToRadian();

    /// <summary>
    /// The maximum vertical angle in degrees. Effective when <see cref="ZoomCamera.Radius"/> is equal to <see cref="ZoomCamera.MaxRadius"/>.
    /// </summary>
    [Description("The maximum vertical angle in degrees. Effective when Radius is equal to MaxRadius."), Category("Behavior")]
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
    public float MaxAngle
    {
        get => _maxAngle.RadianToDegree();
        set
        {
            #region Sanity checks
            if (float.IsInfinity(value) || float.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), Resources.ValueNotPositive);
            if (value >= 90) throw new ArgumentOutOfRangeException(nameof(value), Resources.AngleNotBelow90);
            #endregion

            value.DegreeToRadian().To(ref _maxAngle, ref ViewDirty, ref ViewFrustumDirty);
        }
    }

    /// <summary>
    /// Controls the sensitivity of movement.
    /// </summary>
    [Description("Controls the sensitivity of movement."), Category("Behavior")]
    public double MovementSensitivity { get; set; } = 0.01;

    /// <inheritdoc/>
    public override void Navigate(DoubleVector3 translation, DoubleVector3 rotation)
    {
        Target += Radius * MovementSensitivity *
                  new DoubleVector3(
                      Math.Cos(_horizontalRotation) * translation.X + Math.Sin(_horizontalRotation) * translation.Y,
                      0,
                      Math.Cos(_horizontalRotation) * translation.Y - Math.Sin(_horizontalRotation) * translation.X);

        HorizontalRotation += rotation.X;

        base.Navigate(translation, rotation);
    }

    /// <inheritdoc/>
    protected override void UpdateView()
    {
        if (!ViewDirty) return;

        // Calculate variable vertical rotation based on current radius
        double vRotation = (_minAngle - _maxAngle) / (MinRadius - MaxRadius) * Radius +
            _minAngle - (_minAngle - _maxAngle) / (MinRadius - MaxRadius) * MinRadius;
        while (vRotation > 2 * Math.PI) vRotation -= 2 * Math.PI;
        while (vRotation < 0) vRotation += 2 * Math.PI;

        var newPosition = Radius * new DoubleVector3(
            Math.Cos(vRotation) * -Math.Sin(_horizontalRotation),
            Math.Sin(vRotation),
            Math.Cos(vRotation) * -Math.Cos(_horizontalRotation));

        // Translate these coordinates by the target object's spacial location
        PositionCached = newPosition + Target;

        // Prevent camera from going under terrain
        PositionCached.Y = Math.Max(PositionCached.Y, heightController(PositionCached));

        base.UpdateView();
    }
}
