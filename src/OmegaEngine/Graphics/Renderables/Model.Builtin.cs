/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using NanoByte.Common;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.Renderables;

// This file contains methods for creating models from builtin templates (boxes, spheres, etc.)
partial class Model
{
    #region Quad
    /// <summary>
    /// Creates a model of a textured 2D quad.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
    /// <param name="texture">The texture to place on the model; <c>null</c> for no texture.</param>
    /// <param name="width">The width of the quad.</param>
    /// <param name="height">The height of the quad.</param>
    public static Model Quad(Engine engine, ITextureProvider? texture = null, float width = 5, float height = 5)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        #endregion

        Log.Info("Generate predefined model: Quad");
        return new(TexturedMesh.Quad(engine.Device, width, height), new XMaterial(texture))
        {
            BoundingSphere = new(center: new(), radius:(float)Math.Sqrt(width * width + height * height) / 2f),
            BoundingBox = new(minimum: new(), maximum: new(width, height, 0))
        };
    }
    #endregion

    #region Box
    /// <summary>
    /// Creates a model of a textured box.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
    /// <param name="texture">The texture to place on the model; <c>null</c> for no texture.</param>
    /// <param name="width">The width of the box</param>
    /// <param name="height">The height of the box</param>
    /// <param name="depth">The depth of the box</param>
    public static Model Box(Engine engine, ITextureProvider? texture = null, float width = 5, float height = 5, float depth = 5)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        #endregion

        Log.Info("Generate predefined model: Box");
        Mesh mesh = TexturedMesh.Box(engine.Device, width, height, depth);
        TexturedMeshUtils.GenerateNormals(engine.Device, ref mesh);

        return new(mesh, new XMaterial(texture))
        {
            BoundingBox = new(minimum: new(), maximum: new(width, height, depth))
        };
    }
    #endregion

    #region Sphere
    /// <summary>
    /// Creates a model of a textured sphere with spherical mapping.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
    /// <param name="texture">The texture to place on the model; <c>null</c> for no texture.</param>
    /// <param name="radius">The radius of the sphere.</param>
    /// <param name="slices">The number of vertical slices to divide the sphere into.</param>
    /// <param name="stacks">The number of horizontal stacks to divide the sphere into.</param>
    public static Model Sphere(Engine engine, ITextureProvider? texture = null, float radius = 10, int slices = 20, int stacks = 20)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        #endregion

        Log.Info("Generate predefined model: Sphere");
        Mesh mesh = TexturedMesh.Sphere(engine.Device, radius, slices, stacks);
        TexturedMeshUtils.GenerateNormals(engine.Device, ref mesh);

        return new(mesh, new XMaterial(texture))
        {
            Engine = engine,
            BoundingSphere = new(center: new(), radius)
        };
    }
    #endregion

    #region Cylinder
    /// <summary>
    /// Creates a model of a textured sphere with spherical mapping.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
    /// <param name="texture">The texture to place on the model; <c>null</c> for no texture.</param>
    /// <param name="radiusBottom">The radius of the cylinder at the lower end (negative Z).</param>
    /// <param name="radiusTop">The radius of the cylinder at the upper end (positive Z).</param>
    /// <param name="length">The length of the cylinder.</param>
    /// <param name="slices">The number of vertical slices to divide the cylinder in.</param>
    /// <param name="stacks">The number of horizontal stacks to divide the cylinder in.</param>
    public static Model Cylinder(Engine engine, ITextureProvider? texture, float radiusBottom, float radiusTop, float length, int slices, int stacks)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        #endregion

        Log.Info("Generate predefined model: Cylinder");
        Mesh mesh = TexturedMesh.Cylinder(engine.Device, radiusBottom, radiusTop, length, slices, stacks);
        TexturedMeshUtils.GenerateNormals(engine.Device, ref mesh);

        // ToDo: Calculate bounding box
        return new(mesh, new XMaterial(texture));
    }

    /// <summary>
    /// Creates a model of a textured cylinder with spherical mapping.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
    /// <param name="texture">The texture to place on the model; <c>null</c> for no texture.</param>
    /// <param name="radiusBottom">The radius of the cylinder at the lower end (negative Z).</param>
    /// <param name="radiusTop">The radius of the cylinder at the upper end (positive Z).</param>
    /// <param name="length">The length of the cylinder.</param>
    public static Model Cylinder(Engine engine, ITextureProvider? texture = null, float radiusBottom = 1, float radiusTop = 1, float length = 10)
    {
        float radiusMean = radiusBottom + (radiusTop - radiusBottom) / 2;
        float radiusDiff = Math.Abs(radiusTop - radiusBottom);
        return Cylinder(engine, texture, radiusBottom, radiusTop, length,
            // Auto-determine the number of slices for the cylinder
            (int)(radiusMean * radiusMean).Clamp(8, 1024),
            // Auto-determine the number of stacks for the cylinder
            (int)(radiusDiff * radiusDiff).Clamp(4, 512));
    }
    #endregion

    #region Disc
    /// <summary>
    /// Creates a model of a textured round disc with a hole in the middle.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
    /// <param name="texture">The texture to place on the model; <c>null</c> for no texture.</param>
    /// <param name="radiusInner">The radius of the inner circle of the ring.</param>
    /// <param name="radiusOuter">The radius of the outer circle of the ring.</param>
    /// <param name="height">The height of the ring.</param>
    /// <param name="segments">The number of segments the ring shall consist of.</param>
    public static Model Disc(Engine engine, ITextureProvider texture, float radiusInner, float radiusOuter, float height, int segments)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        #endregion

        // ToDo: Calculate bounding box
        return new(TexturedMesh.Disc(engine.Device, radiusInner, radiusOuter, height, segments), new XMaterial(texture));
    }

    /// <summary>
    /// Creates a model of a textured round disc with a hole in the middle.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to use for rendering.</param>
    /// <param name="texture">The texture to place on the model; <c>null</c> for no texture.</param>
    /// <param name="radiusInner">The radius of the inner circle of the ring.</param>
    /// <param name="radiusOuter">The radius of the outer circle of the ring.</param>
    /// <param name="height">The height of the ring.</param>
    public static Model Disc(Engine engine, ITextureProvider? texture = null, float radiusInner = 5, float radiusOuter = 7, float height = 1)
    {
        float radiusMean = radiusInner + (radiusOuter - radiusInner) / 2;
        return Disc(engine, texture, radiusInner, radiusOuter, height,
            // Auto-determine the number of segments for the disc
            (int)(radiusMean * radiusMean).Clamp(8, 1024));
    }
    #endregion
}
