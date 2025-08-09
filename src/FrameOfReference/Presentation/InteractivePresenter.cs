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
using System.Linq;
using AlphaFramework.World.Components;
using AlphaFramework.World.Positionables;
using FrameOfReference.World;
using FrameOfReference.World.Components;
using FrameOfReference.World.Positionables;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Dispatch;
using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Input;
using SlimDX;

namespace FrameOfReference.Presentation;

/// <summary>
/// Handles the visual representation of <see cref="World"/> content where the user can manually control the perspective
/// </summary>
public abstract partial class InteractivePresenter : Presenter, IInputReceiver
{
    /// <summary>
    /// Creates a new interactive presenter
    /// </summary>
    /// <param name="engine">The engine to use for rendering</param>
    /// <param name="universe">The universe to display</param>
    protected InteractivePresenter(Engine engine, Universe universe) : base(engine, universe)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        if (universe == null) throw new ArgumentNullException(nameof(universe));
        #endregion

        // Add selection highlighting hooks
        engine.ExtraRender += DrawSelectionOutline;

        _selectionsSync = new(SelectedPositionables, Scene.Positionables);

        Universe.Positionables.Removed += OnPositionableRemoved;
    }

    #region Initialize
    private Asset[] _preCachedAssets;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        if (_preCachedAssets == null)
        {
            // Preload selection highlighting meshes
            _preCachedAssets = [XMesh.Get(Engine, "Engine/Circle.x"), XMesh.Get(Engine, "Engine/Rectangle.x")];
            foreach (var asset in _preCachedAssets) asset.HoldReference();
        }

        _selectionsSync.Register<Entity, PositionableRenderable>(GetSelectionHighlighting, UpdateRepresentationShifted);
        _selectionsSync.Initialize();
    }

    /// <summary>
    /// Applies the position and rotation of a Model element to a View representation.
    /// </summary>
    private void UpdateRepresentationShifted(Entity element, PositionableRenderable representation)
    {
        #region Sanity checks
        if (element == null) throw new ArgumentNullException(nameof(element));
        if (representation == null) throw new ArgumentNullException(nameof(representation));
        #endregion

        representation.Position = Universe.Terrain.ToEngineCoords(element.Position) + new DoubleVector3(0, 20, 0);
        representation.Rotation = Quaternion.RotationYawPitchRoll(element.Rotation.DegreeToRadian(), 0, 0);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
            {
                Engine.ExtraRender -= DrawSelectionOutline;
                Universe.Positionables.Removed -= OnPositionableRemoved;

                _selectionsSync.Dispose();

                // Allow the cache management system to clean thes up later
                if (_preCachedAssets != null)
                {
                    foreach (var asset in _preCachedAssets)
                        asset.ReleaseReference();
                    _preCachedAssets = null;
                }
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }
    #endregion

    //--------------------//

    #region Selection highlighting
    /// <summary>
    /// The <see cref="Positionable{TCoordinates}"/>s the user has selected with the mouse
    /// </summary>
    public MonitoredCollection<Positionable<Vector2>> SelectedPositionables { get; } = [];

    /// <summary>
    /// Maps between <see cref="SelectedPositionables"/> and selection highlighting.
    /// </summary>
    private readonly ModelViewSync<Positionable<Vector2>, PositionableRenderable> _selectionsSync;

    /// <summary>
    /// Adds the selection highlighting for a <see cref="EntityBase{TCoordinates,TTemplate}"/>
    /// </summary>
    /// <param name="entity">The <see cref="EntityBase{TCoordinates,TTemplate}"/> to add the selection highlighting for</param>
    private Model GetSelectionHighlighting(Entity entity)
    {
        if (entity.TemplateData.Collision == null) return null;

        var dispatcher = new PerTypeDispatcher<Collision<Vector2>, Model>
        {
            (Circle circle) =>
            {
                // Create a circle around the entity based on the radius
                var hightlight = new Model(XMesh.Get(Engine, "Engine/Circle.x"));
                float scale = circle.Radius / 20 + 1;
                hightlight.PreTransform = Matrix.Scaling(scale, 1, scale);
                return hightlight;
            },
            (Box box) =>
            {
                // Create a rectangle around the entity based on the box corners
                var highlight = new Model(XMesh.Get(Engine, "Engine/Rectangle.x"));

                // Determine the component-wise minimums and maxmimums and the absolute difference
                var min = new Vector2(
                    Math.Min(box.Minimum.X, box.Maximum.X),
                    Math.Min(box.Minimum.Y, box.Maximum.Y));
                var max = new Vector2(
                    Math.Max(box.Minimum.X, box.Maximum.X),
                    Math.Max(box.Minimum.Y, box.Maximum.Y));
                var diff = max - min;

                highlight.PreTransform = Matrix.Scaling(diff.X, 1, diff.Y) * Matrix.Translation(min.X, 0, -min.Y);
                return highlight;
            }
        };

        try
        {
            var selectionHighlight = dispatcher.Dispatch(entity.TemplateData.Collision);
            selectionHighlight.Name = entity.Name + " Selection";
            return selectionHighlight;
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    private void OnPositionableRemoved(Positionable<Vector2> positionable) => SelectedPositionables.Remove(positionable);
    #endregion

    #region Draw selection outline
    /// <summary>
    /// Continuously draw a selection rectangle while the mouse button is pressed
    /// </summary>
    private void DrawSelectionOutline()
    {
        if (_selectionRectangle.HasValue)
            Engine.DrawRectangleOutline(_selectionRectangle.Value, Color.Black);
    }
    #endregion

    /// <summary>
    /// Moves one or more <see cref="Positionable{TCoordinates}"/>s to a new position.
    /// </summary>
    /// <param name="positionables">The <see cref="Positionable{TCoordinates}"/>s to be moved.</param>
    /// <param name="target">The terrain position to move the <paramref name="positionables"/> to.</param>
    protected abstract void MovePositionables(IEnumerable<Positionable<Vector2>> positionables, Vector2 target);

    /// <summary>
    /// Adds one or more <see cref="Positionable{TCoordinates}"/>s to <see cref="SelectedPositionables"/>.
    /// </summary>
    /// <param name="positionables">The selected <see cref="Positionable{TCoordinates}"/>s.</param>
    /// <param name="accumulate"><c>true</c> when the user wants the new selection to be added to the old one.</param>
    protected virtual void PickPositionables(IEnumerable<Positionable<Vector2>> positionables, bool accumulate)
    {
        #region Sanity checks
        if (positionables == null) throw new ArgumentNullException(nameof(positionables));
        #endregion

        // Remove all previous selections unless the user wants to accumulate selections
        if (!accumulate) SelectedPositionables.Clear();

        foreach (var positionable in positionables)
        {
            // Toggle entries when accumulating
            if (accumulate && SelectedPositionables.Contains(positionable)) SelectedPositionables.Remove(positionable);
            else SelectedPositionables.Add(positionable);
        }
    }

    /// <summary>
    /// Switches from the current camera view to a new view using a cinematic effect.
    /// </summary>
    /// <param name="cameraState">The destination state of the camera; <c>null</c> for default (looking at the center of the terrain).</param>
    public void SwingCameraTo(CameraState<Vector2>? cameraState = null)
    {
        View.SwingCameraTo(CreateCamera(cameraState), duration: 1.5f);
    }

    /// <summary>
    /// Swings the camera to look at a specifc set of 2D coordinates.
    /// </summary>
    public void SwingCameraTo(Vector2 target)
    {
        SwingCameraTo(new CameraState<Vector2>
        {
            Name = View.Camera.Name,
            Position = target,
            Radius = 300
        });
    }

    /// <summary>
    /// Swings the camera to look at a specifc <see cref="PositionableRenderable"/>.
    /// </summary>
    public void SwingCameraTo(PositionableRenderable target)
    {
        #region Sanity checks
        if (target == null) throw new ArgumentNullException(nameof(target));
        #endregion

        SwingCameraTo(new CameraState<Vector2>
        {
            Name = View.Camera.Name,
            Position = target.Position.Flatten(),
            Radius = target.WorldBoundingSphere.HasValue ? target.WorldBoundingSphere.Value.Radius * 2.5f : 50,
        });
    }

    /// <summary>
    /// Turns all currently selected <see cref="Entity"/>s into player-controlled characters.
    /// </summary>
    public void TakeOverSelection()
    {
        foreach (var entity in SelectedPositionables.OfType<Entity>())
        {
            entity.Waypoints.RemoveAll(x => x.ActivationTime >= Universe.GameTime);
            entity.IsPlayerControlled = true;
        }
    }
}
