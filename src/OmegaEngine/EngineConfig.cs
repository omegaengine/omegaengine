/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;

namespace OmegaEngine;

/// <summary>
/// Stores settings for initializing <see cref="Engine"/>
/// </summary>
/// <seealso cref="Engine.Config"/>
public struct EngineConfig
{
    /// <summary>
    /// The graphics adapter to use for rendering
    /// </summary>
    public int Adapter { get; set; }

    /// <summary>
    /// Shall the engine run in fullscreen mode?
    /// </summary>
    public bool Fullscreen { get; set; }

    /// <summary>
    /// The size of the render target = the monitor resolution
    /// </summary>
    public Size TargetSize { get; set; }

    /// <summary>
    /// Should the framerate be locked to the monitor's vertical sync rate?
    /// </summary>
    public bool VSync { get; set; }

    /// <summary>
    /// The level of anti-aliasing to be employed
    /// </summary>
    public int AntiAliasing { get; set; }

    /// <summary>
    /// Forces the usage of a certain shader model version without checking the hardware capabilities - requires restart to become effective
    /// </summary>
    public Version? ForceShaderModel { get; set; }
}
