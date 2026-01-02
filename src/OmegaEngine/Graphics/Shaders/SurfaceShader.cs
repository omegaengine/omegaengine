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
using OmegaEngine.Graphics.LightSources;
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
        _lightSpecularHandles = [];
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

        TrySetValue(_lightPositionHandles, index, new Vector4(light.GetFloatingPosition(), 1));
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
                _ = info.Type switch
                {
                    ParameterType.Int => info.SemanticID switch
                    {
                        SemanticID.FilterMode => SetValue(Engine.Anisotropic ? 3 : 2),
                        _ => null
                    },
                    ParameterType.Float => info.Class switch
                    {
                        ParameterClass.Scalar => info.SemanticID switch
                        {
                            SemanticID.SpecularPower => SetValue(material.SpecularPower),
                            SemanticID.Emissive => SetValue(material.EmissiveFactor),
                            SemanticID.Time => SetValue((float)Engine.TotalGameTime),
                            SemanticID.ElapsedTime => SetValue((float)Engine.LastFrameGameTime),
                            _ => null
                        },
                        ParameterClass.Vector => info.SemanticID switch
                        {
                            SemanticID.CameraPosition => SetValue(camera.Position.ApplyOffset(camera.FloatingOrigin)),
                            SemanticID.Position when !_lightParametersHandled => StoreHandle(_lightPositionHandles),
                            SemanticID.Direction when !_lightParametersHandled => StoreHandle(_lightDirectionHandles),
                            SemanticID.Attenuation when !_lightParametersHandled => StoreHandle(_lightAttenuationHandles),
                            SemanticID.Color or SemanticID.Diffuse when !_lightParametersHandled => StoreHandle(_lightDiffuseHandles),
                            SemanticID.Ambient when !_lightParametersHandled => StoreHandle(_lightAmbientHandles),
                            SemanticID.Specular when !_lightParametersHandled => StoreHandle(_lightSpecularHandles),
                            SemanticID.Emissive => SetValue(new Color4(material.Emissive)),
                            _ => null
                        },
                        ParameterClass.MatrixRows or ParameterClass.MatrixColumns => info.SemanticID switch
                        {
                            SemanticID.World => SetValue(Engine.State.WorldTransform),
                            SemanticID.WorldInverse => SetValue(Matrix.Invert(Engine.State.WorldTransform)),
                            SemanticID.WorldTranspose => SetValue(Matrix.Transpose(Engine.State.WorldTransform)),
                            SemanticID.WorldInverseTranspose => SetValue(Matrix.Transpose(Matrix.Invert(Engine.State.WorldTransform))),
                            SemanticID.View => SetValue(camera.View),
                            SemanticID.ViewInverse => SetValue(camera.ViewInverse),
                            SemanticID.ViewTranspose => SetValue(camera.ViewTranspose),
                            SemanticID.ViewInverseTranspose => SetValue(camera.ViewInverseTranspose),
                            SemanticID.Projection => SetValue(camera.Projection),
                            SemanticID.ProjectionInverse => SetValue(camera.ProjectionInverse),
                            SemanticID.ProjectionTranspose => SetValue(camera.ProjectionTranspose),
                            SemanticID.ProjectionInverseTranspose => SetValue(camera.ProjectionInverseTranspose),
                            SemanticID.WorldView => SetValue(Engine.State.WorldTransform * camera.View),
                            SemanticID.WorldViewInverse => SetValue(Matrix.Invert(Engine.State.WorldTransform * camera.View)),
                            SemanticID.WorldViewTranspose => SetValue(Matrix.Transpose(Engine.State.WorldTransform * camera.View)),
                            SemanticID.WorldViewInverseTranspose => SetValue(Matrix.Transpose(Matrix.Invert(Engine.State.WorldTransform * camera.View))),
                            SemanticID.ViewProjection => SetValue(camera.ViewProjection),
                            SemanticID.ViewProjectionInverse => SetValue(camera.ViewProjectionInverse),
                            SemanticID.ViewProjectionTranspose => SetValue(camera.ViewProjectionTranspose),
                            SemanticID.ViewProjectionInverseTranspose => SetValue(camera.ViewProjectionInverseTranspose),
                            SemanticID.WorldViewProjection => SetValue(Engine.State.WorldTransform * camera.ViewProjection),
                            SemanticID.WorldViewProjectionInverse => SetValue(Matrix.Invert(Engine.State.WorldTransform * camera.ViewProjection)),
                            SemanticID.WorldViewProjectionTranspose => SetValue(Matrix.Transpose(Engine.State.WorldTransform * camera.ViewProjection)),
                            SemanticID.WorldViewProjectionInverseTranspose => SetValue(Matrix.Transpose(Matrix.Invert(Engine.State.WorldTransform * camera.ViewProjection))),
                            _ => null
                        },
                        _ => null
                    },
                    ParameterType.Texture => info.SemanticID switch
                    {
                        SemanticID.Diffuse or SemanticID.DiffuseMap => SetDiffuseTexture(),
                        SemanticID.Normal or SemanticID.NormalMap => SetTexture(material.NormalMap?.Texture),
                        SemanticID.Height or SemanticID.HeightMap => SetTexture(material.HeightMap?.Texture),
                        SemanticID.Specular or SemanticID.SpecularMap => SetTexture(material.SpecularMap?.Texture),
                        SemanticID.Emissive => SetTexture(material.EmissiveMap?.Texture),
                        _ => null
                    },
                    _ => null
                };

                Result SetValue<T>(T value) where T : struct
                    => Effect.SetValue(info.Handle, value);

                Result SetTexture(BaseTexture? texture)
                    => Effect.SetTexture(info.Handle, texture);

                Result SetDiffuseTexture()
                {
                    var ret = diffuseMapCount < material.DiffuseMaps.Length && material.DiffuseMaps[diffuseMapCount] != null
                        ? SetTexture(material.DiffuseMaps[diffuseMapCount].Texture)
                        : SetTexture(null);

                    // Increment counter for possible next reference in this shader to a (different) diffuse map
                    diffuseMapCount++;

                    return ret;
                }

                Result? StoreHandle(List<EffectHandle> list)
                {
                    list.Add(info.Handle);
                    return null;
                }
            }
        }

        _lightParametersHandled = true;

        RunPasses(render, material, lights);
    }
    #endregion
}
