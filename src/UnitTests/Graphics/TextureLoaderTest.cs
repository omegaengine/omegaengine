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
using System.IO;
using AwesomeAssertions;
using OmegaEngine.Foundation.Storage;
using Vortice.Direct3D9;
using Xunit;

namespace OmegaEngine.Graphics;

public class TextureLoaderTest : EngineTestBase
{
    private static MemoryStream ToPng(Bitmap bitmap)
    {
        var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        stream.Position = 0;
        return stream;
    }

    [Fact]
    public void LoadsPngWithCompleteMipChain()
    {
        using var bitmap = new Bitmap(16, 8);
        using var stream = ToPng(bitmap);

        using var texture = TextureLoader.FromStream(Engine.Device, stream);

        texture.LevelCount.Should().Be(5);
        var desc = texture.GetLevelDesc(0);
        (desc.Width, desc.Height, desc.Format, desc.Pool).Should().Be((16u, 8u, Format.A8R8G8B8, Pool.Default));
    }

    [Fact]
    public void KeepsNonPowerOfTwoSizes()
    {
        using var bitmap = new Bitmap(30, 10);
        using var stream = ToPng(bitmap);

        using var texture = TextureLoader.FromStream(Engine.Device, stream, mipmaps: false);

        texture.LevelCount.Should().Be(1);
        var desc = texture.GetLevelDesc(0);
        (desc.Width, desc.Height).Should().Be((30u, 10u));
    }

    [Fact]
    public void LoadsDdsWithStoredMipChain()
    {
        using var stream = ContentManager.GetFileStream("Meshes", "Test/Dwarf/DwarfHead.dds");

        using var texture = TextureLoader.FromStream(Engine.Device, stream);

        texture.LevelCount.Should().Be(9);
        texture.GetLevelDesc(0).Format.Should().Be(Format.Dxt1);
    }

    [Fact]
    public void DecompressesDdsSmallerThanABlock()
    {
        using var stream = ContentManager.GetFileStream("Meshes", "Test/Dwarf/Weapons_glow.dds");

        using var texture = TextureLoader.FromStream(Engine.Device, stream);

        texture.GetLevelDesc(0).Format.Should().Be(Format.A8R8G8B8);
    }

    [Fact]
    public void RejectsInvalidData()
    {
        using var stream = new MemoryStream([1, 2, 3, 4]);

        var act = () => TextureLoader.FromStream(Engine.Device, stream);

        act.Should().Throw<InvalidDataException>();
    }
}

public class MipFilterTest
{
    [Fact]
    public void AveragesInLinearSpaceForSrgb()
    {
        // Black and white pixels next to each other (B, G, R, A)
        var source = new TextureLevel(2, 1, [0, 0, 0, 255, 255, 255, 255, 255]);

        var linear = MipFilter.Downsample(source, srgb: false);
        var srgb = MipFilter.Downsample(source, srgb: true);

        (linear.Width, linear.Height).Should().Be((1, 1));
        linear.Data[0].Should().Be(128);
        srgb.Data[0].Should().Be(188, "the physical average of black and white is 50% grey in linear space, i.e. 188 in sRGB");
        srgb.Data[3].Should().Be(255, "alpha is not gamma-encoded");
    }
}

public class DxtDecoderTest
{
    [Fact]
    public void DecodesDxt1Block()
    {
        // Color 0 = pure red, color 1 = pure blue, all pixels use index 1
        byte[] block = [0x00, 0xF8, 0x1F, 0x00, 0x55, 0x55, 0x55, 0x55];
        var image = new TextureImage(Format.Dxt1, [new(4, 4, block)]);

        var decoded = DxtDecoder.Decompress(image).Levels[0].Data;

        decoded.AsSpan(0, 4).ToArray().Should().Equal(255, 0, 0, 255); // B, G, R, A
    }
}
