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
using SlimDX;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;

namespace OmegaEngine.Graphics.Shaders;

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
        _lightPositionHandles = [],
        _lightDirectionHandles = [],
        _lightAttenuationHandles = [],
        _lightDiffuseHandles = [],
        _lightAmbientHandles = [],
        _lightSpecularHandles = [],
        _lightSpecularPowerHandles = [];
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
        TrySetValue(_lightDiffuseHandles, index, Color4.Modulate(light.Diffuse, material.Diffuse));
        TrySetValue(_lightAmbientHandles, index, Color4.Modulate(light.Ambient, material.Ambient));
        TrySetValue(_lightSpecularHandles, index, Color4.Modulate(light.Specular, material.Specular));
        TrySetValue(_lightSpecularPowerHandles, index, material.SpecularPower);
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
        if (light == null) throw new ArgumentNullException(nameof(light));
        #endregion

        SetupLightHelper(light, index, material);

        TrySetValue(_lightPositionHandles, index, new Vector4(((IPositionableOffset)light).EffectivePosition, 1));
        TrySetValue(_lightAttenuationHandles, index, (Vector4)light.Attenuation);
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
        if (light == null) throw new ArgumentNullException(nameof(light));
        #endregion

        SetupLightHelper(light, index, material);
        TrySetValue(_lightDirectionHandles, index, new Vector4(light.Direction, 0));
    }

    void TrySetValue<T>(List<EffectHandle> list, int index, T value) where T : struct
    {
        if (index < list.Count)
            Effect.SetValue(list[index], value);
    }
    #endregion

    #region Passes
    /// <summary>
    /// Runs the actual shader passes
    /// </summary>
    /// <param name="render">The render delegate (is called once for every shader pass)</param>
    /// <param name="material">The material to be used by this shader; <c>null</c> for device texture</param>
    /// <param name="lights">An array of all lights this shader should consider; <c>null</c> for no lighting</param>
    protected virtual void RunPasses([InstantHandle] Action render, XMaterial material, params LightSource[] lights)
    {
        #region Sanity checks
        if (render == null) throw new ArgumentNullException(nameof(render));
        if (lights == null) throw new ArgumentNullException(nameof(lights));
        #endregion

        int passCount = Effect.Begin(FX.None);
        if (Techniques.TryGetValue(Effect.Technique, out var techniqueScript))
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
    /// <param name="material">The material to be used by this shader; <c>null</c> for device texture</param>
    /// <param name="camera">The camera for transformation information</param>
    /// <param name="lights">An array of all lights this shader should consider</param>
    public virtual void Apply([InstantHandle] Action render, XMaterial material, Camera camera, params LightSource[] lights)
    {
        #region Sanity checks
        if (IsDisposed) throw new ObjectDisposedException(ToString());
        if (render == null) throw new ArgumentNullException(nameof(render));
        if (camera == null) throw new ArgumentNullException(nameof(camera));
        if (lights == null) throw new ArgumentNullException(nameof(lights));
        #endregion

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
                            case SemanticID.FilterMode:
                                const int linearFiltering = 2, anisotropicFiltering = 3;
                                Effect.SetValue(info.Handle, Engine.Anisotropic ? anisotropicFiltering : linearFiltering);
                                break;
                        }
                        break;

                    case ParameterType.Float:
                        switch (info.Class)
                        {
                            case ParameterClass.Scalar:
                                switch (info.SemanticID)
                                {
                                    case SemanticID.SpecularPower when !_lightParametersHandled:
                                        _lightSpecularPowerHandles.Add(info.Handle);
                                        break;
                                    case SemanticID.Time:
                                        Effect.SetValue(info.Handle, (float)Engine.TotalGameTime);
                                        break;
                                    case SemanticID.ElapsedTime:
                                        Effect.SetValue(info.Handle, (float)Engine.LastFrameGameTime);
                                        break;
                                }
                                break;

                            case ParameterClass.Vector:
                                switch (info.SemanticID)
                                {
                                    case SemanticID.CameraPosition:
                                        Effect.SetValue(info.Handle, camera.Position.ApplyOffset(camera.PositionBase));
                                        break;

                                    case SemanticID.Position when !_lightParametersHandled:
                                        _lightPositionHandles.Add(info.Handle);
                                        break;
                                    case SemanticID.Direction when !_lightParametersHandled:
                                        _lightDirectionHandles.Add(info.Handle);
                                        break;
                                    case SemanticID.Attenuation when !_lightParametersHandled:
                                        _lightAttenuationHandles.Add(info.Handle);
                                        break;
                                    case SemanticID.Color or SemanticID.Diffuse when !_lightParametersHandled:
                                        _lightDiffuseHandles.Add(info.Handle);
                                        break;
                                    case SemanticID.Ambient when !_lightParametersHandled:
                                        _lightAmbientHandles.Add(info.Handle);
                                        break;
                                    case SemanticID.Specular when !_lightParametersHandled:
                                        _lightSpecularHandles.Add(info.Handle);
                                        break;
                                    case SemanticID.Emissive:
                                        Effect.SetValue(info.Handle, new Color4(material.Emissive));
                                        break;
                                }
                                break;

                            case ParameterClass.MatrixRows or ParameterClass.MatrixColumns:
                                switch (info.SemanticID)
                                {
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
                                }
                                break;
                        }
                        break;

                    case ParameterType.Texture:
                        switch (info.SemanticID)
                        {
                            case SemanticID.Diffuse or SemanticID.DiffuseMap:
                                if (diffuseMapCount < material.DiffuseMaps.Length && material.DiffuseMaps[diffuseMapCount] != null)
                                    Effect.SetTexture(info.Handle, material.DiffuseMaps[diffuseMapCount].Texture);
                                else Effect.SetTexture(info.Handle, null);

                                // Increment counter for possible next reference in this shader to a (different) diffuse map
                                diffuseMapCount++;
                                break;
                            case SemanticID.Normal or SemanticID.NormalMap:
                                Effect.SetTexture(info.Handle, material.NormalMap?.Texture);
                                break;
                            case SemanticID.Height or SemanticID.HeightMap:
                                Effect.SetTexture(info.Handle, material.HeightMap?.Texture);
                                break;
                            case SemanticID.Specular or SemanticID.SpecularMap:
                                Effect.SetTexture(info.Handle, material.SpecularMap?.Texture);
                                break;
                            case SemanticID.Emissive:
                                Effect.SetTexture(info.Handle, material.EmissiveMap?.Texture);
                                break;
                        }
                        break;
                }
            }
        }

        _lightParametersHandled = true;

        RunPasses(render, material, lights);
    }
    #endregion
}
