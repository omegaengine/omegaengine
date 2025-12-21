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
/// A first-person camera that simulates human-like movement with a fixed "up" direction.
/// </summary>
public sealed class FirstPersonCamera : QuaternionCamera
{
    private double _yaw;

    /// <summary>
    /// The horizontal rotation to the right in degrees.
    /// </summary>
    [Description("The horizontal rotation to the right in degrees."), Category("Layout")]
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
    /// The vertical rotation upwards in degrees.
    /// </summary>
    [Description("The vertical rotation upwards in degrees."), Category("Layout")]
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
    public double Pitch
    {
        get => _pitch.RadianToDegree();
        set
        {
            #region Sanity checks
            if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            #endregion

            value.DegreeToRadian()
                 .Clamp(-Math.PI / 2, Math.PI / 2)
                 .To(ref _pitch, ref ViewDirty, ref ViewFrustumDirty);
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

    /// <inheritdoc/>
    public override void Navigate(DoubleVector3 translation, DoubleVector3 rotation)
    {
        Position += translation
                   .RotateAroundAxis(new(0, 1, 0), _yaw)
                   .AdjustReference(from: _defaultWorldUp, to: _worldUp);

        Yaw += rotation.X;
        Pitch += rotation.Y;
    }

    /// <inheritdoc/>
    protected override void UpdateView()
    {
        if (!ViewDirty) return;

        (var axis, double rotation) = _defaultWorldUp.GetRotationTo(_worldUp);
        Quaternion = Quaternion.RotationAxis((Vector3)axis, (float)rotation)
                   * Quaternion.RotationYawPitchRoll((float)-_yaw, pitch: 0, roll: 0)
                   * Quaternion.RotationYawPitchRoll(yaw: 0, (float)_pitch, roll: 0);

        base.UpdateView();
    }
}
