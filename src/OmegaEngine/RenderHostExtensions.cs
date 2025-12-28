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
/// Contains extension methods for <see cref="RenderHostExtensions"/>.
/// </summary>
public static class RenderHostExtensions
{
    /// <summary>
    /// Calls <see cref="InputProvider.AddReceiver"/> for all default <see cref="InputProvider"/>s.
    /// </summary>
    public static void AddInputReceiver(this IRenderHost host, IInputReceiver receiver)
    {
        host.KeyboardInputProvider?.AddReceiver(receiver);
        host.MouseInputProvider?.AddReceiver(receiver);
        host.TouchInputProvider?.AddReceiver(receiver);
    }

    /// <summary>
    /// Calls <see cref="InputProvider.RemoveReceiver"/> for all default <see cref="InputProvider"/>s.
    /// </summary>
    public static void RemoveInputReceiver(this IRenderHost host, IInputReceiver receiver)
    {
        host.KeyboardInputProvider?.RemoveReceiver(receiver);
        host.MouseInputProvider?.RemoveReceiver(receiver);
        host.TouchInputProvider?.RemoveReceiver(receiver);
    }
}
