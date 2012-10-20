/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common;
using Common.Values;
using OmegaEngine.Graphics.Cameras;
using SlimDX;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Graphics.Shaders;
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
    public sealed class Scene : IDisposable
    {
        #region Variables
        private readonly Engine _engine;

        /// <summary>Number of fixed-function light sources used so far</summary>
        private int _dxLightCounter;

        #region Cache lists
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

        #endregion

        #region Properties
        /// <summary>
        /// Has this scene been disposed?
        /// </summary>
        [Browsable(false)]
        public bool Disposed { get; private set; }

        // Order is not important, duplicate entries are not allowed
        private readonly C5.ICollection<PositionableRenderable> _positionables = new C5.HashSet<PositionableRenderable>();

        /// <summary>
        /// All <see cref="PositionableRenderable"/>s contained within this scene.
        /// </summary>
        /// <remarks>All contained elements will automatically be disposed when <see cref="Dispose"/> is called.</remarks>
        public ICollection<PositionableRenderable> Positionables { get { return _positionables; } }

        /// <summary>
        /// The current <see cref="Skybox"/> for this scene
        /// </summary>
        public Skybox Skybox { get; set; }

        // Order is not important, duplicate entries are not allowed
        private readonly C5.ICollection<LightSource> _lights = new C5.HashSet<LightSource>();

        /// <summary>
        /// All light sources affecting the entities in this scene
        /// </summary>
        public ICollection<LightSource> Lights { get { return _lights; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new scene for the engine
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to render the scene with</param>
        public Scene(Engine engine)
        {
            _engine = engine;
        }
        #endregion

        //--------------------//

        #region Access
        /// <summary>
        /// Retrieves a specific <see cref="PositionableRenderable"/>
        /// </summary>
        /// <param name="name">The name of the body to retrieve</param>
        /// <exception cref="ArgumentException">Thrown if the body was not found</exception>
        public PositionableRenderable GetBody(string name)
        {
            foreach (PositionableRenderable body in _positionables)
            {
                if (body.Name == name)
                    return body;
            }
            throw new ArgumentException(Resources.NoSuchBody + name, "name");
        }

        /// <summary>
        /// Retrieves a specific <see cref="LightSource"/>
        /// </summary>
        /// <param name="name">The name of the light source to retrieve</param>
        /// <exception cref="ArgumentException">Thrown if the light source was not found</exception>
        public LightSource GetLight(string name)
        {
            foreach (LightSource light in _lights)
            {
                if (light.Name == name)
                    return light;
            }
            throw new ArgumentException(Resources.NoSuchLightSource + name, "name");
        }
        #endregion

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

            foreach (LightSource light in _lights)
            {
                // Skip disabled lights
                if (!light.Enabled) continue;

                // Fixed-function lighting
                var dxLight = new Light {Diffuse = light.Diffuse, Specular = light.Specular, Ambient = light.Ambient};

                var directionalLight = light as DirectionalLight;
                if (directionalLight != null)
                { // Handle directional lights
                    // Shader lighting
                    _directionalLights.Add(directionalLight);

                    // Fixed-function lighting
                    dxLight.Type = LightType.Directional;
                    dxLight.Direction = directionalLight.Direction;
                }

                else
                {
                    var pointLight = light as PointLight;
                    if (pointLight != null)
                    { // Handle point lights
                        view.ApplyCameraBase(pointLight);

                        // Shader lighting
                        // Place pseudo-directional lights into a different queue, to optimize sorting for shaders
                        if (pointLight.DirectionalForShader) _pseudoDirectionalLights.Add(pointLight);
                        else _pointLights.Add(pointLight);

                        // Fixed-function lighting
                        dxLight.Type = LightType.Point;
                        dxLight.Position = ((IPositionableOffset)pointLight).EffectivePosition;
                        dxLight.Range = pointLight.Range;
                        dxLight.Attenuation0 = pointLight.Attenuation.Constant;
                        dxLight.Attenuation1 = pointLight.Attenuation.Linear;
                        dxLight.Attenuation2 = pointLight.Attenuation.Quadratic;
                    }
                }

                // Only use entry for fixed-function lighting if we haven't reached the hardware limit yet
                if (_dxLightCounter < _engine.Device.Capabilities.MaxActiveLights)
                {
                    _engine.Device.SetLight(_dxLightCounter, dxLight);
                    _engine.Device.EnableLight(_dxLightCounter, true);
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
                _engine.Device.EnableLight(i, false);

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

        //--------------------//

        #region Dispose
        /// <summary>
        /// Disposes <see cref="Positionables"/> and <see cref="Skybox"/>
        /// </summary>
        public void Dispose()
        {
            if (Disposed || _engine == null || _engine.Disposed) return; // Don't try to dispose more than once

            Log.Info("Disposing scene");
            _positionables.Apply(body => body.Dispose());
            if (Skybox != null) Skybox.Dispose();

            Disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Only for debugging, not present in Release code")]
        ~Scene()
        {
            // This block will only be executed on Garbage Collection, not by manual disposal
            Log.Error("Forgot to call Dispose on " + this);
#if DEBUG
            throw new InvalidOperationException("Forgot to call Dispose on " + this);
#endif
        }
        #endregion
    }
}
