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
using OmegaEngine.Graphics.VertexDecl;

namespace OmegaEngine.Graphics;

/// <summary>
/// Methods for creating meshes of simple geometric shapes (box, sphere, etc.) with texture coordinates.
/// </summary>
public static class TexturedMesh
{
    /// <summary>
    /// Clones the mesh, adding texture coordinates
    /// </summary>
    private static void CloneAddTexture(ref Mesh mesh)
    {
        if (mesh.VertexFormat.HasFlag(VertexFormat.Texture1))
            throw new ArgumentException("The mesh already contains texture coordinates.", nameof(mesh));

        // Clones the mesh, disposes the old one and then swaps them
        Mesh newMesh = mesh.Clone(mesh.Device, mesh.CreationOptions, mesh.VertexFormat | VertexFormat.Texture1);
        mesh.Dispose();
        mesh = newMesh;
    }

    /// <summary>
    /// Sets box texture coordinates where a 3x2 grid of face textures is split one per face
    /// </summary>
    private static void SetBoxTextureCoordinates(Mesh mesh)
    {
        #region Sanity checks
        // Check the mesh looks like a box and has texture coordinates
        if (mesh.VertexCount != 24 || mesh.FaceCount != 12)
            throw new ArgumentException("The mesh does not look like a box.", nameof(mesh));
        if (mesh.VertexFormat != PositionNormalTextured.Format)
            throw new ArgumentException("The mesh doesn't have the correct format.", nameof(mesh));
        #endregion

        // Copy the vertex buffer content to an array
        var vertexes = BufferHelper.ReadVertexBuffer<PositionNormalTextured>(mesh);

        // Bottom
        vertexes[13].Tu = 0f;
        vertexes[13].Tv = 0f;
        vertexes[14].Tu = 1f / 3f;
        vertexes[14].Tv = 0f;
        vertexes[15].Tu = 1f / 3f;
        vertexes[15].Tv = 0.5f;
        vertexes[12].Tu = 0f;
        vertexes[12].Tv = 0.5f;

        // Top
        vertexes[5].Tu = 1f / 3f;
        vertexes[5].Tv = 0f;
        vertexes[6].Tu = 2f / 3f;
        vertexes[6].Tv = 0f;
        vertexes[7].Tu = 2f / 3f;
        vertexes[7].Tv = 0.5f;
        vertexes[4].Tu = 1f / 3f;
        vertexes[4].Tv = 0.5f;

        // Back
        vertexes[18].Tu = 2f / 3f;
        vertexes[18].Tv = 0f;
        vertexes[19].Tu = 1f;
        vertexes[19].Tv = 0f;
        vertexes[16].Tu = 1f;
        vertexes[16].Tv = 0.5f;
        vertexes[17].Tu = 2f / 3f;
        vertexes[17].Tv = 0.5f;

        // Left
        vertexes[2].Tu = 0f;
        vertexes[2].Tv = 0.5f;
        vertexes[3].Tu = 1f / 3f;
        vertexes[3].Tv = 0.5f;
        vertexes[0].Tu = 1f / 3f;
        vertexes[0].Tv = 1f;
        vertexes[1].Tu = 0f;
        vertexes[1].Tv = 1f;

        // Front
        vertexes[21].Tu = 1f / 3f;
        vertexes[21].Tv = 0.5f;
        vertexes[22].Tu = 2f / 3f;
        vertexes[22].Tv = 0.5f;
        vertexes[23].Tu = 2f / 3f;
        vertexes[23].Tv = 1f;
        vertexes[20].Tu = 1f / 3f;
        vertexes[20].Tv = 1f;

        // Right
        vertexes[8].Tu = 2f / 3f;
        vertexes[8].Tv = 0.5f;
        vertexes[9].Tu = 1f;
        vertexes[9].Tv = 0.5f;
        vertexes[10].Tu = 1f;
        vertexes[10].Tv = 1f;
        vertexes[11].Tu = 2f / 3f;
        vertexes[11].Tv = 1f;

        // Copy the array back into the vertex buffer
        BufferHelper.WriteVertexBuffer(mesh, vertexes);
    }

    /// <summary>
    /// Fills in a set of spherically mapped texture coordinates to the passed in mesh
    /// </summary>
    private static void SetSphericalTextureCoordinates(Mesh mesh)
    {
        BoundingSphere boundingSphere = BufferHelper.ComputeBoundingSphere(mesh);

        var vertexes = BufferHelper.ReadVertexBuffer<PositionNormalTextured>(mesh);
        for (int i = 0; i < vertexes.Length; i++)
        {
            Vector3 vertexRay = Vector3.Normalize(vertexes[i].Position - boundingSphere.Center);
            double phi = Math.Acos(vertexRay.Z);

            vertexes[i].Tu = CalculateTu(vertexRay, phi);
            vertexes[i].Tv = (float)(phi / Math.PI);
        }
        BufferHelper.WriteVertexBuffer(mesh, vertexes);
    }

    private static float CalculateTu(Vector3 vertexRay, double phi)
    {
        // ReSharper disable CompareOfFloatsByEqualityOperator
        if (vertexRay.Z is 1.0f or -1.0f) return 0.5f;
        // ReSharper restore CompareOfFloatsByEqualityOperator
        else
        {
            var u = (float)(Math.Acos(Math.Max(Math.Min(vertexRay.Y / Math.Sin(phi), 1.0), -1.0)) / (2.0 * Math.PI));
            return (vertexRay.X > 0f) ? u : 1 - u;
        }
    }

    /// <summary>
    /// Creates a new <see cref="Mesh"/> representing a textured 2D quad.
    /// </summary>
    /// <param name="device">The <see cref="Device"/> to create the <see cref="Mesh"/> in.</param>
    /// <param name="width">The width of the quad.</param>
    /// <param name="height">The height of the quad.</param>
    /// <param name="tbn">Generate TBN (tangent, binormal, normal) vectors instead of just normal vectors.</param>
    public static Mesh Quad(Device device, float width, float height, bool tbn = false)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        #endregion

        PositionTextured[] vertexes =
        [
            new(new Vector3(-width / 2, -height / 2, 0), 0, 0),
            new(new Vector3(-width / 2, height / 2, 0), 0, 1),
            new(new Vector3(width / 2, -height / 2, 0), 1, 0),
            new(new Vector3(width / 2, height / 2, 0), 1, 1)
        ];
        short[] indexes = [0, 1, 3, 3, 2, 0];

        var mesh = new Mesh(device, indexes.Length / 3, vertexes.Length, MeshFlags.Managed, PositionTextured.Format);
        BufferHelper.WriteVertexBuffer(mesh, vertexes);
        BufferHelper.WriteIndexBuffer(mesh, indexes);

        if (tbn) TexturedMeshUtils.GenerateTBN(device, ref mesh);
        else TexturedMeshUtils.GenerateNormals(device, ref mesh);

        return mesh;
    }

    /// <summary>
    /// Creates a new <see cref="Mesh"/> representing a textured box.
    /// </summary>
    /// <param name="device">The <see cref="Device"/> to create the <see cref="Mesh"/> in.</param>
    /// <param name="width">The width of the box.</param>
    /// <param name="height">The height of the box.</param>
    /// <param name="depth">The depth of the box.</param>
    /// <param name="tbn">Generate TBN (tangent, binormal, normal) vectors instead of just normal vectors.</param>
    /// <remarks>The box is formed like the one created by <see cref="Mesh.CreateBox"/>.</remarks>
    public static Mesh Box(Device device, float width, float height, float depth, bool tbn = false)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        #endregion

        // Create an untextured box
        Mesh mesh = Mesh.CreateBox(device, width, height, depth);

        // Add texture coordinates
        CloneAddTexture(ref mesh);
        SetBoxTextureCoordinates(mesh);

        if (tbn) TexturedMeshUtils.GenerateTBN(device, ref mesh);

        return mesh;
    }

    /// <summary>
    /// Creates a new <see cref="Mesh"/> representing a textured sphere with spherical mapping.
    /// </summary>
    /// <param name="device">The <see cref="Device"/> to create the mesh in</param>
    /// <param name="radius">The radius of the sphere.</param>
    /// <param name="slices">The number of vertical slices to divide the sphere into.</param>
    /// <param name="stacks">The number of horizontal stacks to divide the sphere into.</param>
    /// <param name="tbn">Generate TBN (tangent, binormal, normal) vectors instead of just normal vectors.</param>
    /// <remarks>The sphere is formed like the one created by <see cref="Mesh.CreateSphere"/>.</remarks>
    public static Mesh Sphere(Device device, float radius, int slices, int stacks, bool tbn = false)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (slices <= 0) throw new ArgumentOutOfRangeException(nameof(slices));
        if (stacks <= 0) throw new ArgumentOutOfRangeException(nameof(stacks));
        #endregion

        int numVertexes = (slices + 1) * (stacks + 1);
        int numFaces = slices * stacks * 2;
        int indexCount = numFaces * 3;

        var mesh = new Mesh(device, numFaces, numVertexes, MeshFlags.Managed, PositionNormalTextured.Format);

        var vertexes = new PositionNormalTextured[mesh.VertexCount];
        int vertIndex = 0;
        for (int slice = 0; slice <= slices; slice++)
        {
            float alphaY = (float)slice / slices * (float)Math.PI * 2.0f; // Angle around Y-axis
            for (int stack = 0; stack <= stacks; stack++)
            {
                if (slice == slices)
                    vertexes[vertIndex] = vertexes[stack];
                else
                {
                    float alphaZ = (stack - stacks * 0.5f) / stacks * (float)Math.PI;
                    var position = new Vector3(
                        (float)(Math.Cos(alphaY) * radius) * (float)Math.Cos(alphaZ),
                        (float)(Math.Sin(alphaZ) * radius),
                        (float)(Math.Sin(alphaY) * radius) * (float)Math.Cos(alphaZ));
                    var normal = position * (1 / radius);
                    vertexes.SetValue(
                        new PositionNormalTextured(
                            position,
                            normal,
                            tu: 0,
                            tv: 0.5f - (float)(Math.Asin(normal.Y) / Math.PI)),
                        vertIndex);
                }
                vertexes[vertIndex++].Tu = (float)slice / slices;
            }
        }

        BufferHelper.WriteVertexBuffer(mesh, vertexes);

        var indexes = new short[indexCount];
        int i = 0;
        for (short x = 0; x < slices; x++)
        {
            var leftVertex = (short)((stacks + 1) * x);
            var rightVertex = (short)(leftVertex + stacks + 1);
            for (int y = 0; y < stacks; y++)
            {
                indexes[i++] = rightVertex;
                indexes[i++] = leftVertex;
                indexes[i++] = (short)(leftVertex + 1);
                indexes[i++] = rightVertex;
                indexes[i++] = (short)(leftVertex + 1);
                indexes[i++] = (short)(rightVertex + 1);
                leftVertex++;
                rightVertex++;
            }
        }

        BufferHelper.WriteIndexBuffer(mesh, indexes);

        if (tbn) TexturedMeshUtils.GenerateTBN(device, ref mesh);

        return mesh;
    }

    /// <summary>
    /// Creates a new <see cref="Mesh"/> representing a textured cylinder with spherical mapping.
    /// </summary>
    /// <param name="device">The <see cref="Device"/> to create the mesh in.</param>
    /// <param name="radius1">The radius of the cylinder at the lower end (negative Z).</param>
    /// <param name="radius2">The radius of the cylinder at the upper end (positive Z).</param>
    /// <param name="length">The length of the cylinder.</param>
    /// <param name="slices">The number of vertical slices to divide the cylinder in.</param>
    /// <param name="stacks">The number of horizontal stacks to divide the cylinder in.</param>
    /// <param name="tbn">Generate TBN (tangent, binormal, normal) vectors instead of just normal vectors.</param>
    /// <remarks>The cylinder is formed like the one created by <see cref="Mesh.CreateCylinder"/>.</remarks>
    public static Mesh Cylinder(Device device, float radius1, float radius2, float length, int slices, int stacks, bool tbn = false)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (slices <= 0) throw new ArgumentOutOfRangeException(nameof(slices));
        if (stacks <= 0) throw new ArgumentOutOfRangeException(nameof(stacks));
        #endregion

        // Create an untextured cylinder
        Mesh mesh = Mesh.CreateCylinder(device, radius1, radius2, length, slices, stacks);

        // Add texture coordinates
        CloneAddTexture(ref mesh);
        SetSphericalTextureCoordinates(mesh);

        if (tbn) TexturedMeshUtils.GenerateTBN(device, ref mesh);

        return mesh;
    }

    /// <summary>
    /// Creates a model of a textured round disc with a hole in the middle.
    /// </summary>
    /// <param name="device">The <see cref="Device"/> to create the mesh in.</param>
    /// <param name="radiusInner">The radius of the inner circle of the ring.</param>
    /// <param name="radiusOuter">The radius of the outer circle of the ring.</param>
    /// <param name="height">The height of the ring.</param>
    /// <param name="segments">The number of segments the ring shall consist of.</param>
    /// <param name="tbn">Generate TBN (tangent, binormal, normal) vectors instead of just normal vectors.</param>
    public static Mesh Disc(Device device, float radiusInner, float radiusOuter, float height, int segments, bool tbn = false)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        #endregion

        const float tuInner = 0, tuOuter = 1;

        Log.Info("Generate predefined model: Disc");
        var posInner = new Vector3(radiusInner, 0, 0);
        var posOuter = new Vector3(radiusOuter, 0, 0);

        int vertCount = 0;
        var step = (float)(Math.PI * 2 / segments);
        var vertexes = new PositionTextured[segments * 4];

        for (int i = 0; i < segments; i++)
        {
            vertexes[vertCount++] = new(posInner.X, posInner.Y - height / 2, posInner.Z, tuInner, 0);
            vertexes[vertCount++] = new(posInner.X, posInner.Y + height / 2, posInner.Z, tuInner, 0);
            vertexes[vertCount++] = new(posOuter.X, posOuter.Y - height / 2, posOuter.Z, tuOuter, 0);
            vertexes[vertCount++] = new(posOuter.X, posOuter.Y + height / 2, posOuter.Z, tuOuter, 0);

            // Increment rotation
            posInner = Vector3.TransformCoordinate(posInner, Matrix.RotationY(step));
            posOuter = Vector3.TransformCoordinate(posOuter, Matrix.RotationY(step));
        }

        int indexCount = 0;
        var indexes = new short[segments * 24];

        for (int i = 0; i < segments; i++)
        {
            short innerBottom1 = (short)(i * 4), innerTop1 = (short)(i * 4 + 1);
            short outerBottom1 = (short)(i * 4 + 2), outerTop1 = (short)(i * 4 + 3);
            short innerBottom2 = (short)(i * 4 + 4), innerTop2 = (short)(i * 4 + 5);
            short outerBottom2 = (short)(i * 4 + 6), outerTop2 = (short)(i * 4 + 7);

            if (innerBottom2 >= vertexes.Length) innerBottom2 -= (short)vertexes.Length;
            if (innerTop2 >= vertexes.Length) innerTop2 -= (short)vertexes.Length;
            if (outerBottom2 >= vertexes.Length) outerBottom2 -= (short)vertexes.Length;
            if (outerTop2 >= vertexes.Length) outerTop2 -= (short)vertexes.Length;

            // Bottom 2 triangles
            indexes[indexCount++] = innerBottom1;
            indexes[indexCount++] = innerBottom2;
            indexes[indexCount++] = outerBottom2;
            indexes[indexCount++] = innerBottom1;
            indexes[indexCount++] = outerBottom2;
            indexes[indexCount++] = outerBottom1;

            // Top 2 triangles
            indexes[indexCount++] = innerTop1;
            indexes[indexCount++] = outerTop2;
            indexes[indexCount++] = innerTop2;
            indexes[indexCount++] = innerTop1;
            indexes[indexCount++] = outerTop1;
            indexes[indexCount++] = outerTop2;

            // Inner 2 triangles
            indexes[indexCount++] = innerTop1;
            indexes[indexCount++] = innerBottom2;
            indexes[indexCount++] = innerBottom1;
            indexes[indexCount++] = innerTop1;
            indexes[indexCount++] = innerTop2;
            indexes[indexCount++] = innerBottom2;

            // Outer 2 triangles
            indexes[indexCount++] = outerBottom1;
            indexes[indexCount++] = outerBottom2;
            indexes[indexCount++] = outerTop1;
            indexes[indexCount++] = outerBottom2;
            indexes[indexCount++] = outerTop2;
            indexes[indexCount++] = outerTop1;
        }

        var mesh = new Mesh(device, indexes.Length / 3, vertexes.Length, MeshFlags.Managed, PositionTextured.Format);
        BufferHelper.WriteVertexBuffer(mesh, vertexes);
        BufferHelper.WriteIndexBuffer(mesh, indexes);

        if (tbn) TexturedMeshUtils.GenerateTBN(device, ref mesh);
        else TexturedMeshUtils.GenerateNormals(device, ref mesh);

        return mesh;
    }
}
