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
using System.Windows.Forms;
using FrameOfReference.World.Positionables;
using NanoByte.Common;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Values;
using SlimDX;

namespace FrameOfReference.Presentation;

partial class InteractivePresenter
{
    /// <inheritdoc/>
    public void PerspectiveChange(Point pan, int rotation, int zoom)
    {
        // Adapt panning speed based on view frustum size
        float panFactor = 1.0f / Math.Max(Engine.RenderSize.Width, Engine.RenderSize.Height);

        View.Camera.PerspectiveChange(
            panX: pan.X * panFactor,
            panY: pan.Y * panFactor,
            rotation: rotation / 2.0f,
            zoom: (float)Math.Pow(1.1, zoom / 15.0));
    }

    /// <inheritdoc/>
    public virtual void Hover(Point target)
    {}

    /// <summary>An outline to show on the screen</summary>
    private Rectangle? _selectionRectangle;

    /// <inheritdoc/>
    public virtual void AreaSelection(Rectangle area, bool accumulate, bool done)
    {
        if (done)
        {
            // Handle inverted rectangles and project to terrain
            var terrainArea = GetTerrainArea(Rectangle.FromLTRB(
                (area.Left < area.Right) ? area.Left : area.Right,
                (area.Top < area.Bottom) ? area.Top : area.Bottom,
                (area.Left < area.Right) ? area.Right : area.Left,
                (area.Top < area.Bottom) ? area.Bottom : area.Top));

            PickPositionables(
                // Check each entity in World if it is positioned on top of the selection area
                Universe.Positionables.OfType<Entity>().Where(x => x.CollisionTest(terrainArea)),
                accumulate);

            // Remove the outline from the screen
            _selectionRectangle = null;
        }
        else
        { // Add a selection outline to the screen
            _selectionRectangle = area;
        }
    }

    /// <summary>
    /// Projects a 2D screen rectangle on to the <see cref="Presenter.Terrain"/>, forming a convex quadrangle.
    /// </summary>
    private Quadrangle GetTerrainArea(Rectangle area)
    {
        Vector2 topLeftCoord, bottomLeftCoord, bottomRightCoord, topRightCoord;
        using (new TimedLogEvent("Calculating terrain coordinates for picking"))
        {
            if (!Terrain.Intersects(View.PickingRay(new(area.Left, area.Top)), out DoubleVector3 topLeftPoint)) return new();
            topLeftCoord = topLeftPoint.Flatten();

            if (!Terrain.Intersects(View.PickingRay(new(area.Left, area.Bottom)), out DoubleVector3 bottomLeftPoint)) return new();
            bottomLeftCoord = bottomLeftPoint.Flatten();

            if (!Terrain.Intersects(View.PickingRay(new(area.Right, area.Bottom)), out DoubleVector3 bottomRightPoint)) return new();
            bottomRightCoord = bottomRightPoint.Flatten();

            if (!Terrain.Intersects(View.PickingRay(new(area.Right, area.Top)), out DoubleVector3 topRightPoint)) return new();
            topRightCoord = topRightPoint.Flatten();
        }

        var terrainArea = new Quadrangle(topLeftCoord, bottomLeftCoord, bottomRightCoord, topRightCoord);
        return terrainArea;
    }

    /// <inheritdoc/>
    public virtual void Click(MouseEventArgs e, bool accumulate)
    {
        #region Sanity checks
        if (e == null) throw new ArgumentNullException(nameof(e));
        #endregion

        // Determine the Engine object the user clicked on
        var pickedObject = View.Pick(e.Location, out var intersectPosition);
        if (pickedObject == null) return;

        switch (e.Button)
        {
            case MouseButtons.Left:
                if (pickedObject is Terrain)
                { // Action: Left-click on terrain to select one nearby entity
                    PickPositionables(
                        Universe.Positionables.OfType<Entity>().Where(entity => entity.CollisionTest(intersectPosition.Flatten())).Take(1),
                        accumulate);
                }
                else
                { // Action: Left-click on entity to select it
                    try
                    {
                        PickPositionables([RenderablesSync.Lookup(pickedObject)], accumulate);
                    }
                    catch (KeyNotFoundException)
                    {}
                }
                break;

            case MouseButtons.Right:
                if (SelectedPositionables.Count != 0 && pickedObject is Terrain)
                { // Action: Right-click on terrain to move
                    // Depending on the actual presenter type this may invoke pathfinding or teleportation
                    MovePositionables(SelectedPositionables, intersectPosition.Flatten());
                }
                break;
        }
    }

    /// <inheritdoc/>
    public virtual void DoubleClick(MouseEventArgs e)
    {
        #region Sanity checks
        if (e == null) throw new ArgumentNullException(nameof(e));
        #endregion

        // Determine the Engine object the user double-clicked on
        var pickedObject = View.Pick(e.Location, out _);

        // Action: Double-click on entity to select and focus camera
        if (pickedObject != null && !(pickedObject is Terrain) && !(View.Camera is CinematicCamera)) /* Each swing must complete before the next one can start */
            SwingCameraTo(pickedObject);
    }
}