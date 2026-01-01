/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using NanoByte.Common;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics.Shaders;
using SlimDX;

namespace OmegaEngine.Graphics.Renderables;

/// <summary>
/// A representation of <see cref="WaterView"/>s used for water refraction and reflections
/// </summary>
internal sealed class WaterViewSource : IDisposable
{
    #region Variables
    /// <summary>
    /// A vector indicating the up-direction
    /// </summary>
    public static Vector3 UpVector = Vector3.UnitY;

    /// <summary>
    /// The height of the <see cref="Water"/> planes (Y axis)
    /// </summary>
    public readonly double Height;

    /// <summary>
    /// The <see cref="View"/> the <see cref="Water"/> planes are shown in
    /// </summary>
    public readonly View BaseView;

    /// <seealso cref="WaterView.CreateRefraction"/>
    public readonly WaterView RefractedView;

    /// <seealso cref="WaterView.CreateReflection"/>
    public readonly WaterView ReflectedView;

    /// <summary>
    /// How far to shift the clip plane along its normal vector to reduce graphical glitches at corners
    /// </summary>
    public readonly float ClipTolerance;

    /// <summary>
    /// The <see cref="WaterShader"/>s used to render these waters
    /// </summary>
    public readonly WaterShader RefractionOnlyShader, RefractionReflectionShader;
    #endregion

    #region Properties
    /// <summary>
    /// How many <see cref="Water"/>s use this <see cref="WaterViewSource"/>
    /// </summary>
    /// <remarks>When this hits zero you can call <see cref="Dispose"/></remarks>
    internal int ReferenceCount { get; private set; }
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new <see cref="WaterViewSource"/>
    /// </summary>
    /// <param name="height">The height of the <see cref="Water"/> planes (Y axis)</param>
    /// <param name="baseView">The <see cref="View"/> the <see cref="Water"/> planes are shown in</param>
    /// <param name="clipTolerance">How far to shift the clip plane along its normal vector to reduce graphical glitches at corners</param>
    private WaterViewSource(double height, View baseView, float clipTolerance)
    {
        Height = height;
        BaseView = baseView;
        ClipTolerance = clipTolerance;

        // Create child views
        var position = new DoubleVector3(0, height, 0);

        RefractedView = WaterView.CreateRefraction(baseView, new(position, -UpVector), clipTolerance);
        ReflectedView = WaterView.CreateReflection(baseView, new(position, UpVector), clipTolerance);

        // Load shaders
        RefractionOnlyShader = new(RefractedView);
        RefractionReflectionShader = new(RefractedView, ReflectedView);
    }
    #endregion

    #region Static access
    /// <summary>
    /// Gets an existing <see cref="WaterViewSource"/> if there is one fitting the parameters, otherwise creates a new one
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> to create the views in</param>
    /// <param name="height">The height of the <see cref="Water"/> planes (Y axis)</param>
    /// <param name="baseView">The <see cref="View"/> the <see cref="Water"/> planes are shown in</param>
    /// <param name="clipTolerance">How far to shift the clip plane along its normal vector to reduce graphical glitches at corners</param>
    /// <returns>A <see cref="WaterViewSource"/> fitting the parameters</returns>
    /// <seealso cref="Engine.WaterViewSources"/>
    public static WaterViewSource FromEngine(Engine engine, double height, View baseView, float clipTolerance)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        if (baseView == null) throw new ArgumentNullException(nameof(baseView));
        #endregion

        // ReSharper disable CompareOfFloatsByEqualityOperator
        var view = engine.WaterViewSources.FirstOrDefault(
            candidate => candidate.Height == height &&
                         candidate.BaseView == baseView &&
                         candidate.ClipTolerance == clipTolerance);
        // ReSharper restore CompareOfFloatsByEqualityOperator

        if (view == null) engine.WaterViewSources.Add(view = new(height, baseView, clipTolerance));

        view.ReferenceCount++;
        return view;
    }
    #endregion

    //--------------------//

    #region Reference control
    /// <summary>
    /// Decrements the <see cref="ReferenceCount"/> by one.
    /// </summary>
    internal void ReleaseReference()
    {
        ReferenceCount--;
    }
    #endregion

    //--------------------//

    #region Dispose
    /// <summary>
    /// Disposes the <see cref="WaterShader"/>s created for this <see cref="WaterViewSource"/>
    /// </summary>
    public void Dispose()
    {
        RefractionOnlyShader.Dispose();
        RefractionReflectionShader.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    ~WaterViewSource()
    {
        // This block will only be executed on Garbage Collection, not by manual disposal
        Log.Error($"Forgot to call Dispose on {this}");
#if DEBUG
        throw new InvalidOperationException($"Forgot to call Dispose on {this}");
#endif
    }
    #endregion
}
