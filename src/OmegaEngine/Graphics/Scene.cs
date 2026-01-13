/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.LightSources;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Graphics.Shaders;
using SlimDX;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics;

/// <summary>
/// Gets the effective light sources for a specific location (in range and potentially shadowed).
/// </summary>
/// <param name="boundingSphere">The position and optional radius in floating world space of the target being lit.</param>
/// <param name="shadowing">Whether to apply shadowing.</param>
/// <seealso cref="Scene.GetEffectiveLights"/>
public delegate LightSource[] GetEffectiveLights(BoundingSphere boundingSphere, bool shadowing);

/// <summary>
/// Represents a scene that can be viewed by a <see cref="Camera"/>.
/// </summary>
/// <remarks>Multiple <see cref="View"/>s can share one <see cref="Scene"/>.</remarks>
/// <seealso cref="View.Scene"/>
public sealed class Scene : EngineElement
{
    #region Variables
    /// <summary>Number of fixed-function light sources used so far</summary>
    private int _dxLightCounter;

    // Note: Using Lists here, because the size of the internal arrays will auto-optimize after a few frames

    /// <summary>
    /// List of enabled (<see cref="LightSource.Enabled"/>) <see cref="DirectionalLight"/>s
    /// </summary>
    /// <remarks>
    /// Subset of <see cref="Lights"/>.
    /// Cache for a single frame, used in <see cref="ActivateLights"/> and <see cref="GetEffectiveLights"/>
    /// </remarks>
    private readonly List<DirectionalLight> _directionalLights = [];

    /// <summary>
    /// List of enabled (<see cref="LightSource.Enabled"/>) <see cref="PointLight"/>s to be treated like <see cref="DirectionalLight"/>s by <see cref="SurfaceShader"/>s
    /// </summary>
    /// <remarks>
    /// Subset of <see cref="Lights"/>.
    /// Cache for a single frame, used in <see cref="ActivateLights"/> and <see cref="GetEffectiveLights"/>
    /// </remarks>
    /// <seealso cref="PointLight.RenderAsDirectional"/>
    private readonly List<PointLight> _pseudoDirectionalLights = [];

    /// <summary>
    /// List of enabled (<see cref="LightSource.Enabled"/>) <see cref="PointLight"/>s
    /// </summary>
    /// <remarks>
    /// Subset of <see cref="Lights"/>.
    /// Cache for a single frame, used in <see cref="ActivateLights"/> and <see cref="GetEffectiveLights"/>
    /// </remarks>
    private readonly List<PointLight> _pointLights = [];
    #endregion

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

    //--------------------//

    #region Activate lights
    /// <summary>
    /// Must be called before rendering this scene or calling <see cref="GetEffectiveLights"/>
    /// </summary>
    /// <remarks>Remember to call <see cref="DeactivateLights"/> when done</remarks>
    internal void ActivateLights()
    {
        #region Sanity checks
        if (_dxLightCounter != 0) throw new InvalidOperationException(Resources.LightsNotDeactivated);
        #endregion

        foreach (var lightSource in _lights.Where(x => x.Enabled))
        {
            if (_dxLightCounter < Engine.Device.Capabilities.MaxActiveLights)
            {
                Engine.Device.SetLight(_dxLightCounter, BuildLight(lightSource));
                Engine.Device.EnableLight(_dxLightCounter, true);
                _dxLightCounter++;
            }
        }
    }

    private Light BuildLight(LightSource source)
    {
        switch (source)
        {
            case DirectionalLight directional:
                // Shader lighting
                _directionalLights.Add(directional);

                // Fixed-function lighting
                return new()
                {
                    Type = LightType.Directional, Direction = directional.Direction,
                    Diffuse = directional.Diffuse, Specular = directional.Specular, Ambient = directional.Ambient
                };

            case PointLight point:
                // Shader lighting
                if (point.RenderAsDirectional) _pseudoDirectionalLights.Add(point);
                else _pointLights.Add(point);

                // Fixed-function lighting
                return new()
                {
                    Type = LightType.Point, Position = point.GetFloatingPosition(), Range = point.Range,
                    Attenuation0 = point.Attenuation.Constant, Attenuation1 = point.Attenuation.Linear, Attenuation2 = point.Attenuation.Quadratic,
                    Diffuse = point.Diffuse, Specular = point.Specular, Ambient = point.Ambient
                };

            default:
                throw new NotSupportedException($"Unknown light source type {source.GetType().Name}.");
        }
    }
    #endregion

    #region Deactivate lights
    /// <summary>
    /// To be called after rendering is done - the counterpart to <see cref="ActivateLights"/>
    /// </summary>
    internal void DeactivateLights()
    {
        _pointLights.Clear();
        _directionalLights.Clear();
        _pseudoDirectionalLights.Clear();

        #region Fixed-function lighting
        for (int i = 0; i < _dxLightCounter; i++)
            Engine.Device.EnableLight(i, false);

        _dxLightCounter = 0;
        #endregion
    }
    #endregion

    #region Get effective lighting
    /// <summary>
    /// Gets the effective light sources for a specific location (in range and potentially shadowed).
    /// </summary>
    /// <param name="boundingSphere">The position and optional radius in floating world space of the target being lit.</param>
    /// <param name="shadowing">Whether to apply shadowing.</param>
    internal LightSource[] GetEffectiveLights(BoundingSphere boundingSphere, bool shadowing)
    {
        var lights = GetLights(boundingSphere, out float maxLightDistance);

        if (shadowing && boundingSphere.Radius > 0)
        {
            var shadowCasters = _positionables.Where(x => x.ShadowCaster);
            if (!float.IsPositiveInfinity(maxLightDistance))
                shadowCasters = shadowCasters.Where(caster => (caster.GetFloatingPosition() - boundingSphere.Center).Length() <= maxLightDistance);

            ApplyShadows(lights, boundingSphere, shadowCasters.ToList());
        }

        return lights;
    }

    /// <summary>
    /// Gets the light sources that are in range of a specific location.
    /// </summary>
    /// <param name="boundingSphere">The position and optional radius in floating world space of the target being lit.</param>
    /// <param name="maxLightDistance">The maximum distance of any returned light source from the specified location.</param>
    private LightSource[] GetLights(BoundingSphere boundingSphere, out float maxLightDistance)
    {
        var lights = new List<LightSource>(capacity: _directionalLights.Count + _pseudoDirectionalLights.Count + _pointLights.Count);

        lights.AddRange(_directionalLights);

        float maxPointLightDistance = 0;
        lights.AddRange(_pseudoDirectionalLights.Where(IsInRange).Select(light => light.AsDirectional(boundingSphere.Center)));
        lights.AddRange(_pointLights.Where(IsInRange));

        bool IsInRange(PointLight light)
        {
            float distance = (light.GetFloatingPosition() - boundingSphere.Center).Length();
            if (distance <= light.Range + boundingSphere.Radius)
            {
                maxPointLightDistance = Math.Max(maxPointLightDistance, distance);
                return true;
            }
            else return false;
        }

        maxLightDistance = _directionalLights.Count > 0 ? float.PositiveInfinity : maxPointLightDistance;
        return lights.ToArray();
    }

    /// <summary>
    /// Applies shadows to light sources.
    /// </summary>
    /// <param name="lights">The set of light source to be modified.</param>
    /// <param name="receiverSphere">The bounding sphere of the shadow receiver in world space.</param>
    /// <param name="casters">The potential shadow casters.</param>
    private static void ApplyShadows(LightSource[] lights, BoundingSphere receiverSphere, IReadOnlyList<PositionableRenderable> casters)
    {
        for (int i = 0; i < lights.Length; i++)
        {
            foreach (var caster in casters)
            {
                if (caster.WorldBoundingSphere is { Radius: > 0.0001f } casterSphere && casterSphere != receiverSphere)
                    lights[i] = lights[i].GetShadowed(receiverSphere, casterSphere);
            }
        }
    }
    #endregion
}
