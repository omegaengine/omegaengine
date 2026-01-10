/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using OmegaEngine.Foundation.Geometry;

namespace OmegaEngine.Graphics.LightSources;

/// <summary>
/// Lighting information effective for a given position.
/// </summary>
/// <param name="LightSources">Light sources effective for this position.</param>
public readonly record struct EffectiveLighting(LightSource[] LightSources)
{
    /// <summary>
    /// No lighting information.
    /// </summary>
    public EffectiveLighting() : this([]) {}
}

/// <summary>
/// Returns <see cref="EffectiveLighting"/> information effective for a given position.
/// </summary>
/// <param name="position">The position to get lighting information for.</param>
/// <param name="radius">The additional search radius to use (usually bounding sphere radius).</param>
/// <seealso cref="Scene.GetEffectiveLighting"/>
internal delegate EffectiveLighting GetEffectiveLighting(DoubleVector3 position, float radius);
