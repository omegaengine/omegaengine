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
using System.Linq;
using AlphaFramework.World.Components;
using AlphaFramework.World.Positionables;
using FrameOfReference.World.Positionables;
using NanoByte.Common;
using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Renderables;
using SlimDX;
using CpuParticleSystem = AlphaFramework.World.Components.CpuParticleSystem;
using LightSource = AlphaFramework.World.Components.LightSource;
using ViewType = OmegaEngine.Graphics.Renderables.ViewType;
using Water = AlphaFramework.World.Positionables.Water;

namespace FrameOfReference.Presentation;

partial class Presenter
{
    #region Register helpers
    /// <summary>
    /// Registers a callback for converting a <see cref="Water"/>s to <see cref="OmegaEngine.Graphics.Renderables.Water"/> representations.
    /// </summary>
    private void RegisterWater()
    {
        RenderablesSync.Register<Water, OmegaEngine.Graphics.Renderables.Water>(
            element =>
            {
                var representation = new OmegaEngine.Graphics.Renderables.Water(Engine, new(element.Size.X, element.Size.Y))
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
    /// A callback for mapping a <see cref="Render"/> component to an <see cref="Engine"/> representation.
    /// </summary>
    /// <typeparam name="TComponent">The specific type of <see cref="Render"/> component to handle.</typeparam>
    /// <param name="entity">The entity containing the <see cref="Render"/> component.</param>
    /// <param name="component">The <see cref="Render"/> component to visualize using the <see cref="Engine"/>.</param>
    /// <returns>The generated <see cref="Engine"/> representation.</returns>
    protected delegate PositionableRenderable RenderComponentToEngine<TComponent>(Entity entity, TComponent component)
        where TComponent : Render;

    /// <summary>
    /// Registers a callback for converting a <see cref="Render"/> component to an <see cref="Engine"/> representation.
    /// </summary>
    /// <typeparam name="TComponent">The specific type of <see cref="Render"/> component to handle.</typeparam>
    /// <param name="create">The callback for mapping a <see cref="Render"/> component to an <see cref="Engine"/> representation.</param>
    protected void RegisterRenderComponent<TComponent>(RenderComponentToEngine<TComponent> create)
        where TComponent : Render
    {
        RenderablesSync.RegisterMultiple<Entity, PositionableRenderable>(
            element => element.TemplateData.Render.OfType<TComponent>().Select(component => create(element, component)),
            UpdateRepresentation);
    }

    /// <summary>
    /// Registers a callback for converting a <see cref="LightSource"/>s to <see cref="PointLight"/> representations.
    /// </summary>
    private void RegisterRenderComponentLight()
    {
        LightsSync.RegisterMultiple<Entity, PointLight>(
            entity => entity.TemplateData?.Render.OfType<LightSource>().Select(component => new PointLight
            {
                Name = entity.Name,
                Range = component.Range,
                Attenuation = component.Attenuation,
                Diffuse = component.Color,
                Shift = component.Shift
            }),
            UpdateRepresentation);
    }
    #endregion

    #region Update helpers
    /// <summary>
    /// Applies the position of a Model element to a View representation.
    /// </summary>
    protected void UpdateRepresentation(Positionable<Vector2> element, IPositionable representation)
    {
        #region Sanity checks
        if (element == null) throw new ArgumentNullException(nameof(element));
        if (representation == null) throw new ArgumentNullException(nameof(representation));
        #endregion

        representation.Position = Universe.Terrain.ToEngineCoords(element.Position);
    }

    /// <summary>
    /// Applies the position and rotation of a Model element to a View representation.
    /// </summary>
    protected void UpdateRepresentation(Entity element, PositionableRenderable representation)
    {
        #region Sanity checks
        if (element == null) throw new ArgumentNullException(nameof(element));
        if (representation == null) throw new ArgumentNullException(nameof(representation));
        #endregion

        representation.Position = Universe.Terrain.ToEngineCoords(element.Position);
        representation.Rotation = Quaternion.RotationYawPitchRoll(element.Rotation.DegreeToRadian(), 0, 0);
    }

    /// <summary>
    /// Applies the position and rotation of a Model element to a View representation.
    /// </summary>
    protected void UpdateRepresentation(Entity element, PointLight representation)
    {
        #region Sanity checks
        if (element == null) throw new ArgumentNullException(nameof(element));
        if (representation == null) throw new ArgumentNullException(nameof(representation));
        #endregion

        representation.Position = Universe.Terrain.ToEngineCoords(element.Position) +
                                  Vector3.TransformCoordinate(representation.Shift, Matrix.RotationY(element.Rotation.DegreeToRadian()));
    }
    #endregion

    /// <inheritdoc/>
    protected override void RegisterRenderablesSync()
    {
        RegisterWater();

        RegisterRenderComponentLight();

        RegisterRenderComponent<StaticMesh>((entity, component) =>
        {
            if (string.IsNullOrEmpty(component.Filename)) return null;

            var model = new Model(XMesh.Get(Engine, component.Filename)) {Name = entity.Name};
            ConfigureModel(model, component);
            return model;
        });
        RegisterRenderComponent<AnimatedMesh>((entity, component) =>
        {
            if (string.IsNullOrEmpty(component.Filename)) return null;

            var model = new AnimatedModel(XAnimatedMesh.Get(Engine, component.Filename)) {Name = entity.Name};
            ConfigureModel(model, component);
            return model;
        });
        RegisterRenderComponent<TestSphere>((entity, component) =>
        {
            var model = Model.Sphere(Engine, XTexture.Get(Engine, component.Texture), component.Radius, component.Slices, component.Stacks);
            model.Name = entity.Name;
            ConfigureModel(model, component);
            return model;
        });
        RegisterRenderComponent<CpuParticleSystem>((entity, component) =>
        {
            if (string.IsNullOrEmpty(component.Filename)) return null;

            var particleSystem = new OmegaEngine.Graphics.Renderables.CpuParticleSystem(CpuParticlePreset.FromContent(component.Filename));
            ConfigureParticleSystem(entity, particleSystem, component);
            return particleSystem;
        });
    }

    private void ConfigureModel(PositionableRenderable model, Mesh component)
    {
        model.PreTransform = Matrix.Scaling(component.Scale, component.Scale, component.Scale) *
                             Matrix.RotationYawPitchRoll(
                                 component.RotationY.DegreeToRadian(),
                                 component.RotationX.DegreeToRadian(),
                                 component.RotationZ.DegreeToRadian()) *
                             Matrix.Translation(component.Shift);
        model.Alpha = component.Alpha;
        model.Pickable = component.Pickable;
        model.RenderIn = (ViewType)component.RenderIn;
        if (!Lighting) model.SurfaceEffect = SurfaceEffect.Plain;
        model.Wireframe = WireframeEntities;
        model.DrawBoundingSphere = BoundingSphereEntities;
        model.DrawBoundingBox = BoundingBoxEntities;
    }

    private void ConfigureModel(Model model, TestSphere component)
    {
        model.PreTransform = Matrix.Translation(component.Shift);
        model.Alpha = component.Alpha;
        if (!Lighting) model.SurfaceEffect = SurfaceEffect.Plain;
        model.Wireframe = WireframeEntities;
        model.DrawBoundingSphere = BoundingSphereEntities;
        model.DrawBoundingBox = BoundingBoxEntities;
    }

    private void ConfigureParticleSystem(Entity entity, PositionableRenderable particleSystem, ParticleSystem component)
    {
        particleSystem.Name = entity.Name;
        particleSystem.PreTransform = Matrix.Translation(component.Shift);
        particleSystem.VisibilityDistance = component.VisibilityDistance;
        particleSystem.Wireframe = WireframeEntities;
        particleSystem.DrawBoundingSphere = BoundingSphereEntities;
        particleSystem.DrawBoundingBox = BoundingBoxEntities;
    }
}
