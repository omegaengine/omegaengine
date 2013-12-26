/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Collections;
using Common.Values;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Graphics.Shaders;
using SlimDX;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics
{

    #region Delegates
    /// <summary>
    /// Returns an array of <see cref="LightSource"/>s effective for this position
    /// </summary>
    /// <param name="position">The position to check for effectiveness of light sources</param>
    /// <param name="radius">The additional search radius to use (usually bounding sphere radius)</param>
    /// <returns>An array of light sources, first all <see cref="DirectionalLight"/>s, then all <see cref="PointLight"/>s</returns>
    /// <seealso cref="Scene.GetEffectiveLights"/>
    internal delegate LightSource[] GetLights(DoubleVector3 position, float radius);
    #endregion

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
        private readonly List<DirectionalLight> _directionalLights = new List<DirectionalLight>();

        /// <summary>
        /// List of enabled (<see cref="LightSource.Enabled"/>) <see cref="PointLight"/>s to be treated like <see cref="DirectionalLight"/>s by <see cref="SurfaceShader"/>s
        /// </summary>
        /// <remarks>
        /// Subset of <see cref="Lights"/>.
        /// Cache for a single frame, used in <see cref="ActivateLights"/> and <see cref="GetEffectiveLights"/>
        /// </remarks>
        /// <seealso cref="PointLight.DirectionalForShader"/>
        private readonly List<PointLight> _pseudoDirectionalLights = new List<PointLight>();

        /// <summary>
        /// List of enabled (<see cref="LightSource.Enabled"/>) <see cref="PointLight"/>s
        /// </summary>
        /// <remarks>
        /// Subset of <see cref="Lights"/>.
        /// Cache for a single frame, used in <see cref="ActivateLights"/> and <see cref="GetEffectiveLights"/>
        /// </remarks>
        private readonly List<PointLight> _pointLights = new List<PointLight>();
        #endregion

        #region Properties
        private readonly EngineElementCollection<PositionableRenderable> _positionables = new EngineElementCollection<PositionableRenderable>();

        /// <summary>
        /// All <see cref="PositionableRenderable"/>s contained within this scene.
        /// </summary>
        public ICollection<PositionableRenderable> Positionables { get { return _positionables; } }

        /// <summary>
        /// The current <see cref="Skybox"/> for this scene
        /// </summary>
        public Skybox Skybox
        {
            get { return _skybox; }
            set
            {
                UnregisterChild(_skybox);
                RegisterChild(_skybox = value);
            }
        }

        // Order is not important, duplicate entries are not allowed
        private readonly C5.HashSet<LightSource> _lights = new C5.HashSet<LightSource>();
        private Skybox _skybox;

        /// <summary>
        /// All light sources affecting the entities in this scene
        /// </summary>
        public ICollection<LightSource> Lights { get { return _lights; } }
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
        /// <param name="view">The <see cref="View"/> used to render this <see cref="Scene"/></param>
        /// <remarks>Remember to call <see cref="DeactivateLights"/> when done</remarks>
        internal void ActivateLights(View view)
        {
            #region Sanity checks
            if (view == null) throw new ArgumentNullException("view");
            if (_dxLightCounter != 0) throw new InvalidOperationException(Resources.LightsNotDeactivated);
            #endregion

            var dispatcher = new PerTypeDispatcher<LightSource, Light>(false)
            {
                (DirectionalLight light) =>
                {
                    // Shader lighting
                    _directionalLights.Add(light);

                    // Fixed-function lighting
                    return new Light
                    {
                        Type = LightType.Directional, Direction = light.Direction,
                        Diffuse = light.Diffuse, Specular = light.Specular, Ambient = light.Ambient
                    };
                },
                (PointLight light) =>
                {
                    // Shader lighting
                    if (light.DirectionalForShader) _pseudoDirectionalLights.Add(light);
                    else _pointLights.Add(light);

                    // Fixed-function lighting
                    return new Light
                    {
                        Type = LightType.Point, Position = ((IPositionableOffset)light).EffectivePosition, Range = light.Range,
                        Attenuation0 = light.Attenuation.Constant, Attenuation1 = light.Attenuation.Linear, Attenuation2 = light.Attenuation.Quadratic,
                        Diffuse = light.Diffuse, Specular = light.Specular, Ambient = light.Ambient
                    };
                },
            };

            foreach (var dxLight in _lights.Where(light => light.Enabled).Select(dispatcher.Dispatch))
            {
                if (_dxLightCounter < Engine.Device.Capabilities.MaxActiveLights)
                {
                    Engine.Device.SetLight(_dxLightCounter, dxLight);
                    Engine.Device.EnableLight(_dxLightCounter, true);
                    _dxLightCounter++;
                }
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

            #region Fixed-function lighting
            for (int i = 0; i < _dxLightCounter; i++)
                Engine.Device.EnableLight(i, false);

            _dxLightCounter = 0;
            #endregion
        }
        #endregion

        #region Get effective lights
        /// <summary>
        /// Returns an array of <see cref="LightSource"/>s effective for this position
        /// </summary>
        /// <param name="position">The position to check for effectiveness of light sources</param>
        /// <param name="radius">The additional search radius to use (usually bounding sphere radius)</param>
        /// <returns>An array of light sources, first all <see cref="DirectionalLight"/>s, then all <see cref="PointLight"/>s</returns>
        internal LightSource[] GetEffectiveLights(DoubleVector3 position, float radius)
        {
            // List for accumulating effective light sources
            var effectiveLights = new List<LightSource>(_directionalLights.Count + _pointLights.Count); // Use upper bound for list capacity

            // Copy all directional lights
            _directionalLights.ForEach(effectiveLights.Add);

            effectiveLights.AddRange((from light in _pseudoDirectionalLights
                // Filter out lights that are too far away
                where (light.Position - position).Length() <= light.Range + radius
                // Convert pseudo-directional point lights to real directional lights
                select new DirectionalLight
                {
                    Name = light.Name, Direction = (Vector3)(position - light.Position), Diffuse = light.Diffuse, Specular = light.Specular, Ambient = light.Ambient,
                }).Cast<LightSource>());

            _pointLights.ForEach(light =>
            {
                // Filter out lights that are too far away
                if ((light.Position - position).Length() <= light.Range + radius)
                    effectiveLights.Add(light);
            });

            return effectiveLights.ToArray();
        }
        #endregion
    }
}
