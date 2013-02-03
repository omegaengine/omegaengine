/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.Shaders
{
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

        #region Constructor
        /// <summary>
        /// Loads a surface shader from a file
        /// </summary>
        /// <param name="engine">The <see cref="OmegaEngine.Engine"/> reference to use for rendering operations</param>
        /// <param name="path">The shader file path relative to the shader directory or as an absolute path</param>
        protected LightingShader(Engine engine, string path) : base(engine, path)
        {}

        /// <summary>
        /// Wraps a DirectX <see cref="Effect"/> in a surface shader
        /// </summary>
        /// <param name="engine">The <see cref="OmegaEngine.Engine"/> reference to use for rendering operations</param>
        /// <param name="effect">The <see cref="Effect"/> to wrap</param>
        protected LightingShader(Engine engine, Effect effect) : base(engine, effect)
        {}
        #endregion

        //--------------------//

        #region Passes
        /// <inheritdoc />
        protected override void RunPasses(Action render, LightSource[] lights, XMaterial material)
        {
            #region Sanity checks
            if (render == null) throw new ArgumentNullException("render");
            if (lights == null) throw new ArgumentNullException("lights");
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
                    var dirLight = lights[i] as DirectionalLight;
                    if (dirLight != null)
                    {
                        if (i == 0 && dirLight.Diffuse == Color.Black && dirLight.Specular == Color.Black)
                        {
                            #region Ambient light
                            using (new ProfilerEvent(() => "Setup light " + i + " as ambient"))
                                SetupLight(dirLight, 0, material);
                            pass = ShaderPasses.AmbientLight;
                            #endregion
                        }
                        else
                        {
                            #region Directional lights
                            using (new ProfilerEvent(() => "Setup light " + i + " as directional"))
                                SetupLight(dirLight, 0, material);
                            pass = (i == 0) ? ShaderPasses.OneDirLight : ShaderPasses.OneDirLightAdd;

                            // Try to handle the next light at the same time
                            if (i + 1 < lights.Length)
                            {
                                var nextDirLight = lights[i + 1] as DirectionalLight;
                                if (nextDirLight != null)
                                {
                                    using (new ProfilerEvent(() => "Setup light " + i + 1 + " as directional")) SetupLight(nextDirLight, 1, material);
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
                        var pointLight = lights[i] as PointLight;
                        if (pointLight != null)
                        {
                            #region Point lights
                            using (new ProfilerEvent(() => "Setup light " + i + " as point"))
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
                    if (Engine.Fog) break;
                }
            }

            Effect.End();
        }
        #endregion
    }
}
