/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.LightSources;
using OmegaEngine.Graphics.Renderables;
using SlimDX;

namespace OmegaEngine.Graphics;

/// <summary>
/// Gets the effective light sources for a specific location (in range and potentially shadowed).
/// </summary>
/// <param name="boundingSphere">The position and optional radius in floating world space of the target being lit.</param>
/// <param name="shadowing">Whether to apply shadowing.</param>
/// <seealso cref="Scene.GetEffectiveLights"/>
public delegate IReadOnlyList<LightSource> GetEffectiveLights(BoundingSphere boundingSphere, bool shadowing);

/// <summary>
/// Represents a scene that can be viewed by a <see cref="Camera"/>.
/// </summary>
/// <remarks>Multiple <see cref="View"/>s can share one <see cref="Scene"/>.</remarks>
/// <seealso cref="View.Scene"/>
public sealed class Scene : EngineElement
{
    #region Properties
    private readonly EngineElementCollection<PositionableRenderable> _positionables = new();

    /// <summary>
    /// All <see cref="PositionableRenderable"/>s contained within this scene.
    /// </summary>
    /// <remarks>Will be disposed when <see cref="EngineElement.Dispose"/> is called.</remarks>
    public ICollection<PositionableRenderable> Positionables => _positionables;

    /// <summary>
    /// The current <see cref="Skybox"/> for this scene
    /// </summary>
    /// <remarks>Will be disposed when <see cref="EngineElement.Dispose"/> is called.</remarks>
    public Skybox? Skybox
    {
        get => _skybox;
        set
        {
            UnregisterChild(_skybox);
            RegisterChild(_skybox = value);
        }
    }

    // Order is not important, duplicate entries are not allowed
    private readonly HashSet<LightSource> _lights = [];
    private Skybox? _skybox;

    /// <summary>
    /// All light sources affecting the entities in this scene
    /// </summary>
    public ICollection<LightSource> Lights => _lights;
    #endregion

    #region Constructor
    public Scene()
    {
        RegisterChild(_positionables);
    }
    #endregion

    #region Get effective lighting
    /// <summary>
    /// Gets the effective light sources for a specific location (in range and potentially shadowed).
    /// </summary>
    /// <param name="boundingSphere">The position and optional radius in floating world space of the target being lit.</param>
    /// <param name="shadowing">Whether to apply shadowing.</param>
    internal IReadOnlyList<LightSource> GetEffectiveLights(BoundingSphere boundingSphere, bool shadowing)
    {
        var lights = GetLights(boundingSphere);
        if (shadowing && Engine.Effects.Shadows)
            ApplyShadows(lights, boundingSphere);
        return lights;
    }

    /// <summary>
    /// Gets the light sources that are in range of a specific location.
    /// </summary>
    /// <param name="boundingSphere">The position and optional radius in floating world space of the target being lit.</param>
    private List<LightSource> GetLights(BoundingSphere boundingSphere)
    {
        var lights = new List<LightSource>(capacity: _lights.Count);
        foreach (var light in _lights)
        {
            switch (light)
            {
                case DirectionalLight:
                    lights.Add(light);
                    break;
                case PointLight point when point.IsInRange(boundingSphere):
                    lights.Add(point.RenderAsDirectional ? point.AsDirectional(boundingSphere.Center) : point);
                    break;
            }
        }
        return lights;
    }

    /// <summary>
    /// Applies shadows to light sources.
    /// </summary>
    /// <param name="lights">The set of light source to be modified.</param>
    /// <param name="receiverSphere">The bounding sphere of the shadow receiver in world space.</param>
    private void ApplyShadows(List<LightSource> lights, BoundingSphere receiverSphere)
    {
        if (receiverSphere.Radius == 0) return;

        for (int i = 0; i < lights.Count; i++)
        {
            foreach (var positionable in _positionables)
            {
                if (positionable is { ShadowCaster: true, WorldBoundingSphere: { Radius: > 0.0001f } casterSphere } && casterSphere != receiverSphere)
                    lights[i] = lights[i].GetShadowed(receiverSphere, casterSphere);
            }
        }
    }
    #endregion
}
