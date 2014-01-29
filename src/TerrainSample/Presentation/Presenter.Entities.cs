/*
 * Copyright 2006-2014 Bastian Eicher
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
using AlphaFramework.World.EntityComponents;
using AlphaFramework.World.Positionables;
using Common.Dispatch;
using Common.Utils;
using Common.Values;
using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Renderables;
using SlimDX;
using TerrainSample.World.Positionables;
using CpuParticleSystem = AlphaFramework.World.EntityComponents.CpuParticleSystem;
using GpuParticleSystem = AlphaFramework.World.EntityComponents.GpuParticleSystem;
using LightSource = AlphaFramework.World.EntityComponents.LightSource;
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

        /// <summary>1:1 association of <see cref="AlphaFramework.World.Positionables.Water"/> to <see cref="OmegaEngine.Graphics.Renderables.Water"/>.</summary>
        private readonly Dictionary<AlphaFramework.World.Positionables.Water, Water> _worldToEngineWater = new Dictionary<AlphaFramework.World.Positionables.Water, Water>();

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

            IPositionable value;
            return _worldToEngine.TryGetValue(renderControl, out value) ? value : null;
        }

        /// <summary>
        /// Returns the <see cref="Positionable{TCoordinates}"/> a <see cref="PositionableRenderable"/> was created from or <see langword="null"/>.
        /// </summary>
        protected Positionable<Vector2> GetWorld(PositionableRenderable engineEntity)
        {
            #region Sanity checks
            if (Disposed) throw new ObjectDisposedException(ToString());
            #endregion

            Positionable<Vector2> value;
            return _engineToWorld.TryGetValue(engineEntity, out value) ? value : null;
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

            new PerTypeDispatcher<Positionable<Vector2>>(ignoreMissing: true)
            {
                (Entity entity) =>
                {
                    AddEntity(entity);

                    // Remove and then re-add entity to apply the new entity template
                    entity.ChangedRebuild += RemoveEntity;
                    entity.ChangedRebuild += AddEntity;
                },
                (AlphaFramework.World.Positionables.Water water) =>
                {
                    AddWater(water);

                    // Remove and then re-add water to apply size changes
                    water.ChangedRebuild += RemoveWater;
                    water.ChangedRebuild += AddWater;
                }
            }.Dispatch(positionable);

            // Keep entity and model in sync via events
            positionable.Changed += UpdatePositionable;
        }

        /// <summary>
        /// Sets up a <see cref="EntityBase{TCoordinates,TTemplate}"/> for rendering via a <see cref="PositionableRenderable"/>
        /// </summary>
        /// <param name="positionable">The <see cref="EntityBase{TCoordinates,TTemplate}"/> to be displayed</param>
        /// <remarks>This is a helper method for <see cref="AddPositionable"/>.</remarks>
        private void AddEntity(Positionable<Vector2> positionable)
        {
            var entity = (Entity)positionable;

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
        /// Sets up a <see cref="AlphaFramework.World.Positionables.Water"/> for rendering via a <see cref="Water"/>
        /// </summary>
        /// <param name="positionable">The <see cref="AlphaFramework.World.Positionables.Water"/> to be displayed</param>
        private void AddWater(Positionable<Vector2> positionable)
        {
            var water = (AlphaFramework.World.Positionables.Water)positionable;

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

            new PerTypeDispatcher<Positionable<Vector2>>(ignoreMissing: true)
            {
                (Entity entity) =>
                {
                    RemoveEntity(entity);

                    // Unhook class update event
                    entity.ChangedRebuild -= RemoveEntity;
                    entity.ChangedRebuild -= AddEntity;
                },
                (AlphaFramework.World.Positionables.Water water) =>
                {
                    RemoveWater(water);

                    // Unhook size update event
                    water.ChangedRebuild -= RemoveWater;
                    water.ChangedRebuild -= AddWater;
                }
            }.Dispatch(positionable);

            // Unhook sync event
            positionable.Changed -= UpdatePositionable;
        }

        /// <summary>
        /// Removes a <see cref="OmegaEngine.Graphics.Renderables.PositionableRenderable"/> used to render a <see cref="Positionable{TCoordinates}"/>
        /// </summary>
        /// <param name="positionable">The <see cref="Positionable{TCoordinates}"/> to be removed</param>
        /// <remarks>This is a helper method for <see cref="RemovePositionable"/>.</remarks>
        private void RemoveEntity(Positionable<Vector2> positionable)
        {
            var entity = (Entity)positionable;

            foreach (var renderControl in entity.TemplateData.RenderControls)
                RemoveEntityHelper(renderControl);
        }

        /// <summary>
        /// Removes a <see cref="OmegaEngine.Graphics.Renderables.Water"/> used to render a <see cref="AlphaFramework.World.Positionables.Water"/>
        /// </summary>
        /// <param name="positionable">The <see cref="AlphaFramework.World.Positionables.Water"/> to be removed</param>
        private void RemoveWater(Positionable<Vector2> positionable)
        {
            var water = (AlphaFramework.World.Positionables.Water)positionable;

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

            new PerTypeDispatcher<Positionable<Vector2>>(ignoreMissing: true)
            {
                (Entity entity) =>
                {
                    foreach (var renderControl in entity.TemplateData.RenderControls)
                        UpdateEntityHelper(renderControl, terrainPoint, entity.Rotation);
                },
                (AlphaFramework.World.Positionables.Water water) =>
                    _worldToEngineWater[water].Position = Terrain.Position + water.EnginePosition
            }.Dispatch(positionable);
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
            new AggregateDispatcher<RenderControl>
            {
                // ----- Apply RenderControl.Shift directly with PreTransform ----- //
                (StaticMesh meshRender) =>
                {
                    // Load the model based on the entity name
                    string id = meshRender.Filename;
                    var model = new Model(XMesh.Get(Engine, id)) {Name = entity.Name};
                    ConfigureModel(model, meshRender);

                    // Add objects to lists
                    Scene.Positionables.Add(model);
                    _engineToWorld.Add(model, entity);
                    _worldToEngine.Add(renderControl, model);
                },
                (AnimatedMesh meshRender) =>
                {
                    // Load the model based on the entity name
                    var model = new AnimatedModel(XAnimatedMesh.Get(Engine, meshRender.Filename)) {Name = entity.Name};
                    ConfigureModel(model, meshRender);

                    // Add objects to lists
                    Scene.Positionables.Add(model);
                    _engineToWorld.Add(model, entity);
                    _worldToEngine.Add(renderControl, model);
                },
                (TestSphere sphereRender) =>
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
                },
                (CpuParticleSystem particleRender) =>
                {
                    var particleSystem = new OmegaEngine.Graphics.Renderables.CpuParticleSystem(
                        CpuParticlePreset.FromContent(particleRender.Filename));
                    ConfigureParticleSystem(entity, particleSystem, particleRender);
                    particleSystem.PreTransform = Matrix.Translation(particleRender.Shift);

                    // Add objects to lists
                    Scene.Positionables.Add(particleSystem);
                    _engineToWorld.Add(particleSystem, entity);
                    _worldToEngine.Add(renderControl, particleSystem);
                },
                (GpuParticleSystem particleRender) =>
                {
                    var particleSystem = new OmegaEngine.Graphics.Renderables.GpuParticleSystem(
                        GpuParticlePreset.FromContent(particleRender.Filename));
                    ConfigureParticleSystem(entity, particleSystem, particleRender);
                    particleSystem.PreTransform = Matrix.Translation(particleRender.Shift);

                    // Add objects to lists
                    Scene.Positionables.Add(particleSystem);
                    _engineToWorld.Add(particleSystem, entity);
                    _worldToEngine.Add(renderControl, particleSystem);
                },

                // ----- Don't apply RenderControl.Shift yet ----- //
                (LightSource lightInfo) =>
                {
                    if (!Lighting) return;

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
            }.Dispatch(renderControl);
        }

        private void ConfigureParticleSystem(Entity entity, PositionableRenderable particleSystem, ParticleSystem particleRender)
        {
            particleSystem.Name = entity.Name;
            particleSystem.VisibilityDistance = particleRender.VisibilityDistance;
            particleSystem.Wireframe = WireframeEntities;
            particleSystem.DrawBoundingSphere = BoundingSphereEntities;
            particleSystem.DrawBoundingBox = BoundingBoxEntities;
        }

        private void ConfigureModel(PositionableRenderable model, Mesh meshRender)
        {
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

            new AggregateDispatcher<RenderControl>
            {
                // ----- RenderControl.Shift already applied ----- //
                (Mesh meshRender) =>
                {
                    if (!string.IsNullOrEmpty(meshRender.Filename))
                    {
                        // Cast to entity to be able to access Rotation in addition to Position
                        var renderable = (PositionableRenderable)render;
                        renderable.Position = Terrain.Position + terrainPoint;
                        renderable.Rotation = rotation;
                    }
                },
                (TestSphere meshRender) =>
                {
                    // Cast to entity to be able to access Rotation in addition to Position
                    var renderable = (PositionableRenderable)render;
                    renderable.Position = Terrain.Position + terrainPoint;
                    renderable.Rotation = rotation;
                },
                (ParticleSystem particleSystem) =>
                {
                    var renderable = (PositionableRenderable)render;
                    renderable.Position = Terrain.Position + terrainPoint;
                    renderable.Rotation = rotation;
                },

                // ----- Apply RenderControl.Shift directly to position ----- //
                (LightSource lightSource) =>
                {
                    // Calculate the rotated position shift
                    var rotatedShift = Vector3.TransformCoordinate(
                        lightSource.Shift, Matrix.RotationQuaternion(rotation));
                    render.Position = Terrain.Position + terrainPoint + rotatedShift;
                }
            }.Dispatch(renderControl);
        }
        #endregion

        #region Remove entity helper
        /// <summary>
        /// Removes a graphical representation from the engine, <see cref="_worldToEngine"/> and <see cref="_engineToWorld"/>
        /// </summary>
        /// <param name="renderControl">The <see cref="RenderControl"/> to be </param>
        private void RemoveEntityHelper(RenderControl renderControl)
        {
            var positionable = GetEngine(renderControl);
            if (positionable == null) return;

            new AggregateDispatcher<IPositionable>
            {
                (PositionableRenderable renderable) =>
                {
                    Scene.Positionables.Remove(renderable);
                    _engineToWorld.Remove(renderable);
                    renderable.Dispose();
                },
                (PointLight light) =>
                    Scene.Lights.Remove(light)
            }.Dispatch(positionable);

            _worldToEngine.Remove(renderControl);
        }
        #endregion
    }
}
