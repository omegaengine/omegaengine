/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Linq;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.LightSources;
using OmegaEngine.Graphics.VertexDecl;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.Renderables;

/// <summary>
/// A cubic skybox existing of 4 or 6 non-animated planes.
/// </summary>
public sealed class SimpleSkybox : Skybox
{
    private VertexBuffer? _vb;
    private IndexBuffer? _ib;

    /// <summary>
    /// Creates a new skybox using texture-files
    /// </summary>
    /// <param name="textures">An array of the 6 textures to be uses (right, left, top, bottom, front, back)</param>
    /// <exception cref="ArgumentException">There are not exactly 6 textures.</exception>
    private SimpleSkybox(XTexture[] textures) : base(textures.Cast<ITextureProvider>().ToArray())
    {}

    /// <summary>
    /// Creates a new skybox using a cached <see cref="XTexture"/>s (loading new ones if they aren't cached).
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> providing the cache and rendering capabilities.</param>
    /// <param name="rt">The ID of the "right" texture.</param>
    /// <param name="lf">The ID of the "left" texture</param>
    /// <param name="up">The ID of the "up" texture.</param>
    /// <param name="dn">The ID of the "down" texture.</param>
    /// <param name="ft">The ID of the "front" texture.</param>
    /// <param name="bk">The ID of the "back" texture.</param>
    /// <returns>The skybox that was created.</returns>
    /// <exception cref="FileNotFoundException">One of the specified texture files could not be found.</exception>
    /// <exception cref="IOException">There was an error reading one of the texture files.</exception>
    /// <exception cref="UnauthorizedAccessException">Read access to one of the texture files is not permitted.</exception>
    /// <exception cref="InvalidDataException">One of the texture files does not contain a valid texture.</exception>
    public static SimpleSkybox FromAssets(Engine engine, string rt, string lf, string? up, string? dn, string ft, string bk)
        => new([
            XTexture.Get(engine, rt), XTexture.Get(engine, lf), XTexture.Get(engine, up),
            XTexture.Get(engine, dn), XTexture.Get(engine, ft), XTexture.Get(engine, bk)
        ]);

    /// <inheritdoc/>
    internal override void Render(Camera camera, GetEffectiveLighting? getEffectiveLighting = null)
    {
        base.Render(camera, getEffectiveLighting);

        Engine.State.SetVertexBuffer(_vb);
        Engine.Device.Indices = _ib;

        // Turn off texture wrapping at edges to prevent seams
        Engine.Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        Engine.Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

        // Draw all sides of the cube
        for (int i = 0; i < 6; ++i)
        {
            // Skip any "empty" sides without errors
            if (Textures[i] == null) continue;

            Engine.State.SetTexture(Textures[i]);
            Engine.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, i * 4, 4, i * 6, 2);
        }

        // Restore normal texture handling
        Engine.Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Wrap);
        Engine.Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Wrap);
    }

    /// <inheritdoc/>
    protected override void OnEngineSet()
    {
        base.OnEngineSet();

        #region Vertexes
        _vb = BufferHelper.CreateVertexBuffer(Engine.Device, new PositionTextured[]
        {
            // Right
            new(1.0f, -1.0f, 1.0f, 0.0f, 1.0f),
            new(1.0f, 1.0f, 1.0f, 0.0f, 0.0f),
            new(1.0f, 1.0f, -1.0f, 1.0f, 0.0f),
            new(1.0f, -1.0f, -1.0f, 1.0f, 1.0f),
            // Left
            new(-1.0f, -1.0f, -1.0f, 0.0f, 1.0f),
            new(-1.0f, 1.0f, -1.0f, 0.0f, 0.0f),
            new(-1.0f, 1.0f, 1.0f, 1.0f, 0.0f),
            new(-1.0f, -1.0f, 1.0f, 1.0f, 1.0f),
            // Top
            new(-1.0f, 1.0f, 1.0f, 0.0f, 1.0f),
            new(-1.0f, 1.0f, -1.0f, 0.0f, 0.0f),
            new(1.0f, 1.0f, -1.0f, 1.0f, 0.0f),
            new(1.0f, 1.0f, 1.0f, 1.0f, 1.0f),
            // Bottom
            new(-1.0f, -1.0f, -1.0f, 0.0f, 1.0f),
            new(-1.0f, -1.0f, 1.0f, 0.0f, 0.0f),
            new(1.0f, -1.0f, 1.0f, 1.0f, 0.0f),
            new(1.0f, -1.0f, -1.0f, 1.0f, 1.0f),
            // Front
            new(-1.0f, -1.0f, 1.0f, 0.0f, 1.0f),
            new(-1.0f, 1.0f, 1.0f, 0.0f, 0.0f),
            new(1.0f, 1.0f, 1.0f, 1.0f, 0.0f),
            new(1.0f, -1.0f, 1.0f, 1.0f, 1.0f),
            // Back
            new(1.0f, -1.0f, -1.0f, 0.0f, 1.0f),
            new(1.0f, 1.0f, -1.0f, 0.0f, 0.0f),
            new(-1.0f, 1.0f, -1.0f, 1.0f, 0.0f),
            new(-1.0f, -1.0f, -1.0f, 1.0f, 1.0f)
        }, PositionTextured.Format);
        #endregion

        #region Indexes
        _ib = BufferHelper.CreateIndexBuffer(Engine.Device, new short[]
        {
            0, 2, 3, 0, 1, 2, // Right
            4, 5, 6, 4, 6, 7, // Left
            8, 9, 10, 8, 10, 11, // Top
            12, 13, 14, 12, 14, 15, // Bottom
            16, 18, 19, 16, 17, 18, // Front
            20, 21, 22, 20, 22, 23 // Back
        });
        #endregion
    }

    /// <inheritdoc/>
    protected override void OnDispose()
    {
        try
        {
            _vb?.Dispose();
            _ib?.Dispose();
        }
        finally
        {
            base.OnDispose();
        }
    }
}
