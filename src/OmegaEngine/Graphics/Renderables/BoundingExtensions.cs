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
        float[] inputMin = new float[3], inputMax = new float[3];
        float[] outputMin = new float[3], outputMax = new float[3];
        var m = new float[3, 3];

        // Copy data into arrays for easy reference
        inputMin[0] = box.Minimum.X;
        inputMax[0] = box.Maximum.X;
        inputMin[1] = box.Minimum.Y;
        inputMax[1] = box.Maximum.Y;
        inputMin[2] = box.Minimum.Z;
        inputMax[2] = box.Maximum.Z;
        m[0, 0] = matrix.M11;
        m[0, 1] = matrix.M12;
        m[0, 2] = matrix.M13;
        m[1, 0] = matrix.M21;
        m[1, 1] = matrix.M22;
        m[1, 2] = matrix.M23;
        m[2, 0] = matrix.M31;
        m[2, 1] = matrix.M32;
        m[2, 2] = matrix.M33;

        // Account for the translation
        outputMin[0] = outputMax[0] = matrix.M41;
        outputMin[1] = outputMax[1] = matrix.M42;
        outputMin[2] = outputMax[2] = matrix.M43;

        // Find the extreme points by considering the product of the min and max with each component of M
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                float a = m[i, j] * inputMin[i];
                float b = m[i, j] * inputMax[i];
                if (a < b)
                {
                    outputMin[j] += a;
                    outputMax[j] += b;
                }
                else
                {
                    outputMin[j] += b;
                    outputMax[j] += a;
                }
            }
        }

        // Copy the result into the new box
        return new(
            new(outputMin[0], outputMin[1], outputMin[2]),
            new(outputMax[0], outputMax[1], outputMax[2]));
    }

    /// <summary>
    /// Applies a matrix transform to a bounding sphere.
    /// </summary>
    /// <param name="sphere">The bounding sphere to apply the transform to.</param>
    /// <param name="matrix">The transformation matrix to apply.</param>
    /// <returns>The transformed bounding sphere.</returns>
    [Pure]
    public static BoundingSphere Transform(this BoundingSphere sphere, Matrix matrix)
    {
        if (sphere.Radius <= 0)
            return new(sphere.Center + new Vector3(matrix.M41, matrix.M42, matrix.M43), radius: 0);

        return new(
            center: Vector3.TransformCoordinate(sphere.Center, matrix),
            radius: sphere.Radius * Math.Max(
                Math.Max(
                    new Vector3(matrix.M11, matrix.M12, matrix.M13).Length(),
                    new Vector3(matrix.M21, matrix.M22, matrix.M23).Length()),
                new Vector3(matrix.M31, matrix.M32, matrix.M33).Length()));
    }
}
