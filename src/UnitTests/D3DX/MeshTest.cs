/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using AwesomeAssertions;
using OmegaEngine.Graphics;
using Vortice.Direct3D9;
using Vortice.Mathematics;
using Xunit;

namespace OmegaEngine.D3DX;

public class MeshTest : EngineTestBase
{
    [DllImport(D3DX9Native.DllName)]
    private static extern int D3DXCreateBox(nint device, float width, float height, float depth, out nint mesh, nint adjacency);

    [DllImport(D3DX9Native.DllName)]
    private static extern int D3DXCreateSphere(nint device, float radius, uint slices, uint stacks, out nint mesh, nint adjacency);

    [DllImport(D3DX9Native.DllName)]
    private static extern int D3DXCreateCylinder(nint device, float radius1, float radius2, float length, uint slices, uint stacks, out nint mesh, nint adjacency);

    private Mesh Wrap(int hr, nint mesh)
    {
        Marshal.ThrowExceptionForHR(hr);
        return new(Engine.Device, mesh);
    }

    [Fact]
    public void BoxMatchesD3DX()
    {
        using var expected = Wrap(D3DXCreateBox(Engine.Device.NativePointer, 1, 2, 3, out var ptr, 0), ptr);
        using var actual = MeshPrimitives.Box(Engine.Device, 1, 2, 3);

        ShouldMatch(actual, expected);
    }

    [Fact]
    public void SphereMatchesD3DX()
    {
        using var expected = Wrap(D3DXCreateSphere(Engine.Device.NativePointer, 2, 12, 8, out var ptr, 0), ptr);
        using var actual = MeshPrimitives.Sphere(Engine.Device, 2, 12, 8);

        ShouldMatch(actual, expected);
    }

    [Fact]
    public void CylinderMatchesD3DX()
    {
        using var expected = Wrap(D3DXCreateCylinder(Engine.Device.NativePointer, 1, 2, 3, 10, 4, out var ptr, 0), ptr);
        using var actual = MeshPrimitives.Cylinder(Engine.Device, 1, 2, 3, 10, 4);

        ShouldMatch(actual, expected);
    }

    private static void ShouldMatch(Mesh actual, Mesh expected)
    {
        actual.VertexFormat.Should().Be(expected.VertexFormat);
        actual.ReadIndexBuffer().Should().Equal(expected.ReadIndexBuffer());

        var actualVertexes = actual.ReadVertexBuffer<PositionNormal>();
        var expectedVertexes = expected.ReadVertexBuffer<PositionNormal>();
        actualVertexes.Length.Should().Be(expectedVertexes.Length);
        for (int i = 0; i < actualVertexes.Length; i++)
        {
            Vector3.Distance(actualVertexes[i].Position, expectedVertexes[i].Position).Should().BeLessThan(1e-5f, $"position {i} should match");
            Vector3.Distance(actualVertexes[i].Normal, expectedVertexes[i].Normal).Should().BeLessThan(1e-5f, $"normal {i} should match");
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private record struct PositionNormal(Vector3 Position, Vector3 Normal);

    [Fact]
    public void LoadsXFilesWithMaterials()
    {
        using var stream = Foundation.Storage.ContentManager.GetFileStream("Meshes", "Test/Box/Normal/Normal.x");
        using var mesh = Mesh.FromStream(Engine.Device, stream, MeshFlags.SystemMemory);

        mesh.FaceCount.Should().BePositive();
        mesh.GetMaterials().Should().ContainSingle().Which.TextureFileName.Should().Be("Riverstone.png");
        mesh.GetDeclaration().Last().Should().Be(VertexElement.VertexDeclarationEnd);
    }

    [Fact]
    public void ProcessesMeshes()
    {
        using var mesh = MeshPrimitives.Box(Engine.Device, 1, 1, 1);

        mesh.GenerateAdjacency(epsilon: 0);
        mesh.ComputeNormals();
        mesh.OptimizeInPlace(MeshOptimizeFlags.VertexCache | MeshOptimizeFlags.AttributeSort);
        mesh.WeldVertices(WeldFlags.WeldPartialMatches, new() {Position = 0.0001f, Normal = 0.0001f});
        mesh.FaceCount.Should().Be(12);

        using var clone = mesh.Clone(Engine.Device, MeshFlags.SystemMemory, mesh.GetDeclaration());
        clone.VertexCount.Should().Be(mesh.VertexCount);
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
    public void ExposesBuffers()
    {
        using var mesh = MeshPrimitives.Box(Engine.Device, 1, 1, 1);

        using var vertexBuffer = mesh.GetVertexBuffer();
        vertexBuffer.Description.Pool.Should().Be(Pool.SystemMemory);
        using var indexBuffer = mesh.GetIndexBuffer();
        indexBuffer.Description.Format.Should().Be(Format.Index16);
    }
}
