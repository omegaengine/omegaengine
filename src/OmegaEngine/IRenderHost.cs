/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using OmegaEngine.Input;

namespace OmegaEngine;

/// <summary>
/// A host that provides an <see cref="OmegaEngine.Engine"/> instance and binds it to the operating system.
/// </summary>
public interface IRenderHost
{
    /// <summary>
    /// The <see cref="Engine"/> instance.
    /// </summary>
    Engine? Engine { get; }

    /// <summary>
    /// Provides keyboard input.
    /// </summary>
    KeyboardInputProvider? KeyboardInputProvider { get; }

    /// <summary>
    /// Provides mouse input.
    /// </summary>
    MouseInputProvider? MouseInputProvider { get; }

    /// <summary>
    /// Provides touch input.
    /// </summary>
    TouchInputProvider? TouchInputProvider { get; }
}
