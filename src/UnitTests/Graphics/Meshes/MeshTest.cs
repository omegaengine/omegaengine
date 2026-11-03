/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using AwesomeAssertions;
using OmegaEngine.Graphics.VertexDecl;
using Vortice.Direct3D9;
using Vortice.Mathematics;
using Xunit;

namespace OmegaEngine.Graphics.Meshes;

public unsafe class MeshTest : EngineTestBase
{
    [StructLayout(LayoutKind.Sequential)]
    private record struct PositionNormal(Vector3 Position, Vector3 Normal);

    #region D3DX reference
    private const string D3DX = "d3dx9_43.dll";

    [DllImport(D3DX)]
    private static extern int D3DXCreateBox(nint device, float width, float height, float depth, out nint mesh, nint adjacency);

    [DllImport(D3DX)]
    private static extern int D3DXCreateSphere(nint device, float radius, uint slices, uint stacks, out nint mesh, nint adjacency);

    [DllImport(D3DX)]
    private static extern int D3DXCreateCylinder(nint device, float radius1, float radius2, float length, uint slices, uint stacks, out nint mesh, nint adjacency);

    /// <summary>
    /// Reads the content of a native <c>ID3DXMesh</c> and releases it.
    /// </summary>
    private static (VertexFormat Format, PositionNormal[] Vertexes, int[] Indexes) ReadNative(int hr, nint mesh)
    {
        Marshal.ThrowExceptionForHR(hr);
        try
        {
            // ID3DXMesh vtable (d3dx9mesh.h)
            var vtable = *(void***)mesh;
            int faceCount = (int)((delegate* unmanaged[Stdcall]<nint, uint>)vtable[4])(mesh);
            int vertexCount = (int)((delegate* unmanaged[Stdcall]<nint, uint>)vtable[5])(mesh);
            var format = (VertexFormat)((delegate* unmanaged[Stdcall]<nint, uint>)vtable[6])(mesh);

            void* data;
            Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<nint, uint, void**, int>)vtable[15])(mesh, (uint)LockFlags.ReadOnly, &data));
            var vertexes = new ReadOnlySpan<PositionNormal>(data, vertexCount).ToArray();
            ((delegate* unmanaged[Stdcall]<nint, int>)vtable[16])(mesh);

            Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<nint, uint, void**, int>)vtable[17])(mesh, (uint)LockFlags.ReadOnly, &data));
            var indexes = new ReadOnlySpan<ushort>(data, faceCount * 3).ToArray().Select(x => (int)x).ToArray();
            ((delegate* unmanaged[Stdcall]<nint, int>)vtable[18])(mesh);

            return (format, vertexes, indexes);
        }
        finally
        {
            Marshal.Release(mesh);
        }
    }

    private static void ShouldMatch(Mesh actual, (VertexFormat Format, PositionNormal[] Vertexes, int[] Indexes) expected)
    {
        actual.VertexFormat.Should().Be(expected.Format);
        actual.ReadIndexBuffer().Should().Equal(expected.Indexes);

        var actualVertexes = actual.ReadVertexBuffer<PositionNormal>();
        actualVertexes.Length.Should().Be(expected.Vertexes.Length);
        for (int i = 0; i < actualVertexes.Length; i++)
        {
            Vector3.Distance(actualVertexes[i].Position, expected.Vertexes[i].Position).Should().BeLessThan(1e-5f, $"position {i} should match");
            Vector3.Distance(actualVertexes[i].Normal, expected.Vertexes[i].Normal).Should().BeLessThan(1e-5f, $"normal {i} should match");
        }
    }

    [Fact]
    public void BoxMatchesD3DX()
    {
        using var actual = MeshPrimitives.Box(Engine.Device, 1, 2, 3);
        ShouldMatch(actual, ReadNative(D3DXCreateBox(Engine.Device.NativePointer, 1, 2, 3, out var mesh, 0), mesh));
    }

    [Fact]
    public void SphereMatchesD3DX()
    {
        using var actual = MeshPrimitives.Sphere(Engine.Device, 2, 12, 8);
        ShouldMatch(actual, ReadNative(D3DXCreateSphere(Engine.Device.NativePointer, 2, 12, 8, out var mesh, 0), mesh));
    }

    [Fact]
    public void CylinderMatchesD3DX()
    {
        using var actual = MeshPrimitives.Cylinder(Engine.Device, 1, 2, 3, 10, 4);
        ShouldMatch(actual, ReadNative(D3DXCreateCylinder(Engine.Device.NativePointer, 1, 2, 3, 10, 4, out var mesh, 0), mesh));
    }
    #endregion

    [Fact]
    public void ComputesNormalsFacingOutwards()
    {
        using var mesh = MeshPrimitives.Box(Engine.Device, 1, 1, 1);
        var expected = mesh.ReadVertexBuffer<PositionNormal>();

        // The box has separate vertexes for each side, so the computed normals must match the flat ones of the primitive
        mesh.WriteVertexBuffer(expected.Select(x => x with {Normal = default}).ToArray());
        mesh.ComputeNormals();

        var actual = mesh.ReadVertexBuffer<PositionNormal>();
        for (int i = 0; i < actual.Length; i++)
            Vector3.Distance(actual[i].Normal, expected[i].Normal).Should().BeLessThan(1e-5f, $"normal {i} should match");
    }

    [Fact]
    public void ComputesTangentsAlongTextureCoordinates()
    {
        using var mesh = new Mesh(Engine.Device, faceCount: 2, vertexCount: 4, PositionNormalTangentBinormalTextured.GetVertexElements());
        mesh.WriteVertexBuffer<PositionNormalTangentBinormalTextured>(
        [
            new(new(0, 0, 0), -Vector3.UnitZ, default, default, 0, 1),
            new(new(0, 1, 0), -Vector3.UnitZ, default, default, 0, 0),
            new(new(1, 0, 0), -Vector3.UnitZ, default, default, 1, 1),
            new(new(1, 1, 0), -Vector3.UnitZ, default, default, 1, 0)
        ]);
        mesh.WriteIndexBuffer([0, 1, 3, 3, 2, 0]);

        mesh.ComputeTangents();

        foreach (var vertex in mesh.ReadVertexBuffer<PositionNormalTangentBinormalTextured>())
        {
            Vector3.Distance(vertex.Tangent, Vector3.UnitX).Should().BeLessThan(1e-5f, "U increases along X");
            Vector3.Distance(vertex.Binormal, -Vector3.UnitY).Should().BeLessThan(1e-5f, "V increases along -Y");
        }
    }

    [Fact]
    public void WeldsCoincidentVertexes()
    {
        using var mesh = new Mesh(Engine.Device, faceCount: 2, vertexCount: 6, VertexFormat.Position);
        mesh.WriteVertexBuffer<Vector3>([new(0, 0, 0), new(0, 1, 0), new(1, 1, 0), new(1, 1.00001f, 0), new(1, 0, 0), new(0, 0, 0)]);
        mesh.WriteIndexBuffer([0, 1, 2, 3, 4, 5]);

        mesh.WeldVertexes(positionEpsilon: 0.0001f, normalEpsilon: 0.0001f);

        mesh.VertexCount.Should().Be(4);
        mesh.FaceCount.Should().Be(2);
        var points = mesh.GetPoints();
        var indexes = mesh.ReadIndexBuffer();
        points[indexes[2]].Should().Be(points[indexes[3]]);
        points[indexes[0]].Should().Be(points[indexes[5]]);
    }

    [Fact]
    public void OptimizeSortsFacesBySubsetAndKeepsGeometry()
    {
        using var mesh = MeshPrimitives.Sphere(Engine.Device, 1, 12, 8);
        mesh.WriteAttributeBuffer(Enumerable.Range(0, mesh.FaceCount).Select(face => face % 3).ToArray());
        var facesBefore = Faces(mesh);

        mesh.Optimize();

        mesh.ReadAttributeBuffer().Should().BeInAscendingOrder();
        Faces(mesh).Should().BeEquivalentTo(facesBefore);
    }

    /// <summary>
    /// Gets the corner positions and subset of each face, independent of vertex and face order.
    /// </summary>
    private static string[] Faces(Mesh mesh)
    {
        var points = mesh.GetPoints();
        var indexes = mesh.ReadIndexBuffer();
        var attributes = mesh.ReadAttributeBuffer();
        return Enumerable.Range(0, mesh.FaceCount)
                         .Select(face => $"{attributes[face]}:{points[indexes[face * 3]]}{points[indexes[face * 3 + 1]]}{points[indexes[face * 3 + 2]]}")
                         .ToArray();
    }

    [Fact]
    public void ClonesWithDifferentDeclaration()
    {
        using var mesh = MeshPrimitives.Box(Engine.Device, 1, 1, 1);

        using var clone = mesh.Clone(PositionNormalTextured.Format);

        clone.VertexCount.Should().Be(mesh.VertexCount);
        clone.ReadIndexBuffer().Should().Equal(mesh.ReadIndexBuffer());
        clone.ReadVertexBuffer<PositionNormalTextured>().Select(x => x.Position)
             .Should().Equal(mesh.ReadVertexBuffer<PositionNormal>().Select(x => x.Position));
    }

    [Fact]
    public void IntersectsRays()
    {
        using var mesh = MeshPrimitives.Box(Engine.Device, 2, 2, 2);

        mesh.Intersects(new Ray(new(0, 0, -5), Vector3.UnitZ), out float distance, out _).Should().BeTrue();
        distance.Should().BeApproximately(4, 1e-5f);

        mesh.Intersects(new Ray(new(5, 5, -5), Vector3.UnitZ), out _, out _).Should().BeFalse();
    }

    [Fact]
    public void UploadsToWriteOnlyBuffersWhenDrawn()
    {
        using var mesh = MeshPrimitives.Box(Engine.Device, 1, 1, 1);
        mesh.VertexBuffer.Should().BeNull("nothing is uploaded before the first draw");

        Engine.Device.BeginScene();
        mesh.DrawSubset(0);
        Engine.Device.EndScene();

        var vertexBuffer = mesh.VertexBuffer!.Description;
        vertexBuffer.Pool.Should().Be(Pool.Default);
        vertexBuffer.Usage.Should().HaveFlag(Usage.WriteOnly);
        vertexBuffer.Usage.Should().NotHaveFlag(Usage.Dynamic);
        mesh.IndexBuffer!.Description.Format.Should().Be(Format.Index16);
    }

    [Fact]
    public void SwitchesToDynamicBufferWhenModifiedAfterDrawing()
    {
        using var mesh = MeshPrimitives.Box(Engine.Device, 1, 1, 1);
        Engine.Device.BeginScene();
        mesh.DrawSubset(0);

        mesh.WriteVertexBuffer(mesh.ReadVertexBuffer<PositionNormal>());
        mesh.DrawSubset(0);
        Engine.Device.EndScene();

        mesh.VertexBuffer!.Description.Usage.Should().HaveFlag(Usage.Dynamic);
    }
}
