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
using NanoByte.Common;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Properties;
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
        _simple14 = "Simple14", _simple20 = "Simple20",
        _light14 = "Light14", _light20 = "Light20", _light2A = "Light2a", _light2B = "Light2b",
        _simpleBlack = "SimpleBlack", _lightBlack = "LightBlack";

    private readonly bool _lighting;
    private readonly IDictionary<string, IEnumerable<int>> _controllers;
    #endregion

    #region Properties
    /// <summary>
    /// The minimum shader model version required to use this shader
    /// </summary>
    public static Version MinShaderModel => new(1, 4);

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
    /// <param name="controllers">A set of int arrays that control the counters</param>
    /// <exception cref="NotSupportedException">The graphics card does not support this shader.</exception>
    public TerrainShader(bool lighting, IDictionary<string, IEnumerable<int>> controllers)
    {
        _lighting = lighting;
        _controllers = controllers ?? throw new ArgumentNullException(nameof(controllers));
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
    public override void Apply(Action render, XMaterial material, Camera camera, params LightSource[] lights)
    {
        #region Sanity checks
        if (render == null) throw new ArgumentNullException(nameof(render));
        if (camera == null) throw new ArgumentNullException(nameof(camera));
        if (lights == null) throw new ArgumentNullException(nameof(lights));
        #endregion

        #region Auto-select technique
        if (lights.Length == 0 && _lighting)
            Effect.Technique = _lighting ? _lightBlack : _simpleBlack;
        else
        {
            if (Engine.Effects.DoubleSampling && _lighting)
            {
                if (Engine.Capabilities.MaxShaderModel >= new Version(2, 0, 2))
                    Effect.Technique = _light2B;
                else if (Engine.Capabilities.MaxShaderModel == new Version(2, 0, 1))
                    Effect.Technique = _light2A;
            }
            else if (Engine.Capabilities.MaxShaderModel >= new Version(2, 0))
                Effect.Technique = _lighting ? _light20 : _simple20;
            else
                Effect.Technique = _lighting ? _light14 : _simple14;
        }
        #endregion

        if (_lighting) base.Apply(render, material, camera, lights);
        else base.Apply(render, material, camera);
    }
    #endregion

    #region Engine
    /// <inheritdoc/>
    protected override void OnEngineSet()
    {
        if (MinShaderModel > Engine.Capabilities.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
        Effect = DynamicShader.FromContent(Engine, "Terrain.fxd", _lighting, _controllers);

        lock (Engine) // Dynamic shaders may be compiled and registered in parallel, but rest of OmegaEngine is single threaded
            base.OnEngineSet();
    }
    #endregion
}
