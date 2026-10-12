/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using OmegaEngine.D3DX;
using Vortice.Direct3D9;

namespace OmegaEngine.Graphics;

/// <summary>
/// Generates simple untextured shapes in <see cref="Pool.SystemMemory"/>.
/// </summary>
/// <remarks>The vertex layout and order match those of <c>D3DXCreateBox</c>, <c>D3DXCreateSphere</c> and <c>D3DXCreateCylinder</c>.</remarks>
public static class MeshPrimitives
{
    /// <summary>
    /// A vertex with a position and a normal (<see cref="VertexFormat.Position"/> | <see cref="VertexFormat.Normal"/>).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private record struct PositionNormal(Vector3 Position, Vector3 Normal);

    private const VertexFormat Format = VertexFormat.Position | VertexFormat.Normal;

    /// <summary>
    /// Creates an axis-aligned box centered on the origin.
    /// </summary>
    /// <param name="device">The device to create the mesh on.</param>
    /// <param name="width">The size of the box along the X axis.</param>
    /// <param name="height">The size of the box along the Y axis.</param>
    /// <param name="depth">The size of the box along the Z axis.</param>
    public static Mesh Box(IDirect3DDevice9 device, float width, float height, float depth)
    {
        ReadOnlySpan<Vector3> unitBox =
        [
            new(-0.5f, -0.5f, -0.5f), new(-0.5f, -0.5f, 0.5f), new(-0.5f, 0.5f, 0.5f), new(-0.5f, 0.5f, -0.5f),
            new(-0.5f, 0.5f, -0.5f), new(-0.5f, 0.5f, 0.5f), new(0.5f, 0.5f, 0.5f), new(0.5f, 0.5f, -0.5f),
            new(0.5f, 0.5f, -0.5f), new(0.5f, 0.5f, 0.5f), new(0.5f, -0.5f, 0.5f), new(0.5f, -0.5f, -0.5f),
            new(-0.5f, -0.5f, 0.5f), new(-0.5f, -0.5f, -0.5f), new(0.5f, -0.5f, -0.5f), new(0.5f, -0.5f, 0.5f),
            new(-0.5f, -0.5f, 0.5f), new(0.5f, -0.5f, 0.5f), new(0.5f, 0.5f, 0.5f), new(-0.5f, 0.5f, 0.5f),
            new(-0.5f, -0.5f, -0.5f), new(-0.5f, 0.5f, -0.5f), new(0.5f, 0.5f, -0.5f), new(0.5f, -0.5f, -0.5f)
        ];
        ReadOnlySpan<Vector3> normals = [-Vector3.UnitX, Vector3.UnitY, Vector3.UnitX, -Vector3.UnitY, Vector3.UnitZ, -Vector3.UnitZ];

        var size = new Vector3(width, height, depth);
        var vertexes = new PositionNormal[unitBox.Length];
        for (int i = 0; i < vertexes.Length; i++)
            vertexes[i] = new(unitBox[i] * size, normals[i / 4]);

        var indexes = new short[6 * 2 * 3];
        for (int side = 0; side < 6; side++)
        {
            int v = side * 4;
            short[] quad = [(short)v, (short)(v + 1), (short)(v + 2), (short)(v + 2), (short)(v + 3), (short)v];
            quad.CopyTo(indexes, side * 6);
        }

        return Build(device, vertexes, indexes);
    }

    /// <summary>
    /// Creates a sphere centered on the origin with its poles on the Z axis.
    /// </summary>
    /// <param name="device">The device to create the mesh on.</param>
    /// <param name="radius">The radius of the sphere.</param>
    /// <param name="slices">The number of slices around the Z axis.</param>
    /// <param name="stacks">The number of stacks along the Z axis.</param>
    public static Mesh Sphere(IDirect3DDevice9 device, float radius, int slices, int stacks)
    {
        #region Sanity checks
        if (slices < 2) throw new ArgumentOutOfRangeException(nameof(slices));
        if (stacks < 2) throw new ArgumentOutOfRangeException(nameof(stacks));
        #endregion

        var vertexes = new PositionNormal[2 + slices * (stacks - 1)];
        var indexes = new short[3 * (2 * slices + (stacks - 2) * 2 * slices)];

        // phi = angle on the xy plane wrt the x axis, theta = angle wrt the z axis
        var (phiSin, phiCos) = SinCosTable(MathF.PI / 2, -2 * MathF.PI / slices, slices);
        float thetaStep = MathF.PI / stacks, theta = thetaStep;

        int vertex = 0, index = 0;
        vertexes[vertex++] = new(new(0, 0, radius), Vector3.UnitZ);

        int stack, slice;
        for (stack = 0; stack < stacks - 1; stack++)
        {
            float sinTheta = MathF.Sin(theta), cosTheta = MathF.Cos(theta);
            for (slice = 0; slice < slices; slice++)
            {
                var normal = new Vector3(sinTheta * phiCos[slice], sinTheta * phiSin[slice], cosTheta);
                vertexes[vertex++] = new(normal * radius, normal);

                if (slice > 0)
                {
                    if (stack == 0)
                    { // Top stack is a triangle fan
                        Face(0, slice + 1, slice);
                    }
                    else
                    { // Stacks in between top and bottom are quad strips
                        Face(VertexIndex(slice - 1, stack - 1), VertexIndex(slice, stack - 1), VertexIndex(slice - 1, stack));
                        Face(VertexIndex(slice, stack - 1), VertexIndex(slice, stack), VertexIndex(slice - 1, stack));
                    }
                }
            }

            theta += thetaStep;

            if (stack == 0) Face(0, 1, slice);
            else
            {
                Face(VertexIndex(slice - 1, stack - 1), VertexIndex(0, stack - 1), VertexIndex(slice - 1, stack));
                Face(VertexIndex(0, stack - 1), VertexIndex(0, stack), VertexIndex(slice - 1, stack));
            }
        }

        vertexes[vertex] = new(new(0, 0, -radius), -Vector3.UnitZ);

        // Bottom stack is a triangle fan
        for (slice = 1; slice < slices; slice++)
            Face(VertexIndex(slice - 1, stack - 1), VertexIndex(slice, stack - 1), vertex);
        Face(VertexIndex(slice - 1, stack - 1), VertexIndex(0, stack - 1), vertex);

        return Build(device, vertexes, indexes);

        int VertexIndex(int sliceIndex, int stackIndex) => stackIndex * slices + sliceIndex + 1;

        void Face(int a, int b, int c)
        {
            indexes[index++] = (short)a;
            indexes[index++] = (short)b;
            indexes[index++] = (short)c;
        }
    }

    /// <summary>
    /// Creates a cylinder (or truncated cone) centered on the origin along the Z axis, with closed ends.
    /// </summary>
    /// <param name="device">The device to create the mesh on.</param>
    /// <param name="radius1">The radius at the negative Z end.</param>
    /// <param name="radius2">The radius at the positive Z end.</param>
    /// <param name="length">The length along the Z axis.</param>
    /// <param name="slices">The number of slices around the Z axis.</param>
    /// <param name="stacks">The number of stacks along the Z axis.</param>
    public static Mesh Cylinder(IDirect3DDevice9 device, float radius1, float radius2, float length, int slices, int stacks)
    {
        #region Sanity checks
        if (slices < 2) throw new ArgumentOutOfRangeException(nameof(slices));
        if (stacks < 1) throw new ArgumentOutOfRangeException(nameof(stacks));
        #endregion

        var vertexes = new PositionNormal[2 + slices * (3 + stacks)];
        var indexes = new short[3 * (2 * slices + stacks * 2 * slices)];

        var (thetaSin, thetaCos) = SinCosTable(MathF.PI / 2, -2 * MathF.PI / slices, slices);

        float deltaRadius = radius1 - radius2;
        float radius = radius1, radiusStep = deltaRadius / stacks;
        float z = -length / 2, zStep = length / stacks;
        float zNormal = deltaRadius / length;
        if (float.IsNaN(zNormal)) zNormal = 0;

        int vertex = 0, index = 0;
        vertexes[vertex++] = new(new(0, 0, z), -Vector3.UnitZ);

        int slice;
        for (slice = 0; slice < slices; slice++, vertex++)
        {
            vertexes[vertex] = new(new(radius * thetaCos[slice], radius * thetaSin[slice], z), -Vector3.UnitZ);
            if (slice > 0) Face(0, slice, slice + 1);
        }
        Face(0, slice, 1);

        int stack;
        for (stack = 1; stack <= stacks + 1; stack++)
        {
            for (slice = 0; slice < slices; slice++, vertex++)
            {
                vertexes[vertex] = new(
                    new(radius * thetaCos[slice], radius * thetaSin[slice], z),
                    Vector3.Normalize(new(thetaCos[slice], thetaSin[slice], zNormal)));

                if (stack > 1 && slice > 0)
                {
                    Face(VertexIndex(slice - 1, stack - 1), VertexIndex(slice - 1, stack), VertexIndex(slice, stack - 1));
                    Face(VertexIndex(slice, stack - 1), VertexIndex(slice - 1, stack), VertexIndex(slice, stack));
                }
            }

            if (stack > 1)
            {
                Face(VertexIndex(slice - 1, stack - 1), VertexIndex(slice - 1, stack), VertexIndex(0, stack - 1));
                Face(VertexIndex(0, stack - 1), VertexIndex(slice - 1, stack), VertexIndex(0, stack));
            }

            if (stack < stacks + 1)
            {
                z += zStep;
                radius -= radiusStep;
            }
        }

        for (slice = 0; slice < slices; slice++, vertex++)
        {
            vertexes[vertex] = new(new(radius * thetaCos[slice], radius * thetaSin[slice], z), Vector3.UnitZ);
            if (slice > 0) Face(VertexIndex(slice - 1, stack), vertexes.Length - 1, VertexIndex(slice, stack));
        }

        vertexes[vertex] = new(new(0, 0, z), Vector3.UnitZ);
        Face(VertexIndex(slice - 1, stack), vertexes.Length - 1, VertexIndex(0, stack));

        return Build(device, vertexes, indexes);

        int VertexIndex(int sliceIndex, int stackIndex) => stackIndex * slices + sliceIndex + 1;

        void Face(int a, int b, int c)
        {
            indexes[index++] = (short)a;
            indexes[index++] = (short)b;
            indexes[index++] = (short)c;
        }
    }

    private static (float[] sin, float[] cos) SinCosTable(float start, float step, int count)
    {
        var sin = new float[count];
        var cos = new float[count];
        float angle = start;
        for (int i = 0; i < count; i++, angle += step)
        {
            sin[i] = MathF.Sin(angle);
            cos[i] = MathF.Cos(angle);
        }
        return (sin, cos);
    }

    private static Mesh Build(IDirect3DDevice9 device, PositionNormal[] vertexes, short[] indexes)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (vertexes.Length > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(vertexes), "Too many vertexes for 16-bit indexes.");
        #endregion

        var mesh = new Mesh(device, indexes.Length / 3, vertexes.Length, MeshFlags.SystemMemory, Format);
        try
        {
            mesh.WriteVertexBuffer(vertexes);
            mesh.WriteIndexBuffer(indexes);
            mesh.WriteAttributeBuffer(new int[indexes.Length / 3]);
        }
        catch
        {
            mesh.Dispose();
            throw;
        }
        return mesh;
    }
}
