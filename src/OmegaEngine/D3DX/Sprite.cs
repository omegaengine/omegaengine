/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using System.Numerics;
using SharpGen.Runtime;
using Vortice.Direct3D9;

namespace OmegaEngine.D3DX;

/// <summary>
/// Wraps an <c>ID3DXSprite</c>: draws batches of textured screen-space quads.
/// </summary>
public sealed unsafe class Sprite : ComObject
{
    // ID3DXSprite (d3dx9core.h)
    private const int
        VtSetTransform = 5,
        VtBegin = 8,
        VtDraw = 9,
        VtFlush = 10,
        VtEnd = 11,
        VtOnLostDevice = 12,
        VtOnResetDevice = 13;

    /// <summary>
    /// Creates a sprite (<c>D3DXCreateSprite</c>).
    /// </summary>
    /// <exception cref="SharpGenException">The sprite could not be created.</exception>
    public Sprite(IDirect3DDevice9 device) : base(Create(device))
    {}

    private static nint Create(IDirect3DDevice9 device)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        #endregion

        nint sprite;
        new Result(D3DX9Native.D3DXCreateSprite(device.NativePointer, &sprite)).CheckError();
        return sprite;
    }

    /// <summary>
    /// Sets the transform applied to all sprites drawn afterwards.
    /// </summary>
    public Matrix4x4 Transform
    {
        set => Check(((delegate* unmanaged[Stdcall]<nint, Matrix4x4*, int>)this[VtSetTransform])(NativePointer, &value));
    }

    /// <summary>
    /// Prepares the device for drawing sprites.
    /// </summary>
    public void Begin(SpriteFlags flags) => Check(((delegate* unmanaged[Stdcall]<nint, uint, int>)this[VtBegin])(NativePointer, (uint)flags));

    /// <summary>
    /// Adds a sprite to the batch.
    /// </summary>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="sourceRect">The part of the texture to draw.</param>
    /// <param name="center">The center of the sprite in texture space.</param>
    /// <param name="position">The position of the sprite.</param>
    /// <param name="color">The color and alpha channel to modulate the texture with, as an ARGB value.</param>
    public void Draw(IDirect3DTexture9 texture, Rectangle sourceRect, Vector3 center, Vector3 position, int color)
    {
        #region Sanity checks
        if (texture == null) throw new ArgumentNullException(nameof(texture));
        #endregion

        var rect = stackalloc int[] {sourceRect.Left, sourceRect.Top, sourceRect.Right, sourceRect.Bottom};
        Check(((delegate* unmanaged[Stdcall]<nint, nint, int*, Vector3*, Vector3*, int, int>)this[VtDraw])(NativePointer, texture.NativePointer, rect, &center, &position, color));
        GC.KeepAlive(texture);
    }

    /// <summary>
    /// Draws all batched sprites immediately.
    /// </summary>
    public void Flush() => Check(((delegate* unmanaged[Stdcall]<nint, int>)this[VtFlush])(NativePointer));

    /// <summary>
    /// Draws all batched sprites and restores the device state.
    /// </summary>
    public void End() => Check(((delegate* unmanaged[Stdcall]<nint, int>)this[VtEnd])(NativePointer));

    /// <summary>
    /// Releases video memory resources before the device is reset.
    /// </summary>
    public void OnLostDevice() => Check(((delegate* unmanaged[Stdcall]<nint, int>)this[VtOnLostDevice])(NativePointer));

    /// <summary>
    /// Restores video memory resources after the device was reset.
    /// </summary>
    public void OnResetDevice() => Check(((delegate* unmanaged[Stdcall]<nint, int>)this[VtOnResetDevice])(NativePointer));

    private static void Check(int hr) => new Result(hr).CheckError();
}

/// <summary>
/// Options for <see cref="Sprite.Begin"/> (<c>D3DXSPRITE_*</c>).
/// </summary>
[Flags]
public enum SpriteFlags
{
    None = 0,
    DoNotSaveState = 1 << 0,
    DoNotModifyRenderState = 1 << 1,
    ObjectSpace = 1 << 2,
    Billboard = 1 << 3,
    AlphaBlend = 1 << 4,
    SortTexture = 1 << 5,
    SortDepthFrontToBack = 1 << 6,
    SortDepthBackToFront = 1 << 7,
    DoNotAddRefTexture = 1 << 8
}
