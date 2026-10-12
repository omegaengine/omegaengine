/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpGen.Runtime;
using Vortice.Direct3D9;
using Vortice.Mathematics;

namespace OmegaEngine.Graphics;

/// <summary>
/// Direct3D 9 result codes that Vortice does not expose (<c>D3DERR_*</c>, <c>S_PRESENT_*</c> and <c>D3DXERR_*</c>).
/// </summary>
public static class D3DResultCode
{
    public static readonly Result DeviceLost = new(0x88760868u);
    public static readonly Result DeviceNotReset = new(0x88760869u);
    public static readonly Result NotAvailable = new(0x8876086Au);
    public static readonly Result InvalidCall = new(0x8876086Cu);
    public static readonly Result DeviceRemoved = new(0x88760870u);
    public static readonly Result DeviceHung = new(0x88760874u);
    public static readonly Result OutOfVideoMemory = new(0x8876017Cu);
    public static readonly Result PresentModeChanged = new(0x08760877u);
    public static readonly Result PresentOccluded = new(0x08760878u);

    /// <summary>D3DX: the data is invalid (<c>D3DXERR_INVALIDDATA</c>).</summary>
    public static readonly Result InvalidData = new(0x88760B59u);
}

/// <summary>
/// <see cref="TransformState"/> values that Vortice does not expose.
/// </summary>
public static class D3DTransformState
{
    /// <summary>The world transform (<c>D3DTS_WORLD</c>, i.e. <c>D3DTS_WORLDMATRIX(0)</c>).</summary>
    public const TransformState World = (TransformState)256;
}

/// <summary>
/// The state of a Direct3D 9Ex device as reported by <see cref="Direct3D9Extensions.GetDeviceState"/>.
/// </summary>
public enum DeviceState
{
    Ok,
    DeviceLost,
    DeviceHung,
    DeviceRemoved,
    OutOfVideoMemory,
    PresentModeChanged,
    PresentOccluded
}

/// <summary>
/// Fills gaps in the Vortice Direct3D 9 bindings: calls that are missing or have incorrect signatures there.
/// </summary>
public static unsafe partial class Direct3D9Extensions
{
    /// <summary>
    /// Reports the current cooperative-level status of the device for a windowed or fullscreen application (<c>IDirect3DDevice9Ex::CheckDeviceState</c>).
    /// </summary>
    /// <remarks>Unlike Vortice's binding, this returns the success codes <see cref="D3DResultCode.PresentOccluded"/> and <see cref="D3DResultCode.PresentModeChanged"/> instead of discarding them.</remarks>
    public static DeviceState GetDeviceState(this IDirect3DDevice9Ex device, nint window)
    {
        // IDirect3DDevice9Ex::CheckDeviceState
        var result = new Result(((delegate* unmanaged[Stdcall]<nint, nint, int>)VTable(device)[128])(device.NativePointer, window));
        if (result == Result.Ok) return DeviceState.Ok;
        if (result == D3DResultCode.DeviceLost) return DeviceState.DeviceLost;
        if (result == D3DResultCode.DeviceHung) return DeviceState.DeviceHung;
        if (result == D3DResultCode.DeviceRemoved) return DeviceState.DeviceRemoved;
        if (result == D3DResultCode.OutOfVideoMemory) return DeviceState.OutOfVideoMemory;
        if (result == D3DResultCode.PresentModeChanged) return DeviceState.PresentModeChanged;
        if (result == D3DResultCode.PresentOccluded) return DeviceState.PresentOccluded;
        result.CheckError();
        return DeviceState.Ok;
    }

    /// <summary>
    /// Gets the current display mode of an adapter (<c>IDirect3D9Ex::GetAdapterDisplayModeEx</c>).
    /// </summary>
    /// <remarks>Unlike Vortice's binding, this initializes <see cref="DisplayModeEx.Size"/> as required by Direct3D.</remarks>
    public static DisplayModeEx GetCurrentDisplayMode(this IDirect3D9Ex direct3D, int adapter)
    {
        var mode = new DisplayModeEx {Size = (uint)sizeof(DisplayModeEx)};
        DisplayRotation rotation;
        // IDirect3D9Ex::GetAdapterDisplayModeEx
        new Result(((delegate* unmanaged[Stdcall]<nint, uint, DisplayModeEx*, DisplayRotation*, int>)VTable(direct3D)[19])(direct3D.NativePointer, (uint)adapter, &mode, &rotation)).CheckError();
        return mode;
    }

    /// <summary>
    /// Lists the display modes of an adapter (<c>IDirect3D9Ex::EnumAdapterModesEx</c>).
    /// </summary>
    /// <remarks>Unlike Vortice's binding, this initializes <see cref="DisplayModeEx.Size"/> as required by Direct3D.</remarks>
    public static DisplayModeEx[] GetDisplayModes(this IDirect3D9Ex direct3D, int adapter, Format format, ScanlineOrdering scanlineOrdering)
    {
        var filter = new DisplayModeFilter {Size = (uint)sizeof(DisplayModeFilter), Format = format, ScanLineOrdering = scanlineOrdering};
        var modes = new DisplayModeEx[direct3D.GetAdapterModeCountEx((uint)adapter, filter)];
        for (int i = 0; i < modes.Length; i++)
        {
            var mode = new DisplayModeEx {Size = (uint)sizeof(DisplayModeEx)};
            // IDirect3D9Ex::EnumAdapterModesEx
            new Result(((delegate* unmanaged[Stdcall]<nint, uint, DisplayModeFilter*, uint, DisplayModeEx*, int>)VTable(direct3D)[18])(direct3D.NativePointer, (uint)adapter, &filter, (uint)i, &mode)).CheckError();
            modes[i] = mode;
        }
        return modes;
    }

    /// <summary>
    /// Retrieves the capabilities of the device.
    /// </summary>
    public static Capabilities GetCapabilities(this IDirect3DDevice9 device)
    {
        var caps = new Capabilities();
        device.GetDeviceCaps(ref caps);
        return caps;
    }

    /// <summary>
    /// Sets the material used by the fixed-function pipeline.
    /// </summary>
    public static void SetMaterial(this IDirect3DDevice9 device, Material9 material) => device.SetMaterial(ref material);

    /// <summary>
    /// Sets the properties of a light used by the fixed-function pipeline.
    /// </summary>
    public static void SetLight(this IDirect3DDevice9 device, int index, Light9 light) => device.SetLight(index, ref light);

    /// <summary>
    /// Presents the back buffer and throws a <see cref="SharpGenException"/> if that fails.
    /// </summary>
    public static void PresentChecked(this IDirect3DDevice9 device) => device.Present().CheckError();

    /// <summary>
    /// Gets a sampler state value (<c>IDirect3DDevice9::GetSamplerState</c>).
    /// </summary>
    public static int GetSamplerStateValue(this IDirect3DDevice9 device, int sampler, SamplerState type)
    {
        int value;
        // IDirect3DDevice9::GetSamplerState
        new Result(((delegate* unmanaged[Stdcall]<nint, uint, int, int*, int>)VTable(device)[68])(device.NativePointer, (uint)sampler, (int)type, &value)).CheckError();
        return value;
    }

    /// <summary>
    /// Sets a sampler state to an enum value, such as a <see cref="TextureFilter"/> or <see cref="TextureAddress"/>.
    /// </summary>
    public static void SetSamplerState<T>(this IDirect3DDevice9 device, int sampler, SamplerState type, T value)
        where T : unmanaged, Enum
        => device.SetSamplerState(sampler, type, Unsafe.As<T, int>(ref value));

    /// <summary>
    /// Sets a texture stage state to an enum value, such as a <see cref="TextureOperation"/>.
    /// </summary>
    public static void SetTextureStageState<T>(this IDirect3DDevice9 device, int stage, TextureStage type, T value)
        where T : unmanaged, Enum
        => device.SetTextureStageState(stage, type, Unsafe.As<T, int>(ref value));

    /// <summary>
    /// Renders primitives from vertex data in a managed array.
    /// </summary>
    public static void DrawUserPrimitives<T>(this IDirect3DDevice9 device, PrimitiveType type, int primitiveCount, T[] vertexes)
        where T : unmanaged
    {
        fixed (T* pVertexes = vertexes)
            device.DrawPrimitiveUP(type, (uint)primitiveCount, (nint)pVertexes, (uint)sizeof(T));
    }

    /// <summary>
    /// Renders indexed primitives from index and vertex data in managed arrays.
    /// </summary>
    public static void DrawIndexedUserPrimitives<TIndex, TVertex>(this IDirect3DDevice9 device, PrimitiveType type, int minVertexIndex, int vertexCount, int primitiveCount, TIndex[] indexes, Format indexFormat, TVertex[] vertexes)
        where TIndex : unmanaged
        where TVertex : unmanaged
    {
        fixed (TIndex* pIndexes = indexes)
        fixed (TVertex* pVertexes = vertexes)
            device.DrawIndexedPrimitiveUP(type, (uint)minVertexIndex, (uint)vertexCount, (uint)primitiveCount, (nint)pIndexes, indexFormat, (nint)pVertexes, (uint)sizeof(TVertex));
    }

    /// <summary>
    /// The vertex shader version supported by the device.
    /// </summary>
    public static Version GetVertexShaderVersion(this Capabilities caps) => DecodeShaderVersion(ReadCapsField(caps, "VertexShaderVersion_"));

    /// <summary>
    /// The pixel shader version supported by the device.
    /// </summary>
    public static Version GetPixelShaderVersion(this Capabilities caps) => DecodeShaderVersion(ReadCapsField(caps, "PixelShaderVersion_"));

    // Vortice does not expose the shader versions in D3DCAPS9 publicly
    private static int ReadCapsField(Capabilities caps, string fieldName)
        => *(int*)((byte*)&caps + Marshal.OffsetOf<Capabilities>(fieldName));

    // D3DVS_VERSION/D3DPS_VERSION: major version in bits 8-15, minor version in bits 0-7
    private static Version DecodeShaderVersion(int version) => new((version >> 8) & 0xFF, version & 0xFF);

    /// <summary>
    /// Calculates the size of a vertex in bytes from its fixed-function vertex format (like <c>D3DXGetFVFVertexSize</c>).
    /// </summary>
    public static int GetVertexSize(this VertexFormat format)
    {
        int fvf = (int)format;
        int size = (fvf & 0x400E) switch // D3DFVF_POSITION_MASK
        {
            0x002 => 12, // XYZ
            0x004 => 16, // XYZRHW
            0x006 => 16, // XYZB1
            0x008 => 20, // XYZB2
            0x00A => 24, // XYZB3
            0x00C => 28, // XYZB4
            0x00E => 32, // XYZB5
            0x4002 => 16, // XYZW
            _ => 0
        };
        if ((fvf & 0x010) != 0) size += 12; // NORMAL
        if ((fvf & 0x020) != 0) size += 4; // PSIZE
        if ((fvf & 0x040) != 0) size += 4; // DIFFUSE
        if ((fvf & 0x080) != 0) size += 4; // SPECULAR

        int textureCount = (fvf & 0xF00) >> 8;
        for (int i = 0; i < textureCount; i++)
        {
            size += ((fvf >> (16 + 2 * i)) & 0x3) switch // D3DFVF_TEXCOORDSIZEn
            {
                1 => 12,
                2 => 16,
                3 => 4,
                _ => 8
            };
        }
        return size;
    }

    #region Colors
    /// <summary>
    /// Converts a <see cref="Color4"/> to a <c>D3DCOLORVALUE</c>, as used by <see cref="Light9"/> and <see cref="Material9"/>.
    /// </summary>
    public static Colorvalue ToColorvalue(this Color4 color) => new() {R = color.R, G = color.G, B = color.B, A = color.A};

    /// <summary>
    /// Converts a <c>D3DCOLORVALUE</c> to a <see cref="Color4"/>.
    /// </summary>
    public static Color4 ToColor4(this Colorvalue color) => new(color.R, color.G, color.B, color.A);

    /// <summary>
    /// Clears the render target, depth buffer and/or stencil buffer.
    /// </summary>
    public static void Clear(this IDirect3DDevice9 device, ClearFlags flags, System.Drawing.Color color, float z, int stencil)
        => device.Clear(flags, new Color(color.R, color.G, color.B, color.A), z, stencil);
    #endregion

    #region PIX
    /// <summary>
    /// Marks the beginning of a section of event code for PIX (<c>D3DPERF_BeginEvent</c>).
    /// </summary>
    public static void BeginPerfEvent(string name) => D3DPERF_BeginEvent(0, name);

    /// <summary>
    /// Marks the end of a section of event code for PIX (<c>D3DPERF_EndEvent</c>).
    /// </summary>
    public static void EndPerfEvent() => D3DPERF_EndEvent();

    /// <summary>
    /// Marks an instantaneous event for PIX (<c>D3DPERF_SetMarker</c>).
    /// </summary>
    public static void SetPerfMarker(string name) => D3DPERF_SetMarker(0, name);

    [LibraryImport("d3d9.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int D3DPERF_BeginEvent(uint color, string name);

    [LibraryImport("d3d9.dll")]
    private static partial int D3DPERF_EndEvent();

    [LibraryImport("d3d9.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial void D3DPERF_SetMarker(uint color, string name);
    #endregion

    private static void** VTable(CppObject obj) => *(void***)obj.NativePointer;
}
