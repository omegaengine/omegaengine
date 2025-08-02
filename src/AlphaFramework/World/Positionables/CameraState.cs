/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Drawing.Design;
using OmegaEngine.Values.Design;

namespace AlphaFramework.World.Positionables;

/// <summary>
/// Stores the position and direction of the camera in the game.
/// </summary>
/// <seealso cref="UniverseBase{TCoordinates}.CurrentCamera"/>
/// <typeparam name="TCoordinates">Data type for storing position coordinates of objects in the game world.</typeparam>
public class CameraState<TCoordinates> : Positionable<TCoordinates>
    where TCoordinates : struct
{
    /// <summary>
    /// The camera's distance from the focused position.
    /// </summary>
    [Description("The camera's distance from the focused position.")]
    public float Radius { get; set; }

    /// <summary>
    /// The horizontal rotation of the view direction in degrees.
    /// </summary>
    [DefaultValue(0f), Description("The horizontal rotation of the view direction in degrees.")]
    [Editor(typeof(AngleEditor), typeof(UITypeEditor))]
    public float Rotation { get; set; }
}