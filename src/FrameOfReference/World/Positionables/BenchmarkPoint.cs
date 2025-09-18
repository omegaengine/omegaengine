/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;

namespace FrameOfReference.World.Positionables;

/// <summary>
/// A marker that controls camera positions for the benchmark mode of the game.
/// </summary>
/// <typeparam name="TCoordinates">Data type for storing position coordinates of objects in the game world.</typeparam>
public class BenchmarkPoint<TCoordinates> : CameraState<TCoordinates>
    where TCoordinates : struct
{
    /// <summary>
    /// Cycle through different water quality settings here (will take 3x as long).
    /// </summary>
    [DefaultValue(false)]
    public bool TestWater { get; set; }

    /// <summary>
    /// Cycle through particle system quality settings here (will take 2x as long).
    /// </summary>
    [DefaultValue(false)]
    public bool TestParticleSystem { get; set; }
}
