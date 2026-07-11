/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using AlphaFramework.World;
using AlphaFramework.World.Positionables;
using NanoByte.Common.Dispatch;
using OmegaEngine;
using OmegaEngine.Audio;
using OmegaEngine.Graphics.Renderables;
using LightSource = OmegaEngine.Graphics.LightSources.LightSource;

namespace AlphaFramework.Presentation;

/// <summary>
/// Uses the <see cref="Engine"/> to present a <see cref="CoordinateUniverse{TCoordinates}"/> game world.
/// </summary>
/// <typeparam name="TUniverse">The type of universe to present.</typeparam>
/// <typeparam name="TCoordinates">Data type for storing position coordinates of objects in the game world.</typeparam>
public abstract class CoordinatePresenter<TUniverse, TCoordinates> : PresenterBase<TUniverse>
    where TUniverse : CoordinateUniverse<TCoordinates>
    where TCoordinates : struct
{
    /// <summary>
    /// Creates a new presenter.
    /// </summary>
    /// <param name="engine">The engine to use for rendering.</param>
    /// <param name="universe">The game world to present.</param>
    protected CoordinatePresenter(Engine engine, TUniverse universe)
        : base(engine, universe)
    {
        RenderablesSync = new(Universe.Positionables, Scene.Positionables);
        LightsSync = new(Universe.Positionables, Scene.Lights);
        SoundsSync = new(Universe.Positionables, _sounds);
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        RegisterRenderablesSync();
        RenderablesSync.Initialize();
        LightsSync.Initialize();
        SoundsSync.Initialize();

        base.Initialize();
    }

    /// <summary>
    /// Maps between <see cref="CoordinateUniverse{TCoordinates}.Positionables"/> and <see cref="OmegaEngine.Graphics.Scene.Positionables"/>.
    /// </summary>
    protected readonly ModelViewSync<Positionable<TCoordinates>, PositionableRenderable> RenderablesSync;

    /// <summary>
    /// Maps between <see cref="CoordinateUniverse{TCoordinates}.Positionables"/> and <see cref="OmegaEngine.Graphics.Scene.Lights"/>.
    /// </summary>
    protected readonly ModelViewSync<Positionable<TCoordinates>, LightSource> LightsSync;

    private readonly List<Sound3D> _sounds = [];

    /// <summary>
    /// Maps between <see cref="CoordinateUniverse{TCoordinates}.Positionables"/> and <see cref="Sound3D"/> representations for entities with a <see cref="AlphaFramework.World.Components.Sound"/> component.
    /// </summary>
    protected readonly ModelViewSync<Positionable<TCoordinates>, Sound3D> SoundsSync;

    /// <summary>
    /// Hook to configure <see cref="RenderablesSync"/> and <see cref="LightsSync"/>.
    /// </summary>
    protected virtual void RegisterRenderablesSync()
    {}

    private bool _wireframeEntities;

    /// <summary>
    /// Render all entities in wireframe-mode
    /// </summary>
    public bool WireframeEntities
    {
        get => _wireframeEntities;
        set
        {
            _wireframeEntities = value;
            foreach (var positionable in RenderablesSync.Representations)
                positionable.Wireframe = value;
        }
    }

    private bool _boundingSpheresEntities;

    /// <summary>
    /// Visualize the bounding spheres of all entities
    /// </summary>
    public bool BoundingSphereEntities
    {
        get => _boundingSpheresEntities;
        set
        {
            _boundingSpheresEntities = value;
            foreach (var positionable in RenderablesSync.Representations)
                positionable.DrawBoundingSphere = value;
        }
    }

    private bool _boundingBoxEntities;

    /// <summary>
    /// Visualize the bounding boxes of all entities
    /// </summary>
    public bool BoundingBoxEntities
    {
        get => _boundingBoxEntities;
        set
        {
            _boundingBoxEntities = value;
            foreach (var positionable in RenderablesSync.Representations)
                positionable.DrawBoundingBox = value;
        }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
            {
                RenderablesSync.Dispose();
                LightsSync.Dispose();
                SoundsSync.Dispose();
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }
}
