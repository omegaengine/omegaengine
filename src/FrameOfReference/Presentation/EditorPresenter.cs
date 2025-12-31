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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AlphaFramework.Presentation;
using AlphaFramework.World.Positionables;
using AlphaFramework.World.Terrains;
using FrameOfReference.World;
using FrameOfReference.World.Components;
using FrameOfReference.World.Positionables;
using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics.Renderables;
using SlimDX;

namespace FrameOfReference.Presentation;

#region Delegates
/// <seealso cref="EditorPresenter.PostionableMove"/>
/// <param name="positionables">The <see cref="Positionable{TCoordinates}"/>s to be moved.</param>
/// <param name="target">The terrain position to move the entities to.</param>
public delegate void PostionableMoveHandler(IEnumerable<Positionable<Vector2>> positionables, Vector2 target);

/// <seealso cref="EditorPresenter.TerrainPaint"/>
/// <param name="terrainCoords">The terrain coordinates in world space.</param>
/// <param name="done">True when the user has finished his painting (e.g. released the mouse).</param>
public delegate void TerrainPaint(Vector2 terrainCoords, bool done);
#endregion

/// <summary>
/// Displays a map for editing
/// </summary>
public sealed class EditorPresenter : InteractivePresenter
{
    /// <summary>
    /// Creates a new editor presenter
    /// </summary>
    /// <param name="engine">The engine to use for rendering</param>
    /// <param name="universe">The universe to display</param>
    /// <param name="lighting">Shall lighting be used for rendering?</param>
    public EditorPresenter(Engine engine, Universe universe, bool lighting) : base(engine, universe)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        if (universe == null) throw new ArgumentNullException(nameof(universe));
        #endregion

        // Restore previous camera position (or default to center of terrain)
        var mainCamera = CreateCamera(universe.CurrentCamera);

        View = new(Scene, mainCamera)
        {
            Name = "Editor",
            Lighting = lighting,
            BackgroundColor = universe.FogColor
        };

        // Floating axis-arrows for easier orientation
        var axisArrows = new FloatingModel(XMesh.Get(engine, "Engine/AxisArrows.x"))
        {
            Name = "AxisArrows",
            Alpha = 160,
            Position = new(-16, -12, 40),
            Rotation = Quaternion.RotationYawPitchRoll(0, 0, 0)
        };
        axisArrows.SetScale(0.03f);
        View.FloatingModels.Add(axisArrows);
    }

    /// <inheritdoc/>
    protected override void RegisterRenderablesSync()
    {
        base.RegisterRenderablesSync();

        RenderablesSync.Register(
            (Waypoint _) => new Model(XMesh.Get(Engine, "Engine/Waypoint.x")) {Scale = new(100)},
            UpdateRepresentation);
        RenderablesSync.Register(
            (Trigger trigger) =>
            {
                var area = Model.Cylinder(Engine, XTexture.Get(Engine, "flag.png"), radiusBottom: trigger.Range, radiusTop: trigger.Range, length: 150);
                area.Rotation = Quaternion.RotationYawPitchRoll(0, (float)Math.PI / 2, 0);
                area.Alpha = 160;
                return area;
            },
            UpdateRepresentation);
    }

    /// <inheritdoc/>
    protected override double MaxCameraRadius => 10000;

    #region Movement
    /// <summary>
    /// Occurs when an <see cref="Positionable{TCoordinates}"/> is to be moved.
    /// </summary>
    [Description("Occurs when an entity is to be moved")]
    public event PostionableMoveHandler? PostionableMove;

    /// <summary>
    /// Informs observers that one or more <see cref="Positionable{TCoordinates}"/>s are to be moved to a new position.
    /// </summary>
    /// <param name="positionables">The <see cref="Positionable{TCoordinates}"/>s to be moved.</param>
    /// <param name="target">The terrain position to move the <paramref name="positionables"/> to.</param>
    /// <remarks>This replaces <see cref="InteractivePresenter"/>s pathfinding based movement with a callback event.</remarks>
    protected override void MovePositionables(IEnumerable<Positionable<Vector2>> positionables, Vector2 target)
    {
        PostionableMove?.Invoke(positionables, target);
    }
    #endregion

    #region Terrain painting
    private Model
        _terrainPaintingBrushCircle = null!,
        _terrainPaintingBrushSquare = null!;

    private TerrainBrush? _terrainBrush;

    /// <summary>
    /// Controls the shape and size of the area that is visually highlighted for <see cref="TerrainPaint"/>ing.
    /// </summary>
    /// <remarks>Raise the <see cref="TerrainPaint"/> event instead of selecting <see cref="Positionable{TCoordinates}"/>s when set to a value other than <c>null</c>.</remarks>
    public TerrainBrush? TerrainBrush
    {
        get => _terrainBrush;
        set
        {
            _terrainBrush = value;

            if (value.HasValue)
            {
                // Update painting brush mesh sizes
                float scaleHorizontal = value.Value.Size * Universe.Terrain.Size.StretchH;
                _terrainPaintingBrushCircle.PreTransform = Matrix.Scaling(scaleHorizontal / 30, 1, scaleHorizontal / 30);
                _terrainPaintingBrushSquare.PreTransform = Matrix.Translation(-0.5f, 0, 0.5f) * Matrix.Scaling(scaleHorizontal, 1, scaleHorizontal) * Matrix.Translation(0, 20, 0);

                // Update painting brush mesh visibility
                _terrainPaintingBrushCircle.Visible = value.Value.Circle;
                _terrainPaintingBrushSquare.Visible = !value.Value.Circle;
            }
            else _terrainPaintingBrushCircle.Visible = _terrainPaintingBrushSquare.Visible = false;
        }
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        // Prepare painting brush meshes
        Scene.Positionables.Add(_terrainPaintingBrushCircle = new(XMesh.Get(Engine, "Engine/Circle.x")) {Visible = false});
        Scene.Positionables.Add(_terrainPaintingBrushSquare = new(XMesh.Get(Engine, "Engine/Rectangle.x")) {Visible = false});
    }

    /// <inheritdoc/>
    public override void Hover(Point target)
    {
        if (!TerrainBrush.HasValue) return;

        if (Terrain.Intersects(View.PickingRay(target), out DoubleVector3 hoverPoint))
        {
            // ToDo: Make steps discrete
            _terrainPaintingBrushCircle.Position = _terrainPaintingBrushSquare.Position = hoverPoint;

            _terrainPaintingBrushCircle.Visible = TerrainBrush.Value.Circle;
            _terrainPaintingBrushSquare.Visible = !TerrainBrush.Value.Circle;
        }
        else
            _terrainPaintingBrushCircle.Visible = _terrainPaintingBrushSquare.Visible = false;
    }

    /// <summary>
    /// Occurs when the user selects an area while <see cref="TerrainBrush"/> is set to a value other than <c>null</c>. Passes the coordinates in world space.
    /// </summary>
    [Description("Occurs when the user selects an area while PaintingMode is set to true.")]
    public event TerrainPaint? TerrainPaint;

    /// <inheritdoc/>
    public override void AreaSelection(Rectangle area, bool done, bool accumulate = false)
    {
        if (TerrainBrush != null)
        {
            if (TerrainPaint != null)
            {
                if (Terrain.Intersects(View.PickingRay(new(area.Right, area.Bottom)), out DoubleVector3 paintPoint))
                { // Determine the terrain point at the location the user is currently "selecting" and "paint" on it
                    TerrainPaint(paintPoint.Flatten(), done);
                }
                else if (done)
                { // If there is no terrain where the user is currently "selecting" finish running "paint" operations with a null operation (indicated by negative value)
                    TerrainPaint(new(-1), true);
                }
            }
        }
        else base.AreaSelection(area, done, accumulate);
    }

    /// <inheritdoc/>
    public override void Click(MouseEventArgs e, bool accumulate = false)
    {
        #region Sanity checks
        if (e == null) throw new ArgumentNullException(nameof(e));
        #endregion

        if (TerrainBrush != null)
        {
            if (TerrainPaint != null && e.Button == MouseButtons.Left)
            {
                // Determine the point the user click on and "paint" on it
                if (Terrain.Intersects(View.PickingRay(e.Location), out DoubleVector3 paintPoint))
                    TerrainPaint(paintPoint.Flatten(), true);
            }
        }
        else base.Click(e, accumulate);
    }
    #endregion

    #region Collision bodies
    /// <summary>
    /// Calculates a collision <see cref="Circle"/> from the <see cref="BoundingSphere"/>s of pickable <see cref="OmegaEngine.Graphics.Renderables.PositionableRenderable"/>s.
    /// </summary>
    public Circle GetCollisionCircle()
    {
        float radius =
            (from positionable in RenderablesSync.Representations
             where positionable.Pickable && positionable.BoundingSphere.HasValue
             // ReSharper disable once PossibleInvalidOperationException
             select positionable.BoundingSphere.Value.Transform(positionable.PreTransform)).
            Aggregate<BoundingSphere, float>(0, (current, boundingSphere) => Math.Max(current, boundingSphere.Radius + boundingSphere.Center.Length()));
        return new() {Radius = radius};
    }

    /// <summary>
    /// Calculates a collision <see cref="Box"/> from the <see cref="BoundingBox"/>s of pickable <see cref="OmegaEngine.Graphics.Renderables.PositionableRenderable"/>s.
    /// </summary>
    public Box GetCollisionBox()
    {
        var boundingBoxes =
            (from positionable in RenderablesSync.Representations
             where positionable.Pickable && positionable.BoundingBox.HasValue
             // ReSharper disable once PossibleInvalidOperationException
             select positionable.BoundingBox.Value.Transform(positionable.PreTransform))
           .ToList();

        return new()
        {
            Minimum = new(
                boundingBoxes.Min(box => box.Minimum.X),
                boundingBoxes.Min(box => -box.Maximum.Z)),
            Maximum = new(
                boundingBoxes.Max(box => box.Maximum.X),
                boundingBoxes.Max(box => -box.Minimum.Z))
        };
    }
    #endregion
}
