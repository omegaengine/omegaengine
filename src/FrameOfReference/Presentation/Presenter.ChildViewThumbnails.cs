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

using OmegaEngine;

namespace FrameOfReference.Presentation;

partial class Presenter
{
    private bool _childViewThumbnails;

    /// <summary>
    /// Shows child views as picture-in-picture thumbnails stacked in the bottom-right corner.
    /// </summary>
    public bool ChildViewThumbnails
    {
        get => _childViewThumbnails;
        set
        {
            if (value == _childViewThumbnails) return;
            _childViewThumbnails = value;
            if (value) Engine.ExtraRender += DrawChildViewThumbnails;
            else Engine.ExtraRender -= DrawChildViewThumbnails;
        }
    }

    /// <summary>
    /// Draws the render targets as thumbnails stacked in the bottom-right corner.
    /// </summary>
    private void DrawChildViewThumbnails()
    {
        var fullViewport = Engine.RenderViewport;
        int thumbWidth = fullViewport.Width / 4;
        int thumbHeight = fullViewport.Height / 4;
        const int margin = 16;

        int index = 0;
        foreach (var childView in View.ChildViews)
        {
            // Place the thumbnails in a vertical stack in the bottom-right corner
            var viewport = fullViewport;
            viewport.X = fullViewport.X + fullViewport.Width - thumbWidth - margin;
            viewport.Y = fullViewport.Y + fullViewport.Height - (thumbHeight + margin) * (index + 1);
            viewport.Width = thumbWidth;
            viewport.Height = thumbHeight;
            Engine.Device.Viewport = viewport;

            Engine.State.SetTexture(childView.GetRenderTarget());
            Engine.DrawQuadTextured();

            index++;
        }

        // Restore the full-screen viewport for any subsequent rendering
        Engine.Device.Viewport = fullViewport;
        Engine.State.SetTexture(null);
    }
}
