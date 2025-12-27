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
/// An RTS-style camera with an adjustable horizontal rotation and an automatic pitch angle.
/// </summary>
/// <param name="heightController">This delegate is called to control the minimum height of the strategy camera based on its 2D coordinates.</param>
public class StrategyCamera(Func<DoubleVector3, double> heightController) : ZoomCamera
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

    private double _rotation;

    /// <summary>
    /// The clockwise horizontal rotation around the target in degrees.
    /// </summary>
    [Description("The clockwise horizontal rotation around the target in degrees."), Category("Layout")]
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
    public double Rotation
    {
        get => _rotation.RadianToDegree();
        set
        {
            #region Sanity checks
            if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            #endregion

            value.DegreeToRadian()
                 .Modulo(2 * Math.PI)
                 .To(ref _rotation, ref ViewDirty, ref ViewFrustumDirty);
        }
    }

    private float _minPitch = 25f.DegreeToRadian();

    /// <summary>
    /// The minimum pitch angle in degrees. Effective when <see cref="ZoomCamera.Radius"/> is equal to <see cref="ZoomCamera.MinRadius"/>.
    /// </summary>
    [FloatRange(0, 90), Description("The minimum pitch angle in degrees. Effective when Radius is equal to MinRadius."), Category("Behavior")]
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
    public float MinPitch
    {
        get => _minPitch.RadianToDegree();
        set
        {
            #region Sanity checks
            if (float.IsInfinity(value) || float.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), Resources.ValueNotPositive);
            if (value >= 90) throw new ArgumentOutOfRangeException(nameof(value), Resources.AngleNotBelow90);
            #endregion

            value.DegreeToRadian().To(ref _minPitch, ref ViewDirty, ref ViewFrustumDirty);
        }
    }

    private float _maxPitch = 60f.DegreeToRadian();

    /// <summary>
    /// The maximum pitch angle in degrees. Effective when <see cref="ZoomCamera.Radius"/> is equal to <see cref="ZoomCamera.MaxRadius"/>.
    /// </summary>
    [FloatRange(0, 90), Description("The maximum pitch angle in degrees. Effective when Radius is equal to MaxRadius."), Category("Behavior")]
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
    public float MaxPitch
    {
        get => _maxPitch.RadianToDegree();
        set
        {
            #region Sanity checks
            if (float.IsInfinity(value) || float.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), Resources.ValueNotPositive);
            if (value >= 90) throw new ArgumentOutOfRangeException(nameof(value), Resources.AngleNotBelow90);
            #endregion

            value.DegreeToRadian().To(ref _maxPitch, ref ViewDirty, ref ViewFrustumDirty);
        }
    }

    /// <summary>
    /// Controls the sensitivity of movement.
    /// </summary>
    [FloatRange(0, 100), Description("Controls the sensitivity of movement."), Category("Behavior")]
    [Editor(typeof(SliderEditor), typeof(UITypeEditor))]
    public double MovementSensitivity { get; set; } = 0.01;

    /// <inheritdoc/>
    public override void Navigate(DoubleVector3 translation = default, DoubleVector3 rotation = default)
    {
        Target += Radius * MovementSensitivity *
                  new DoubleVector3(
                      Math.Cos(_rotation) * translation.X + Math.Sin(_rotation) * translation.Y,
                      0,
                      Math.Cos(_rotation) * translation.Y - Math.Sin(_rotation) * translation.X);

        Rotation += rotation.X + rotation.Z;

        base.Navigate(translation, rotation);
    }

    /// <inheritdoc/>
    protected override void UpdateView()
    {
        if (!ViewDirty) return;

        // Calculate variable pitch based on current radius
        double pitch = (_minPitch - _maxPitch) / (MinRadius - MaxRadius) * Radius +
            _minPitch - (_minPitch - _maxPitch) / (MinRadius - MaxRadius) * MinRadius;
        while (pitch > 2 * Math.PI) pitch -= 2 * Math.PI;
        while (pitch < 0) pitch += 2 * Math.PI;

        var newPosition = Radius * new DoubleVector3(
            Math.Cos(pitch) * -Math.Sin(_rotation),
            Math.Sin(pitch),
            Math.Cos(pitch) * -Math.Cos(_rotation));

        // Translate these coordinates by the target object's spacial location
        PositionCached = newPosition + Target;

        // Prevent camera from going under terrain
        PositionCached.Y = Math.Max(PositionCached.Y, heightController(PositionCached));

        base.UpdateView();
    }
}
