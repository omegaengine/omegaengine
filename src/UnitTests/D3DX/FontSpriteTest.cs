/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using System.Numerics;
using AwesomeAssertions;
using Vortice.Direct3D9;
using Xunit;

namespace OmegaEngine.D3DX;

public class FontSpriteTest : EngineTestBase
{
    private Font CreateFont()
        => new(Engine.Device, height: 16, width: 0, FontWeight.Normal, mipLevels: 1, italic: false, CharacterSet.Default, Precision.Default, FontQuality.Default, PitchAndFamily.Default, "Arial");

    [Fact]
    public void MeasuresText()
    {
        using var font = CreateFont();

        var shortText = font.MeasureString(null, "Hi", DrawTextFormat.Left);
        var longText = font.MeasureString(null, "Hello world", DrawTextFormat.Left);

        shortText.Height.Should().BePositive();
        longText.Width.Should().BeGreaterThan(shortText.Width);
    }

    [Fact]
    public void DrawsTextAndSprites()
    {
        using var font = CreateFont();
        using var sprite = new Sprite(Engine.Device);
        using var texture = Engine.Device.CreateTexture(4, 4, 1, Usage.None, Format.A8R8G8B8, Pool.Default);

        Engine.Device.BeginScene();
        sprite.Begin(SpriteFlags.AlphaBlend);
        sprite.Transform = Matrix4x4.Identity;
        sprite.Draw(texture, new Rectangle(0, 0, 4, 4), Vector3.Zero, new Vector3(10, 10, 0), Color.White.ToArgb());
        font.DrawString(sprite, "Hello", new Rectangle(0, 0, 100, 20), DrawTextFormat.Left | DrawTextFormat.Top, Color.White.ToArgb()).Should().BePositive();
        sprite.End();
        Engine.Device.EndScene();
    }

    [Fact]
    public void SurvivesDeviceReset()
    {
        using var font = CreateFont();
        using var sprite = new Sprite(Engine.Device);

        font.OnLostDevice();
        sprite.OnLostDevice();
        font.OnResetDevice();
        sprite.OnResetDevice();
    }
}
