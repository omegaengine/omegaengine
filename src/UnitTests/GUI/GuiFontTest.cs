/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using AwesomeAssertions;
using OmegaEngine;
using Vortice.Direct3D9;
using Xunit;

namespace OmegaGUI.Render;

public class GuiFontTest : EngineTestBase
{
    private static GuiFont GetFont(DialogManager manager, FontWeight weight = FontWeight.Normal)
        => manager.GetFontNode(manager.AddFont("Arial", 16, weight)).Font!;

    [Fact]
    public void LoadsSystemFonts()
    {
        using var manager = new DialogManager(Engine);

        var font = GetFont(manager);
        font.Should().NotBeNull();
        font.LineHeight.Should().BeInRange(14, 24);
    }

    [Fact]
    public void MeasuresText()
    {
        using var manager = new DialogManager(Engine);
        var font = GetFont(manager);

        var shortText = font.MeasureString("Hi", DrawTextFormat.Left);
        var longText = font.MeasureString("Hello world", DrawTextFormat.Left);

        shortText.Height.Should().Be(font.LineHeight);
        longText.Width.Should().BeGreaterThan(shortText.Width);
        font.MeasureString("Hello\nworld", DrawTextFormat.Left).Height.Should().Be(2 * font.LineHeight);
        font.MeasureString("Hello\nworld", DrawTextFormat.SingleLine).Height.Should().Be(font.LineHeight);
    }

    [Fact]
    public void BoldTextIsWider()
    {
        using var manager = new DialogManager(Engine);

        GetFont(manager, FontWeight.Bold).MeasureString("Hello world", DrawTextFormat.Left).Width
            .Should().BeGreaterThan(GetFont(manager).MeasureString("Hello world", DrawTextFormat.Left).Width);
    }

    [Fact]
    public void ExpandsTabsToTabStops()
    {
        using var manager = new DialogManager(Engine);
        var font = GetFont(manager);

        font.MeasureString("a\tb", DrawTextFormat.ExpandTabs).Width
            .Should().Be(font.MeasureString("ab\tb", DrawTextFormat.ExpandTabs).Width, "both reach the same tab stop");
    }

    [Fact]
    public void DrawsTextAndSprites()
    {
        using var manager = new DialogManager(Engine);
        var font = GetFont(manager);
        using var texture = Engine.Device.CreateTexture(4, 4, 1, Usage.None, Format.A8R8G8B8, Pool.Default);

        Engine.Device.BeginScene();
        var sprite = manager.Sprite!;
        sprite.Begin();
        sprite.Draw(texture, new Rectangle(0, 0, 4, 4), new RectangleF(10, 10, 8, 8), Color.White.ToArgb());
        font.DrawString(sprite, "Hello", new Rectangle(0, 0, 100, 20), DrawTextFormat.Left | DrawTextFormat.Top, Color.White.ToArgb())
            .Should().Be(font.LineHeight);
        font.DrawString(sprite, "Hello world, this wraps", new Rectangle(0, 0, 60, 100), DrawTextFormat.WordBreak, Color.White.ToArgb())
            .Should().BeGreaterThan(font.LineHeight, "the text does not fit into one line");
        sprite.End();
        Engine.Device.EndScene();
    }
}
