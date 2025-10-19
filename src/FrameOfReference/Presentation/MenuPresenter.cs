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
using System.Diagnostics;
using FrameOfReference.World;
using OmegaEngine;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics.Cameras;

namespace FrameOfReference.Presentation;

/// <summary>
/// Displays the background world for the main menu
/// </summary>
public sealed class MenuPresenter : Presenter
{
    /// <summary>
    /// Creates a new background presenter for the main menu
    /// </summary>
    /// <param name="engine">The engine to use for rendering</param>
    /// <param name="universe">The universe to display</param>
    public MenuPresenter(Engine engine, Universe universe) : base(engine, universe)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        if (universe == null) throw new ArgumentNullException(nameof(universe));
        #endregion

        // Target a point slightly above the center of the map and then rotate
        const float rotationHeight = 150, rotationRadius = 750;

        // Map X = Engine +X
        // Map Y = Engine -Z
        var cameraTarget = new DoubleVector3(
            Universe.Terrain.Center.X, rotationHeight, -Universe.Terrain.Center.Y);
        var mainCamera = new TrackCamera(rotationRadius, rotationRadius) {Target = cameraTarget, HorizontalRotation = 0, VerticalRotation = 15, Name = "Menu"};

        View = new(Scene, mainCamera)
        {
            Name = "Menu",
            BackgroundColor = universe.FogColor,
            Lighting = true
        };
        View.PreRender += RotateCamera;
    }

    #region Rotate camera
    private readonly Stopwatch _cameraTimer = Stopwatch.StartNew();

    private void RotateCamera(Camera camera)
    {
        ((TrackCamera)camera).HorizontalRotation += _cameraTimer.Elapsed.TotalSeconds * -5;
        _cameraTimer.Reset();
        _cameraTimer.Start();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
                View.PreRender -= RotateCamera;
        }
        finally
        {
            base.Dispose(disposing);
        }
    }
    #endregion
}
