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

using System.Drawing;
using System.Linq;
using AlphaFramework.World.EntityComponents;
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
using ViewType = OmegaEngine.Graphics.Renderables.ViewType;
using WorldWater = AlphaFramework.World.Positionables.Water;

namespace TerrainSample.Presentation
{
    partial class Presenter
    {
        #region Register helpers
        /// <summary>
        /// Registers a callback for converting a <see cref="WorldWater"/>s to <see cref="Water"/> representations.
        /// </summary>
        private void RegisterWater()
        {
            RenderablesSync.Register<WorldWater, Water>(
                element =>
                {
                    var representation = new Water(Engine, new SizeF(element.Size.X, element.Size.Y))
                    {
                        Name = element.Name,
                        // NOTE: Height must be set before child views are initialized
                        Position = Terrain.Position + new DoubleVector3(0, element.Height, 0)
                    };
                    representation.SetupChildViews(View);
                    return representation;
                },
                (element, representation) => representation.Position = Terrain.Position + element.EnginePosition);
        }

        /// <summary>
        /// A callback for mapping a <see cref="RenderControl"/> to an <see cref="Engine"/> representation.
        /// </summary>
        /// <typeparam name="TRenderComponent">The specific type of <see cref="RenderControl"/> to handle.</typeparam>
        /// <param name="entity">The entity containing the <see cref="RenderControl"/>.</param>
        /// <param name="renderComponent">The <see cref="RenderControl"/> to visualize using the <see cref="Engine"/>.</param>
        /// <returns>The generated <see cref="Engine"/> representation.</returns>
        protected delegate PositionableRenderable RenderCompononentToEngine<TRenderComponent>(Entity entity, TRenderComponent renderComponent)
            where TRenderComponent : RenderControl;

        /// <summary>
        /// Registers a callback for converting a <see cref="RenderControl"/> to an <see cref="Engine"/> representation.
        /// </summary>
        /// <typeparam name="TRenderComponent">The specific type of <see cref="RenderControl"/> to handle.</typeparam>
        /// <param name="create">The callback for mapping a <see cref="RenderControl"/> to an <see cref="Engine"/> representation.</param>
        protected void RegisterRenderComponent<TRenderComponent>(RenderCompononentToEngine<TRenderComponent> create)
            where TRenderComponent : RenderControl
        {
            RenderablesSync.RegisterMultiple<Entity, PositionableRenderable>(
                element => element.TemplateData.RenderControls.OfType<TRenderComponent>()
                    .Select(renderControl => create(element, renderControl)),
                (entity, representation) =>
                {
                    representation.Position = GetTerrainPosition(entity);
                    representation.Rotation = Quaternion.RotationYawPitchRoll(entity.Rotation.DegreeToRadian(), 0, 0);
                });
        }

        /// <summary>
        /// Registers a callback for converting a <see cref="LightSource"/>s to <see cref="PointLight"/> representations.
        /// </summary>
        private void RegisterRenderComponentLight()
        {
            LightsSync.RegisterMultiple<Entity, PointLight>(
                entity => entity.TemplateData.RenderControls.OfType<LightSource>()
                    .Select(renderControl => new PointLight
                    {
                        Name = entity.Name,
                        Range = renderControl.Range,
                        Attenuation = renderControl.Attenuation,
                        Diffuse = renderControl.Color,
                        Shift = renderControl.Shift
                    }),
                (entity, light) =>
                {
                    var rotatedShift = Vector3.TransformCoordinate(light.Shift, Matrix.RotationY(entity.Rotation.DegreeToRadian()));
                    light.Position = GetTerrainPosition(entity) + rotatedShift;
                });
        }
        #endregion

        /// <inheritdoc/>
        protected override void RegisterRenderablesSync()
        {
            RegisterWater();

            RegisterRenderComponentLight();

            RegisterRenderComponent<StaticMesh>((entity, renderControl) =>
            {
                var model = new Model(XMesh.Get(Engine, renderControl.Filename)) {Name = entity.Name};
                ConfigureModel(model, renderControl);
                return model;
            });
            RegisterRenderComponent<AnimatedMesh>((entity, renderControl) =>
            {
                var model = new AnimatedModel(XAnimatedMesh.Get(Engine, renderControl.Filename)) {Name = entity.Name};
                ConfigureModel(model, renderControl);
                return model;
            });
            RegisterRenderComponent<TestSphere>((entity, renderControl) =>
            {
                var model = Model.Sphere(Engine, XTexture.Get(Engine, renderControl.Texture), renderControl.Radius, renderControl.Slices, renderControl.Stacks);
                model.Name = entity.Name;
                ConfigureModel(model, renderControl);
                return model;
            });
            RegisterRenderComponent<CpuParticleSystem>((entity, renderControl) =>
            {
                var particleSystem = new OmegaEngine.Graphics.Renderables.CpuParticleSystem(CpuParticlePreset.FromContent(renderControl.Filename));
                ConfigureParticleSystem(entity, particleSystem, renderControl);
                return particleSystem;
            });
            RegisterRenderComponent<GpuParticleSystem>((entity, renderControl) =>
            {
                var particleSystem = new OmegaEngine.Graphics.Renderables.GpuParticleSystem(GpuParticlePreset.FromContent(renderControl.Filename));
                ConfigureParticleSystem(entity, particleSystem, renderControl);
                return particleSystem;
            });
        }

        private void ConfigureModel(PositionableRenderable model, Mesh meshRenderControl)
        {
            model.PreTransform = Matrix.Scaling(meshRenderControl.Scale, meshRenderControl.Scale, meshRenderControl.Scale) *
                                 Matrix.RotationYawPitchRoll(
                                     meshRenderControl.RotationY.DegreeToRadian(),
                                     meshRenderControl.RotationX.DegreeToRadian(),
                                     meshRenderControl.RotationZ.DegreeToRadian()) *
                                 Matrix.Translation(meshRenderControl.Shift);
            model.Alpha = meshRenderControl.Alpha;
            model.Pickable = meshRenderControl.Pickable;
            model.RenderIn = (ViewType)meshRenderControl.RenderIn;
            if (Lighting) model.SurfaceEffect = SurfaceEffect.Shader;
            model.Wireframe = WireframeEntities;
            model.DrawBoundingSphere = BoundingSphereEntities;
            model.DrawBoundingBox = BoundingBoxEntities;
        }

        private void ConfigureModel(Model model, TestSphere renderControl)
        {
            model.PreTransform = Matrix.Translation(renderControl.Shift);
            model.Alpha = renderControl.Alpha;
            if (Lighting) model.SurfaceEffect = SurfaceEffect.Shader;
            model.Wireframe = WireframeEntities;
            model.DrawBoundingSphere = BoundingSphereEntities;
            model.DrawBoundingBox = BoundingBoxEntities;
        }

        private void ConfigureParticleSystem(Entity entity, PositionableRenderable particleSystem, ParticleSystem particleRenderControl)
        {
            particleSystem.Name = entity.Name;
            particleSystem.PreTransform = Matrix.Translation(particleRenderControl.Shift);
            particleSystem.VisibilityDistance = particleRenderControl.VisibilityDistance;
            particleSystem.Wireframe = WireframeEntities;
            particleSystem.DrawBoundingSphere = BoundingSphereEntities;
            particleSystem.DrawBoundingBox = BoundingBoxEntities;
        }
    }
}
