/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using NanoByte.Common;
using OmegaEngine.Foundation.Design;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Input;

namespace OmegaEngine.Graphics.Cameras;

/// <summary>
/// Determines the perspective from which a <see cref="Scene"/> is displayed.
/// </summary>
/// <seealso cref="OmegaEngine.Graphics.View.Camera"/>
public abstract partial class Camera : InputReceiverBase, IPositionable
{
    /// <summary>
    /// Text value to make it easier to identify a particular camera
    /// </summary>
    [Description("Text value to make it easier to identify a particular camera"), Category("Design")]
    public string? Name { get; set; }

    public override string ToString()
    {
        string value = GetType().Name;
        if (!string.IsNullOrEmpty(Name))
            value += $": {Name}";
        return value;
    }

    protected DoubleVector3 PositionCached;

    /// <summary>
    /// The camera's position in 3D-space
    /// </summary>
    [Description("The camera's position in 3D-space"), Category("Layout")]
    public DoubleVector3 Position
    {
        get
        {
            UpdateView(); // Some cameras automatically update their positions
            return PositionCached;
        }
        set => value.To(ref PositionCached, ref ViewDirty, ref ViewFrustumDirty);
    }

    protected DoubleVector3 FloatingOriginCached;

    /// <summary>
    /// The origin used to transform 64-bit positions into 32-bit render coordinates
    /// </summary>
    /// <seealso cref="IFloatingOriginAware"/>
    [Description("The origin used to transform 64-bit positions into 32-bit render coordinates"), Category("Behavior")]
    public DoubleVector3 FloatingOrigin
    {
        get
        {
            UpdateView(); // Some cameras automatically update their positions
            return FloatingOriginCached;
        }
        set => value.To(ref FloatingOriginCached, ref ViewDirty, ref ViewFrustumDirty);
    }

    /// <summary>
    /// The maximum distance <see cref="FloatingOrigin"/> can have from <see cref="Position"/> before it is automatically reset
    /// </summary>
    [DefaultValue(10000f), Description("The maximum distance FloatingOrigin can have from Position before it is automatically reset"), Category("Behavior")]
    public float FloatingOriginMaxDistance { get; set; } = 10000f;

    /// <summary>
    /// Adjusts <see cref="FloatingOrigin"/> if <see cref="Position"/> is too far away.
    /// </summary>
    protected void AdjustFloatingOrigin()
    {
        if (PositionCached.ApplyOffset(FloatingOriginCached).Length() > FloatingOriginMaxDistance)
            FloatingOrigin = PositionCached;
    }

    private Size _size;

    /// <summary>
    /// The size of the output (i.e. screen size)
    /// </summary>
    internal Size Size { get => _size; set => value.To(ref _size, ref ProjectionDirty, ref ViewFrustumDirty); }

    private float _fieldOfView = (float)Math.PI / 4f;

    /// <summary>
    /// The view angle in degrees
    /// </summary>
    [DefaultValue(45f), Description("The view angle in degrees"), Category("Layout")]
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
    public float FieldOfView { get => _fieldOfView.RadianToDegree(); set => value.DegreeToRadian().To(ref _fieldOfView, ref ProjectionDirty, ref ViewFrustumDirty); }

    private float _nearClip = 1f;

    /// <summary>
    /// Minimum distance of objects to the camera
    /// </summary>
    [DefaultValue(1f), Description("Minimum distance of objects to the camera"), Category("Clipping")]
    public float NearClip { get => _nearClip; set => value.To(ref _nearClip, ref ProjectionDirty, ref ViewFrustumDirty); }

    private float _farClip = 100_000f;

    /// <summary>
    /// Maximum distance of objects to the camera
    /// </summary>
    [DefaultValue(100_000f), Description("Maximum distance of objects to the camera"), Category("Clipping")]
    public float FarClip { get => _farClip; set => value.To(ref _farClip, ref ProjectionDirty, ref ViewFrustumDirty); }

    private DoublePlane _clipPlane;

    /// <summary>
    /// A custom clip plane behind which all objects are culled
    /// </summary>
    [DefaultValue(typeof(DoublePlane), "0; 0; 0; 0; 0; 0"), Description("A custom clip plane behind which all objects are culled"), Category("Clipping")]
    public DoublePlane ClipPlane { get => _clipPlane; set => value.To(ref _clipPlane, ref ProjectionDirty, ref ViewFrustumDirty); }

    /// <summary>
    /// Shall the engine use view frustum culling to optimize the rendering performance?
    /// </summary>
    [DefaultValue(true), Description("Shall the engine use view frustum culling to optimize the rendering performance?"), Category("Behavior")]
    public bool FrustumCulling { get; set; } = true;
}
