/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using NanoByte.Common;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.Renderables;

// This file contains methods for creating models from builtin templates (boxes, spheres, etc.)
partial class Model
{
    /// <summary>
    /// Creates a model of a textured 2D quad.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
    /// <param name="material">The material used to render the surface of the model; must be <see cref="XMaterial.IsTextured"/>.</param>
    /// <param name="size">The size of the quad.</param>
    /// <exception cref="NotSupportedException">The <paramref name="material"/> is not textured.</exception>
    public static Model Quad(Engine engine, XMaterial material, Vector2 size = default)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        #endregion

        if (size == default) size = new(5);

        if (!material.IsTextured) throw new NotSupportedException("The material must be textured.");
        var mesh = TexturedMesh.Quad(engine.Device, size, material.NeedsTBN);

        return new(mesh, material)
        {
            Engine = engine,
            BoundingSphere = CenteredBoundingSphere(radius: size.Length() / 2),
            BoundingBox = CenteredBoundingBox(corner: new(size / 2, 0))
        };
    }

    /// <summary>
    /// Creates a model of a box.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
    /// <param name="material">The material used to render the surface of the model.</param>
    /// <param name="size">The size of the box</param>
    public static Model Box(Engine engine, XMaterial material, Vector3 size = default)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        #endregion

        if (size == default) size = new(5);

        var mesh = material.IsTextured
            ? TexturedMesh.Box(engine.Device, size, material.NeedsTBN)
            : Mesh.CreateBox(engine.Device, size.X, size.Y, size.Z);

        return new(mesh, material)
        {
            Engine = engine,
            SurfaceEffect = material.IsTextured ? SurfaceEffect.Shader : SurfaceEffect.FixedFunction,
            BoundingSphere = CenteredBoundingSphere(radius: size.Length() / 2),
            BoundingBox = CenteredBoundingBox(corner: size / 2)
        };
    }

    /// <summary>
    /// Creates a model of a sphere with spherical mapping.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
    /// <param name="material">The material used to render the surface of the model.</param>
    /// <param name="radius">The radius of the sphere.</param>
    /// <param name="slices">The number of vertical slices to divide the sphere into.</param>
    /// <param name="stacks">The number of horizontal stacks to divide the sphere into.</param>
    public static Model Sphere(Engine engine, XMaterial material, float radius = 1, int slices = 20, int stacks = 20)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        #endregion

        var mesh = material.IsTextured
            ? TexturedMesh.Sphere(engine.Device, radius, slices, stacks, material.NeedsTBN)
            : Mesh.CreateSphere(engine.Device, radius, slices, stacks);

        return new(mesh, material)
        {
            Engine = engine,
            SurfaceEffect = material.IsTextured ? SurfaceEffect.Shader : SurfaceEffect.FixedFunction,
            BoundingSphere = CenteredBoundingSphere(radius)
        };
    }

    /// <summary>
    /// Creates a model of a cylinder with spherical mapping.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
    /// <param name="material">The material used to render the surface of the model.</param>
    /// <param name="radiusBottom">The radius of the cylinder at the lower end (negative Z).</param>
    /// <param name="radiusTop">The radius of the cylinder at the upper end (positive Z).</param>
    /// <param name="length">The length of the cylinder.</param>
    /// <param name="slices">The number of vertical slices to divide the cylinder in.</param>
    /// <param name="stacks">The number of horizontal stacks to divide the cylinder in.</param>
    public static Model Cylinder(Engine engine, XMaterial material, float radiusBottom, float radiusTop, float length, int slices, int stacks)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        #endregion

        var mesh = material.IsTextured
            ? TexturedMesh.Cylinder(engine.Device, radiusBottom, radiusTop, length, slices, stacks, material.NeedsTBN)
            : Mesh.CreateCylinder(engine.Device, radiusBottom, radiusTop, length, slices, stacks);

        float radiusMax = Math.Max(radiusBottom, radiusTop);
        return new(mesh, material)
        {
            Engine = engine,
            SurfaceEffect = material.IsTextured ? SurfaceEffect.Shader : SurfaceEffect.FixedFunction,
            BoundingSphere = CenteredBoundingSphere(radius: (float)Math.Sqrt(radiusMax * radiusMax + length * length / 4)),
            BoundingBox = CenteredBoundingBox(corner: new(radiusMax, radiusMax, length / 2))
        };
    }

    /// <summary>
    /// Creates a model of a cylinder with spherical mapping.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
    /// <param name="material">The material used to render the surface of the model.</param>
    /// <param name="radiusBottom">The radius of the cylinder at the lower end (negative Z).</param>
    /// <param name="radiusTop">The radius of the cylinder at the upper end (positive Z).</param>
    /// <param name="length">The length of the cylinder.</param>
    public static Model Cylinder(Engine engine, XMaterial material, float radiusBottom = 1, float radiusTop = 1, float length = 10)
    {
        float radiusMean = radiusBottom + (radiusTop - radiusBottom) / 2;
        float radiusDiff = Math.Abs(radiusTop - radiusBottom);
        return Cylinder(engine, material, radiusBottom, radiusTop, length,
            // Auto-determine the number of slices for the cylinder
            (int)(radiusMean * radiusMean).Clamp(8, 1024),
            // Auto-determine the number of stacks for the cylinder
            (int)(radiusDiff * radiusDiff).Clamp(4, 512));
    }

    /// <summary>
    /// Creates a model of a textured round disc with a hole in the middle.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
    /// <param name="material">The material used to render the surface of the model; must be <see cref="XMaterial.IsTextured"/>.</param>
    /// <param name="radiusInner">The radius of the inner circle of the ring.</param>
    /// <param name="radiusOuter">The radius of the outer circle of the ring.</param>
    /// <param name="height">The height of the ring.</param>
    /// <param name="segments">The number of segments the ring shall consist of.</param>
    /// <param name="subsets">The number of subsets to split the mesh into. Must be a divisor of <paramref name="segments"/>.</param>
    /// <exception cref="NotSupportedException">The <paramref name="material"/> is not textured.</exception>
    public static Model Disc(Engine engine, XMaterial material, float radiusInner, float radiusOuter, float height, int segments, int subsets = 1)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        #endregion

        if (!material.IsTextured) throw new NotSupportedException("The material must be textured.");
        var mesh = TexturedMesh.Disc(engine.Device, radiusInner, radiusOuter, height, segments, subsets, material.NeedsTBN);

        var model = new Model(mesh, material)
        {
            Engine = engine,
            NumberSubsets = subsets,
            BoundingSphere = CenteredBoundingSphere(radius: (float)Math.Sqrt(radiusOuter * radiusOuter + height * height / 4)),
            BoundingBox = CenteredBoundingBox(corner: new(radiusOuter, height / 2, radiusOuter))
        };

        if (subsets > 1)
        {
            model.SubsetBoundingBoxes = new BoundingBox[subsets];
            model.SubsetBoundingSpheres = new BoundingSphere[subsets];

            float anglePerSubset = (float)(Math.PI * 2 / subsets);

            for (int i = 0; i < subsets; i++)
            {
                float angleStart = i * anglePerSubset;
                float angleEnd = (i + 1) * anglePerSubset;
                float angleMid = (angleStart + angleEnd) / 2;

                // Calculate bounding box by finding min/max X and Z across all corners
                float innerStartX = radiusInner * (float)Math.Cos(angleStart);
                float innerStartZ = radiusInner * (float)Math.Sin(angleStart);
                float innerEndX = radiusInner * (float)Math.Cos(angleEnd);
                float innerEndZ = radiusInner * (float)Math.Sin(angleEnd);
                float outerStartX = radiusOuter * (float)Math.Cos(angleStart);
                float outerStartZ = radiusOuter * (float)Math.Sin(angleStart);
                float outerEndX = radiusOuter * (float)Math.Cos(angleEnd);
                float outerEndZ = radiusOuter * (float)Math.Sin(angleEnd);

                float minX = Math.Min(Math.Min(innerStartX, innerEndX), Math.Min(outerStartX, outerEndX));
                float maxX = Math.Max(Math.Max(innerStartX, innerEndX), Math.Max(outerStartX, outerEndX));
                float minZ = Math.Min(Math.Min(innerStartZ, innerEndZ), Math.Min(outerStartZ, outerEndZ));
                float maxZ = Math.Max(Math.Max(innerStartZ, innerEndZ), Math.Max(outerStartZ, outerEndZ));

                // Check if cardinal directions are within the angular range
                // X-positive (angle = 0)
                if (angleStart <= 0 && angleEnd > 0)
                    maxX = Math.Max(maxX, radiusOuter);
                // Z-positive (angle = π/2)
                if (angleStart < Math.PI / 2 && angleEnd >= Math.PI / 2)
                    maxZ = Math.Max(maxZ, radiusOuter);
                // X-negative (angle = π)
                if (angleStart < Math.PI && angleEnd >= Math.PI)
                    minX = Math.Min(minX, -radiusOuter);
                // Z-negative (angle = 3π/2)
                if (angleStart < Math.PI * 3 / 2 && angleEnd >= Math.PI * 3 / 2)
                    minZ = Math.Min(minZ, -radiusOuter);

                model.SubsetBoundingBoxes[i] = new(
                    minimum: new(minX, -height / 2, minZ),
                    maximum: new(maxX, height / 2, maxZ));

                // Calculate bounding sphere center and radius
                // Center is at the midpoint of the sector
                float centerRadius = (radiusInner + radiusOuter) / 2;
                var center = new Vector3(
                    centerRadius * (float)Math.Cos(angleMid),
                    0,
                    centerRadius * (float)Math.Sin(angleMid));

                // Find maximum distance from center to any corner
                // There are 8 corners: 2 radii × 2 angles × 2 heights
                float radiusSquared = 0;
                float halfHeight = height / 2;

                for (int corner = 0; corner < 8; corner++)
                {
                    // Use bit patterns to enumerate all corner combinations:
                    // bit 0: radius (0=inner, 1=outer)
                    // bit 1: angle (0=start, 1=end)
                    // bit 2: height (0=bottom, 1=top)
                    float r = (corner & 1) == 0 ? radiusInner : radiusOuter;
                    float angle = (corner & 2) == 0 ? angleStart : angleEnd;
                    float y = (corner & 4) == 0 ? -halfHeight : halfHeight;

                    float dx = r * (float)Math.Cos(angle) - center.X;
                    float dy = y;
                    float dz = r * (float)Math.Sin(angle) - center.Z;
                    float distSquared = dx * dx + dy * dy + dz * dz;

                    radiusSquared = Math.Max(radiusSquared, distSquared);
                }

                model.SubsetBoundingSpheres[i] = new(center, (float)Math.Sqrt(radiusSquared));
            }
        }

        return model;
    }

    /// <summary>
    /// Creates a model of a textured round disc with a hole in the middle.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
    /// <param name="material">The material used to render the surface of the model; must be <see cref="XMaterial.IsTextured"/>.</param>
    /// <param name="radiusInner">The radius of the inner circle of the ring.</param>
    /// <param name="radiusOuter">The radius of the outer circle of the ring.</param>
    /// <param name="height">The height of the ring.</param>
    /// <exception cref="NotSupportedException">The <paramref name="material"/> is not textured.</exception>
    public static Model Disc(Engine engine, XMaterial material, float radiusInner = 5, float radiusOuter = 7, float height = 1)
    {
        float radiusMean = radiusInner + (radiusOuter - radiusInner) / 2;
        return Disc(engine, material, radiusInner, radiusOuter, height,
            // Auto-determine the number of segments for the disc
            (int)(radiusMean * radiusMean).Clamp(8, 1024));
    }

    private static BoundingSphere CenteredBoundingSphere(float radius)
        => new(center: new(), radius);

    private static BoundingBox CenteredBoundingBox(Vector3 corner)
        => new(minimum: -corner, maximum: corner);
}
