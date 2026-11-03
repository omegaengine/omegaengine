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
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.VertexDecl;
using Vortice.Direct3D9;

namespace OmegaGUI.Render;

/// <summary>
/// Draws batches of textured screen-space quads.
/// </summary>
public sealed class SpriteBatch(IDirect3DDevice9 device)
{
    private readonly IDirect3DDevice9 _device = device ?? throw new ArgumentNullException(nameof(device));
    private readonly List<TransformedColoredTextured> _vertexes = [];
    private IDirect3DTexture9? _texture;

    /// <summary>
    /// Prepares the device for drawing sprites. Does not save the previous device state.
    /// </summary>
    public void Begin()
    {
        _vertexes.Clear();
        _texture = null;

        _device.VertexShader = null;
        _device.PixelShader = null;
        _device.SetRenderState(RenderState.CullMode, (int)Cull.None);
        _device.SetRenderState(RenderState.FillMode, (int)FillMode.Solid);
        _device.SetRenderState(RenderState.Lighting, false);
        _device.SetRenderState(RenderState.FogEnable, false);

        // Modulate the texture with the vertex color
        _device.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.Modulate);
        _device.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
        _device.SetTextureStageState(0, TextureStage.ColorArg2, TextureArgument.Diffuse);
        _device.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
        _device.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);
        _device.SetTextureStageState(0, TextureStage.AlphaArg2, TextureArgument.Diffuse);
        _device.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.Disable);
        _device.SetTextureStageState(1, TextureStage.AlphaOperation, TextureOperation.Disable);

        _device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        _device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);
        _device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);
        _device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);
        _device.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.Linear);
    }

    /// <summary>
    /// Adds a sprite to the batch.
    /// </summary>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="sourceRect">The part of the texture to draw, in pixels.</param>
    /// <param name="destinationRect">Where to draw the sprite on the screen, in pixels.</param>
    /// <param name="color">The color and alpha channel to modulate the texture with, as an ARGB value.</param>
    public void Draw(IDirect3DTexture9 texture, Rectangle sourceRect, RectangleF destinationRect, int color)
    {
        #region Sanity checks
        if (texture == null) throw new ArgumentNullException(nameof(texture));
        #endregion

        var size = texture.GetLevelDesc(0);
        float u0 = sourceRect.Left / (float)size.Width, u1 = sourceRect.Right / (float)size.Width;
        float v0 = sourceRect.Top / (float)size.Height, v1 = sourceRect.Bottom / (float)size.Height;
        DrawQuad(texture, destinationRect, u0, v0, u1, v1, color);
    }

    /// <summary>
    /// Adds an axis-aligned quad to the batch.
    /// </summary>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="destinationRect">Where to draw the quad on the screen, in pixels.</param>
    /// <param name="u0">The left texture coordinate.</param>
    /// <param name="v0">The top texture coordinate.</param>
    /// <param name="u1">The right texture coordinate.</param>
    /// <param name="v1">The bottom texture coordinate.</param>
    /// <param name="color">The color and alpha channel to modulate the texture with, as an ARGB value.</param>
    internal void DrawQuad(IDirect3DTexture9 texture, RectangleF destinationRect, float u0, float v0, float u1, float v1, int color)
    {
        if (!ReferenceEquals(texture, _texture))
        {
            Flush();
            _texture = texture;
        }

        // Shift by half a pixel to map texels directly to pixels
        float left = destinationRect.Left - 0.5f, right = destinationRect.Right - 0.5f;
        float top = destinationRect.Top - 0.5f, bottom = destinationRect.Bottom - 0.5f;

        TransformedColoredTextured Vertex(float x, float y, float u, float v)
            => new(new(x, y, 0.5f), 1, Color.Empty, u, v) {Color = color};

        var topLeft = Vertex(left, top, u0, v0);
        var topRight = Vertex(right, top, u1, v0);
        var bottomLeft = Vertex(left, bottom, u0, v1);
        var bottomRight = Vertex(right, bottom, u1, v1);
        _vertexes.AddRange([topLeft, topRight, bottomLeft, topRight, bottomRight, bottomLeft]);
    }

    /// <summary>
    /// Draws all batched sprites immediately.
    /// </summary>
    public void Flush()
    {
        if (_vertexes.Count == 0 || _texture == null) return;

        _device.SetTexture(0, _texture);
        _device.VertexFormat = TransformedColoredTextured.Format;
        _device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertexes.Count / 3, _vertexes.ToArray());
        _vertexes.Clear();
    }

    /// <summary>
    /// Draws all batched sprites.
    /// </summary>
    public void End()
    {
        Flush();
        _texture = null;
    }
}
