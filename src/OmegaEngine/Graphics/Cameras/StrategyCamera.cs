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
/// A RTS-style camera with a rotatable horizontal view and an automatic vertical angle.
/// </summary>
/// <param name="minRadius">The minimum radius allowed. Also used as the initial radius.</param>
/// <param name="maxRadius">The maximum radius allowed.</param>
/// <param name="minAngle">The minimum vertical angle in degrees. Effective when <see cref="Radius"/> is equal to <see cref="MinRadius"/>.</param>
/// <param name="maxAngle">The maximum vertical angle in degrees. Effective when <see cref="Radius"/> is equal to <see cref="MaxRadius"/>.</param>
/// <param name="heightController">This delegate is called to control the minimum height of the strategy camera based on its 2D coordinates.</param>
public class StrategyCamera(double minRadius, double maxRadius, float minAngle, float maxAngle, Func<DoubleVector3, double> heightController) : MatrixCamera
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

    private double _radius = minRadius;

    /// <summary>
    /// The distance between the camera and the center of the focuses object.
    /// </summary>
    /// <remarks>Must be a positive real number.</remarks>
    [Description("The distance between the camera and the center of the focuses object."), Category("Layout")]
    public double Radius
    {
        get => _radius;
        set
        {
            #region Sanity checks
            if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), Resources.ValueNotPositive);
            #endregion

            // Apply limits (in case of conflict minimum is more important than maximum)
            value = Math.Max(Math.Min(value, MaxRadius), MinRadius);

            value.To(ref _radius, ref ViewDirty, ref ViewFrustumDirty);
        }
    }

    private float _horizontalRotation;

    /// <summary>
    /// The horizontal rotation in degrees.
    /// </summary>
    /// <remarks>Must be a real number.</remarks>
    [Description("The horizontal rotation in degrees."), Category("Layout")]
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
    public float HorizontalRotation
    {
        get => _horizontalRotation.RadianToDegree();
        set
        {
            #region Sanity checks
            if (float.IsInfinity(value) || float.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            #endregion

            value.DegreeToRadian().To(ref _horizontalRotation, ref ViewDirty, ref ViewFrustumDirty);
        }
    }

    private double _minRadius = minRadius;

    /// <summary>
    /// The minimum radius allowed.
    /// </summary>
    /// <remarks>Must be a positive real number.</remarks>
    [Description("The minimum radius allowed."), Category("Behavior")]
    public double MinRadius
    {
        get => _minRadius;
        set
        {
            #region Sanity checks
            if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), Resources.ValueNotPositive);
            #endregion

            value.To(ref _minRadius, ref ViewDirty, ref ViewFrustumDirty);
        }
    }

    private double _maxRadius = maxRadius;

    /// <summary>
    /// The maximum radius allowed.
    /// </summary>
    /// <remarks>Must be a positive real number.</remarks>
    [Description("The maximum radius allowed."), Category("Behavior")]
    public double MaxRadius
    {
        get => _maxRadius;
        set
        {
            #region Sanity checks
            if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), Resources.ValueNotPositive);
            #endregion

            value.To(ref _maxRadius, ref ViewDirty, ref ViewFrustumDirty);
        }
    }

    private float _minAngle = minAngle.DegreeToRadian();

    /// <summary>
    /// The minimum vertical angle in degrees. Effective when <see cref="Radius"/> is equal to <see cref="MinRadius"/>.
    /// </summary>
    /// <remarks>Must be a real number.</remarks>
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
    /// The maximum vertical angle in degrees. Effective when <see cref="Radius"/> is equal to <see cref="MaxRadius"/>.
    /// </summary>
    /// <remarks>Must be a real number.</remarks>
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

    /// <inheritdoc/>
    public override void PerspectiveChange(float panX, float panY, float rotation, float zoom)
    {
        double x = panX * Radius;
        double y = panY * Radius;
        Target += new DoubleVector3(
            Math.Sin(_horizontalRotation) * y - Math.Cos(_horizontalRotation) * x,
            0,
            Math.Sin(_horizontalRotation) * x + Math.Cos(_horizontalRotation) * y);

        Radius *= zoom;
        HorizontalRotation += rotation;
    }

    /// <summary>
    /// Update cached versions of <see cref="View"/> and related matrices.
    /// </summary>
    protected override void UpdateView()
    {
        // Only execute this if the view has changed
        if (!ViewDirty) return;

        // Keep radius within boundaries
        if (_radius < _minRadius) _radius = _minRadius;
        if (_radius > _maxRadius) _radius = _maxRadius;

        // Calculate variable vertical rotation based on current radius
        double vRotation = (_minAngle - _maxAngle) / (_minRadius - _maxRadius) * _radius +
            _minAngle - (_minAngle - _maxAngle) / (_minRadius - _maxRadius) * _minRadius;
        while (vRotation > 2 * Math.PI) vRotation -= 2 * Math.PI;
        while (vRotation < 0) vRotation += 2 * Math.PI;

        // (radius * Math.Cos(vRotation)) is the temporary radius after the y component shift
        var newPosition = new DoubleVector3(
            _radius * Math.Cos(vRotation) * -Math.Sin(_horizontalRotation),
            _radius * Math.Sin(vRotation),
            _radius * Math.Cos(vRotation) * -Math.Cos(_horizontalRotation));

        // Translate these coordinates by the target object's spacial location
        PositionCached = newPosition + Target;

        // Prevent camera from going under terrain
        PositionCached.Y = Math.Max(PositionCached.Y, heightController(PositionCached));

        base.UpdateView();

        ViewDirty = false;
    }
}
