using System;
using JetBrains.Annotations;
using SlimDX;

namespace OmegaEngine.Graphics.Renderables;

/// <summary>
/// Provides extension methods for <see cref="BoundingBox"/> and <see cref="BoundingSphere"/>.
/// </summary>
public static class BoundingExtensions
{
    /// <summary>
    /// Applies a matrix transform to a bounding box.
    /// </summary>
    /// <param name="box">The bounding box to apply the transform to.</param>
    /// <param name="matrix">The transformation matrix to apply.</param>
    /// <returns>The transformed bounding box.</returns>
    [Pure]
    public static BoundingBox Transform(this BoundingBox box, Matrix matrix)
    {
        var minimum = new Vector3(matrix.M41, matrix.M42, matrix.M43);
        var maximum = minimum;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                float a = matrix[i, j] * box.Minimum[i];
                float b = matrix[i, j] * box.Maximum[i];
                if (a < b)
                {
                    minimum[j] += a;
                    maximum[j] += b;
                }
                else
                {
                    minimum[j] += b;
                    maximum[j] += a;
                }
            }
        }

        return new(minimum, maximum);
    }

    /// <summary>
    /// Applies a matrix transform to a bounding sphere.
    /// </summary>
    /// <param name="sphere">The bounding sphere to apply the transform to.</param>
    /// <param name="matrix">The transformation matrix to apply.</param>
    /// <returns>The transformed bounding sphere.</returns>
    [Pure]
    public static BoundingSphere Transform(this BoundingSphere sphere, Matrix matrix)
        => new(
            center: Vector3.TransformCoordinate(sphere.Center, matrix),
            radius: sphere.Radius * Math.Max(
                Math.Max(
                    new Vector3(matrix.M11, matrix.M12, matrix.M13).Length(),
                    new Vector3(matrix.M21, matrix.M22, matrix.M23).Length()),
                new Vector3(matrix.M31, matrix.M32, matrix.M33).Length()));
}
