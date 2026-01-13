/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using JetBrains.Annotations;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics;

/// <summary>
/// Helper methods for <see cref="BoundingBox"/>es and <see cref="BoundingSphere"/>s.
/// </summary>
public static class BoundingBodyUtils
{
    /// <summary>
    /// Generates a <see cref="BoundingBox"/> that completely contains all points within a <see cref="VertexBuffer"/>.
    /// </summary>
    /// <param name="vb">The <see cref="VertexBuffer"/> to be contained within the <see cref="BoundingBox"/>.</param>
    /// <param name="vertexCount">The total number of vertex contained within <paramref name="vb"/>.</param>
    public static BoundingBox ComputeBoundingBox(this VertexBuffer vb, int vertexCount)
        => BoundingBox.FromPoints(vb.GetPoints(vertexCount));

    /// <summary>
    /// Generates a <see cref="BoundingBox"/> that completely contains all points within a <see cref="Mesh"/>.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/> to be contained within the <see cref="BoundingBox"/>.</param>
    public static BoundingBox ComputeBoundingBox(this Mesh mesh)
        => BoundingBox.FromPoints(mesh.GetPoints());

    /// <summary>
    /// Generates a <see cref="BoundingSphere"/> that completely contains all points within a <see cref="VertexBuffer"/>.
    /// </summary>
    /// <param name="vb">The <see cref="VertexBuffer"/> to be contained within the <see cref="BoundingSphere"/>.</param>
    /// <param name="vertexCount">The total number of vertexes contained within <paramref name="vb"/>.</param>
    public static BoundingSphere ComputeBoundingSphere(this VertexBuffer vb, int vertexCount)
        => BoundingSphere.FromPoints(vb.GetPoints(vertexCount));

    /// <summary>
    /// Generates a <see cref="BoundingSphere"/> that completely contains all points within a <see cref="Mesh"/>.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/> to be contained within the <see cref="BoundingSphere"/>.</param>
    public static BoundingSphere ComputeBoundingSphere(this Mesh mesh)
        => BoundingSphere.FromPoints(mesh.GetPoints());

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

    /// <summary>
    /// A vector pointing to the center of the bounding box.
    /// </summary>
    [Pure]
    public static Vector3 Center(this BoundingBox box)
        => (box.Maximum + box.Minimum) / 2;

    /// <summary>
    /// A vector pointing from the minimum corner to the maximum corner of the bounding box.
    /// </summary>
    [Pure]
    public static Vector3 Diagonal(this BoundingBox box)
        => box.Maximum - box.Minimum;
}
