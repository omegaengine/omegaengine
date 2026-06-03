/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using OmegaEngine.Foundation.Light;

namespace OmegaEngine;

/// <summary>
/// Turn specific rendering effects in the <see cref="Engine"/> on or off.
/// </summary>
/// <param name="capabilities">Determines which effects can be turned on.</param>
public sealed class EngineEffects(EngineCapabilities capabilities)
{
    private bool _perPixelLighting, _normalMapping, _postScreenEffects;

    /// <summary>
    /// Use per-pixel lighting
    /// </summary>
    /// <seealso cref="EngineCapabilities.PerPixelEffects"/>
    public bool PerPixelLighting { get => _perPixelLighting; set => _perPixelLighting = capabilities.PerPixelEffects && value; }

    /// <summary>
    /// Use normal mapping
    /// </summary>
    /// <seealso cref="EngineCapabilities.PerPixelEffects"/>
    public bool NormalMapping { get => _normalMapping; set => _normalMapping = capabilities.PerPixelEffects && value; }

    /// <summary>
    /// Use post-screen effects
    /// </summary>
    /// <seealso cref="EngineCapabilities.PerPixelEffects"/>
    public bool PostScreenEffects { get => _postScreenEffects; set => _postScreenEffects = capabilities.PerPixelEffects && value; }

    /// <summary>
    /// Enable or disable shadowing casting (does not affect terrain self-shadowing)
    /// </summary>
    public bool Shadows { get; set; }

    private bool _detailMapping;

    /// <summary>
    /// Sample textures twice with different texture coordinates to create an illusion of more details
    /// </summary>
    /// <seealso cref="EngineCapabilities.DetailMapping"/>
    public bool DetailMapping { get => _detailMapping; set => _detailMapping = capabilities.DetailMapping && value; }

    private WaterEffectsType _waterEffects;

    /// <summary>
    /// The effects to be display on water (e.g. reflections)
    /// </summary>
    public WaterEffectsType WaterEffects
    {
        get => _waterEffects;
        set => _waterEffects = capabilities.PerPixelEffects ? value : WaterEffectsType.None;
    }
}
