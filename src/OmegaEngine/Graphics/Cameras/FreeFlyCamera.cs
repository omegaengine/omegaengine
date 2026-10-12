using System;
using System.ComponentModel;
using System.Drawing.Design;
using OmegaEngine.Foundation.Design;
using OmegaEngine.Foundation.Geometry;
using System.Numerics;
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
        get => ((DoubleVector3)Quaternion.GetAxis()).Normalize();
        set => Quaternion = Quaternion.CreateFromAxisAngle(VectorMath.NormalizeOrZero((Vector3)value), Quaternion.GetAngle());
    }

    /// <summary>
    /// The rotation around the <see cref="Axis"/> in degrees.
    /// </summary>
    [Description("The rotation around the axis in degrees."), Category("Layout")]
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
    public float Angle
    {
        get => Quaternion.GetAngle().RadianToDegree();
        set
        {
            #region Sanity checks
            if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), Resources.NumberNotReal);
            #endregion

            Quaternion = Quaternion.CreateFromAxisAngle(VectorMath.NormalizeOrZero(Quaternion.GetAxis()), value.DegreeToRadian());
        }
    }

    /// <inheritdoc/>
    public override void Navigate(DoubleVector3 translation = default, DoubleVector3 rotation = default)
    {
        Position += translation.RotateAroundAxis(Axis, Quaternion.GetAngle());
        // System.Numerics multiplies quaternions in the opposite order to D3DX: the right-hand rotation is applied first
        Quaternion = Quaternion.CreateFromYawPitchRoll(
            (float)-rotation.X.DegreeToRadian(),
            (float)rotation.Y.DegreeToRadian(),
            (float)rotation.Z.DegreeToRadian()) * Quaternion;
    }
}
