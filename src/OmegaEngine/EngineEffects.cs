/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using OmegaEngine.Foundation.Light;

namespace OmegaEngine;

/// <summary>
/// Turn specific rendering effects in the <see cref="Engine"/> on or off.
/// </summary>
public sealed class EngineEffects
{
    #region Dependencies
    private readonly EngineCapabilities _capabilities;

    /// <summary>
    /// Creates a new engine effects object.
    /// </summary>
    /// <param name="capabilities">Determines which effects can be turned on.</param>
    internal EngineEffects(EngineCapabilities capabilities)
    {
        _capabilities = capabilities ?? throw new ArgumentNullException(nameof(capabilities));
    }
    #endregion

    //--------------------//

    #region Per-pixel effects
    private bool _perPixelLighting, _normalMapping, _postScreenEffects;

    /// <summary>
    /// Use per-pixel lighting
    /// </summary>
    /// <seealso cref="EngineCapabilities.PerPixelEffects"/>
    public bool PerPixelLighting { get => _perPixelLighting; set => _perPixelLighting = _capabilities.PerPixelEffects && value; }

    /// <summary>
    /// Use normal mapping
    /// </summary>
    /// <seealso cref="EngineCapabilities.PerPixelEffects"/>
    public bool NormalMapping { get => _normalMapping; set => _normalMapping = _capabilities.PerPixelEffects && value; }

    /// <summary>
    /// Use post-screen effects
    /// </summary>
    /// <seealso cref="EngineCapabilities.PerPixelEffects"/>
    public bool PostScreenEffects { get => _postScreenEffects; set => _postScreenEffects = _capabilities.PerPixelEffects && value; }

    /// <summary>
    /// Enable or disable shadowing casting (does not affect terrain self-shadowing)
    /// </summary>
    public bool Shadows { get; set; } = true;
    #endregion

    #region Double sampling
    private bool _doubleSampling;

    /// <summary>
    /// Sample terrain textures twice with different texture coordinates for better image quality
    /// </summary>
    /// <seealso cref="EngineCapabilities.DoubleSampling"/>
    public bool DoubleSampling { get => _doubleSampling; set => _doubleSampling = _capabilities.DoubleSampling && value; }
    #endregion

    #region Water effects
    private WaterEffectsType _waterEffects = WaterEffectsType.None;

    /// <summary>
    /// The effects to be display on water (e.g. reflections)
    /// </summary>
    public WaterEffectsType WaterEffects
    {
        get => _waterEffects;
        set =>
            // Check if the selected effect mode is supported by the hardware
            _waterEffects = _capabilities.PerPixelEffects ? value : WaterEffectsType.None;
    }
    #endregion
}
