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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Common.Utils;
using Common.Values;
using SlimDX;
using OmegaEngine;
using OmegaEngine.Assets;
using World;
using World.EntityComponents;
using EngineRenderable = OmegaEngine.Graphics.Renderables;
using View = OmegaEngine.Graphics.View;

namespace Presentation
{

    #region Delegates
    /// <seealso cref="EditorPresenter.PostionableMove"/>
    /// <param name="positionables">The <see cref="Positionable"/>s to be moved.</param>
    /// <param name="target">The terrain position to move the entities to.</param>
    public delegate void PostionableMoveHandler(IEnumerable<Positionable> positionables, Vector2 target);

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
        #region Events
        /// <summary>
        /// Occurs when an <see cref="Positionable"/> is to be moved.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [Description("Occurs when an entity is to be moved")]
        public event PostionableMoveHandler PostionableMove;

        /// <summary>
        /// Occurs when the user selects an area while <see cref="TerrainBrush"/> is set to a value other than <see langword="null"/>. Passes the coordinates in world space.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [Description("Occurs when the user selects an area while PaintingMode is set to true.")]
        public event TerrainPaint TerrainPaint;
        #endregion

        #region Variables
        private EngineRenderable.Model _terrainPaintingBrushCircle, _terrainPaintingBrushSquare;
        #endregion

        #region Properties
        private TerrainBrush? _terrainBrush;

        /// <summary>
        /// Controls the shape and size of the area that is visuallly highlighted for <see cref="TerrainPaint"/>ing.
        /// </summary>
        /// <remarks>Raise the <see cref="TerrainPaint"/> event instead of selecting <see cref="Positionable"/>s when set to a value other than <see langword="null"/>.</remarks>
        public TerrainBrush? TerrainBrush
        {
            get { return _terrainBrush; }
            set
            {
                _terrainBrush = value;

                if (value.HasValue)
                {
                    // Update painting brush mesh sizes
                    float scaleHorizontal = value.Value.Size * Universe.Terrain.Size.StretchH;
                    _terrainPaintingBrushCircle.PreTransform = Matrix.Scaling(scaleHorizontal / 30, 1, scaleHorizontal / 30);
                    _terrainPaintingBrushSquare.PreTransform = Matrix.Translation(-0.5f, 0, 0.5f) * Matrix.Scaling(scaleHorizontal, 1, scaleHorizontal);

                    // Update painting brush mesh visibility
                    _terrainPaintingBrushCircle.Visible = value.Value.Circle;
                    _terrainPaintingBrushSquare.Visible = !value.Value.Circle;
                }
                else _terrainPaintingBrushCircle.Visible = _terrainPaintingBrushSquare.Visible = false;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new editor presenter
        /// </summary>
        /// <param name="engine">The engine to use for rendering</param>
        /// <param name="universe">The universe to display</param>
        /// <param name="lighting">Shall lighting be used for rendering?</param>
        public EditorPresenter(Engine engine, Universe universe, bool lighting) : base(engine, universe)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (universe == null) throw new ArgumentNullException("universe");
            #endregion

            Lighting = lighting;

            // Restore previous camera position (or default to center of terrain)
            var mainCamera = CreateCamera(universe.Camera);

            View = new View(engine, Scene, mainCamera) {Name = "Editor", BackgroundColor = universe.FogColor};

            // Floating axis-arrows for easier orientation
            var axisArrows = new EngineRenderable.FloatingModel(engine, XMesh.Get(engine, "Engine/AxisArrows.x"))
            {Name = "AxisArrows", Alpha = 160, Position = new DoubleVector3(-16, -12, 40), Rotation = Quaternion.RotationYawPitchRoll(0, 0, 0)};
            axisArrows.SetScale(0.03f);
            View.FloatingModels.Add(axisArrows);
        }
        #endregion

        //--------------------//

        #region Initialize
        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();

            // Prepare painting brush meshes
            Scene.Positionables.Add(_terrainPaintingBrushCircle = EngineRenderable.Model.FromAsset(Engine, "Engine/Circle.x"));
            Scene.Positionables.Add(_terrainPaintingBrushSquare = EngineRenderable.Model.FromAsset(Engine, "Engine/Rectangle.x"));
            _terrainPaintingBrushCircle.Visible = _terrainPaintingBrushSquare.Visible = false;
        }
        #endregion

        //--------------------//

        #region Movement
        /// <summary>
        /// Informs observers that one or more <see cref="Positionable"/>s are to be moved to a new position.
        /// </summary>
        /// <param name="positionables">The <see cref="Positionable"/>s to be moved.</param>
        /// <param name="target">The terrain position to move the <paramref name="positionables"/> to.</param>
        /// <remarks>This replaces <see cref="InteractivePresenter"/>s path-finding based movement with a callback event.</remarks>
        protected override void MovePositionables(PositionableCollection positionables, Vector2 target)
        {
            if (PostionableMove != null) PostionableMove(positionables, target);
        }
        #endregion

        #region Terrain paint
        /// <inheritdoc/>
        public override void Hover(Point target)
        {
            if (!TerrainBrush.HasValue) return;

            DoubleVector3 hoverPoint;
            if (Terrain.Intersects(View.PickingRay(target), out hoverPoint))
            {
                // ToDo: Make steps discrete
                _terrainPaintingBrushCircle.Position = _terrainPaintingBrushSquare.Position = hoverPoint;

                _terrainPaintingBrushCircle.Visible = TerrainBrush.Value.Circle;
                _terrainPaintingBrushSquare.Visible = !TerrainBrush.Value.Circle;
            }
            else
                _terrainPaintingBrushCircle.Visible = _terrainPaintingBrushSquare.Visible = false;
        }

        /// <inheritdoc/>
        public override void AreaSelection(Rectangle area, bool accumulate, bool done)
        {
            if (TerrainBrush != null)
            {
                if (TerrainPaint != null)
                {
                    DoubleVector3 paintPoint;
                    if (Terrain.Intersects(View.PickingRay(new Point(area.Right, area.Bottom)), out paintPoint))
                    { // Determine the terrain point at the location the user is currently "selecting" and "paint" on it
                        TerrainPaint(World.Terrain.ToWorldCoords(paintPoint), done);
                    }
                    else if (done)
                    { // If there is no terrain where the user is currently "selecting" finish running "paint" operations with a null operation (indicated by negative value)
                        TerrainPaint(new Vector2(-1), true);
                    }
                }
            }
            else base.AreaSelection(area, accumulate, done);
        }

        /// <inheritdoc/>
        public override void Click(MouseEventArgs e, bool accumulate)
        {
            if (TerrainBrush != null)
            {
                if (TerrainPaint != null && e.Button == MouseButtons.Left)
                {
                    // Determine the point the user click on and "paint" on it
                    DoubleVector3 paintPoint;
                    if (Terrain.Intersects(View.PickingRay(e.Location), out paintPoint))
                        TerrainPaint(World.Terrain.ToWorldCoords(paintPoint), true);
                }
            }
            else base.Click(e, accumulate);
        }
        #endregion

        #region Collision bodies
        /// <summary>
        /// Calculates a collision <see cref="Circle"/> from the <see cref="BoundingSphere"/>s of pickable <see cref="EngineRenderable.PositionableRenderable"/>s.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Performs a calculation based on the currently set of visible bodies")]
        public Circle GetCollisionCircle()
        {
            float radius =
                (from positionable in PositionableRenderables
                 where positionable.Pickable && positionable.BoundingSphere.HasValue
                 select MathUtils.Transform(positionable.BoundingSphere.Value, positionable.PreTransform)).
                 Aggregate<BoundingSphere, float>(0, (current, boundingSphere) => Math.Max(current, boundingSphere.Radius + boundingSphere.Center.Length()));
            return new Circle {Radius = radius};
        }

        /// <summary>
        /// Calculates a collision <see cref="Box"/> from the <see cref="BoundingBox"/>s of pickable <see cref="EngineRenderable.PositionableRenderable"/>s.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Performs a calculation based on the currently set of visible bodies")]
        public Box GetCollisionBox()
        {
            float xMin = 0, yMin = 0, xMax = 0, yMax = 0;
            foreach (var positionable in PositionableRenderables)
            {
                if (!positionable.Pickable || !positionable.BoundingBox.HasValue) continue;

                var boundingBox = MathUtils.Transform(positionable.BoundingBox.Value, positionable.PreTransform);
                xMin = Math.Min(xMin, boundingBox.Minimum.X);
                yMin = Math.Min(yMin, -boundingBox.Maximum.Z);
                xMax = Math.Max(xMax, boundingBox.Maximum.X);
                yMax = Math.Max(yMax, -boundingBox.Minimum.Z);
            }

            return new Box {Minimum = new Vector2(xMin, yMin), Maximum = new Vector2(xMax, yMax)};
        }
        #endregion
    }
}
