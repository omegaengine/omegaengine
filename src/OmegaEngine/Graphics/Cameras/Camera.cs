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
using NanoByte.Common;
using OmegaEngine.Foundation.Geometry;

namespace OmegaEngine.Graphics.Cameras;

/// <summary>
/// Determines the perspective from which a <see cref="Scene"/> is displayed.
/// </summary>
/// <seealso cref="OmegaEngine.Graphics.View.Camera"/>
public abstract partial class Camera : IPositionable
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

    protected DoubleVector3 PositionBaseCached;

    /// <summary>
    /// A value that is subtracted from all positions (including the <see cref="Camera"/>'s) before handing them to the graphics hardware
    /// </summary>
    /// <remarks>Used to improve floating-point precision by keeping effective values small</remarks>
    /// <seealso cref="IPositionableOffset"/>
    [Description("A value that is subtracted from all positions (including the camera's) before handing them to the graphics hardware"), Category("Behavior")]
    public DoubleVector3 PositionBase
    {
        get
        {
            UpdateView(); // Some cameras automatically update their positions
            return PositionBaseCached;
        }
        set => value.To(ref PositionBaseCached, ref ViewDirty, ref ViewFrustumDirty);
    }

    /// <summary>
    /// The maximum distance between <see cref="Position"/> and <see cref="PositionBase"/> before <see cref="PositionBase"/> is automatically adjusted, to avoid floating point errors.
    /// </summary>
    [DefaultValue(10000f), Description("The maximum distance between Position and PositionBase before PositionBase is automatically adjusted, to avoid floating point errors"), Category("Behavior")]
    public float MaxPositionOffset { get; set; } = 10000f;

    /// <summary>
    /// Adjusts <see cref="PositionBase"/> if <see cref="Position"/> is too far away, to avoid floating point errors.
    /// </summary>
    protected void AdjustPositionBase()
    {
        if (PositionCached.ApplyOffset(PositionBaseCached).Length() > MaxPositionOffset)
            PositionBase = PositionCached;
    }

    private Size _size;

    /// <summary>
    /// The size of the output (i.e. screen size)
    /// </summary>
    internal Size Size { get => _size; set => value.To(ref _size, ref ProjectionDirty, ref ViewFrustumDirty); }

    private float _fieldOfView = (float)Math.PI / 4.0f;

    /// <summary>
    /// The view angle in degrees
    /// </summary>
    [DefaultValue(45f), Description("The view angle in degrees"), Category("Layout")]
    public float FieldOfView { get => _fieldOfView.RadianToDegree(); set => value.DegreeToRadian().To(ref _fieldOfView, ref ProjectionDirty, ref ViewFrustumDirty); }

    private float _nearClip = 20.0f;

    /// <summary>
    /// Minimum distance of objects to the camera
    /// </summary>
    [DefaultValue(20.0f), Description("Minimum distance of objects to the camera"), Category("Clipping")]
    public float NearClip { get => _nearClip; set => value.To(ref _nearClip, ref ProjectionDirty, ref ViewFrustumDirty); }

    private float _farClip = 1e+6f;

    /// <summary>
    /// Maximum distance of objects to the camera
    /// </summary>
    [DefaultValue(1e+6f), Description("Maximum distance of objects to the camera"), Category("Clipping")]
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

    /// <summary>
    /// Called when the user changes the view perspective.
    /// </summary>
    /// <param name="translation">Movement in pixels. X = pan left-to-right, Y = pan top-to-bottom, Z = zoom / into screen</param>
    /// <param name="rotation">Rotation in degrees. X = yaw clockwise, Y = pitch clockwise, Z = roll clockwise</param>
    public abstract void PerspectiveChange(DoubleVector3 translation, DoubleVector3 rotation);
}
