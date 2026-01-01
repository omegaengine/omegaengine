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
            BoundingSphere = new(center: new(), radius: size.Length() / 2),
            BoundingBox = new(minimum: new(-size / 2, 0), maximum: new(size / 2, 0))
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
            BoundingBox = new(minimum: -size / 2, maximum: size / 2)
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
    public static Model Sphere(Engine engine, XMaterial material, float radius = 10, int slices = 20, int stacks = 20)
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
            BoundingSphere = new(center: new(), radius)
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

        return new(mesh, material)
        {
            Engine = engine,
            SurfaceEffect = material.IsTextured ? SurfaceEffect.Shader : SurfaceEffect.FixedFunction
            // TODO: BoundingBox = new()
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
    /// <exception cref="NotSupportedException">The <paramref name="material"/> is not textured.</exception>
    public static Model Disc(Engine engine, XMaterial material, float radiusInner, float radiusOuter, float height, int segments)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        #endregion

        if (!material.IsTextured) throw new NotSupportedException("The material must be textured.");
        var mesh = TexturedMesh.Disc(engine.Device, radiusInner, radiusOuter, height, segments, material.NeedsTBN);

        return new(mesh, material)
        {
            Engine = engine,
            // TODO: BoundingBox = new()
        };
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
}
