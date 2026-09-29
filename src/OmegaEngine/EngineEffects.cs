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
public sealed class EngineEffects
{
    /// <summary>
    /// Use per-pixel lighting
    /// </summary>
    public bool PerPixelLighting { get; set; }

    /// <summary>
    /// Use normal mapping
    /// </summary>
    public bool NormalMapping { get; set; }

    /// <summary>
    /// Use post-screen effects
    /// </summary>
    public bool PostScreenEffects { get; set; }

    /// <summary>
    /// Enable or disable shadowing casting (does not affect terrain self-shadowing)
    /// </summary>
    public bool Shadows { get; set; }

    /// <summary>
    /// Sample textures twice with different texture coordinates to create an illusion of more details
    /// </summary>
    public bool DetailMapping { get; set; }

    /// <summary>
    /// The effects to be display on water (e.g. reflections)
    /// </summary>
    public WaterEffectsType WaterEffects { get; set; }
}
