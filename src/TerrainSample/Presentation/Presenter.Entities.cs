/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Common.Utils;
using Common.Values;
using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Renderables;
using SlimDX;
using TemplateWorld.EntityComponents;
using TemplateWorld.Positionables;
using TerrainSample.World.Positionables;
using CpuParticleSystem = TemplateWorld.EntityComponents.CpuParticleSystem;
using GpuParticleSystem = TemplateWorld.EntityComponents.GpuParticleSystem;
using LightSource = TemplateWorld.EntityComponents.LightSource;
using Terrain = OmegaEngine.Graphics.Renderables.Terrain;
using ViewType = OmegaEngine.Graphics.Renderables.ViewType;
using Water = OmegaEngine.Graphics.Renderables.Water;

namespace TerrainSample.Presentation
{
    /*
     * This file provides helper methods for visualizing World.Positionables.
     * 
     * The method AddPositionable() is to be called once for every new World.Positionable<Vector2> in the Universe.
     * It internally uses either AddEntity() or AddWater() to create the actual visual representations in OmegaEngine.
     * 
     * AddEntity() creates one or multiple Engine.PositionableRenderables for a single World.Entity, based on the RenderControls the Entity lists.
     * _worldToEngine is maintained to allow forward lookups, which are needed for updating and removing.
     * 
     * AddWater() creates exactly one Engine.Water for a World.Water.
     * _worldToEngineWater is maintained to allow forward lookups, which are needed for updating and removing.
     * 
     * _engineToWorld is maintained to allow reverse lookups, which are needed for mouse picking.
     * 
     * The method RemovePositionable() is to be called once for every World.Positionable<Vector2> removed from the Universe.
     * It internally uses RemoveEntity() or RemoveWater() to remove the visual representations from OmegaEngine.
     * 
     * The method UpdatePositionable() is automatically called every time a World.Positionable's position or other rendering-relevant property has changed.
     * The corresponding hook is set up by AddPositionable().
     * Additional hooks are also set up that call RemoveEntity()/RemoveWater() and AddEntity()/AddWater() when changes are made that require an entirely new rendering object (new entity template or different plane size).
     */

    partial class Presenter
    {
        #region Variables
        /// <summary>1:1 association of <see cref="RenderControl"/> to <see cref="IPositionableOffset"/> (usually <see cref="PositionableRenderable"/>.</summary>
        private readonly Dictionary<RenderControl, IPositionable> _worldToEngine = new Dictionary<RenderControl, IPositionable>();

        /// <summary>1:1 association of <see cref="TemplateWorld.Positionables.Water"/> to <see cref="OmegaEngine.Graphics.Renderables.Water"/>.</summary>
        private readonly Dictionary<TemplateWorld.Positionables.Water, Water> _worldToEngineWater = new Dictionary<TemplateWorld.Positionables.Water, Water>();

        /// <summary>n:1 association of <see cref="PositionableRenderable"/> to <see cref="Positionable{TCoordinates}"/>.</summary>
        private readonly Dictionary<PositionableRenderable, Positionable<Vector2>> _engineToWorld = new Dictionary<PositionableRenderable, Positionable<Vector2>>();
        #endregion

        #region Properties
        /// <summary>
        /// Returns a list of all <see cref="PositionableRenderable"/>s associated with elements in <see cref="Universe"/>.
        /// </summary>
        protected IEnumerable<PositionableRenderable> PositionableRenderables { get { return _worldToEngine.Values.Where(positionable => !(positionable is Terrain)).OfType<PositionableRenderable>().ToList(); } }
        #endregion

        //--------------------//

        #region Dictionary access
        /// <summary>
        /// Returns the <see cref="IPositionable"/> that was created for a <see cref="RenderControl"/>.
        /// </summary>
        protected IPositionable GetEngine(RenderControl renderControl)
        {
            #region Sanity checks
            if (Disposed) throw new ObjectDisposedException(ToString());
            #endregion

            return _worldToEngine.ContainsKey(renderControl) ? _worldToEngine[renderControl] : null;
        }

        /// <summary>
        /// Returns the <see cref="Positionable{TCoordinates}"/> a <see cref="PositionableRenderable"/> was created from or <see langword="null"/>.
        /// </summary>
        protected Positionable<Vector2> GetWorld(PositionableRenderable engineEntity)
        {
            #region Sanity checks
            if (Disposed) throw new ObjectDisposedException(ToString());
            #endregion

            return _engineToWorld.ContainsKey(engineEntity) ? _engineToWorld[engineEntity] : null;
        }
        #endregion

        //--------------------//

        #region Add positionables
        /// <summary>
        /// Sets up a <see cref="Positionable{TCoordinates}"/> for rendering via a <see cref="OmegaEngine.Graphics.Renderables.PositionableRenderable"/>.
        /// </summary>
        /// <param name="positionable">The <see cref="Positionable{TCoordinates}"/> to be displayed.</param>
        /// <exception cref="FileNotFoundException">Thrown if a required <see cref="Asset"/> file could not be found.</exception>
        /// <exception cref="IOException">Thrown if there was an error reading an <see cref="Asset"/> file.</exception>
        /// <exception cref="InvalidDataException">Thrown if an <see cref="Asset"/> file contains invalid data.</exception>
        /// <remarks>Calls <see cref="AddEntity"/>.</remarks>
        protected virtual void AddPositionable(Positionable<Vector2> positionable)
        {
            #region Sanity checks
            if (positionable == null) throw new ArgumentNullException("positionable");
            if (Disposed) throw new ObjectDisposedException(ToString());
            #endregion

            #region Entity
            var entity = positionable as Entity;
            if (entity != null)
            {
                AddEntity(entity);

                // Remove and then re-add entity to apply the new entity template
                entity.TemplateChanging += RemoveEntity;
                entity.TemplateChanged += AddEntity;
            }
                #endregion

            else
            {
                #region Water
                var water = positionable as TemplateWorld.Positionables.Water;
                if (water != null)
                {
                    AddWater(water);

                    // Remove and then re-add water to apply size changes
                    water.SizeChanged += RemoveWater;
                    water.SizeChanged += AddWater;
                }
                #endregion
            }

            // Keep entity and model in sync via events
            positionable.RenderPropertyChanged += UpdatePositionable;
        }

        /// <summary>
        /// Sets up a <see cref="EntityBase{TSelf,TCoordinates,TTemplate}"/> for rendering via a <see cref="PositionableRenderable"/>
        /// </summary>
        /// <param name="entity">The <see cref="EntityBase{TSelf,TCoordinates,TTemplate}"/> to be displayed</param>
        /// <remarks>This is a helper method for <see cref="AddPositionable"/>.</remarks>
        private void AddEntity(Entity entity)
        {
            try
            {
                foreach (var renderControl in entity.TemplateData.RenderControls)
                    AddEntityHelper(entity, renderControl);
            }
                #region Error handling
            catch (Exception)
            {
                // Clean up any already created controls
                foreach (var renderControl in entity.TemplateData.RenderControls)
                    RemoveEntityHelper(renderControl);

                // Pass all other exceptions along
                throw;
            }
            #endregion

            // Set up initial position and rotation
            UpdatePositionable(entity);
        }

        /// <summary>
        /// Sets up a <see cref="TemplateWorld.Positionables.Water"/> for rendering via a <see cref="Water"/>
        /// </summary>
        /// <param name="water">The <see cref="TemplateWorld.Positionables.Water"/> to be displayed</param>
        private void AddWater(TemplateWorld.Positionables.Water water)
        {
            // Create an engine representation of the water plane
            var engineWater = new Water(Engine, new SizeF(water.Size.X, water.Size.Y))
            {
                Name = water.Name,
                Position = water.EnginePosition
            };

            Scene.Positionables.Add(engineWater);
            _engineToWorld.Add(engineWater, water);
            _worldToEngineWater.Add(water, engineWater);

            // Set up initial position and rotation
            UpdatePositionable(water);

            // Insert reflection and refraction views into render queue before main view
            engineWater.SetupChildViews(View);
        }
        #endregion

        #region Remove positionables
        /// <summary>
        /// Removes a <see cref="OmegaEngine.Graphics.Renderables.PositionableRenderable"/> used to render a <see cref="Positionable{TCoordinates}"/>
        /// </summary>
        /// <param name="positionable">The <see cref="Positionable{TCoordinates}"/> to be removed</param>
        protected virtual void RemovePositionable(Positionable<Vector2> positionable)
        {
            #region Sanity checks
            if (positionable == null) throw new ArgumentNullException("positionable");
            if (Disposed) throw new ObjectDisposedException(ToString());
            #endregion

            #region Entity
            var entity = positionable as Entity;
            if (entity != null)
            {
                RemoveEntity(entity);

                // Unhook class update event
                entity.TemplateChanging -= RemoveEntity;
                entity.TemplateChanged -= AddEntity;
            }
                #endregion

            else
            {
                #region Water
                var water = positionable as TemplateWorld.Positionables.Water;
                if (water != null)
                {
                    RemoveWater(water);

                    // Unhook size update event
                    water.SizeChanged -= RemoveWater;
                    water.SizeChanged -= AddWater;
                }
                #endregion
            }

            // Unhook sync event
            positionable.RenderPropertyChanged -= UpdatePositionable;
        }

        /// <summary>
        /// Removes a <see cref="OmegaEngine.Graphics.Renderables.PositionableRenderable"/> used to render a <see cref="Positionable{TCoordinates}"/>
        /// </summary>
        /// <param name="entity">The <see cref="Positionable{TCoordinates}"/> to be removed</param>
        /// <remarks>This is a helper method for <see cref="RemovePositionable"/>.</remarks>
        private void RemoveEntity(Entity entity)
        {
            foreach (var renderControl in entity.TemplateData.RenderControls)
                RemoveEntityHelper(renderControl);
        }

        /// <summary>
        /// Removes a <see cref="OmegaEngine.Graphics.Renderables.Water"/> used to render a <see cref="TemplateWorld.Positionables.Water"/>
        /// </summary>
        /// <param name="water">The <see cref="TemplateWorld.Positionables.Water"/> to be removed</param>
        private void RemoveWater(TemplateWorld.Positionables.Water water)
        {
            Water engineWater = _worldToEngineWater[water];
            Scene.Positionables.Remove(engineWater);
            engineWater.Dispose();
            _engineToWorld.Remove(engineWater);
            _worldToEngineWater.Remove(water);
        }
        #endregion

        #region Update positionables
        /// <summary>
        /// Updates the position and other properties of one or more <see cref="OmegaEngine.Graphics.Renderables.PositionableRenderable"/>s based on data from a <see cref="Positionable{TCoordinates}"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the entity's coordinates lie outside the range of the terrain and this application does not have "Editor" in its name.</exception>
        /// <param name="positionable">The <see cref="Positionable{TCoordinates}"/> to update.</param>
        protected virtual void UpdatePositionable(Positionable<Vector2> positionable)
        {
            #region Sanity checks
            if (positionable == null) throw new ArgumentNullException("positionable");
            if (Disposed) throw new ObjectDisposedException(ToString());
            #endregion

            // Determine the entity's position on the terrain
            DoubleVector3 terrainPoint;
            try
            {
                terrainPoint = Universe.Terrain.ToEngineCoords(positionable.Position);
            }
                #region Error handling
            catch (ArgumentOutOfRangeException ex)
            {
                // Wrap exception since only data exceptions are handled by the ditor
                throw new InvalidDataException(ex.Message, ex);
            }
            #endregion

            #region Entity
            var entity = positionable as Entity;
            if (entity != null)
            {
                foreach (var renderControl in entity.TemplateData.RenderControls)
                    UpdateEntityHelper(renderControl, terrainPoint, entity.Rotation);
            }
                #endregion

            else
            {
                #region Water
                var water = positionable as TemplateWorld.Positionables.Water;
                if (water != null)
                    _worldToEngineWater[water].Position = Terrain.Position + water.EnginePosition;
                #endregion
            }
        }
        #endregion

        //--------------------//

        #region Add entity helper
        /// <summary>
        /// Adds a graphical representation to the engine, <see cref="_worldToEngine"/> and <see cref="_engineToWorld"/>.
        /// </summary>
        /// <param name="entity">The <see cref="Positionable{TCoordinates}"/> <paramref name="renderControl"/> is associated with.</param>
        /// <param name="renderControl">The <see cref="RenderControl"/> to be displayed.</param>
        /// <exception cref="FileNotFoundException">Thrown if a required <see cref="Asset"/> file could not be found.</exception>
        /// <exception cref="IOException">Thrown if there was an error reading an <see cref="Asset"/> file.</exception>
        /// <exception cref="InvalidDataException">Thrown if an <see cref="Asset"/> file contains invalid data.</exception>
        private void AddEntityHelper(Entity entity, RenderControl renderControl)
        {
            // ----- Apply RenderControl.Shift directly with PreTransform ----- //

            #region Test sphere
            var sphereRender = renderControl as TestSphere;
            if (sphereRender != null)
            {
                // Load the model based on the entity name
                var model = Model.Sphere(Engine, XTexture.Get(Engine, sphereRender.Texture), sphereRender.Radius, sphereRender.Slices, sphereRender.Stacks);
                model.Name = entity.Name;

                // Set model properties
                model.PreTransform = Matrix.Translation(sphereRender.Shift);
                model.Alpha = sphereRender.Alpha;
                if (Lighting) model.SurfaceEffect = SurfaceEffect.Shader;
                model.Wireframe = WireframeEntities;
                model.DrawBoundingSphere = BoundingSphereEntities;
                model.DrawBoundingBox = BoundingBoxEntities;

                // Add objects to lists
                Scene.Positionables.Add(model);
                _engineToWorld.Add(model, entity);
                _worldToEngine.Add(renderControl, model);
                return;
            }
            #endregion

            #region Mesh
            var meshRender = renderControl as Mesh;
            if (meshRender != null && !string.IsNullOrEmpty(meshRender.Filename))
            {
                // Load the model based on the entity name (differentiate between static and animated meshes)
                PositionableRenderable model;
                if (meshRender is StaticMesh) model = Model.FromAsset(Engine, meshRender.Filename);
                else if (meshRender is AnimatedMesh) model = AnimatedModel.FromAsset(Engine, meshRender.Filename);
                else throw new InvalidOperationException();
                model.Name = entity.Name;

                // Set model properties
                model.PreTransform = Matrix.Scaling(meshRender.Scale, meshRender.Scale, meshRender.Scale) *
                                     Matrix.RotationYawPitchRoll(
                                         meshRender.RotationY.DegreeToRadian(),
                                         meshRender.RotationX.DegreeToRadian(),
                                         meshRender.RotationZ.DegreeToRadian()) *
                                     Matrix.Translation(meshRender.Shift);
                model.Alpha = meshRender.Alpha;
                model.Pickable = meshRender.Pickable;
                model.RenderIn = (ViewType)meshRender.RenderIn;
                if (Lighting) model.SurfaceEffect = SurfaceEffect.Shader;
                model.Wireframe = WireframeEntities;
                model.DrawBoundingSphere = BoundingSphereEntities;
                model.DrawBoundingBox = BoundingBoxEntities;

                // Add objects to lists
                Scene.Positionables.Add(model);
                _engineToWorld.Add(model, entity);
                _worldToEngine.Add(renderControl, model);
                return;
            }
            #endregion

            // ----- Don't apply RenderControl.Shift yet ----- //

            #region ParticleSystem
            var particleRender = renderControl as ParticleSystem;
            if (particleRender != null && !string.IsNullOrEmpty(particleRender.Filename))
            {
                PositionableRenderable particleSystem;

                if (particleRender is CpuParticleSystem)
                    particleSystem = OmegaEngine.Graphics.Renderables.CpuParticleSystem.FromPreset(Engine, particleRender.Filename);
                else if (particleRender is GpuParticleSystem)
                    particleSystem = OmegaEngine.Graphics.Renderables.GpuParticleSystem.FromPreset(Engine, particleRender.Filename);
                else return;

                particleSystem.Name = entity.Name;
                particleSystem.VisibilityDistance = particleRender.VisibilityDistance;
                particleSystem.Wireframe = WireframeEntities;
                particleSystem.DrawBoundingSphere = BoundingSphereEntities;
                particleSystem.DrawBoundingBox = BoundingBoxEntities;

                // Add objects to lists
                Scene.Positionables.Add(particleSystem);
                _engineToWorld.Add(particleSystem, entity);
                _worldToEngine.Add(renderControl, particleSystem);
                return;
            }
            #endregion

            #region LightSource
            if (!Lighting) return;
            var lightInfo = renderControl as LightSource;
            if (lightInfo != null)
            {
                var light = new PointLight
                {
                    Name = entity.Name,
                    Range = lightInfo.Range,
                    Attenuation = lightInfo.Attenuation,
                    Diffuse = lightInfo.Color
                };

                // Add objects to lists
                Scene.Lights.Add(light);
                _worldToEngine.Add(lightInfo, light);
            }
            #endregion
        }
        #endregion

        #region Update entity helper
        /// <summary>
        /// Updates a specific <see cref="RenderControl"/> representation in the engine
        /// </summary>
        /// <param name="renderControl">The <see cref="RenderControl"/> to be updated</param>
        /// <param name="terrainPoint">The location of the associated <see cref="Positionable{TCoordinates}"/> on the terrain</param>
        /// <param name="dirRotation">The rotation of the associated <see cref="Positionable{TCoordinates}"/> on the terrain</param>
        private void UpdateEntityHelper(RenderControl renderControl, DoubleVector3 terrainPoint, float dirRotation)
        {
            var render = GetEngine(renderControl);
            if (render == null) return;

            var rotation = Quaternion.RotationYawPitchRoll(dirRotation.DegreeToRadian(), 0, 0);

            // ----- RenderControl.Shift already applied ----- //

            #region Mesh / TestSphere
            var meshRender = renderControl as Mesh;
            if ((meshRender != null && !string.IsNullOrEmpty(meshRender.Filename)) || renderControl is TestSphere)
            {
                // Cast to entity to be able to access Rotation in addition to Position
                var renderable = (PositionableRenderable)render;
                renderable.Position = Terrain.Position + terrainPoint;
                renderable.Rotation = rotation;
                return;
            }
            #endregion

            // ----- Apply RenderControl.Shift directly to position ----- //

            // Calculate the rotated position shift
            var rotatedShift = Vector3.TransformCoordinate(
                renderControl.Shift, Matrix.RotationQuaternion(rotation));

            #region ParticleSystem
            if (renderControl is ParticleSystem)
            {
                render.Position = Terrain.Position + terrainPoint + rotatedShift;
                return;
            }
            #endregion

            #region LightSource
            if (renderControl is LightSource)
                render.Position = Terrain.Position + terrainPoint + rotatedShift;
            #endregion
        }
        #endregion

        #region Remove entity helper
        /// <summary>
        /// Removes a graphical representation from the engine, <see cref="_worldToEngine"/> and <see cref="_engineToWorld"/>
        /// </summary>
        /// <param name="renderControl">The <see cref="RenderControl"/> to be </param>
        private void RemoveEntityHelper(RenderControl renderControl)
        {
            var render = GetEngine(renderControl);
            if (render == null) return;

            var renderable = render as PositionableRenderable;
            if (renderable != null)
            {
                Scene.Positionables.Remove(renderable);
                _engineToWorld.Remove(renderable);
                renderable.Dispose();
            }

            var light = render as PointLight;
            if (light != null) Scene.Lights.Remove(light);

            _worldToEngine.Remove(renderControl);
        }
        #endregion
    }
}
