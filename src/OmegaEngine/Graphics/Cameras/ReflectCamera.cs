/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using OmegaEngine.Foundation.Geometry;
using SlimDX;

namespace OmegaEngine.Graphics.Cameras;

/// <summary>
/// A camera that reflects the perspective of another camera along a plane.
/// </summary>
/// <param name="parentCamera">The parent camera to track</param>
/// <param name="reflectPlane">The plane along which to reflect the world</param>
public class ReflectCamera(Camera parentCamera, DoublePlane reflectPlane) : CloneCamera(parentCamera)
{
    /// <summary>
    /// A plane alongside which to reflect the camera view
    /// </summary>
    [Description("A plane alongside which to reflect the camera view"), Category("Behavior")]
    public DoublePlane ReflectPlane { get; set; } = reflectPlane;

    /// <inheritdoc/>
    protected override Matrix GetView()
        => Matrix.Reflection(ReflectPlane.ApplyOffset(FloatingOriginCached)) * base.GetView();
}
