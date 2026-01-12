/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Diagnostics.Contracts;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics.Renderables;
using SlimDX;

namespace OmegaEngine.Graphics.LightSources;

/// <summary>
/// Lighting information effective for a given position.
/// </summary>
/// <param name="LightSources">Light sources effective for this position.</param>
/// <param name="ShadowCasters">Potential shadow casters for this position.</param>
public readonly record struct EffectiveLighting(LightSource[] LightSources, PositionableRenderable[] ShadowCasters)
{
    /// <summary>
    /// No lighting information.
    /// </summary>
    public EffectiveLighting() : this([], []) {}

    /// <summary>
    /// Applies shadows to light sources based on shadow casters.
    /// </summary>
    /// <param name="receiverSphere">The bounding sphere of the of the renderable receiving shadows.</param>
    /// <returns>Modified light sources with shadows applied, or the original light sources if no shadows apply.</returns>
    [Pure]
    public LightSource[] GetShadowedLightSources(BoundingSphere receiverSphere)
    {
        var modifiedLights = new LightSource[LightSources.Length];
        for (int i = 0; i < LightSources.Length; i++)
        {
            var lightSource = LightSources[i];
            foreach (var caster in ShadowCasters)
            {
                if (caster.WorldBoundingSphere is { Radius: > 0.0001f } casterSphere && casterSphere != receiverSphere)
                    lightSource = lightSource.GetShadowed(receiverSphere, casterSphere);
            }
            modifiedLights[i] = lightSource;
        }
        return modifiedLights;
    }
}

/// <summary>
/// Returns <see cref="EffectiveLighting"/> information effective for a given position.
/// </summary>
/// <param name="position">The position to get lighting information for.</param>
/// <param name="radius">The additional search radius to use (usually bounding sphere radius).</param>
/// <seealso cref="Scene.GetEffectiveLighting"/>
internal delegate EffectiveLighting GetEffectiveLighting(DoubleVector3 position, float radius);
