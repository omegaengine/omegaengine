/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.LightSources;

namespace OmegaEngine.Graphics.Shaders;

/// <summary>
/// A general-purpose surface shader with optional support for normal and specular maps
/// </summary>
/// <seealso cref="Engine.DefaultShader"/>
public class GeneralShader : LightingShader
{
    #region Variables
    private readonly EffectHandle
        _coloredPerVertex = "ColoredPerVertex", _colored = "Colored", _coloredEmissiveOnly = "ColoredEmissiveOnly",
        _texturedPerVertex = "TexturedPerVertex", _textured = "Textured", _texturedNormalMap = "TexturedNormalMap",
        _texturedSpecularMap = "TexturedSpecularMap", _texturedNormalSpecularMap = "TexturedNormalSpecularMap",
        _texturedEmissiveMap = "TexturedEmissiveMap", _texturedNormalEmissiveMap = "TexturedNormalEmissiveMap",
        _texturedNormalSpecularEmissiveMap = "TexturedNormalSpecularEmissiveMap", _texturedEmissiveOnly = "TexturedEmissiveOnly", _texturedEmissiveMapOnly = "TexturedEmissiveMapOnly";
    #endregion

    //--------------------//

    #region Apply
    /// <inheritdoc/>
    public override void Apply([InstantHandle] Action render, XMaterial material, Camera camera, params IReadOnlyList<LightSource> lights)
    {
        #region Sanity checks
        if (render == null) throw new ArgumentNullException(nameof(render));
        if (camera == null) throw new ArgumentNullException(nameof(camera));
        if (lights == null) throw new ArgumentNullException(nameof(lights));
        #endregion

        #region Auto-select technique
        if (lights.Count == 0)
        { // Only emissive lighting
            if (Engine.Effects.PerPixelLighting && material.EmissiveMap != null)
                Effect.Technique = _texturedEmissiveMapOnly;
            else
                Effect.Technique = (material.DiffuseMap == null) ? _coloredEmissiveOnly : _texturedEmissiveOnly;
        }
        else
        {
            if (Engine.Effects.PerPixelLighting)
            { // Normal per-pixel lighting
                if (material.DiffuseMap == null)
                    Effect.Technique = _colored;
                else
                {
                    #region Flags
                    bool normal = material.NormalMap != null && Engine.Effects.NormalMapping;
                    bool specular = material.SpecularMap != null;
                    bool emissive = material.EmissiveMap != null;
                    #endregion

                    if (normal && specular && emissive) Effect.Technique = _texturedNormalSpecularEmissiveMap;
                    else if (normal && emissive) Effect.Technique = _texturedNormalEmissiveMap;
                    else if (emissive) Effect.Technique = _texturedEmissiveMap;
                    else if (normal && specular) Effect.Technique = _texturedNormalSpecularMap;
                    else if (specular) Effect.Technique = _texturedSpecularMap;
                    else if (normal) Effect.Technique = _texturedNormalMap;
                    else Effect.Technique = _textured;
                }
            }
            else
            { // Normal per-vertex lighting
                Effect.Technique = (material.DiffuseMap == null) ? _coloredPerVertex : _texturedPerVertex;
            }
        }
        #endregion

        base.Apply(render, material, camera, lights);
    }
    #endregion

    #region Engine
    protected override void OnEngineSet()
    {
        LoadShaderFile("General.fxo");

        base.OnEngineSet();
    }
    #endregion
}
