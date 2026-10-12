/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using OmegaEngine.Foundation.Light;
using Vortice.Direct3D9;

namespace OmegaEngine.Graphics;

/// <summary>
/// Loads textures from image files (PNG, JPG, BMP, GIF, TIFF and DDS).
/// </summary>
public static class TextureLoader
{
    /// <summary>
    /// Loads a texture from an image file into <see cref="Pool.Default"/>.
    /// </summary>
    /// <param name="device">The device to create the texture on.</param>
    /// <param name="stream">The image file.</param>
    /// <param name="mipmaps">Generate a complete chain of mip-maps if the file does not contain one.</param>
    /// <param name="srgb">Does the file hold sRGB-encoded color data? Generated mip-maps are then filtered in linear space.</param>
    /// <exception cref="InvalidDataException"><paramref name="stream"/> does not contain a supported image.</exception>
    public static IDirect3DTexture9 FromStream(IDirect3DDevice9 device, Stream stream, bool mipmaps = true, bool srgb = true)
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

        var image = IsDds(data) ? DdsImage.Load(data) : LoadBitmap(data);
        image = image.WithBlockAlignedSize();
        if (mipmaps) image = image.WithMipmaps(srgb);
        return image.Upload(device);
    }

    private static bool IsDds(byte[] data)
        => data.Length >= 4 && data[0] == 'D' && data[1] == 'D' && data[2] == 'S' && data[3] == ' ';

    private static TextureImage LoadBitmap(byte[] data)
    {
        Bitmap bitmap;
        try
        {
            bitmap = new Bitmap(new MemoryStream(data));
        }
        catch (ArgumentException ex)
        {
            throw new InvalidDataException("The file does not contain a supported image.", ex);
        }

        using (bitmap)
        {
            var bitmapData = bitmap.LockBits(new(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                // 32bpp ARGB is stored as B, G, R, A in memory, just like D3DFMT_A8R8G8B8
                var pixels = new byte[bitmap.Width * bitmap.Height * 4];
                for (int y = 0; y < bitmap.Height; y++)
                    Marshal.Copy(bitmapData.Scan0 + y * bitmapData.Stride, pixels, y * bitmap.Width * 4, bitmap.Width * 4);
                return new(Format.A8R8G8B8, [new(bitmap.Width, bitmap.Height, pixels)]);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }
    }
}

/// <summary>
/// A single mip level of a <see cref="TextureImage"/>.
/// </summary>
internal sealed record TextureLevel(int Width, int Height, byte[] Data);

/// <summary>
/// Texture data in system memory, ready to be uploaded to the GPU.
/// </summary>
internal sealed record TextureImage(Format Format, IReadOnlyList<TextureLevel> Levels)
{
    private bool IsCompressed => Format is Format.Dxt1 or Format.Dxt2 or Format.Dxt3 or Format.Dxt4 or Format.Dxt5;

    /// <summary>
    /// The number of mip levels in a complete chain for the size of the top level.
    /// </summary>
    private int FullChainLength
    {
        get
        {
            int length = 1;
            for (int size = Math.Max(Levels[0].Width, Levels[0].Height); size > 1; size /= 2) length++;
            return length;
        }
    }

    /// <summary>
    /// Returns a decompressed copy if the image uses a block-compressed format but its size is not a multiple of 4, which Direct3D 9 rejects.
    /// </summary>
    public TextureImage WithBlockAlignedSize()
        => IsCompressed && (Levels[0].Width % 4 != 0 || Levels[0].Height % 4 != 0)
            ? DxtDecoder.Decompress(this)
            : this;

    /// <summary>
    /// Returns a copy with a complete mip-map chain.
    /// </summary>
    /// <param name="srgb">Filter in linear space, because the data is sRGB-encoded.</param>
    public TextureImage WithMipmaps(bool srgb)
    {
        if (Levels.Count >= FullChainLength) return this;

        var image = this;
        if (IsCompressed)
        {
            // Only a complete top level can be filtered; keep partial chains that were stored in the file
            if (Levels.Count > 1) return this;
            image = DxtDecoder.Decompress(this);
        }
        else if (Format is not (Format.A8R8G8B8 or Format.X8R8G8B8)) return this;

        var levels = new List<TextureLevel>(image.Levels);
        while (levels.Count < FullChainLength)
            levels.Add(MipFilter.Downsample(levels[^1], srgb));
        return image with {Levels = levels};
    }

    /// <summary>
    /// Copies the data to a new texture in <see cref="Pool.Default"/>.
    /// </summary>
    public unsafe IDirect3DTexture9 Upload(IDirect3DDevice9 device)
    {
        // Pool.Default resources cannot be locked on Direct3D 9Ex, so the data goes through a system memory texture
        using var staging = device.CreateTexture((uint)Levels[0].Width, (uint)Levels[0].Height, (uint)Levels.Count, Usage.None, Format, Pool.SystemMemory);
        for (int i = 0; i < Levels.Count; i++)
        {
            var level = Levels[i];
            int rowSize = RowSize(level.Width), rowCount = RowCount(level.Height);
            var locked = staging.LockRect((uint)i, LockFlags.None);
            try
            {
                fixed (byte* pData = level.Data)
                {
                    for (int row = 0; row < rowCount; row++)
                        Buffer.MemoryCopy(pData + row * rowSize, (byte*)locked.DataPointer + row * locked.Pitch, locked.Pitch, rowSize);
                }
            }
            finally
            {
                staging.UnlockRect((uint)i);
            }
        }

        var texture = device.CreateTexture((uint)Levels[0].Width, (uint)Levels[0].Height, (uint)Levels.Count, Usage.None, Format, Pool.Default);
        try
        {
            device.UpdateTexture(staging, texture);
        }
        catch
        {
            texture.Dispose();
            throw;
        }
        return texture;
    }

    /// <summary>
    /// The number of bytes in a row of pixels or (for compressed formats) a row of 4x4 blocks.
    /// </summary>
    public int RowSize(int width) => Format switch
    {
        Format.Dxt1 => Math.Max(1, (width + 3) / 4) * 8,
        Format.Dxt2 or Format.Dxt3 or Format.Dxt4 or Format.Dxt5 => Math.Max(1, (width + 3) / 4) * 16,
        _ => width * BytesPerPixel
    };

    /// <summary>
    /// The number of rows of pixels or (for compressed formats) rows of 4x4 blocks.
    /// </summary>
    public int RowCount(int height) => IsCompressed ? Math.Max(1, (height + 3) / 4) : height;

    private int BytesPerPixel => Format switch
    {
        Format.A8R8G8B8 or Format.X8R8G8B8 or Format.A8B8G8R8 or Format.X8B8G8R8 or Format.R32F or Format.G16R16F => 4,
        Format.R5G6B5 or Format.A1R5G5B5 or Format.X1R5G5B5 or Format.A4R4G4B4 or Format.A8L8 or Format.R16F => 2,
        Format.L8 or Format.A8 => 1,
        Format.A16B16G16R16F or Format.G32R32F => 8,
        Format.A32B32G32R32F => 16,
        _ => throw new InvalidDataException($"Unsupported texture format: {Format}")
    };
}

/// <summary>
/// Parses DirectDraw Surface (.dds) files.
/// </summary>
internal static class DdsImage
{
    // DDS_PIXELFORMAT flags
    private const int DdpfAlphaPixels = 0x1, DdpfAlpha = 0x2, DdpfFourCC = 0x4, DdpfRgb = 0x40, DdpfLuminance = 0x20000;

    // DDS_HEADER caps2 flags
    private const int Caps2Cubemap = 0x200, Caps2Volume = 0x200000;

    /// <summary>
    /// Reads all mip levels of a 2D texture from a DDS file.
    /// </summary>
    /// <exception cref="InvalidDataException">The file is not a valid DDS file or uses an unsupported layout.</exception>
    public static TextureImage Load(byte[] data)
    {
        if (data.Length < 128 || BitConverter.ToInt32(data, 4) != 124)
            throw new InvalidDataException("Invalid DDS header.");

        int height = BitConverter.ToInt32(data, 12);
        int width = BitConverter.ToInt32(data, 16);
        int mipCount = Math.Max(1, BitConverter.ToInt32(data, 28));
        int pixelFlags = BitConverter.ToInt32(data, 80);
        uint fourCC = BitConverter.ToUInt32(data, 84);
        int bitCount = BitConverter.ToInt32(data, 88);
        uint redMask = BitConverter.ToUInt32(data, 92), greenMask = BitConverter.ToUInt32(data, 96), blueMask = BitConverter.ToUInt32(data, 100), alphaMask = BitConverter.ToUInt32(data, 104);
        int caps2 = BitConverter.ToInt32(data, 112);

        if ((caps2 & (Caps2Cubemap | Caps2Volume)) != 0)
            throw new InvalidDataException("Cube and volume DDS textures are not supported.");

        var format = (pixelFlags & DdpfFourCC) != 0
            ? FromFourCC(fourCC)
            : FromMasks(pixelFlags, bitCount, redMask, greenMask, blueMask, alphaMask);

        var image = new TextureImage(format, []);
        var levels = new List<TextureLevel>(mipCount);
        int offset = 128;
        for (int i = 0; i < mipCount; i++)
        {
            int levelWidth = Math.Max(1, width >> i), levelHeight = Math.Max(1, height >> i);
            int size = image.RowSize(levelWidth) * image.RowCount(levelHeight);
            if (offset + size > data.Length) throw new InvalidDataException("The DDS file is truncated.");

            levels.Add(new(levelWidth, levelHeight, data.AsSpan(offset, size).ToArray()));
            offset += size;
        }
        return image with {Levels = levels};
    }

    private static Format FromFourCC(uint fourCC) => fourCC switch
    {
        0x31545844 => Format.Dxt1, // "DXT1"
        0x32545844 => Format.Dxt2, // "DXT2"
        0x33545844 => Format.Dxt3, // "DXT3"
        0x34545844 => Format.Dxt4, // "DXT4"
        0x35545844 => Format.Dxt5, // "DXT5"
        0x30315844 => throw new InvalidDataException("DDS files with a DX10 header are not supported."), // "DX10"
        // Some writers store D3DFORMAT values directly in place of a FourCC (e.g. floating-point formats)
        < 0x100 => (Format)fourCC,
        _ => throw new InvalidDataException($"Unsupported DDS FourCC: 0x{fourCC:X8}")
    };

    private static Format FromMasks(int flags, int bitCount, uint r, uint g, uint b, uint a)
    {
        bool hasAlpha = (flags & DdpfAlphaPixels) != 0 && a != 0;
        if ((flags & DdpfRgb) != 0)
        {
            return (bitCount, r, g, b) switch
            {
                (32, 0xFF0000, 0xFF00, 0xFF) => hasAlpha ? Format.A8R8G8B8 : Format.X8R8G8B8,
                (32, 0xFF, 0xFF00, 0xFF0000) => hasAlpha ? Format.A8B8G8R8 : Format.X8B8G8R8,
                (16, 0xF800, 0x7E0, 0x1F) => Format.R5G6B5,
                (16, 0x7C00, 0x3E0, 0x1F) => hasAlpha ? Format.A1R5G5B5 : Format.X1R5G5B5,
                (16, 0xF00, 0xF0, 0xF) => Format.A4R4G4B4,
                _ => throw new InvalidDataException($"Unsupported DDS pixel format: {bitCount}-bit RGB")
            };
        }
        if ((flags & DdpfLuminance) != 0) return hasAlpha ? Format.A8L8 : Format.L8;
        if ((flags & DdpfAlpha) != 0) return Format.A8;
        throw new InvalidDataException("Unsupported DDS pixel format.");
    }
}

/// <summary>
/// Generates mip levels for 32-bit (A8R8G8B8 or X8R8G8B8) images.
/// </summary>
internal static class MipFilter
{
    private static readonly float[] _srgbToLinear = BuildSrgbTable();

    private static float[] BuildSrgbTable()
    {
        var table = new float[256];
        for (int i = 0; i < table.Length; i++)
            table[i] = ColorUtils.SrgbToLinear(i / 255f);
        return table;
    }

    /// <summary>
    /// Halves the size of an image by averaging boxes of up to 2x2 pixels.
    /// </summary>
    /// <param name="source">The image to downsample.</param>
    /// <param name="srgb">Average the color channels in linear space, because they are sRGB-encoded. The alpha channel is always averaged as is.</param>
    public static TextureLevel Downsample(TextureLevel source, bool srgb)
    {
        int width = Math.Max(1, source.Width / 2), height = Math.Max(1, source.Height / 2);
        var result = new byte[width * height * 4];
        Span<float> sum = stackalloc float[4];

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            sum.Clear();
            int count = 0;
            for (int sy = 2 * y; sy < Math.Min(2 * y + 2, source.Height); sy++)
            for (int sx = 2 * x; sx < Math.Min(2 * x + 2, source.Width); sx++)
            {
                int i = (sy * source.Width + sx) * 4;
                for (int c = 0; c < 3; c++)
                    sum[c] += srgb ? _srgbToLinear[source.Data[i + c]] : source.Data[i + c] / 255f;
                sum[3] += source.Data[i + 3] / 255f;
                count++;
            }

            int o = (y * width + x) * 4;
            for (int c = 0; c < 3; c++)
                result[o + c] = ToByte(srgb ? ColorUtils.LinearToSrgb(sum[c] / count) : sum[c] / count);
            result[o + 3] = ToByte(sum[3] / count);
        }

        return new(width, height, result);
    }

    private static byte ToByte(float value) => (byte)(Math.Clamp(value, 0, 1) * 255 + 0.5f);
}

/// <summary>
/// Decompresses DXT1 (BC1), DXT2/DXT3 (BC2) and DXT4/DXT5 (BC3) images to A8R8G8B8.
/// </summary>
internal static class DxtDecoder
{
    public static TextureImage Decompress(TextureImage image)
    {
        var levels = new List<TextureLevel>(image.Levels.Count);
        foreach (var level in image.Levels)
            levels.Add(Decompress(image, level));
        return new(Format.A8R8G8B8, levels);
    }

    private static TextureLevel Decompress(TextureImage image, TextureLevel level)
    {
        int blockSize = image.Format == Format.Dxt1 ? 8 : 16;
        int blocksX = Math.Max(1, (level.Width + 3) / 4), blocksY = Math.Max(1, (level.Height + 3) / 4);
        var result = new byte[level.Width * level.Height * 4];
        Span<byte> colors = stackalloc byte[16]; // 4 BGRA colors
        Span<byte> alphas = stackalloc byte[8];

        for (int by = 0; by < blocksY; by++)
        for (int bx = 0; bx < blocksX; bx++)
        {
            int block = (by * blocksX + bx) * blockSize;
            int colorBlock = blockSize == 16 ? block + 8 : block;

            ushort c0 = BitConverter.ToUInt16(level.Data, colorBlock), c1 = BitConverter.ToUInt16(level.Data, colorBlock + 2);
            Expand565(c0, colors[..4]);
            Expand565(c1, colors[4..8]);
            if (c0 > c1 || blockSize == 16)
            {
                for (int c = 0; c < 3; c++)
                {
                    colors[8 + c] = (byte)((2 * colors[c] + colors[4 + c]) / 3);
                    colors[12 + c] = (byte)((colors[c] + 2 * colors[4 + c]) / 3);
                }
                colors[11] = colors[15] = 255;
            }
            else
            {
                for (int c = 0; c < 3; c++)
                    colors[8 + c] = (byte)((colors[c] + colors[4 + c]) / 2);
                colors[11] = 255;
                colors[12] = colors[13] = colors[14] = colors[15] = 0; // Transparent black
            }
            uint indexes = BitConverter.ToUInt32(level.Data, colorBlock + 4);

            if (image.Format is Format.Dxt4 or Format.Dxt5)
            {
                byte a0 = level.Data[block], a1 = level.Data[block + 1];
                alphas[0] = a0;
                alphas[1] = a1;
                if (a0 > a1)
                {
                    for (int i = 1; i < 7; i++) alphas[i + 1] = (byte)(((7 - i) * a0 + i * a1) / 7);
                }
                else
                {
                    for (int i = 1; i < 5; i++) alphas[i + 1] = (byte)(((5 - i) * a0 + i * a1) / 5);
                    alphas[6] = 0;
                    alphas[7] = 255;
                }
            }

            for (int py = 0; py < 4; py++)
            for (int px = 0; px < 4; px++)
            {
                int x = bx * 4 + px, y = by * 4 + py;
                if (x >= level.Width || y >= level.Height) continue;

                int pixel = py * 4 + px;
                int colorIndex = (int)((indexes >> (2 * pixel)) & 0x3);
                int o = (y * level.Width + x) * 4;
                result[o] = colors[colorIndex * 4];
                result[o + 1] = colors[colorIndex * 4 + 1];
                result[o + 2] = colors[colorIndex * 4 + 2];
                result[o + 3] = image.Format switch
                {
                    Format.Dxt2 or Format.Dxt3 => (byte)(((level.Data[block + pixel / 2] >> (4 * (pixel % 2))) & 0xF) * 17),
                    Format.Dxt4 or Format.Dxt5 => alphas[(int)((ReadUInt48(level.Data, block + 2) >> (3 * pixel)) & 0x7)],
                    _ => colors[colorIndex * 4 + 3]
                };
            }
        }

        return new(level.Width, level.Height, result);
    }

    /// <summary>
    /// Expands an R5G6B5 color to B, G, R, A bytes.
    /// </summary>
    private static void Expand565(ushort color, Span<byte> bgra)
    {
        int r = (color >> 11) & 0x1F, g = (color >> 5) & 0x3F, b = color & 0x1F;
        bgra[0] = (byte)((b << 3) | (b >> 2));
        bgra[1] = (byte)((g << 2) | (g >> 4));
        bgra[2] = (byte)((r << 3) | (r >> 2));
        bgra[3] = 255;
    }

    private static ulong ReadUInt48(byte[] data, int offset)
    {
        ulong value = 0;
        for (int i = 0; i < 6; i++) value |= (ulong)data[offset + i] << (8 * i);
        return value;
    }
}
