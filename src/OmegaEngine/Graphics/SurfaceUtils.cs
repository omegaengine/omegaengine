/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using Vortice.Direct3D9;

namespace OmegaEngine.Graphics;

/// <summary>
/// Helper methods for <see cref="IDirect3DSurface9"/>s.
/// </summary>
public static class SurfaceUtils
{
    /// <summary>
    /// Copies the content of a render target surface (e.g., a back buffer or a <see cref="RenderTarget"/>) to a bitmap.
    /// </summary>
    /// <param name="surface">The render target surface to read. Must use <see cref="Format.X8R8G8B8"/> or <see cref="Format.A8R8G8B8"/>.</param>
    /// <param name="device">The device the <paramref name="surface"/> belongs to.</param>
    /// <exception cref="NotSupportedException">The <paramref name="surface"/> uses an unsupported format.</exception>
    public static unsafe Bitmap ToBitmap(this IDirect3DSurface9 surface, IDirect3DDevice9 device)
    {
        #region Sanity checks
        if (surface == null) throw new ArgumentNullException(nameof(surface));
        if (device == null) throw new ArgumentNullException(nameof(device));
        #endregion

        var desc = surface.Description;
        var pixelFormat = desc.Format switch
        {
            Format.X8R8G8B8 => PixelFormat.Format32bppRgb,
            Format.A8R8G8B8 => PixelFormat.Format32bppArgb,
            _ => throw new NotSupportedException($"Cannot read surfaces with format {desc.Format}.")
        };

        using var staging = device.CreateOffscreenPlainSurface(desc.Width, desc.Height, desc.Format, Pool.SystemMemory);
        if (desc.MultiSampleType == MultisampleType.None)
            device.GetRenderTargetData(surface, staging);
        else
        { // Multisampled surfaces have to be resolved first
            using var resolved = device.CreateRenderTarget(desc.Width, desc.Height, desc.Format, MultisampleType.None, multisampleQuality: 0, lockable: false);
            var fullRect = new Rect(0, 0, (int)desc.Width, (int)desc.Height);
            device.StretchRect(surface, fullRect, resolved, fullRect, TextureFilter.None);
            device.GetRenderTargetData(resolved, staging);
        }

        var bitmap = new Bitmap((int)desc.Width, (int)desc.Height, pixelFormat);
        var bitmapData = bitmap.LockBits(new(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, pixelFormat);
        var locked = staging.LockRect(LockFlags.ReadOnly);
        try
        {
            // Both formats store B, G, R, A/X in memory, just like the corresponding GDI+ pixel formats
            for (int y = 0; y < bitmap.Height; y++)
                Buffer.MemoryCopy((byte*)locked.DataPointer + y * locked.Pitch, (byte*)bitmapData.Scan0 + y * bitmapData.Stride, bitmapData.Stride, bitmap.Width * 4);
        }
        finally
        {
            staging.UnlockRect();
            bitmap.UnlockBits(bitmapData);
        }
        return bitmap;
    }

    /// <summary>
    /// Saves the content of a render target surface (e.g., a back buffer or a <see cref="RenderTarget"/>) to an image file.
    /// </summary>
    /// <param name="surface">The render target surface to read. Must use <see cref="Format.X8R8G8B8"/> or <see cref="Format.A8R8G8B8"/>.</param>
    /// <param name="device">The device the <paramref name="surface"/> belongs to.</param>
    /// <param name="path">The path of the file to write.</param>
    /// <param name="format">The image file format to use.</param>
    /// <param name="area">The part of the surface to save; <c>null</c> for all of it.</param>
    /// <exception cref="NotSupportedException">The <paramref name="surface"/> uses an unsupported format.</exception>
    public static void SaveToFile(this IDirect3DSurface9 surface, IDirect3DDevice9 device, string path, ImageFormat format, Rectangle? area = null)
    {
        #region Sanity checks
        if (path == null) throw new ArgumentNullException(nameof(path));
        if (format == null) throw new ArgumentNullException(nameof(format));
        #endregion

        using var bitmap = surface.ToBitmap(device);
        if (area is {} rect && rect != new Rectangle(Point.Empty, bitmap.Size))
        {
            using var cropped = bitmap.Clone(rect, bitmap.PixelFormat);
            cropped.Save(path, format);
        }
        else bitmap.Save(path, format);
    }
}
