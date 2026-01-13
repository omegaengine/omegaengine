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
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.LightSources;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Graphics.Shaders;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics;

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
    /// Gets the effective light sources for a specific location.
    /// </summary>
    /// <param name="position">The position to get lighting information for.</param>
    /// <param name="radius">The additional search radius to use (usually bounding sphere radius).</param>
    internal EffectiveLighting GetEffectiveLights(DoubleVector3 position, float radius)
    {
        double maxDistance = 0;

        bool IsInRange(PointLight light)
        {
            double distance = (light.Position - position).Length();
            if (distance <= light.Range + radius)
            {
                maxDistance = Math.Max(maxDistance, distance);
                return true;
            }
            else return false;
        }

        var effectiveLights = new List<LightSource>(capacity: _directionalLights.Count + _pseudoDirectionalLights.Count + _pointLights.Count);
        effectiveLights.AddRange(_directionalLights);
        effectiveLights.AddRange(_pseudoDirectionalLights.Where(IsInRange).Select(light => light.AsDirectional(position)));
        effectiveLights.AddRange(_pointLights.Where(IsInRange));

        var shadowCasters = _positionables.Where(x => x.ShadowCaster);

        // If there are only point lights, only shadow casters closer than the most distant light source matter
        if (_directionalLights.Count == 0)
            shadowCasters = shadowCasters.Where(caster => (caster.Position - position).Length() <= maxDistance);

        return new(effectiveLights.ToArray(), shadowCasters.ToArray());
    }
    #endregion
}
