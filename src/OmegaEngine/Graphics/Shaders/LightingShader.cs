/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
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
        None = -1,
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
    protected override void RunPasses([InstantHandle] Action render, XMaterial material, params LightSource[] lights)
    {
        #region Sanity checks
        if (render == null) throw new ArgumentNullException(nameof(render));
        if (lights == null) throw new ArgumentNullException(nameof(lights));
        #endregion

        // Note: Manual control of shader logic (no SAS scripting)

        Effect.Begin(FX.None);

        // If this shader supports light-less shading, it will have to do that with its first pass
        if (lights.Length == 0)
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
            for (int i = 0; i < lights.Length; i++)
            {
                var pass = ShaderPasses.None;

                // ReSharper disable AccessToModifiedClosure
                if (lights[i] is DirectionalLight dirLight)
                {
                    if (i == 0 && dirLight.Diffuse == Color.Black && dirLight.Specular == Color.Black)
                    {
                        #region Ambient light
                        using (new ProfilerEvent(() => $"Setup light {i} as ambient"))
                            SetupLight(dirLight, 0, material);
                        pass = ShaderPasses.AmbientLight;
                        #endregion
                    }
                    else
                    {
                        #region Directional lights
                        using (new ProfilerEvent(() => $"Setup light {i} as directional"))
                            SetupLight(dirLight, 0, material);
                        pass = (i == 0) ? ShaderPasses.OneDirLight : ShaderPasses.OneDirLightAdd;

                        // Try to handle the next light at the same time
                        if (i + 1 < lights.Length)
                        {
                            if (lights[i + 1] is DirectionalLight nextDirLight)
                            {
                                using (new ProfilerEvent(() => $"Setup light {i}{1} as directional")) SetupLight(nextDirLight, 1, material);
                                pass = (i == 0) ? ShaderPasses.TwoDirLights : ShaderPasses.TwoDirLightsAdd;

                                // Handled two lights at once, so bump up counter
                                i++;
                            }
                        }
                        #endregion
                    }
                }
                else
                {
                    if (lights[i] is PointLight pointLight)
                    {
                        #region Point lights
                        using (new ProfilerEvent(() => $"Setup light {i} as point"))
                            SetupLight(pointLight, 0, material);
                        pass = (i == 0) ? ShaderPasses.OnePointLight : ShaderPasses.OnePointLightAdd;
                        #endregion
                    }
                }
                // ReSharper restore AccessToModifiedClosure

                if (pass == ShaderPasses.None) continue;

                #region Render pass
                using (new ProfilerEvent(() => "Pass " + pass))
                {
                    Effect.BeginPass((int)pass);
                    render();
                    Effect.EndPass();
                }
                #endregion

                // Only apply one pass if fog is turned on, since it would mess up additive blending
                if (Engine.State.Fog) break;
            }
        }

        Effect.End();
    }
    #endregion
}
