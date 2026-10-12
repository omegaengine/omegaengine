/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using SharpGen.Runtime;
using Vortice.Direct3D9;
using Vortice.Mathematics;

namespace OmegaEngine.D3DX;

/// <summary>
/// Wraps an <c>ID3DXMesh</c>: a vertex buffer, an index buffer and an attribute buffer assigning faces to subsets.
/// </summary>
/// <remarks>
/// Keeps the face adjacency information returned by <see cref="FromStream"/>, <see cref="GenerateAdjacency"/>, <see cref="OptimizeInPlace"/> and <see cref="WeldVertices"/>
/// and passes it on to the D3DX functions that use it. <see cref="Clone(IDirect3DDevice9,MeshFlags,VertexElement[])"/> does not copy it.
/// </remarks>
public sealed unsafe class Mesh : ComObject
{
    #region vtable
    // ID3DXMesh (d3dx9mesh.h)
    private const int
        VtDrawSubset = 3,
        VtGetNumFaces = 4,
        VtGetNumVertices = 5,
        VtGetFVF = 6,
        VtGetDeclaration = 7,
        VtGetNumBytesPerVertex = 8,
        VtGetOptions = 9,
        VtCloneMeshFVF = 11,
        VtCloneMesh = 12,
        VtGetVertexBuffer = 13,
        VtGetIndexBuffer = 14,
        VtLockVertexBuffer = 15,
        VtUnlockVertexBuffer = 16,
        VtLockIndexBuffer = 17,
        VtUnlockIndexBuffer = 18,
        VtGenerateAdjacency = 22,
        VtLockAttributeBuffer = 24,
        VtUnlockAttributeBuffer = 25,
        VtOptimizeInplace = 27;

    /// <summary><c>MAX_FVF_DECL_SIZE</c></summary>
    private const int MaxFvfDeclSize = 65;
    #endregion

    /// <summary>
    /// The device the mesh was created on.
    /// </summary>
    public IDirect3DDevice9 Device { get; }

    private uint[]? _adjacency;
    private ExtendedMaterial[] _materials = [];
    private EffectInstance[] _effects = [];

    internal Mesh(IDirect3DDevice9 device, nint nativePointer) : base(nativePointer)
    {
        Device = device;
    }

    #region Creation
    /// <summary>
    /// Creates an empty mesh with a fixed-function vertex format.
    /// </summary>
    public Mesh(IDirect3DDevice9 device, int faceCount, int vertexCount, MeshFlags options, VertexFormat format)
        : this(device, Create(device, faceCount, vertexCount, options, format))
    {}

    /// <summary>
    /// Creates an empty mesh with a vertex declaration.
    /// </summary>
    public Mesh(IDirect3DDevice9 device, int faceCount, int vertexCount, MeshFlags options, VertexElement[] vertexDeclaration)
        : this(device, Create(device, faceCount, vertexCount, options, vertexDeclaration))
    {}

    private static nint Create(IDirect3DDevice9 device, int faceCount, int vertexCount, MeshFlags options, VertexFormat format)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        #endregion

        nint mesh;
        new Result(D3DX9Native.D3DXCreateMeshFVF((uint)faceCount, (uint)vertexCount, (uint)options, (uint)format, device.NativePointer, &mesh)).CheckError();
        return mesh;
    }

    private static nint Create(IDirect3DDevice9 device, int faceCount, int vertexCount, MeshFlags options, VertexElement[] vertexDeclaration)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (vertexDeclaration == null) throw new ArgumentNullException(nameof(vertexDeclaration));
        #endregion

        nint mesh;
        fixed (VertexElement* pDecl = vertexDeclaration)
            new Result(D3DX9Native.D3DXCreateMesh((uint)faceCount, (uint)vertexCount, (uint)options, pDecl, device.NativePointer, &mesh)).CheckError();
        return mesh;
    }

    /// <summary>
    /// Loads a mesh, its materials and effect instances from an .X file.
    /// </summary>
    /// <exception cref="SharpGenException">The data is not a valid .X file.</exception>
    public static Mesh FromStream(IDirect3DDevice9 device, Stream stream, MeshFlags options)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        #endregion

        byte[] data;
        using (var memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            data = memoryStream.ToArray();
        }

        nint meshPtr, adjacency, materials, effectInstances;
        uint materialCount;
        fixed (byte* pData = data)
            new Result(D3DX9Native.D3DXLoadMeshFromXInMemory(pData, (uint)data.Length, (uint)options, device.NativePointer, &adjacency, &materials, &effectInstances, &materialCount, &meshPtr)).CheckError();

        var mesh = new Mesh(device, meshPtr);
        try
        {
            mesh._adjacency = ReadBuffer<uint>(adjacency, mesh.FaceCount * 3);
            mesh._materials = ExtendedMaterial.FromBuffer(materials, (int)materialCount);
            mesh._effects = EffectInstance.FromBuffer(effectInstances, (int)materialCount);
        }
        catch
        {
            mesh.Dispose();
            throw;
        }
        finally
        {
            D3DXBuffer.ReleaseBuffer(adjacency);
            D3DXBuffer.ReleaseBuffer(materials);
            D3DXBuffer.ReleaseBuffer(effectInstances);
        }
        return mesh;
    }

    private static T[]? ReadBuffer<T>(nint buffer, int count) where T : unmanaged
    {
        if (buffer == 0) return null;
        var (pointer, size) = D3DXBuffer.GetData(buffer);
        return new Span<T>((void*)pointer, Math.Min(count, size / sizeof(T))).ToArray();
    }

    /// <summary>
    /// Creates a copy of the mesh with a different vertex declaration and/or options.
    /// </summary>
    public Mesh Clone(IDirect3DDevice9 device, MeshFlags options, VertexElement[] vertexDeclaration)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (vertexDeclaration == null) throw new ArgumentNullException(nameof(vertexDeclaration));
        #endregion

        nint clone;
        fixed (VertexElement* pDecl = vertexDeclaration)
            Check(((delegate* unmanaged[Stdcall]<nint, uint, VertexElement*, nint, nint*, int>)this[VtCloneMesh])(NativePointer, (uint)options, pDecl, device.NativePointer, &clone));
        return new(device, clone);
    }

    /// <summary>
    /// Creates a copy of the mesh with a different fixed-function vertex format and/or options.
    /// </summary>
    public Mesh Clone(IDirect3DDevice9 device, MeshFlags options, VertexFormat format)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        #endregion

        nint clone;
        Check(((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, nint*, int>)this[VtCloneMeshFVF])(NativePointer, (uint)options, (uint)format, device.NativePointer, &clone));
        return new(device, clone);
    }
    #endregion

    #region Properties
    /// <summary>
    /// The materials loaded along with the mesh by <see cref="FromStream"/>, one per subset.
    /// </summary>
    public ExtendedMaterial[] GetMaterials() => _materials;

    /// <summary>
    /// The effect instances loaded along with the mesh by <see cref="FromStream"/>, one per subset.
    /// </summary>
    public EffectInstance[] GetEffects() => _effects;

    /// <summary>
    /// The number of faces (triangles) in the mesh.
    /// </summary>
    public int FaceCount => (int)((delegate* unmanaged[Stdcall]<nint, uint>)this[VtGetNumFaces])(NativePointer);

    /// <summary>
    /// The number of vertices in the mesh.
    /// </summary>
    public int VertexCount => (int)((delegate* unmanaged[Stdcall]<nint, uint>)this[VtGetNumVertices])(NativePointer);

    /// <summary>
    /// The fixed-function vertex format of the mesh; <see cref="VertexFormat.None"/> if it uses a vertex declaration that has no equivalent.
    /// </summary>
    public VertexFormat VertexFormat => (VertexFormat)((delegate* unmanaged[Stdcall]<nint, uint>)this[VtGetFVF])(NativePointer);

    /// <summary>
    /// The size of a single vertex in bytes.
    /// </summary>
    public int BytesPerVertex => (int)((delegate* unmanaged[Stdcall]<nint, uint>)this[VtGetNumBytesPerVertex])(NativePointer);

    /// <summary>
    /// The options the mesh was created with.
    /// </summary>
    public MeshFlags CreationOptions => (MeshFlags)((delegate* unmanaged[Stdcall]<nint, uint>)this[VtGetOptions])(NativePointer);

    /// <summary>
    /// Gets the vertex declaration of the mesh, including the <see cref="VertexElement.VertexDeclarationEnd"/> marker.
    /// </summary>
    public VertexElement[] GetDeclaration()
    {
        var decl = stackalloc VertexElement[MaxFvfDeclSize];
        Check(((delegate* unmanaged[Stdcall]<nint, VertexElement*, int>)this[VtGetDeclaration])(NativePointer, decl));

        int count = 0;
        while (count < MaxFvfDeclSize && decl[count].Stream != 0xFF) count++;
        return new Span<VertexElement>(decl, Math.Min(count + 1, MaxFvfDeclSize)).ToArray();
    }

    /// <summary>
    /// Gets the vertex buffer of the mesh. The caller is responsible for disposing it.
    /// </summary>
    public IDirect3DVertexBuffer9 GetVertexBuffer()
    {
        nint buffer;
        Check(((delegate* unmanaged[Stdcall]<nint, nint*, int>)this[VtGetVertexBuffer])(NativePointer, &buffer));
        return new(buffer);
    }

    /// <summary>
    /// Gets the index buffer of the mesh. The caller is responsible for disposing it.
    /// </summary>
    public IDirect3DIndexBuffer9 GetIndexBuffer()
    {
        nint buffer;
        Check(((delegate* unmanaged[Stdcall]<nint, nint*, int>)this[VtGetIndexBuffer])(NativePointer, &buffer));
        return new(buffer);
    }
    #endregion

    #region Buffer access
    /// <summary>
    /// Locks the vertex buffer and returns a pointer to its data. Must be followed by <see cref="UnlockVertexBuffer"/>.
    /// </summary>
    public nint LockVertexBuffer(LockFlags flags) => Lock(VtLockVertexBuffer, flags);

    /// <summary>
    /// Unlocks the vertex buffer.
    /// </summary>
    public void UnlockVertexBuffer() => Check(((delegate* unmanaged[Stdcall]<nint, int>)this[VtUnlockVertexBuffer])(NativePointer));

    /// <summary>
    /// Locks the index buffer and returns a pointer to its data. Must be followed by <see cref="UnlockIndexBuffer"/>.
    /// </summary>
    public nint LockIndexBuffer(LockFlags flags) => Lock(VtLockIndexBuffer, flags);

    /// <summary>
    /// Unlocks the index buffer.
    /// </summary>
    public void UnlockIndexBuffer() => Check(((delegate* unmanaged[Stdcall]<nint, int>)this[VtUnlockIndexBuffer])(NativePointer));

    /// <summary>
    /// Locks the attribute buffer (one 32-bit subset ID per face) and returns a pointer to its data. Must be followed by <see cref="UnlockAttributeBuffer"/>.
    /// </summary>
    public nint LockAttributeBuffer(LockFlags flags) => Lock(VtLockAttributeBuffer, flags);

    /// <summary>
    /// Unlocks the attribute buffer.
    /// </summary>
    public void UnlockAttributeBuffer() => Check(((delegate* unmanaged[Stdcall]<nint, int>)this[VtUnlockAttributeBuffer])(NativePointer));

    private nint Lock(int slot, LockFlags flags)
    {
        nint data;
        Check(((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)this[slot])(NativePointer, (uint)flags, &data));
        return data;
    }
    #endregion

    #region Rendering
    /// <summary>
    /// Draws all faces of a subset.
    /// </summary>
    public void DrawSubset(int subset) => Check(((delegate* unmanaged[Stdcall]<nint, uint, int>)this[VtDrawSubset])(NativePointer, (uint)subset));
    #endregion

    #region Processing
    /// <summary>
    /// Generates and stores face adjacency information.
    /// </summary>
    /// <param name="epsilon">Vertices closer than this are treated as coincident.</param>
    public void GenerateAdjacency(float epsilon)
    {
        var adjacency = new uint[FaceCount * 3];
        fixed (uint* pAdjacency = adjacency)
            Check(((delegate* unmanaged[Stdcall]<nint, float, uint*, int>)this[VtGenerateAdjacency])(NativePointer, epsilon, pAdjacency));
        _adjacency = adjacency;
    }

    /// <summary>
    /// Reorders the faces and vertices of the mesh to improve rendering performance.
    /// </summary>
    public void OptimizeInPlace(MeshOptimizeFlags flags)
    {
        var adjacencyOut = new uint[FaceCount * 3];
        int hr;
        fixed (uint* pAdjacencyIn = _adjacency)
        fixed (uint* pAdjacencyOut = adjacencyOut)
            hr = ((delegate* unmanaged[Stdcall]<nint, uint, uint*, uint*, uint*, nint*, int>)this[VtOptimizeInplace])(NativePointer, (uint)flags, pAdjacencyIn, pAdjacencyOut, null, null);
        _adjacency = hr < 0 ? null : adjacencyOut;
        Check(hr);
    }

    /// <summary>
    /// Computes vertex normals by averaging the normals of the adjacent faces.
    /// </summary>
    public void ComputeNormals()
    {
        fixed (uint* pAdjacency = _adjacency)
            Check(D3DX9Native.D3DXComputeNormals(NativePointer, pAdjacency));
    }

    /// <summary>
    /// Computes tangent and binormal vectors for the given texture coordinate set.
    /// </summary>
    public void ComputeTangent(int textureStage, int tangentIndex, int binormalIndex, bool wrap)
    {
        fixed (uint* pAdjacency = _adjacency)
            Check(D3DX9Native.D3DXComputeTangent(NativePointer, (uint)textureStage, (uint)tangentIndex, (uint)binormalIndex, wrap ? 1u : 0u, pAdjacency));
    }

    /// <summary>
    /// Merges vertices that are equal within the given tolerances.
    /// </summary>
    public void WeldVertices(WeldFlags flags, WeldEpsilons epsilons)
    {
        var adjacencyOut = new uint[FaceCount * 3];
        int hr;
        fixed (uint* pAdjacencyIn = _adjacency)
        fixed (uint* pAdjacencyOut = adjacencyOut)
            hr = D3DX9Native.D3DXWeldVertices(NativePointer, (uint)flags, &epsilons, pAdjacencyIn, pAdjacencyOut, null, null);
        _adjacency = hr < 0 ? null : adjacencyOut;
        Check(hr);
    }

    /// <summary>
    /// Determines whether a ray intersects the mesh.
    /// </summary>
    /// <param name="ray">The ray in mesh space.</param>
    /// <param name="distance">The distance from the ray's origin to the closest intersection, in multiples of the ray's direction vector.</param>
    /// <param name="faceIndex">The index of the closest face that was hit.</param>
    /// <returns><c>true</c> if the ray hits any face; <c>false</c> otherwise.</returns>
    public bool Intersects(Ray ray, out float distance, out int faceIndex)
    {
        var position = ray.Position;
        var direction = ray.Direction;
        int hit;
        uint face;
        float u, v, dist;
        Check(D3DX9Native.D3DXIntersect(NativePointer, &position, &direction, &hit, &face, &u, &v, &dist, null, null));
        distance = dist;
        faceIndex = (int)face;
        return hit != 0;
    }
    #endregion

    private static void Check(int hr) => new Result(hr).CheckError();
}

/// <summary>
/// A material with a texture file name, as loaded from an .X file (<c>D3DXMATERIAL</c>).
/// </summary>
public readonly record struct ExtendedMaterial(Material9 MaterialD3D, string? TextureFileName)
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Native
    {
        public Material9 MaterialD3D;
        public nint TextureFileName;
    }

    internal static unsafe ExtendedMaterial[] FromBuffer(nint buffer, int count)
    {
        if (buffer == 0 || count == 0) return [];

        var native = (Native*)D3DXBuffer.GetData(buffer).pointer;
        var result = new ExtendedMaterial[count];
        for (int i = 0; i < count; i++)
            result[i] = new(native[i].MaterialD3D, Marshal.PtrToStringAnsi(native[i].TextureFileName));
        return result;
    }
}

/// <summary>
/// An effect file and its parameter defaults, as loaded from an .X file (<c>D3DXEFFECTINSTANCE</c>).
/// </summary>
public sealed record EffectInstance(string? EffectFileName, EffectDefault[] Defaults)
{
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct Native
    {
        public nint EffectFileName;
        public uint NumDefaults;
        public NativeDefault* Defaults;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeDefault
    {
        public nint ParamName;
        public EffectDefaultType Type;
        public uint NumBytes;
        public nint Value;
    }

    internal static unsafe EffectInstance[] FromBuffer(nint buffer, int count)
    {
        if (buffer == 0 || count == 0) return [];

        var native = (Native*)D3DXBuffer.GetData(buffer).pointer;
        var result = new EffectInstance[count];
        for (int i = 0; i < count; i++)
        {
            var defaults = new EffectDefault[native[i].NumDefaults];
            for (int j = 0; j < defaults.Length; j++)
            {
                var d = native[i].Defaults[j];
                defaults[j] = new(Marshal.PtrToStringAnsi(d.ParamName) ?? "", d.Type, new Span<byte>((void*)d.Value, (int)d.NumBytes).ToArray());
            }
            result[i] = new(Marshal.PtrToStringAnsi(native[i].EffectFileName), defaults);
        }
        return result;
    }
}

/// <summary>
/// A parameter default of an <see cref="EffectInstance"/> (<c>D3DXEFFECTDEFAULT</c>).
/// </summary>
public sealed record EffectDefault(string ParameterName, EffectDefaultType Type, byte[] Value);

/// <summary>
/// The type of an <see cref="EffectDefault"/> (<c>D3DXEFFECTDEFAULTTYPE</c>).
/// </summary>
public enum EffectDefaultType
{
    String = 1,
    Floats = 2,
    Dword = 3
}

/// <summary>
/// Options for creating meshes (<c>D3DXMESH</c>).
/// </summary>
[Flags]
public enum MeshFlags
{
    None = 0,
    Use32Bit = 0x001,
    DoNotClip = 0x002,
    Points = 0x004,
    RTPatches = 0x008,
    NPatches = 0x4000,
    VertexBufferSystemMemory = 0x010,
    VertexBufferManaged = 0x020,
    VertexBufferWriteOnly = 0x040,
    VertexBufferDynamic = 0x080,
    VertexBufferSoftware = 0x8000,
    IndexBufferSystemMemory = 0x100,
    IndexBufferManaged = 0x200,
    IndexBufferWriteOnly = 0x400,
    IndexBufferDynamic = 0x800,
    IndexBufferSoftware = 0x10000,
    VertexBufferShare = 0x1000,
    UseHardwareOnly = 0x2000,
    SystemMemory = VertexBufferSystemMemory | IndexBufferSystemMemory,
    Managed = VertexBufferManaged | IndexBufferManaged,
    WriteOnly = VertexBufferWriteOnly | IndexBufferWriteOnly,
    Dynamic = VertexBufferDynamic | IndexBufferDynamic,
    Software = VertexBufferSoftware | IndexBufferSoftware
}

/// <summary>
/// Options for <see cref="Mesh.OptimizeInPlace"/> (<c>D3DXMESHOPT</c>).
/// </summary>
[Flags]
public enum MeshOptimizeFlags
{
    Compact = 0x01000000,
    AttributeSort = 0x02000000,
    VertexCache = 0x04000000,
    StripReorder = 0x08000000,
    IgnoreVertices = 0x10000000,
    DoNotSplit = 0x20000000,
    DeviceIndependent = 0x00400000
}

/// <summary>
/// Options for <see cref="Mesh.WeldVertices"/> (<c>D3DXWELDEPSILONSFLAGS</c>).
/// </summary>
[Flags]
public enum WeldFlags
{
    WeldAll = 0x1,
    WeldPartialMatches = 0x2,
    DoNotRemoveVertices = 0x4,
    DoNotSplit = 0x8
}

/// <summary>
/// Tolerances for <see cref="Mesh.WeldVertices"/> (<c>D3DXWELDEPSILONS</c>).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct WeldEpsilons
{
    public float Position, BlendWeights, Normal, PSize, Specular, Diffuse;
    public float TextureCoordinate1, TextureCoordinate2, TextureCoordinate3, TextureCoordinate4, TextureCoordinate5, TextureCoordinate6, TextureCoordinate7, TextureCoordinate8;
    public float Tangent, Binormal, TessFactor;
}
