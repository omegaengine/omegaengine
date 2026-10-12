/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;

namespace OmegaEngine.D3DX;

/// <summary>
/// Reads data from <c>ID3DXBuffer</c>s returned by D3DX functions.
/// </summary>
internal static unsafe class D3DXBuffer
{
    // ID3DXBuffer vtable (d3dx9core.h)
    private const int Release = 2, GetBufferPointer = 3, GetBufferSize = 4;

    private static void** VTable(nint buffer) => *(void***)buffer;

    /// <summary>
    /// Returns the location and size of the buffer's data.
    /// </summary>
    public static (nint pointer, int size) GetData(nint buffer)
    {
        var vtable = VTable(buffer);
        nint pointer = ((delegate* unmanaged[Stdcall]<nint, nint>)vtable[GetBufferPointer])(buffer);
        int size = (int)((delegate* unmanaged[Stdcall]<nint, uint>)vtable[GetBufferSize])(buffer);
        return (pointer, size);
    }

    /// <summary>
    /// Copies the buffer's data to a managed array and releases the buffer.
    /// </summary>
    public static byte[] ReadAndRelease(nint buffer)
    {
        try
        {
            var (pointer, size) = GetData(buffer);
            return new Span<byte>((void*)pointer, size).ToArray();
        }
        finally
        {
            ReleaseBuffer(buffer);
        }
    }

    /// <summary>
    /// Reads the buffer's data as an ANSI string and releases the buffer.
    /// </summary>
    public static string ReadStringAndRelease(nint buffer)
    {
        try
        {
            var (pointer, size) = GetData(buffer);
            return (Marshal.PtrToStringAnsi(pointer, size) ?? "").TrimEnd('\0');
        }
        finally
        {
            ReleaseBuffer(buffer);
        }
    }

    /// <summary>
    /// Releases a buffer, if not <c>0</c>.
    /// </summary>
    public static void ReleaseBuffer(nint buffer)
    {
        if (buffer != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)VTable(buffer)[Release])(buffer);
    }
}
