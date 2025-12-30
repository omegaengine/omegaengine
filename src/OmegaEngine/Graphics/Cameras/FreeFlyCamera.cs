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
/// A camera with free 6-degrees-of-freedom movement (movement and rotation in all directions).<para/>
/// Useful for building tools, debugging, or games with free-flying movement (space sims, spectator modes).
/// </summary>
public class FreeFlyCamera : QuaternionCamera
{
    /// <summary>
    /// A unit vector describing the axis around which the camera is rotated.
    /// </summary>
    [Description("A unit vector describing the axis around which the camera is rotated."), Category("Layout")]
    public DoubleVector3 Axis
    {
        get => ((DoubleVector3)Quaternion.Axis).Normalize();
        set => Quaternion = Quaternion.RotationAxis((Vector3)value, Quaternion.Angle);
    }

    /// <summary>
    /// The rotation around the <see cref="Axis"/> in degrees.
    /// </summary>
    [Description("The rotation around the axis in degrees."), Category("Layout")]
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
    public float Angle
    {
        get => Quaternion.Angle.RadianToDegree();
        set
        {
            #region Sanity checks
            if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            #endregion

            Quaternion = Quaternion.RotationAxis(Quaternion.Axis, value.DegreeToRadian());
        }
    }

    /// <inheritdoc/>
    public override void Navigate(DoubleVector3 translation = default, DoubleVector3 rotation = default)
    {
        Position += translation.RotateAroundAxis(Axis, Quaternion.Angle);
        Quaternion *= Quaternion.RotationYawPitchRoll(
            (float)-rotation.X.DegreeToRadian(),
            (float)rotation.Y.DegreeToRadian(),
            (float)rotation.Z.DegreeToRadian());
    }
}
