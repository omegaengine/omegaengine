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
using System.Drawing;
using JetBrains.Annotations;
using NanoByte.Common;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.LightSources;
using OmegaEngine.Graphics.Renderables;
using SlimDX;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Shaders;

/// <summary>
/// A water surface shader
/// </summary>
/// <seealso cref="Water"/>
public class WaterShader : SurfaceShader
{
    #region Variables
    private EffectHandle? _reflectionMapHandle, _refractionMapHandle, _normalTextureHandle;
    private XTexture? _waterTexture;
    private XTexture? _normalTexture;
    private readonly TextureView? _reflectionView, _refractionView;
    #endregion

    #region Properties
    /// <summary>
    /// The minimum shader model version required to use this shader
    /// </summary>
    public static Version MinShaderModel => new(1, 1);

    private Color _dullColor = Color.FromArgb(77, 77, 128);

    /// <summary>
    /// The basic color of the water (usually blueish)
    /// </summary>
    [Description("The basic color of the water (usually blueish)")]
    public Color DullColor { get => _dullColor; set => value.To(ref _dullColor, () => SetShaderParameter("DullColor", value)); }

    private float _dullBlendFactor = 0.15f;

    /// <summary>
    /// How strongly to factor in <see cref="DullColor"/> - values between 0 and 1
    /// </summary>
    [DefaultValue(0.15f), Description("How strongly to factor in DullColor - values between 0 and 1")]
    public float DullBlendFactor
    {
        get => _dullBlendFactor;
        set
        {
            value = value.Clamp();
            value.To(ref _dullBlendFactor, () => SetShaderParameter("DullBlendFactor", value));
        }
    }

    private float _waveLength = 0.1f, _waveHeight = 0.01f, _windForce = 0.2f;

    /// <summary>
    /// The length of waves on the water surface - values between 0 and 1
    /// </summary>
    [DefaultValue(0.1f), Description("The length of waves on the water surface - values between 0 and 1")]
    public float WaveLength
    {
        get => _waveLength;
        set
        {
            value = value.Clamp();
            value.To(ref _waveLength, () => SetShaderParameter("WaveLength", value));
        }
    }

    /// <summary>
    /// The height of waves on the water surface - values between 0 and 0.2
    /// </summary>
    [DefaultValue(0.01f), Description("The height of waves on the water surface - values between 0 and 0.2")]
    public float WaveHeight
    {
        get => _waveHeight;
        set
        {
            value = value.Clamp(0, 0.2f);
            value.To(ref _waveHeight, () => SetShaderParameter("WaveHeight", value));
        }
    }

    /// <summary>
    /// The strength of the wind moving the waves - values between 0 and 1
    /// </summary>
    [DefaultValue(0.2f), Description("The strength of the wind moving the waves - values between 0 and 1")]
    public float WindForce
    {
        get => _windForce;
        set
        {
            value = value.Clamp();
            value.To(ref _windForce, () => SetShaderParameter("WindForce", value));
        }
    }

    private Matrix _windDirection;

    /// <summary>
    /// The direction of the wind moving the waves
    /// </summary>
    [Description("The direction of the wind moving the waves")]
    public Matrix WindDirection { get => _windDirection; set => value.To(ref _windDirection, () => SetShaderParameter("WindDirection", value)); }

    private Matrix _reflectionViewProjection;

    /// <summary>
    /// The reflected view-projection from the <see cref="Camera"/>
    /// </summary>
    [Browsable(false)]
    public Matrix ReflectionViewProjection { get => _reflectionViewProjection; set => value.To(ref _reflectionViewProjection, () => SetShaderParameter("ReflectionViewProjection", value)); }
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new instance of the water shader
    /// </summary>
    /// <param name="refractionView">A render target storing the refraction of the current view; <c>null</c> for no refraction or reflection</param>
    /// <param name="reflectionView">A render target storing the reflection of the current view; <c>null</c> for no reflection</param>
    public WaterShader(TextureView? refractionView = null, TextureView? reflectionView = null)
    {
        _refractionView = refractionView;
        _reflectionView = reflectionView;

        WindDirection = Matrix.RotationZ(2.0f);
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
    /// <param name="lights">An array of all lights this shader should consider; should be <c>null</c>.</param>
    public override void Apply([InstantHandle] Action render, XMaterial material, Camera camera, params IReadOnlyList<LightSource> lights)
    {
        #region Sanity checks
        if (render == null) throw new ArgumentNullException(nameof(render));
        if (camera == null) throw new ArgumentNullException(nameof(camera));
        if (lights == null) throw new ArgumentNullException(nameof(lights));
        #endregion

        // Always reset the textures since they might change their memory address at a device reset
        Effect.SetTexture(_normalTextureHandle, _normalTexture);
        if (_refractionView != null) Effect.SetTexture(_refractionMapHandle, _refractionView.GetRenderTarget());
        if (_waterTexture == null) Effect.SetTexture(_reflectionMapHandle, _reflectionView.GetRenderTarget());
        else Effect.SetTexture(_reflectionMapHandle, _waterTexture);

        base.Apply(render, material, camera, lights);
    }
    #endregion

    //--------------------//

    #region Engine
    /// <inheritdoc/>
    protected override void OnEngineSet()
    {
        if (MinShaderModel > Engine.Capabilities.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);

        LoadShaderFile("Water.fxo");

        _reflectionMapHandle = Effect.GetParameter(null, "ReflectionMap");
        _refractionMapHandle = Effect.GetParameter(null, "RefractionMap");
        _normalTextureHandle = Effect.GetParameter(null, "NormalTexture");

        if (_refractionView == null)
        {
            _waterTexture = XTexture.Get(Engine, @"Water\surface.png");
            _waterTexture.HoldReference();
            Effect.Technique = "Simple";
        }
        else
        {
            _normalTexture = XTexture.Get(Engine, @"Water\normal.png");
            _normalTexture.HoldReference();

            if (_reflectionView == null)
            {
                _waterTexture = XTexture.Get(Engine, @"Water\surface.png");
                _waterTexture.HoldReference();
                Effect.Technique = "Refraction";
            }
            else Effect.Technique = "RefractionReflection";
        }

        base.OnEngineSet();
    }
    #endregion

    #region Dispose
    /// <inheritdoc/>
    protected override void OnDispose()
    {
        try
        {
            _waterTexture?.ReleaseReference();
            _normalTexture?.ReleaseReference();
        }
        finally
        {
            base.OnDispose();
        }
    }
    #endregion
}
