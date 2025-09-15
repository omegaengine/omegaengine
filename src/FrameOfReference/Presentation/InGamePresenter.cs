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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AlphaFramework.Presentation;
using AlphaFramework.World.Positionables;
using FrameOfReference.World;
using FrameOfReference.World.Positionables;
using OmegaEngine;
using OmegaEngine.Graphics.Cameras;
using SlimDX;

namespace FrameOfReference.Presentation;

/// <summary>
/// Main in-game interaction
/// </summary>
public sealed class InGamePresenter : InteractivePresenter
{
    /// <summary>
    /// Creates a new presenter for the actual running game
    /// </summary>
    /// <param name="engine">The engine to use for rendering</param>
    /// <param name="universe">The universe to display</param>
    [SetsRequiredMembers]
    public InGamePresenter(Engine engine, Universe universe) : base(engine, universe)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        if (universe == null) throw new ArgumentNullException(nameof(universe));
        #endregion

        // Restore previous camera position (or default to center of terrain)
        var mainCamera = CreateCamera(universe.CurrentCamera);

        View = new(Scene, mainCamera) {Name = "InGame", BackgroundColor = universe.FogColor};
    }

    /// <inheritdoc/>
    public override void HookIn()
    {
        base.HookIn();
        Engine.PreRender += HandleLockedOnEntity;
    }

    /// <inheritdoc/>
    public override void HookOut()
    {
        Engine.PreRender -= HandleLockedOnEntity;
        PrepareSave();
        base.HookOut();
    }

    /// <summary>
    /// Writes back data to <see cref="Universe"/> so that state gets stored in savegames.
    /// </summary>
    public void PrepareSave()
    {
        Universe.CurrentCamera = CameraState;
    }

    //--------------------//

    /// <inheritdoc/>
    protected override void MovePositionables(IEnumerable<Positionable<Vector2>> positionables, Vector2 target)
    {
        #region Sanity checks
        if (positionables == null) throw new ArgumentNullException(nameof(positionables));
        #endregion

        foreach (var entity in positionables.OfType<Entity>())
            Universe.PlayerMove(entity, target);
    }

    /// <summary>
    /// Switches from the current camera view to a new view using a cinematic effect.
    /// </summary>
    /// <param name="name">The <see cref="Positionable{TCoordinates}.Name"/> of a <see cref="CameraState{TCoordinates}"/> stored in the <see cref="PresenterBase{TUniverse,TCoordinates}.Universe"/>.</param>
    public void SwingCameraTo(string name)
    {
        View.SwingCameraTo(CreateCamera(Universe.GetCamera(name)), duration: 4);
    }

    private Entity? _lockedOnEntity;

    /// <summary>
    /// Sets <see cref="InteractivePresenter.SelectedPositionables"/> to a single specific <see cref="Entity"/> and forces the <see cref="Camera"/> to stay close to it.
    /// </summary>
    /// <param name="name">The <see cref="Positionable{TCoordinates}.Name"/> of a <see cref="Entity"/> stored in the <see cref="PresenterBase{TUniverse,TCoordinates}.Universe"/>.</param>
    public void LockOn(string name)
    {
        _lockedOnEntity = Universe.GetEntity(name);

        SelectedPositionables.Clear();
        SelectedPositionables.Add(_lockedOnEntity);
    }

    /// <summary>
    /// Releases a camera lock applied by <see cref="LockOn"/>.
    /// </summary>
    public void ReleaseLock()
    {
        _lockedOnEntity = null;

        SelectedPositionables.Clear();
    }

    private void HandleLockedOnEntity()
    {
        if (_lockedOnEntity == null) return;

        if (View.Camera is StrategyCamera camera)
        {
            var distanceVector = camera.Target - Universe.Terrain.ToEngineCoords(_lockedOnEntity.Position);
            double distanceLength = distanceVector.Length();
            double lockRange = camera.Radius * camera.Radius / 4000;

            if (distanceLength > lockRange)
                camera.Target -= distanceVector * (1 - lockRange / distanceLength);
        }
    }

    /// <inheritdoc/>
    protected override void PickPositionables(IEnumerable<Positionable<Vector2>> positionables, bool accumulate)
    {
        if (_lockedOnEntity == null)
            base.PickPositionables(positionables, accumulate);
    }
}
