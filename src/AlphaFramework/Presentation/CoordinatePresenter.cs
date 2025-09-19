/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using AlphaFramework.World;
using AlphaFramework.World.Positionables;
using NanoByte.Common.Dispatch;
using OmegaEngine;
using OmegaEngine.Graphics.Renderables;
using LightSource = OmegaEngine.Graphics.LightSource;

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
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        RegisterRenderablesSync();
        RenderablesSync.Initialize();
        LightsSync.Initialize();

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

    /// <summary>
    /// Hook to configure <see cref="RenderablesSync"/> and <see cref="LightsSync"/>.
    /// </summary>
    protected virtual void RegisterRenderablesSync()
    {}

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
            {
                RenderablesSync.Dispose();
                LightsSync.Dispose();
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }
}
