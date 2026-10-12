/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpGen.Runtime;
using Vortice.Direct3D9;
using Vortice.Mathematics;

namespace OmegaEngine.D3DX;

/// <summary>
/// A frame hierarchy with meshes and animations, as loaded from an .X file by <c>D3DXLoadMeshHierarchyFromX</c>.
/// </summary>
/// <remarks>
/// The native <c>D3DXFRAME</c>/<c>D3DXMESHCONTAINER</c> hierarchy is copied into managed <see cref="Frame"/>s and <see cref="MeshContainer"/>s and destroyed right after loading.
/// The <c>ID3DXAnimationController</c> is only used to get at the <see cref="AnimationSets"/> and released after loading too.
/// </remarks>
internal sealed class FrameHierarchy : IDisposable
{
    /// <summary>
    /// The top-level frames.
    /// </summary>
    public IReadOnlyList<Frame> RootFrames { get; }

    /// <summary>
    /// All mesh containers in the hierarchy, in load order.
    /// </summary>
    public IReadOnlyList<MeshContainer> MeshContainers { get; }

    /// <summary>
    /// A bounding sphere surrounding all meshes in the hierarchy in its bind pose.
    /// </summary>
    public BoundingSphere BoundingSphere { get; }

    /// <summary>
    /// The animation sets, in the order the animation controller lists them (the reverse of the order in the file).
    /// </summary>
    public ImmutableArray<AnimationSet> AnimationSets { get; }

    private FrameHierarchy(IReadOnlyList<Frame> rootFrames, IReadOnlyList<MeshContainer> meshContainers, BoundingSphere boundingSphere, ImmutableArray<AnimationSet> animationSets)
    {
        RootFrames = rootFrames;
        MeshContainers = meshContainers;
        BoundingSphere = boundingSphere;
        AnimationSets = animationSets;
    }

    /// <summary>
    /// Loads a frame hierarchy from an .X file.
    /// </summary>
    /// <exception cref="SharpGenException">The data is not a valid .X file.</exception>
    public static unsafe FrameHierarchy FromStream(IDirect3DDevice9 device, Stream stream, MeshFlags options)
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

        var allocator = new HierarchyAllocator();
        var handle = GCHandle.Alloc(allocator);
        var nativeAllocator = new HierarchyAllocator.Native {VTable = HierarchyAllocator.VTable, Handle = GCHandle.ToIntPtr(handle)};
        NativeFrame* root = null;
        nint controller = 0;
        try
        {
            fixed (byte* pData = data)
                new Result(D3DX9Native.D3DXLoadMeshHierarchyFromXInMemory(pData, (uint)data.Length, (uint)options, device.NativePointer, &nativeAllocator, 0, (void**)&root, &controller)).CheckError();

            // Must happen before converting, which takes the meshes out of the native hierarchy
            Vector3 center;
            float radius;
            new Result(D3DX9Native.D3DXFrameCalculateBoundingSphere(root, &center, &radius)).CheckError();

            var meshContainers = new List<MeshContainer>();
            var animationSets = ImmutableArray<AnimationSet>.Empty;
            try
            {
                var rootFrames = allocator.ToManaged(device, root, meshContainers);
                if (controller != 0) animationSets = GetAnimationSets(controller);
                return new(rootFrames, meshContainers, new(center, radius), animationSets);
            }
            catch
            {
                foreach (var container in meshContainers) container.Dispose();
                foreach (var set in animationSets) set.Dispose();
                throw;
            }
        }
        finally
        {
            if (controller != 0) Marshal.Release(controller);
            if (root != null) D3DX9Native.D3DXFrameDestroy(root, &nativeAllocator);
            handle.Free();
        }
    }

    private static unsafe ImmutableArray<AnimationSet> GetAnimationSets(nint controller)
    {
        // ID3DXAnimationController (d3dx9anim.h)
        const int vtGetNumAnimationSets = 10, vtGetAnimationSet = 11;
        var vtable = *(void***)controller;

        int count = (int)((delegate* unmanaged[Stdcall]<nint, uint>)vtable[vtGetNumAnimationSets])(controller);
        var builder = ImmutableArray.CreateBuilder<AnimationSet>(count);
        try
        {
            for (int i = 0; i < count; i++)
            {
                nint set;
                new Result(((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)vtable[vtGetAnimationSet])(controller, (uint)i, &set)).CheckError();
                builder.Add(new(set));
            }
        }
        catch
        {
            foreach (var set in builder) set.Dispose();
            throw;
        }
        return builder.MoveToImmutable();
    }

    /// <summary>
    /// Disposes all <see cref="MeshContainers"/> and <see cref="AnimationSets"/>.
    /// </summary>
    public void Dispose()
    {
        foreach (var container in MeshContainers) container.Dispose();
        foreach (var set in AnimationSets) set.Dispose();
    }

    #region Native structs
    /// <summary><c>D3DXFRAME</c></summary>
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeFrame
    {
        public byte* Name;
        public Matrix4x4 TransformationMatrix;
        public NativeMeshContainer* MeshContainer;
        public NativeFrame* Sibling;
        public NativeFrame* FirstChild;
    }

    /// <summary><c>D3DXMESHDATA</c></summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeMeshData
    {
        /// <summary><c>D3DXMESHTYPE_MESH</c></summary>
        public const int TypeMesh = 1;

        public int Type;
        public nint Mesh;
    }

    /// <summary><c>D3DXMESHCONTAINER</c></summary>
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeMeshContainer
    {
        public byte* Name;
        public NativeMeshData MeshData;
        public nint Materials;
        public nint Effects;
        public uint NumMaterials;
        public uint* Adjacency;
        public nint SkinInfo;
        public NativeMeshContainer* NextMeshContainer;
    }
    #endregion

    /// <summary>
    /// Implements <c>ID3DXAllocateHierarchy</c>, which D3DX calls back to allocate and free frames and mesh containers.
    /// </summary>
    /// <remarks>
    /// Mesh containers keep a reference to their mesh and skin info, but not the materials, which are copied to managed memory instead.
    /// </remarks>
    private sealed unsafe class HierarchyAllocator
    {
        private const int S_OK = 0, E_FAIL = unchecked((int)0x80004005), E_NOTIMPL = unchecked((int)0x80004001);

        /// <summary>The native object D3DX calls methods on. Starts with the vtable, like a C++ object.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Native
        {
            public void** VTable;
            public nint Handle;
        }

        /// <summary>The <c>ID3DXAllocateHierarchy</c> vtable shared by all instances.</summary>
        public static readonly void** VTable = CreateVTable();

        private static void** CreateVTable()
        {
            var vtable = (void**)NativeMemory.Alloc(4, (nuint)sizeof(void*));
            vtable[0] = (delegate* unmanaged[Stdcall]<Native*, byte*, NativeFrame**, int>)&CreateFrame;
            vtable[1] = (delegate* unmanaged[Stdcall]<Native*, byte*, NativeMeshData*, nint, nint, uint, uint*, nint, NativeMeshContainer**, int>)&CreateMeshContainer;
            vtable[2] = (delegate* unmanaged[Stdcall]<Native*, NativeFrame*, int>)&DestroyFrame;
            vtable[3] = (delegate* unmanaged[Stdcall]<Native*, NativeMeshContainer*, int>)&DestroyMeshContainer;
            return vtable;
        }

        /// <summary>Materials and effects of the mesh containers, copied in <see cref="CreateMeshContainer"/>.</summary>
        private readonly Dictionary<nint, (ExtendedMaterial[] Materials, EffectInstance[] Effects)> _materials = [];

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        private static int CreateFrame(Native* self, byte* name, NativeFrame** newFrame)
        {
            var frame = (NativeFrame*)NativeMemory.AllocZeroed((nuint)sizeof(NativeFrame));
            frame->Name = CopyString(name);
            frame->TransformationMatrix = Matrix4x4.Identity;
            *newFrame = frame;
            return S_OK;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        private static int CreateMeshContainer(Native* self, byte* name, NativeMeshData* meshData, nint materials, nint effectInstances, uint numMaterials, uint* adjacency, nint skinInfo, NativeMeshContainer** newContainer)
        {
            *newContainer = null;
            if (meshData->Type != NativeMeshData.TypeMesh) return E_NOTIMPL;

            (ExtendedMaterial[], EffectInstance[]) copied;
            try
            {
                copied = (ExtendedMaterial.FromPointer(materials, (int)numMaterials), EffectInstance.FromPointer(effectInstances, (int)numMaterials));
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return E_FAIL;
            }

            var container = (NativeMeshContainer*)NativeMemory.AllocZeroed((nuint)sizeof(NativeMeshContainer));
            container->Name = CopyString(name);
            container->MeshData = *meshData;
            Marshal.AddRef(meshData->Mesh);
            if (skinInfo != 0)
            {
                container->SkinInfo = skinInfo;
                Marshal.AddRef(skinInfo);
            }

            var allocator = (HierarchyAllocator)GCHandle.FromIntPtr(self->Handle).Target!;
            allocator._materials[(nint)container] = copied;

            *newContainer = container;
            return S_OK;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        private static int DestroyFrame(Native* self, NativeFrame* frame)
        {
            NativeMemory.Free(frame->Name);
            NativeMemory.Free(frame);
            return S_OK;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        private static int DestroyMeshContainer(Native* self, NativeMeshContainer* container)
        {
            var allocator = (HierarchyAllocator)GCHandle.FromIntPtr(self->Handle).Target!;
            allocator._materials.Remove((nint)container);

            if (container->MeshData.Mesh != 0) Marshal.Release(container->MeshData.Mesh);
            if (container->SkinInfo != 0) Marshal.Release(container->SkinInfo);
            NativeMemory.Free(container->Name);
            NativeMemory.Free(container);
            return S_OK;
        }

        private static byte* CopyString(byte* source)
        {
            if (source == null) return null;
            var span = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(source);
            var copy = (byte*)NativeMemory.Alloc((nuint)span.Length + 1);
            span.CopyTo(new Span<byte>(copy, span.Length));
            copy[span.Length] = 0;
            return copy;
        }

        /// <summary>
        /// Copies a frame and its siblings to managed <see cref="Frame"/>s, taking over the meshes and skin infos.
        /// </summary>
        /// <param name="device">The device the meshes were created on.</param>
        /// <param name="frame">The first frame to copy.</param>
        /// <param name="meshContainers">Collects all mesh containers.</param>
        public List<Frame> ToManaged(IDirect3DDevice9 device, NativeFrame* frame, List<MeshContainer> meshContainers)
        {
            var frames = new List<Frame>();
            for (; frame != null; frame = frame->Sibling)
            {
                var containers = new List<MeshContainer>();
                for (var container = frame->MeshContainer; container != null; container = container->NextMeshContainer)
                {
                    var mesh = new Mesh(device, container->MeshData.Mesh);
                    container->MeshData.Mesh = 0;
                    var skinInfo = container->SkinInfo == 0 ? null : new SkinInfo(container->SkinInfo);
                    container->SkinInfo = 0;

                    var (materials, effects) = _materials[(nint)container];
                    var managed = new MeshContainer(ToString(container->Name), mesh, materials, effects, skinInfo);
                    containers.Add(managed);
                    meshContainers.Add(managed);
                }

                frames.Add(new(ToString(frame->Name), frame->TransformationMatrix, containers, ToManaged(device, frame->FirstChild, meshContainers)));
            }
            return frames;
        }

        private static string ToString(byte* value) => value == null ? "" : Marshal.PtrToStringAnsi((nint)value) ?? "";
    }
}

/// <summary>
/// A node in a <see cref="FrameHierarchy"/> (<c>D3DXFRAME</c>).
/// </summary>
/// <param name="Name">The name of the frame, referenced by bones and animations.</param>
/// <param name="TransformationMatrix">The transform relative to the parent frame in the bind pose.</param>
/// <param name="MeshContainers">The meshes attached to the frame.</param>
/// <param name="Children">The child frames.</param>
internal sealed record Frame(string Name, Matrix4x4 TransformationMatrix, IReadOnlyList<MeshContainer> MeshContainers, IReadOnlyList<Frame> Children);

/// <summary>
/// A mesh attached to a <see cref="Frame"/> (<c>D3DXMESHCONTAINER</c>).
/// </summary>
/// <param name="Name">The name of the mesh.</param>
/// <param name="Mesh">The mesh in its bind pose.</param>
/// <param name="Materials">The materials, one per subset.</param>
/// <param name="Effects">The effect instances, one per subset.</param>
/// <param name="SkinInfo">The skinning information; <c>null</c> for rigid meshes.</param>
internal sealed record MeshContainer(string Name, Mesh Mesh, ExtendedMaterial[] Materials, EffectInstance[] Effects, SkinInfo? SkinInfo) : IDisposable
{
    /// <summary>
    /// Disposes <see cref="Mesh"/> and <see cref="SkinInfo"/>.
    /// </summary>
    public void Dispose()
    {
        Mesh.Dispose();
        SkinInfo?.Dispose();
    }
}
