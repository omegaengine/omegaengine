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
using JetBrains.Annotations;
using OmegaEngine.Graphics.LightSources;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.Shaders;

/// <summary>
/// A <see cref="SurfaceShader"/> that properly reacts to <see cref="LightSource"/>s
/// </summary>
public abstract class LightingShader : SurfaceShader
{
    #region Enums
    private enum ShaderPasses
    {
        AmbientLight = 0,
        TwoDirLights = 1,
        TwoDirLightsAdd = 2,
        OneDirLight = 3,
        OneDirLightAdd = 4,
        OnePointLight = 5,
        OnePointLightAdd = 6
    };
    #endregion

    //--------------------//

    #region Passes
    /// <inheritdoc/>
    protected override void RunPasses([InstantHandle] Action render, XMaterial material, params IReadOnlyList<LightSource> lights)
    {
        #region Sanity checks
        if (render == null) throw new ArgumentNullException(nameof(render));
        if (lights == null) throw new ArgumentNullException(nameof(lights));
        #endregion

        // Note: Manual control of shader logic (no SAS scripting)

        Effect.Begin(FX.None);

        // If this shader supports light-less shading, it will have to do that with its first pass
        if (lights.Count == 0)
        {
            #region Render pass
            using (new ProfilerEvent("Pass 0"))
            {
                Effect.BeginPass(0);
                render();
                Effect.EndPass();
            }
            #endregion
        }
        else
        {
            for (int i = 0; i < lights.Count; i++)
            {
                var pass = lights[i] switch
                {
                    DirectionalLight dirLight when i == 0 && dirLight.Diffuse == Color.Black && dirLight.Specular == Color.Black
                        => SetupAmbientLight(dirLight, material),
                    DirectionalLight dirLight => SetupDirectionalLights(dirLight, lights, ref i, material),
                    PointLight pointLight => SetupPointLight(pointLight, i, material)
                };

                #region Render pass
                using (new ProfilerEvent(() => "Pass " + pass))
                {
                    Effect.BeginPass((int)pass);
                    render();
                    Effect.EndPass();
                }
                #endregion
            }
        }

        Effect.End();
    }

    private ShaderPasses SetupAmbientLight(DirectionalLight light, XMaterial material)
    {
        using (new ProfilerEvent("Setup light 0 as ambient"))
            SetupLight(light, 0, material);
        return ShaderPasses.AmbientLight;
    }

    /// <summary>
    /// Sets up a directional light, and the next light too if it is also directional.
    /// </summary>
    /// <param name="light">The light at <paramref name="index"/>.</param>
    /// <param name="lights">All lights to be rendered.</param>
    /// <param name="index">The index of <paramref name="light"/> in <paramref name="lights"/>; incremented if the next light was handled too.</param>
    /// <param name="material">The material to apply.</param>
    private ShaderPasses SetupDirectionalLights(DirectionalLight light, IReadOnlyList<LightSource> lights, ref int index, XMaterial material)
    {
        int i = index;
        using (new ProfilerEvent(() => $"Setup light {i} as directional"))
            SetupLight(light, 0, material);

        // Try to handle the next light at the same time
        if (i + 1 < lights.Count && lights[i + 1] is DirectionalLight nextLight)
        {
            using (new ProfilerEvent(() => $"Setup light {i + 1} as directional"))
                SetupLight(nextLight, 1, material);
            index++;
            return (i == 0) ? ShaderPasses.TwoDirLights : ShaderPasses.TwoDirLightsAdd;
        }

        return (i == 0) ? ShaderPasses.OneDirLight : ShaderPasses.OneDirLightAdd;
    }

    private ShaderPasses SetupPointLight(PointLight light, int index, XMaterial material)
    {
        using (new ProfilerEvent(() => $"Setup light {index} as point"))
            SetupLight(light, 0, material);
        return (index == 0) ? ShaderPasses.OnePointLight : ShaderPasses.OnePointLightAdd;
    }
    #endregion
}
