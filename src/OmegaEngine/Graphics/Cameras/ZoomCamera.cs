using System;
using System.ComponentModel;
using System.Drawing.Design;
using NanoByte.Common;
using OmegaEngine.Foundation.Design;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Properties;

namespace OmegaEngine.Graphics.Cameras;

/// <summary>
/// Common base class for cameras that support zooming.
/// </summary>
public abstract class ZoomCamera : MatrixCamera
{
    private double _radius = 2;

    /// <summary>
    /// The distance between the camera and the center of the target.
    /// </summary>
    /// <remarks>Must be a positive real number.</remarks>
    [Description("The distance between the camera and the center of the target."), Category("Layout")]
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


    private double _minRadius = 2;

    /// <summary>
    /// The minimum radius allowed.
    /// </summary>
    /// <remarks>Must be a positive real number.</remarks>
    [DefaultValue(2.0), Description("The minimum radius allowed."), Category("Behavior")]
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

    private double _maxRadius = 10000;

    /// <summary>
    /// The maximum radius allowed.
    /// </summary>
    /// <remarks>Must be a positive real number.</remarks>
    [DefaultValue(10000.0), Description("The maximum radius allowed."), Category("Behavior")]
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

    /// <summary>
    /// Base factor used for exponential zoom scaling.
    /// </summary>
    [FloatRange(0, 2), Description("Base factor used for exponential zoom scaling."), Category("Behavior")]
    [Editor(typeof(SliderEditor), typeof(UITypeEditor))]
    public double ZoomBase { get; set; } = 1.1;

    /// <summary>
    /// Controls the sensitivity of zoom operations.
    /// </summary>
    [FloatRange(0, 10), Description("Controls the sensitivity of zoom operations."), Category("Behavior")]
    [Editor(typeof(SliderEditor), typeof(UITypeEditor))]
    public double ZoomSensitivity { get; set; } = 0.15;

    /// <inheritdoc />
    public override void Navigate(DoubleVector3 translation = default, DoubleVector3 rotation = default)
    {
        Radius *= Math.Pow(ZoomBase, -ZoomSensitivity * translation.Z);
    }
}
