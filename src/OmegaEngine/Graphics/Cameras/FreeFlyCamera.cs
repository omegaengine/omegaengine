using NanoByte.Common;
using OmegaEngine.Foundation.Geometry;
using SlimDX;

namespace OmegaEngine.Graphics.Cameras;

/// <summary>
/// A free-flying camera that supports unrestricted movement and rotation in all directions, similar to an editor or spectator camera.
/// </summary>
public class FreeFlyCamera : QuaternionCamera
{
    /// <inheritdoc/>
    public override void Navigate(DoubleVector3 translation, DoubleVector3 rotation)
    {
        Position += translation.RotateAroundAxis((DoubleVector3)Quaternion.Axis, -Quaternion.Angle);
        Quaternion *= Quaternion.RotationYawPitchRoll(
            (float)-rotation.X.DegreeToRadian(),
            (float)rotation.Y.DegreeToRadian(),
            (float)rotation.Z.DegreeToRadian());
    }
}
