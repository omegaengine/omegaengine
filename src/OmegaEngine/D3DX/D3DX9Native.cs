/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Numerics;
using System.Runtime.InteropServices;
using SharpGen.Runtime;

namespace OmegaEngine.D3DX;

/// <summary>
/// P/Invoke declarations for the D3DX9 functions used by the engine.
/// </summary>
/// <remarks>See <c>d3dx9effect.h</c>, <c>d3dx9mesh.h</c> and <c>d3dx9core.h</c> from the DirectX SDK.</remarks>
internal static unsafe partial class D3DX9Native
{
    /// <summary>
    /// The DLL containing D3DX9, installed by the DirectX End-User Runtime (June 2010).
    /// </summary>
    public const string DllName = "d3dx9_43.dll";

    [LibraryImport(DllName, StringMarshalling = StringMarshalling.Utf16)]
    public static partial int D3DXCreateEffectFromFileW(nint device, string srcFile, D3DXMacro* defines, nint include, uint flags, nint pool, nint* effect, nint* compilationErrors);

    [LibraryImport(DllName)]
    public static partial int D3DXCreateEffect(nint device, void* srcData, uint srcDataLen, D3DXMacro* defines, nint include, uint flags, nint pool, nint* effect, nint* compilationErrors);

    [LibraryImport(DllName)]
    public static partial int D3DXCreateEffectCompiler(void* srcData, uint srcDataLen, D3DXMacro* defines, nint include, uint flags, nint* compiler, nint* parseErrors);

    [LibraryImport(DllName)]
    public static partial int D3DXLoadMeshFromXInMemory(void* memory, uint sizeOfMemory, uint options, nint device, nint* adjacency, nint* materials, nint* effectInstances, uint* numMaterials, nint* mesh);

    [LibraryImport(DllName)]
    public static partial int D3DXCreateMesh(uint numFaces, uint numVertices, uint options, Vortice.Direct3D9.VertexElement* declaration, nint device, nint* mesh);

    [LibraryImport(DllName)]
    public static partial int D3DXCreateMeshFVF(uint numFaces, uint numVertices, uint options, uint fvf, nint device, nint* mesh);

    [LibraryImport(DllName)]
    public static partial int D3DXComputeNormals(nint mesh, uint* adjacency);

    [LibraryImport(DllName)]
    public static partial int D3DXComputeTangent(nint mesh, uint texStage, uint tangentIndex, uint binormIndex, uint wrap, uint* adjacency);

    [LibraryImport(DllName)]
    public static partial int D3DXWeldVertices(nint mesh, uint flags, WeldEpsilons* epsilons, uint* adjacencyIn, uint* adjacencyOut, uint* faceRemap, nint* vertexRemap);

    [LibraryImport(DllName)]
    public static partial int D3DXIntersect(nint mesh, Vector3* rayPos, Vector3* rayDir, int* hit, uint* faceIndex, float* u, float* v, float* dist, nint* allHits, uint* countOfHits);

    [LibraryImport(DllName, StringMarshalling = StringMarshalling.Utf16)]
    public static partial int D3DXCreateFontW(nint device, int height, uint width, uint weight, uint mipLevels, int italic, uint charSet, uint outputPrecision, uint quality, uint pitchAndFamily, string faceName, nint* font);

    [LibraryImport(DllName)]
    public static partial int D3DXCreateSprite(nint device, nint* sprite);

    /// <summary>
    /// Throws a <see cref="SharpGenException"/> for failed <see cref="Result"/>s, using the text of an optional <c>ID3DXBuffer</c> with error messages.
    /// </summary>
    /// <param name="hr">The result code returned by the native function.</param>
    /// <param name="errors">A pointer to an <c>ID3DXBuffer</c> containing error messages; may be <c>0</c>. Is always released.</param>
    public static void Check(int hr, nint errors = 0)
    {
        string? message = errors == 0 ? null : D3DXBuffer.ReadStringAndRelease(errors);
        if (hr < 0) throw new SharpGenException(new Result(hr), message);
    }
}

/// <summary>
/// <c>D3DXMACRO</c>
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct D3DXMacro
{
    public nint Name;
    public nint Definition;
}
