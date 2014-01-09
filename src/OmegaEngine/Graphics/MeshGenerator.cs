/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Common;
using SlimDX;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics.VertexDecl;

namespace OmegaEngine.Graphics
{
    /// <summary>
    /// Provides methods for creating meshes of simple geometric shapes (box, sphere, etc.) with texture coordinates.
    /// </summary>
    internal static class MeshGenerator
    {
        #region Add texture
        /// <summary>
        /// Clones the mesh, adding texture coordinates
        /// </summary>
        private static void CloneAddTexture(ref Mesh mesh)
        {
            if ((mesh.VertexFormat & VertexFormat.Texture1) != 0)
                throw new ArgumentException("The mesh already contains texture coordinates.", "mesh");

            // Clones the mesh, disposes the old one and then swaps them
            Mesh newMesh = mesh.Clone(mesh.Device, mesh.CreationOptions, mesh.VertexFormat | VertexFormat.Texture1);
            mesh.Dispose();
            mesh = newMesh;
        }
        #endregion

        #region Box texture coordinates
        /// <summary>
        /// Sets box texture coordinates where a 3x2 grid of face textures is split one per face
        /// </summary>
        private static void SetBoxTextureCoordinates(Mesh mesh)
        {
            #region Sanity checks
            // Check the mesh looks like a box and has texture coordinates
            if (mesh.VertexCount != 24 || mesh.FaceCount != 12)
                throw new ArgumentException("The mesh does not look like a box.", "mesh");
            if (mesh.VertexFormat != PositionNormalTextured.Format)
                throw new ArgumentException("The mesh doesn't have the correct format.", "mesh");
            #endregion

            // Copy the vertex buffer content to an array
            var vertexes = BufferHelper.ReadVertexBuffer<PositionNormalTextured>(mesh);

            #region Set texture coordinates
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
            #endregion

            // Copy the array back into the vertex buffer
            BufferHelper.WriteVertexBuffer(mesh, vertexes);
        }
        #endregion

        #region Spherical texture coordinates
        /// <summary>
        /// Fills in a set of spherically mapped texture coordinates to the passed in mesh
        /// </summary>
        private static void SetSphericalTextureCoordinates(Mesh mesh)
        {
            BoundingSphere boundingSphere = BufferHelper.ComputeBoundingSphere(mesh);

            // Copy the vertex buffer content to an array
            var vertexes = BufferHelper.ReadVertexBuffer<PositionNormalTextured>(mesh);

            #region Set texture coordinates
            for (int i = 0; i < vertexes.Length; i++)
            {
                // For each vertex take a ray from the centre of the mesh to the vertex 
                // and normalize so the dot products work
                Vector3 vertexRay = Vector3.Normalize(vertexes[i].Position - boundingSphere.Center);

                // If north and vertex ray are coincident then we can pick an arbitray u since its the entire top/bottom line of the texture
                double phi = Math.Acos(vertexRay.Z);
                vertexes[i].Tv = (float)(phi / Math.PI);

                if (vertexRay.Z == 1.0f || vertexRay.Z == -1.0f)
                    vertexes[i].Tu = 0.5f;
                else
                {
                    var u = (float)
                        (Math.Acos(Math.Max(Math.Min(vertexRay.Y / Math.Sin(phi), 1.0), -1.0)) / (2.0 * Math.PI));
                    // Since the cross product is just giving us (1;0;0) i.e. the x-axis 
                    // and the dot product was giving us a +ve or -ve angle, we can just compare the x value with 0
                    vertexes[i].Tu = (vertexRay.X > 0f) ? u : 1 - u;
                }
            }
            #endregion

            // Copy the array back into the vertex buffer
            BufferHelper.WriteVertexBuffer(mesh, vertexes);
        }
        #endregion

        //--------------------//

        #region Quad
        /// <summary>
        /// Creates a new <see cref="Mesh"/> representing a textured 2D quad.
        /// </summary>
        /// <param name="device">The <see cref="Device"/> to create the <see cref="Mesh"/> in.</param>
        /// <param name="width">The width of the quad.</param>
        /// <param name="height">The height of the quad.</param>
        public static Mesh Quad(Device device, float width, float height)
        {
            #region Sanity checks
            if (device == null) throw new ArgumentNullException("device");
            #endregion

            var vertexes = new[]
            {
                new PositionTextured(new Vector3(-width / 2, -height / 2, 0), 0, 0),
                new PositionTextured(new Vector3(-width / 2, height / 2, 0), 0, 1),
                new PositionTextured(new Vector3(width / 2, -height / 2, 0), 1, 0),
                new PositionTextured(new Vector3(width / 2, height / 2, 0), 1, 1)
            };
            short[] indexes = {0, 1, 3, 3, 2, 0};

            var mesh = new Mesh(device, indexes.Length / 3, vertexes.Length, MeshFlags.Managed, PositionTextured.Format);
            BufferHelper.WriteVertexBuffer(mesh, vertexes);
            BufferHelper.WriteIndexBuffer(mesh, indexes);
            MeshHelper.GenerateNormalsAndTangents(device, ref mesh, false);

            return mesh;
        }
        #endregion

        #region Box
        /// <summary>
        /// Creates a new <see cref="Mesh"/> representing a textured box.
        /// </summary>
        /// <param name="device">The <see cref="Device"/> to create the <see cref="Mesh"/> in.</param>
        /// <param name="width">The width of the box.</param>
        /// <param name="height">The height of the box.</param>
        /// <param name="depth">The depth of the box.</param>
        /// <remarks>The box is formed like the one created by <see cref="Mesh.CreateBox"/>.</remarks>
        public static Mesh Box(Device device, float width, float height, float depth)
        {
            #region Sanity checks
            if (device == null) throw new ArgumentNullException("device");
            #endregion

            // Create an untextured box
            Mesh mesh = Mesh.CreateBox(device, width, height, depth);

            // Add texture coordinates
            CloneAddTexture(ref mesh);
            SetBoxTextureCoordinates(mesh);

            return mesh;
        }
        #endregion

        #region Sphere
        /// <summary>
        /// Creates a new <see cref="Mesh"/> representing a textured sphere with spherical mapping.
        /// </summary>
        /// <param name="device">The <see cref="Device"/> to create the mesh in</param>
        /// <param name="radius">The radius of the sphere.</param>
        /// <param name="slices">The number of vertical slices to divide the sphere into.</param>
        /// <param name="stacks">The number of horizontal stacks to divide the sphere into.</param>
        /// <remarks>The sphere is formed like the one created by <see cref="Mesh.CreateSphere"/>.</remarks>
        public static Mesh Sphere(Device device, float radius, int slices, int stacks)
        {
            #region Sanity checks
            if (device == null) throw new ArgumentNullException("device");
            if (slices <= 0) throw new ArgumentOutOfRangeException("slices");
            if (stacks <= 0) throw new ArgumentOutOfRangeException("stacks");
            #endregion

            int numVertexes = (slices + 1) * (stacks + 1);
            int numFaces = slices * stacks * 2;
            int indexCount = numFaces * 3;

            var mesh = new Mesh(device, numFaces, numVertexes, MeshFlags.Managed, PositionNormalTextured.Format);

            #region Build sphere vertexes
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
                        var pnt = new PositionNormalTextured();
                        float alphaZ = ((stack - stacks * 0.5f) / stacks) * (float)Math.PI * 1.0f; // Angle around Z-axis
                        pnt.X = (float)(Math.Cos(alphaY) * radius) * (float)Math.Cos(alphaZ);
                        pnt.Z = (float)(Math.Sin(alphaY) * radius) * (float)Math.Cos(alphaZ);
                        pnt.Y = (float)(Math.Sin(alphaZ) * radius);
                        pnt.Nx = pnt.X / radius;
                        pnt.Ny = pnt.Y / radius;
                        pnt.Nz = pnt.Z / radius;
                        pnt.Tv = 0.5f - (float)(Math.Asin(pnt.Y / radius) / Math.PI);
                        vertexes.SetValue(pnt, vertIndex);
                    }
                    vertexes[vertIndex++].Tu = (float)slice / slices;
                }
            }
            #endregion

            BufferHelper.WriteVertexBuffer(mesh, vertexes);

            #region Build index buffer
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
            #endregion

            BufferHelper.WriteIndexBuffer(mesh, indexes);

            return mesh;
        }
        #endregion

        #region Cylinder
        /// <summary>
        /// Creates a new <see cref="Mesh"/> representing a textured cylinder with spherical mapping.
        /// </summary>
        /// <param name="device">The <see cref="Device"/> to create the mesh in.</param>
        /// <param name="radius1">The radius of the cylinder at the lower end (negative Z).</param>
        /// <param name="radius2">The radius of the cylinder at the upper end (positive Z).</param>
        /// <param name="length">The length of the cylinder.</param>
        /// <param name="slices">The number of vertical slices to divide the cylinder in.</param>
        /// <param name="stacks">The number of horizontal stacks to divide the cylinder in.</param>
        /// <remarks>The cylinder is formed like the one created by <see cref="Mesh.CreateCylinder"/>.</remarks>
        public static Mesh Cylinder(Device device, float radius1, float radius2, float length, int slices, int stacks)
        {
            #region Sanity checks
            if (device == null) throw new ArgumentNullException("device");
            if (slices <= 0) throw new ArgumentOutOfRangeException("slices");
            if (stacks <= 0) throw new ArgumentOutOfRangeException("stacks");
            #endregion

            // Create an untextured cylinder
            Mesh mesh = Mesh.CreateCylinder(device, radius1, radius2, length, slices, stacks);

            // Add texture coordinates
            CloneAddTexture(ref mesh);
            SetSphericalTextureCoordinates(mesh);

            return mesh;
        }
        #endregion

        #region Disc
        /// <summary>
        /// Creates a model of a textured round disc with a hole in the middle.
        /// </summary>
        /// <param name="device">The <see cref="Device"/> to create the mesh in.</param>
        /// <param name="radiusInner">The radius of the inner circle of the ring.</param>
        /// <param name="radiusOuter">The radius of the outer circle of the ring.</param>
        /// <param name="height">The height of the ring.</param>
        /// <param name="segments">The number of segments the ring shall consist of.</param>
        public static Mesh Disc(Device device, float radiusInner, float radiusOuter, float height, int segments)
        {
            #region Sanity checks
            if (device == null) throw new ArgumentNullException("device");
            #endregion

            const float tuInner = 0, tuOuter = 1;

            Log.Info("Generate predefined model: Disc");
            var posInner = new Vector3(radiusInner, 0, 0);
            var posOuter = new Vector3(radiusOuter, 0, 0);

            #region Generate vertexes
            int vertCount = 0;
            var step = (float)(Math.PI * 2 / segments);
            var vertexes = new PositionTextured[segments * 4];

            for (int i = 0; i < segments; i++)
            {
                vertexes[vertCount++] = new PositionTextured(posInner.X, posInner.Y - height / 2, posInner.Z, tuInner, 0);
                vertexes[vertCount++] = new PositionTextured(posInner.X, posInner.Y + height / 2, posInner.Z, tuInner, 0);
                vertexes[vertCount++] = new PositionTextured(posOuter.X, posOuter.Y - height / 2, posOuter.Z, tuOuter, 0);
                vertexes[vertCount++] = new PositionTextured(posOuter.X, posOuter.Y + height / 2, posOuter.Z, tuOuter, 0);

                // Increment rotation
                posInner = Vector3.TransformCoordinate(posInner, Matrix.RotationY(step));
                posOuter = Vector3.TransformCoordinate(posOuter, Matrix.RotationY(step));
            }
            #endregion

            #region Generate indexes
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
            #endregion

            var mesh = new Mesh(device, indexes.Length / 3, vertexes.Length, MeshFlags.Managed, PositionTextured.Format);
            BufferHelper.WriteVertexBuffer(mesh, vertexes);
            BufferHelper.WriteIndexBuffer(mesh, indexes);
            MeshHelper.GenerateNormalsAndTangents(device, ref mesh, false);

            return mesh;
        }
        #endregion
    }
}
