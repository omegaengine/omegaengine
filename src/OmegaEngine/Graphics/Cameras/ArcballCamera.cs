/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using NanoByte.Common;
using OmegaEngine.Foundation.Geometry;
using SlimDX;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Cameras;

/// <summary>
/// A camera that can be rotated around a specific point in space.
/// </summary>
/// <param name="minRadius">The minimum radius allowed. Also used as the initial radius.</param>
/// <param name="maxRadius">The maximum radius allowed.</param>
public sealed class ArcballCamera(double minRadius = 50, double maxRadius = 100) : MatrixCamera
{
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

    private double _horizontalRotation;

    /// <summary>
    /// The clockwise horizontal rotation around the target in degrees.
    /// </summary>
    [Description("The clockwise horizontal rotation around the target in degrees."), Category("Layout")]
    public double HorizontalRotation
    {
        get => _horizontalRotation.RadianToDegree();
        set
        {
            #region Sanity checks
            if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            #endregion

            RadianWrapAround(value.DegreeToRadian())
               .To(ref _horizontalRotation, ref ViewDirty, ref ViewFrustumDirty);
        }
    }

    private double _verticalRotation;

    /// <summary>
    /// The clockwise vertical rotation around the target in degrees.
    /// </summary>
    [Description("The clockwise vertical rotation around the target in degrees."), Category("Layout")]
    public double VerticalRotation
    {
        get => _verticalRotation.RadianToDegree();
        set
        {
            #region Sanity checks
            if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            #endregion

            RadianWrapAround(value.DegreeToRadian())
               .To(ref _verticalRotation, ref ViewDirty, ref ViewFrustumDirty);
        }
    }

    private double _roll;

    /// <summary>
    /// The clockwise roll along the view direction in degrees.
    /// </summary>
    [Description("The clockwise roll along the view direction in degrees."), Category("Layout")]
    public double Roll
    {
        get => _roll.RadianToDegree();
        set
        {
            #region Sanity checks
            if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            #endregion

            RadianWrapAround(value.DegreeToRadian())
               .To(ref _roll, ref ViewDirty, ref ViewFrustumDirty);
        }
    }

    /// <summary>
    /// Keep rotations between 0 and 2PI.
    /// </summary>
    private static double RadianWrapAround(double value)
        => value is < 0 or > 2 * Math.PI
            ? (value % (2 * Math.PI) + 2 * Math.PI) % (2 * Math.PI)
            : value;

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

    private DoubleVector3 ApplyWorldUp(DoubleVector3 vector)
        => vector.RotateAroundAxis(
            _defaultWorldUp.CrossProduct(_worldUp).Normalize(),
            Math.Acos(_defaultWorldUp.DotProduct(_worldUp)));

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

    /// <inheritdoc/>
    public override void Navigate(DoubleVector3 translation, DoubleVector3 rotation)
    {
        var viewDir = (Target - PositionCached).Normalize();
        translation = translation.RotateAroundAxis(viewDir, _roll);

        Target += ApplyWorldUp(new DoubleVector3(
            translation.X * Math.Cos(_horizontalRotation),
            translation.Y,
            translation.X * -Math.Sin(_horizontalRotation)) * Radius);
        Radius *= Math.Pow(1.1, -16 * translation.Z);

        HorizontalRotation += rotation.X;
        VerticalRotation += rotation.Y;
        Roll += rotation.Z;
    }

    /// <summary>
    /// Update cached versions of <see cref="View"/> and related matrices.
    /// </summary>
    protected override void UpdateView()
    {
        // Only execute this if the view has changed
        if (!ViewDirty) return;

        var viewDirection = ApplyWorldUp(new DoubleVector3(
            -Math.Cos(_verticalRotation) * Math.Sin(_horizontalRotation),
            -Math.Sin(_verticalRotation),
            -Math.Cos(_verticalRotation) * Math.Cos(_horizontalRotation)));
        PositionCached = Target - _radius * viewDirection;

        bool upsideDown = _verticalRotation is > Math.PI / 2 and < Math.PI / 2 * 3;
        UpVector = (Vector3)(upsideDown ? -1 * _worldUp : _worldUp).RotateAroundAxis(viewDirection, _roll);

        base.UpdateView();

        ViewDirty = false;
    }
}
