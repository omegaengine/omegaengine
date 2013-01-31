/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Drawing;
using Common;
using SlimDX;
using SlimDX.Direct3D9;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// A water surface shader
    /// </summary>
    /// <seealso cref="Water"/>
    public class WaterShader : SurfaceShader
    {
        #region Variables
        private Color _dullColor = Color.FromArgb(77, 77, 128);
        private float _dullBlendFactor = 0.15f;
        private float _waveLength = 0.1f, _waveHeight = 0.01f, _windForce = 0.2f;
        private Matrix _windDirection;
        private Matrix _reflectionViewProjection;

        private EffectHandle _reflectionMapHandle, _refractionMapHandle, _normalTextureHandle;
        private EffectHandle _dullColorHandle, _dullBlendFactorHandle;
        private EffectHandle _waveLengthHandle, _waveHeightHandle, _windForceHandle, _windDirectionHandle;
        private EffectHandle _reflectionViewProjectionHandle;

        private readonly XTexture _waterTexture, _normalTexture;
        private readonly TextureView _reflectionView, _refractionView;
        #endregion

        #region Properties
        /// <summary>
        /// The minimum shader model version required to use this shader
        /// </summary>
        public static Version MinShaderModel { get { return new Version(1, 1); } }

        /// <summary>
        /// The basic color of the water (usually blueish)
        /// </summary>
        [Description("The basic color of the water (usually blueish)")]
        public Color DullColor
        {
            get { return _dullColor; }
            set
            {
                if (Disposed) return;
                UpdateHelper.Do(ref _dullColor, value, () => Effect.SetValue(_dullColorHandle, new Color4(value)));
            }
        }

        /// <summary>
        /// How strongly to factor in <see cref="DullColor"/> - values between 0 and 1
        /// </summary>
        [DefaultValue(0.15f), Description("How strongly to factor in DullColor - values between 0 and 1")]
        public float DullBlendFactor
        {
            get { return _dullBlendFactor; }
            set
            {
                if (Disposed) return;
                if (value < 0 || value > 1) throw new ArgumentOutOfRangeException("value");
                UpdateHelper.Do(ref _dullBlendFactor, value, () => Effect.SetValue(_dullBlendFactorHandle, value));
            }
        }

        /// <summary>
        /// The length of waves on the water surface - values between 0 and 1
        /// </summary>
        [DefaultValue(0.1f), Description("The length of waves on the water surface - values between 0 and 1")]
        public float WaveLength
        {
            get { return _waveLength; }
            set
            {
                if (Disposed) return;
                if (value < 0 || value > 1) throw new ArgumentOutOfRangeException("value");
                UpdateHelper.Do(ref _waveLength, value, () => Effect.SetValue(_waveLengthHandle, value));
            }
        }

        /// <summary>
        /// The height of waves on the water surface - values between 0 and 0.2
        /// </summary>
        [DefaultValue(0.01f), Description("The height of waves on the water surface - values between 0 and 0.2")]
        public float WaveHeight
        {
            get { return _waveHeight; }
            set
            {
                if (Disposed) return;
                if (value < 0 || value > 0.2) throw new ArgumentOutOfRangeException("value");
                UpdateHelper.Do(ref _waveHeight, value, () => Effect.SetValue(_waveHeightHandle, value));
            }
        }

        /// <summary>
        /// The strength of the wind moving the waves - values between 0 and 1
        /// </summary>
        [DefaultValue(0.2f), Description("The strength of the wind moving the waves - values between 0 and 1")]
        public float WindForce
        {
            get { return _windForce; }
            set
            {
                if (Disposed) return;
                if (value < 0 || value > 1) throw new ArgumentOutOfRangeException("value");
                UpdateHelper.Do(ref _windForce, value, () => Effect.SetValue(_windForceHandle, value));
            }
        }

        /// <summary>
        /// The direction of the wind moving the waves
        /// </summary>
        [Description("The direction of the wind moving the waves")]
        public Matrix WindDirection
        {
            get { return _windDirection; }
            set
            {
                if (Disposed) return;
                UpdateHelper.Do(ref _windDirection, value, () => Effect.SetValue(_windDirectionHandle, value));
            }
        }

        /// <summary>
        /// The reflected view-projection from the <see cref="Camera"/>
        /// </summary>
        [Browsable(false)]
        public Matrix ReflectionViewProjection
        {
            get { return _reflectionViewProjection; }
            set
            {
                if (Disposed) return;
                UpdateHelper.Do(ref _reflectionViewProjection, value, () => Effect.SetValue(_reflectionViewProjectionHandle, value));
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the water shader with refraction and reflection
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the shader into</param>
        /// <param name="refractionView">A render target storing the refraction of the current view</param>
        /// <param name="reflectionView">A render target storing the reflection of the current view</param>
        public WaterShader(Engine engine, TextureView refractionView, TextureView reflectionView) : base(engine, "Water.fxo")
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (MinShaderModel > engine.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            #endregion

            _normalTexture = XTexture.Get(engine, @"Water\normal.png", false);
            _normalTexture.HoldReference();
            _refractionView = refractionView;
            _reflectionView = reflectionView;

            Effect.Technique = "RefractionReflection";

            // Get handles to shader parameters for quick access
            SetHandles();

            // Set default values
            WindDirection = Matrix.RotationZ(2.0f);
        }

        /// <summary>
        /// Creates a new instance of the water shader with refraction but without reflection
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the shader into</param>
        /// <param name="refractionView">A render target storing the refraction of the current view</param>
        public WaterShader(Engine engine, TextureView refractionView) : base(engine, "Water.fxo")
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (MinShaderModel > engine.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            #endregion

            _waterTexture = XTexture.Get(engine, @"Water\surface.png", false);
            _waterTexture.HoldReference();
            _normalTexture = XTexture.Get(engine, @"Water\normal.png", false);
            _normalTexture.HoldReference();
            _refractionView = refractionView;

            Effect.Technique = "Refraction";

            // Get handles to shader parameters for quick access
            SetHandles();

            // Set default values
            WindDirection = Matrix.RotationZ(2.0f);
        }

        /// <summary>
        /// Creates a new instance of the water shader with no refraction or reflection
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the shader into</param>
        /// <exception cref="NotSupportedException">Thrown if the graphics card does not support this shader</exception>
        public WaterShader(Engine engine) : base(engine, "Water.fxo")
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (MinShaderModel > engine.MaxShaderModel) throw new NotSupportedException(Resources.NotSupportedShader);
            #endregion

            _waterTexture = XTexture.Get(engine, @"Water\surface.png", false);
            _waterTexture.HoldReference();

            Effect.Technique = "Simple";

            // Get handles to shader parameters for quick access
            SetHandles();

            // Set default values
            WindDirection = Matrix.RotationZ(2.0f);
        }
        #endregion

        //--------------------//

        #region Handles
        private void SetHandles()
        {
            _reflectionMapHandle = Effect.GetParameter(null, "ReflectionMap");
            _refractionMapHandle = Effect.GetParameter(null, "RefractionMap");
            _normalTextureHandle = Effect.GetParameter(null, "NormalTexture");

            _dullColorHandle = Effect.GetParameter(null, "DullColor");
            _dullBlendFactorHandle = Effect.GetParameter(null, "DullBlendFactor");

            _waveLengthHandle = Effect.GetParameter(null, "WaveLength");
            _waveHeightHandle = Effect.GetParameter(null, "WaveHeight");
            _windForceHandle = Effect.GetParameter(null, "WindForce");
            _windDirectionHandle = Effect.GetParameter(null, "WindDirection");
            _reflectionViewProjectionHandle = Effect.GetParameter(null, "ReflectionViewProjection");
        }
        #endregion

        #region Apply
        /// <summary>
        /// Applies the shader to the content in the render delegate.
        /// </summary>
        /// <param name="render">The render delegate (is called once for every shader pass).</param>
        /// <param name="material">The material to be used by this shader; <see langword="null"/> for device texture.</param>
        /// <param name="camera">The camera for transformation information.</param>
        /// <param name="lights">An array of all lights this shader should consider; should be <see langword="null"/>.</param>
        internal override void Apply(Action render, XMaterial material, Camera camera, LightSource[] lights)
        {
            #region Sanity checks
            if (render == null) throw new ArgumentNullException("render");
            if (camera == null) throw new ArgumentNullException("camera");
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

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (Disposed || Engine == null || Engine.Disposed) return; // Don't try to dispose more than once

            try
            {
                if (disposing)
                { // This block will only be executed on manual disposal, not by Garbage Collection
                    if (_waterTexture != null) _waterTexture.ReleaseReference();
                    if (_normalTexture != null) _normalTexture.ReleaseReference();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion
    }
}
