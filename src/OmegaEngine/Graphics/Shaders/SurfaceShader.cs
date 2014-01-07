/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SlimDX;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// A shader that controls the look of a <see cref="PositionableRenderable"/>'s surface
    /// </summary>
    /// <seealso cref="PositionableRenderable.SurfaceShader"/>
    public abstract class SurfaceShader : Shader
    {
        #region Variables
        /// <summary>Indicates whether <see cref="Apply"/> has already run once and detected all light <see cref="EffectHandle"/>s.</summary>
        private bool _lightParametersHandled;

        // Note: Using Lists here, because the size of the internal arrays will auto-optimize after a few frames and because we need index access
        private readonly List<EffectHandle>
            _lightPositionHandles = new List<EffectHandle>(),
            _lightDirectionHandles = new List<EffectHandle>(),
            _lightAttenuationHandles = new List<EffectHandle>(),
            _lightDiffuseHandles = new List<EffectHandle>(),
            _lightAmbientHandles = new List<EffectHandle>(),
            _lightSpecularHandles = new List<EffectHandle>(),
            _lightSpecularPowerHandles = new List<EffectHandle>();
        #endregion

        #region Constructor
        /// <summary>
        /// Loads a surface shader from a file
        /// </summary>
        /// <param name="engine">The <see cref="OmegaEngine.Engine"/> reference to use for rendering operations</param>
        /// <param name="path">The shader file path relative to the shader directory or as an absolute path</param>
        protected SurfaceShader(Engine engine, string path) : base(engine, path)
        {}

        /// <summary>
        /// Wraps a DirectX <see cref="Effect"/> in a surface shader
        /// </summary>
        /// <param name="engine">The <see cref="OmegaEngine.Engine"/> reference to use for rendering operations</param>
        /// <param name="effect">The <see cref="Effect"/> to wrap</param>
        protected SurfaceShader(Engine engine, Effect effect) : base(engine, effect)
        {}
        #endregion

        //--------------------//

        #region Light helper
        /// <summary>
        /// Transfer lighting data from a <see cref="LightSource"/> to the shader
        /// </summary>
        /// <param name="light">The <see cref="LightSource"/> to get the information from</param>
        /// <param name="index">The light index in the shader to set</param>
        /// <param name="material">The currently active material</param>
        private void SetupLightHelper(LightSource light, int index, XMaterial material)
        {
            if (index < _lightDiffuseHandles.Count)
                Effect.SetValue(_lightDiffuseHandles[index], Color4.Modulate(light.Diffuse, material.Diffuse));
            if (index < _lightAmbientHandles.Count)
                Effect.SetValue(_lightAmbientHandles[index], Color4.Modulate(light.Ambient, material.Ambient));
            if (index < _lightSpecularHandles.Count)
                Effect.SetValue(_lightSpecularHandles[index], Color4.Modulate(light.Specular, material.Specular));
            if (index < _lightSpecularPowerHandles.Count)
                Effect.SetValue(_lightSpecularPowerHandles[index], material.SpecularPower);
        }

        /// <summary>
        /// Transfer lighting data from a <see cref="PointLight"/> to the shader
        /// </summary>
        /// <param name="light">The <see cref="PointLight"/> to get the information from</param>
        /// <param name="index">The light index in the shader to set</param>
        /// <param name="material">The currently active material</param>
        protected void SetupLight(PointLight light, int index, XMaterial material)
        {
            #region Sanity checks
            if (light == null) throw new ArgumentNullException("light");
            #endregion

            SetupLightHelper(light, index, material);

            if (index < _lightPositionHandles.Count)
            {
                Effect.SetValue(_lightPositionHandles[index],
                    new Vector4(((IPositionableOffset)light).EffectivePosition, 1));
            }
            if (index < _lightAttenuationHandles.Count)
                Effect.SetValue(_lightAttenuationHandles[index], (Vector4)light.Attenuation);
        }

        /// <summary>
        /// Transfer lighting data from a <see cref="DirectionalLight"/> to the shader
        /// </summary>
        /// <param name="light">The <see cref="DirectionalLight"/> to get the information from</param>
        /// <param name="index">The light index in the shader to set</param>
        /// <param name="material">The currently active material</param>
        protected void SetupLight(DirectionalLight light, int index, XMaterial material)
        {
            #region Sanity checks
            if (light == null) throw new ArgumentNullException("light");
            #endregion

            SetupLightHelper(light, index, material);
            Effect.SetValue(_lightDirectionHandles[index], new Vector4(light.Direction, 0));
        }
        #endregion

        #region Passes
        /// <summary>
        /// Runs the actual shader passes
        /// </summary>
        /// <param name="render">The render delegate (is called once for every shader pass)</param>
        /// <param name="material">The material to be used by this shader; <see langword="null"/> for device texture</param>
        /// <param name="lights">An array of all lights this shader should consider; <see langword="null"/> for no lighting</param>
        protected virtual void RunPasses(Action render, XMaterial material, params LightSource[] lights)
        {
            #region Sanity checks
            if (render == null) throw new ArgumentNullException("render");
            if (lights == null) throw new ArgumentNullException("lights");
            #endregion

            int passCount = Effect.Begin(FX.None);
            IList<SasScriptCommand> techniqueScript;
            if (Techniques.TryGetValue(Effect.Technique, out techniqueScript))
                ExecuteScript(techniqueScript, render);
            else
            {
                for (int i = 0; i < passCount; i++)
                {
                    Effect.BeginPass(i);
                    render();
                    Effect.EndPass();
                }
            }

            Effect.End();
        }
        #endregion

        #region Apply
        /// <summary>
        /// Applies the shader to the content in the render delegate
        /// </summary>
        /// <param name="render">The render delegate (is called once for every shader pass)</param>
        /// <param name="material">The material to be used by this shader; <see langword="null"/> for device texture</param>
        /// <param name="camera">The camera for transformation information</param>
        /// <param name="lights">An array of all lights this shader should consider</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "Default parameters are set via a huge set of cascading switch-statements.")]
        public virtual void Apply(Action render, XMaterial material, Camera camera, params LightSource[] lights)
        {
            #region Sanity checks
            if (Disposed) throw new ObjectDisposedException(ToString());
            if (render == null) throw new ArgumentNullException("render");
            if (camera == null) throw new ArgumentNullException("camera");
            if (lights == null) throw new ArgumentNullException("lights");
            #endregion

            #region Values
            using (new ProfilerEvent("Set shader parameters"))
            {
                byte diffuseMapCount = 0;
                foreach (ParameterInfo info in ParameterInfos)
                {
                    switch (info.Type)
                    {
                        case ParameterType.Int:
                            switch (info.SemanticID)
                            {
                                    #region Texture filtering
                                case SemanticID.FilterMode:
                                    const int linearFiltering = 2, anisotropicFiltering = 3;
                                    Effect.SetValue(info.Handle, Engine.Anisotropic ? anisotropicFiltering : linearFiltering);
                                    break;
                                    #endregion
                            }
                            break;

                        case ParameterType.Float:
                            switch (info.SemanticID)
                            {
                                    #region Camera
                                case SemanticID.CameraPosition:
                                    Effect.SetValue(info.Handle, camera.Position.ApplyOffset(camera.PositionBase));
                                    break;
                                    #endregion

                                    #region Normal transformations
                                case SemanticID.World:
                                    Effect.SetValue(info.Handle, Engine.State.WorldTransform);
                                    break;
                                case SemanticID.WorldInverse:
                                    Effect.SetValue(info.Handle, Matrix.Invert(Engine.State.WorldTransform));
                                    break;
                                case SemanticID.WorldTranspose:
                                    Effect.SetValue(info.Handle, Matrix.Transpose(Engine.State.WorldTransform));
                                    break;
                                case SemanticID.WorldInverseTranspose:
                                    Effect.SetValue(info.Handle, Matrix.Transpose(Matrix.Invert(Engine.State.WorldTransform)));
                                    break;

                                case SemanticID.View:
                                    Effect.SetValue(info.Handle, camera.View);
                                    break;
                                case SemanticID.ViewInverse:
                                    Effect.SetValue(info.Handle, camera.ViewInverse);
                                    break;
                                case SemanticID.ViewTranspose:
                                    Effect.SetValue(info.Handle, camera.ViewTranspose);
                                    break;
                                case SemanticID.ViewInverseTranspose:
                                    Effect.SetValue(info.Handle, camera.ViewInverseTranspose);
                                    break;

                                case SemanticID.Projection:
                                    Effect.SetValue(info.Handle, camera.Projection);
                                    break;
                                case SemanticID.ProjectionInverse:
                                    Effect.SetValue(info.Handle, camera.ProjectionInverse);
                                    break;
                                case SemanticID.ProjectionTranspose:
                                    Effect.SetValue(info.Handle, camera.ProjectionTranspose);
                                    break;
                                case SemanticID.ProjectionInverseTranspose:
                                    Effect.SetValue(info.Handle, camera.ProjectionInverseTranspose);
                                    break;
                                    #endregion

                                    #region Composite transformations
                                case SemanticID.WorldView:
                                    Effect.SetValue(info.Handle, Engine.State.WorldTransform * camera.View);
                                    break;
                                case SemanticID.WorldViewInverse:
                                    Effect.SetValue(info.Handle, Matrix.Invert(Engine.State.WorldTransform * camera.View));
                                    break;
                                case SemanticID.WorldViewTranspose:
                                    Effect.SetValue(info.Handle, Matrix.Transpose(Engine.State.WorldTransform * camera.View));
                                    break;
                                case SemanticID.WorldViewInverseTranspose:
                                    Effect.SetValue(info.Handle, Matrix.Transpose(Matrix.Invert(Engine.State.WorldTransform * camera.View)));
                                    break;

                                case SemanticID.ViewProjection:
                                    Effect.SetValue(info.Handle, camera.ViewProjection);
                                    break;
                                case SemanticID.ViewProjectionInverse:
                                    Effect.SetValue(info.Handle, camera.ViewProjectionInverse);
                                    break;
                                case SemanticID.ViewProjectionTranspose:
                                    Effect.SetValue(info.Handle, camera.ViewProjectionTranspose);
                                    break;
                                case SemanticID.ViewProjectionInverseTranspose:
                                    Effect.SetValue(info.Handle, camera.ViewProjectionInverseTranspose);
                                    break;

                                case SemanticID.WorldViewProjection:
                                    Effect.SetValue(info.Handle, Engine.State.WorldTransform * camera.ViewProjection);
                                    break;
                                case SemanticID.WorldViewProjectionInverse:
                                    Effect.SetValue(info.Handle, Matrix.Invert(Engine.State.WorldTransform * camera.ViewProjection));
                                    break;
                                case SemanticID.WorldViewProjectionTranspose:
                                    Effect.SetValue(info.Handle, Matrix.Transpose(Engine.State.WorldTransform * camera.ViewProjection));
                                    break;
                                case SemanticID.WorldViewProjectionInverseTranspose:
                                    Effect.SetValue(info.Handle, Matrix.Transpose(Matrix.Invert(Engine.State.WorldTransform * camera.ViewProjection)));
                                    break;
                                    #endregion

                                    #region Lighting
                                case SemanticID.Emissive:
                                    Effect.SetValue(info.Handle, new Color4(material.Emissive));
                                    break;

                                case SemanticID.Position:
                                    if (!_lightParametersHandled) _lightPositionHandles.Add(info.Handle);
                                    break;
                                case SemanticID.Direction:
                                    if (!_lightParametersHandled) _lightDirectionHandles.Add(info.Handle);
                                    break;
                                case SemanticID.Attenuation:
                                    if (!_lightParametersHandled) _lightAttenuationHandles.Add(info.Handle);
                                    break;
                                case SemanticID.Color:
                                case SemanticID.Diffuse:
                                    if (!_lightParametersHandled) _lightDiffuseHandles.Add(info.Handle);
                                    break;
                                case SemanticID.Ambient:
                                    if (!_lightParametersHandled) _lightAmbientHandles.Add(info.Handle);
                                    break;
                                case SemanticID.Specular:
                                    if (!_lightParametersHandled) _lightSpecularHandles.Add(info.Handle);
                                    break;
                                case SemanticID.SpecularPower:
                                    if (!_lightParametersHandled) _lightSpecularPowerHandles.Add(info.Handle);
                                    break;
                                    #endregion

                                    #region Timer
                                case SemanticID.Time:
                                    Effect.SetValue(info.Handle, (float)Engine.TotalGameTime);
                                    break;
                                case SemanticID.ElapsedTime:
                                    Effect.SetValue(info.Handle, (float)Engine.LastFrameGameTime);
                                    break;
                                    #endregion
                            }
                            break;

                        case ParameterType.Texture:
                            switch (info.SemanticID)
                            {
                                    #region Texture maps
                                case SemanticID.Diffuse:
                                case SemanticID.DiffuseMap:
                                    if (diffuseMapCount < material.DiffuseMaps.Length && material.DiffuseMaps[diffuseMapCount] != null)
                                        Effect.SetTexture(info.Handle, material.DiffuseMaps[diffuseMapCount].Texture);
                                    else Effect.SetTexture(info.Handle, null);

                                    // Increment counter for possible next reference in this shader to a (different) diffuse map
                                    diffuseMapCount++;
                                    break;
                                case SemanticID.Normal:
                                case SemanticID.NormalMap:
                                    Effect.SetTexture(info.Handle, (material.NormalMap == null) ? null : material.NormalMap.Texture);
                                    break;
                                case SemanticID.Height:
                                case SemanticID.HeightMap:
                                    Effect.SetTexture(info.Handle, (material.HeightMap == null) ? null : material.HeightMap.Texture);
                                    break;
                                case SemanticID.Specular:
                                case SemanticID.SpecularMap:
                                    Effect.SetTexture(info.Handle, (material.SpecularMap == null) ? null : material.SpecularMap.Texture);
                                    break;
                                case SemanticID.Emissive:
                                    Effect.SetTexture(info.Handle, (material.EmissiveMap == null) ? null : material.EmissiveMap.Texture);
                                    break;
                                    #endregion
                            }
                            break;
                    }
                }
            }
            #endregion

            _lightParametersHandled = true;

            RunPasses(render, material, lights);
        }
        #endregion
    }
}
