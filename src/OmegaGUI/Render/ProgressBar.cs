/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaGUI.Render;

/// <summary>
/// A bar that displays a value within a range, e.g. a health bar
/// </summary>
/// <remarks>This control is purely informative; it does not handle any user input.</remarks>
public class ProgressBar : Control
{
    public const int BackgroundLayer = 0;
    public const int FillLayer = 1;

    /// <summary>The fraction of the texture region used for <see cref="BackgroundLayer"/> that its border takes up on each side</summary>
    private const float BorderX = 8f / 240f, BorderY = 9f / 42f;

    /// <summary>The factor the fill color is darkened by when the control is disabled</summary>
    private const float DisabledDim = 0.5f;

    #region Instance Data
    /// <summary>Fill the bar from right to left instead of from left to right</summary>
    public bool Reversed;

    /// <summary>The color of the bar when <see cref="Value"/> is at <see cref="Max"/></summary>
    public Color4 FillColor = Dialog.WhiteColorValue;

    /// <summary>The color of the bar when <see cref="Value"/> is at <see cref="Min"/>; transparent to always use <see cref="FillColor"/></summary>
    public Color4 FillColorLow;

    /// <summary>The lower end of the range displayed by the bar</summary>
    public float Min { get; set; }

    /// <summary>The upper end of the range displayed by the bar</summary>
    public float Max { get; set; } = 1;

    /// <summary>The current value of the bar; values outside of <see cref="Min"/> and <see cref="Max"/> are clamped for display</summary>
    public float Value { get; set; }

    /// <summary>How much of the bar is filled, between 0 and 1</summary>
    public float Fraction
    {
        get
        {
            if (Max <= Min) return 0;
            return Math.Max(0, Math.Min(1, (Value - Min) / (Max - Min)));
        }
    }

    /// <summary>The area inside the frame's border that the fill is drawn in</summary>
    /// <remarks>Rounded up, so that the border stays visible even on small controls.</remarks>
    private Rectangle FillArea
    {
        get
        {
            var area = boundingBox;
            area.Inflate(
                -Math.Max(1, (int)Math.Ceiling(area.Width * BorderX)),
                -Math.Max(1, (int)Math.Ceiling(area.Height * BorderY)));
            return area;
        }
    }
    #endregion

    /// <summary>Create new progress bar instance</summary>
    public ProgressBar(Dialog parent) : base(parent)
    {
        ctrlType = ControlType.ProgressBar;
    }

    /// <summary>Render the progress bar</summary>
    public override void Render(Device device, float elapsedTime)
    {
        if (!IsVisible) return;

        var state = IsEnabled ? ControlState.Normal : ControlState.Disabled;

        Element background = elementList[BackgroundLayer];
        background.TextureColor.Blend(state, elapsedTime);
        parentDialog.DrawSprite(background, boundingBox);

        float fraction = Fraction;
        if (fraction <= 0) return;

        Element fill = elementList[FillLayer];
        Rectangle sourceRect = fill.textureRect, destRect = FillArea;

        int destWidth = (int)(destRect.Width * fraction), sourceWidth = (int)(sourceRect.Width * fraction);
        if (destWidth < 1 || sourceWidth < 1) return;

        // Crop instead of squeezing the texture by scaling the source region along with the destination
        if (Reversed)
        {
            destRect.X = destRect.Right - destWidth;
            fill.textureRect = new(sourceRect.Right - sourceWidth, sourceRect.Y, sourceWidth, sourceRect.Height);
        }
        else fill.textureRect = new(sourceRect.X, sourceRect.Y, sourceWidth, sourceRect.Height);
        destRect.Width = destWidth;

        // Set the color directly instead of blending, since the fill color can change every frame
        fill.TextureColor.Current = GetFillColor(fraction, state);

        parentDialog.DrawSprite(fill, destRect);
        fill.textureRect = sourceRect;
    }

    /// <summary>Determines the color to modulate the fill texture with</summary>
    private Color4 GetFillColor(float fraction, ControlState state)
    {
        var color = (FillColorLow.Alpha == 0) ? FillColor : Color4.Lerp(FillColorLow, FillColor, fraction);

        // Darken rather than fade out, since lowering the alpha of a solid bar is barely noticeable
        if (state == ControlState.Disabled)
            color = new(color.Alpha, color.Red * DisabledDim, color.Green * DisabledDim, color.Blue * DisabledDim);

        return color;
    }
}
