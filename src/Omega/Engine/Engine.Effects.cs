/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Common;
using Common.Utils;
using Common.Values;
using SlimDX.Direct3D9;

namespace OmegaEngine
{
    // This file contains properties that control rendering effects that can be turned on or off
    partial class Engine
    {
        #region Anisotropic filtering
        private bool _anisotropicFiltering;

        /// <summary>
        /// Does the hardware the engine is running on support anisotropic texture filtering?
        /// </summary>
        /// <seealso cref="Anisotropic"/>
        public bool SupportsAnisotropic { get { return MathUtils.CheckFlag((int)Manager.GetDeviceCaps(0, DeviceType.Hardware).TextureFilterCaps, (int)(FilterCaps.MinAnisotropic | FilterCaps.MagAnisotropic)); } }

        /// <summary>
        /// Use anisotropic texture filtering
        /// </summary>
        /// <seealso cref="SupportsAnisotropic"/>
        public bool Anisotropic
        {
            get { return _anisotropicFiltering; }
            set
            {
                if (value && !SupportsAnisotropic) value = false;
                UpdateHelper.Do(ref _anisotropicFiltering, value, SetupTextureFiltering);
            }
        }
        #endregion

        #region Per-pixel effects
        private bool _perPixelLighting, _normalMapping, _postScreenEffects, _shadows;

        /// <summary>
        /// Does the hardware the engine is running on support per-pixel effects?
        /// </summary>
        /// <seealso cref="PerPixelLighting"/>
        /// <seealso cref="NormalMapping"/>
        /// <seealso cref="PostScreenEffects"/>
        /// <seealso cref="Shadows"/>
        public bool SupportsPerPixelEffects { get { return MaxShaderModel >= new Version(2, 0); } }

        /// <summary>
        /// Use per-pixel lighting
        /// </summary>
        /// <seealso cref="SupportsPerPixelEffects"/>
        public bool PerPixelLighting { get { return _perPixelLighting; } set { _perPixelLighting = SupportsPerPixelEffects && value; } }

        /// <summary>
        /// Use normal mapping
        /// </summary>
        /// <seealso cref="SupportsPerPixelEffects"/>
        public bool NormalMapping { get { return _normalMapping; } set { _normalMapping = SupportsPerPixelEffects && value; } }

        /// <summary>
        /// Use post-screen effects
        /// </summary>
        /// <seealso cref="SupportsPerPixelEffects"/>
        public bool PostScreenEffects { get { return _postScreenEffects; } set { _postScreenEffects = SupportsPerPixelEffects && value; } }

        /// <summary>
        /// Apply shadows
        /// </summary>
        /// <seealso cref="SupportsPerPixelEffects"/>
        public bool Shadows { get { return _shadows; } set { _shadows = SupportsPerPixelEffects && value; } }
        #endregion

        #region Double sampling
        private bool _doubleSampling;

        /// <summary>
        /// Does the hardware the engine is running on support terrain texture double sampling?
        /// </summary>
        /// <seealso cref="DoubleSampling"/>
        public bool SupportsDoubleSampling { get { return MaxShaderModel >= new Version(2, 0, 1); } }

        /// <summary>
        /// Sample terrain textures twice with different texture coordinates for better image quality
        /// </summary>
        /// <seealso cref="SupportsDoubleSampling"/>
        public bool DoubleSampling { get { return _doubleSampling; } set { _doubleSampling = SupportsDoubleSampling && value; } }
        #endregion

        #region Water effects
        private WaterEffectsType _waterEffects = WaterEffectsType.None;

        /// <summary>
        /// The effects to be display on water (e.g. reflections)
        /// </summary>
        public WaterEffectsType WaterEffects
        {
            get { return _waterEffects; }
            set
            {
                // Check if the selected effect mode is supported by the hardware
                _waterEffects = SupportsPerPixelEffects ? value : WaterEffectsType.None;
            }
        }
        #endregion

        #region Particle system quality
        private Quality _particleSystemQuality = Quality.Medium;

        /// <summary>
        /// The quality of CPU-based particle systems
        /// </summary>
        public Quality ParticleSystemQuality { get { return _particleSystemQuality; } set { _particleSystemQuality = value; } }
        #endregion
    }
}
