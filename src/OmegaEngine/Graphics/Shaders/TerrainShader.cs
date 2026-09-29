/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using NanoByte.Common.Values;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.LightSources;
using OmegaEngine.Graphics.Renderables;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.Shaders;

/// <summary>
/// A shader that blends multiple textures together
/// </summary>
/// <seealso cref="Terrain"/>
public class TerrainShader : LightingShader
{
    #region Variables
    private readonly EffectHandle
        _simple = "Simple", _light = "Light", _lightDetail = "LightDetail",
        _black = "Black", _depth = "Depth";

    private readonly bool _lighting;
    private readonly byte[] _effectCode;

    /// <summary>
    /// When set to <c>true</c> before calling <see cref="Apply"/>, causes the shader's technique selection to be overridden to output normalized camera-relative depth as a grayscale color instead of its normal appearance.
    /// </summary>
    /// <seealso cref="SurfaceEffect.Depth"/>
    internal bool RenderDepthOnly { get; set; }
    #endregion

    #region Properties
    private float _blendDistance = 400, _blendWidth = 700;

    /// <summary>
    /// The distance at which to show the pure near texture
    /// </summary>
    [DefaultValue(400f), Description("The distance at which to show the pure near texture")]
    public float BlendDistance { get => _blendDistance; set => value.To(ref _blendDistance, () => SetShaderParameter("BlendDistance", value)); }

    /// <summary>
    /// The distance from <see cref="BlendDistance"/> where to show the pure far texture
    /// </summary>
    [DefaultValue(700f), Description("The distance from BlendDistance where to show the pure far texture")]
    public float BlendWidth { get => _blendWidth; set => value.To(ref _blendWidth, () => SetShaderParameter("BlendWidth", value)); }
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a specialized instance of the shader
    /// </summary>
    /// <param name="lighting">Shall this shader apply lighting to the terrain?</param>
    /// <param name="textureMask">A bitmask that indicates which textures are enabled</param>
    /// <exception cref="ShaderCompileException">The expanded shader code could not be compiled.</exception>
    public TerrainShader(bool lighting, int textureMask)
    {
        _lighting = lighting;

        var textureIndexes = new List<int>();
        for (int i = 0; i < 16; i++)
        {
            if (textureMask.HasFlag(1 << i))
                textureIndexes.Add(i + 1);
        }

        using var stream = DynamicShader.FromContent(
            "Terrain.fxd",
            controllers: new() {["textures"] = textureIndexes},
            lighting);
        stream.Position = 0;
        _effectCode = new byte[stream.Length];
        stream.Read(_effectCode, 0, _effectCode.Length); // Copy to managed array to avoid memory leak if not disposed
    }
    #endregion

    //--------------------//

    #region Apply
    /// <summary>
    /// Applies the shader to the content in the render delegate.
    /// </summary>
    /// <param name="render">The render delegate (is called once for every shader pass).</param>
    /// <param name="material">The material to be used by this shader; <c>null</c> for device texture.</param>
    /// <param name="camera">The camera for transformation information.</param>
    /// <param name="lights">An array of all lights this shader should consider. Mustn't be <c>null</c>!</param>
    public override void Apply([InstantHandle] Action render, XMaterial material, Camera camera, params IReadOnlyList<LightSource> lights)
    {
        #region Sanity checks
        if (render == null) throw new ArgumentNullException(nameof(render));
        if (camera == null) throw new ArgumentNullException(nameof(camera));
        if (lights == null) throw new ArgumentNullException(nameof(lights));
        #endregion

        #region Auto-select technique
        if (RenderDepthOnly)
            Effect.Technique = _depth;
        else if (lights.Count == 0 && _lighting)
            Effect.Technique = _black;
        else if (!_lighting)
            Effect.Technique = _simple;
        else
            Effect.Technique = Engine.Effects.DetailMapping ? _lightDetail : _light;
        #endregion

        if (_lighting) base.Apply(render, material, camera, lights);
        else base.Apply(render, material, camera);
    }
    #endregion

    #region Engine
    /// <inheritdoc/>
    protected override void OnEngineSet()
    {
        Effect = Effect.FromMemory(Engine.Device, _effectCode, ShaderFlags.None);

        base.OnEngineSet();
    }
    #endregion
}
