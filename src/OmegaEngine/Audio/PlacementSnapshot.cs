/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace OmegaEngine.Audio;

/// <summary>
/// An immutable per-frame snapshot of where a <see cref="Sound3D"/> sits relative to the listener, read by the audio thread during mixing.
/// </summary>
/// <remarks>
/// Distance and panning are derived on the main thread from a single <see cref="ListenerSnapshot"/> and sound position, then published as one reference.
/// The audio thread therefore always sees a listener and a sound position from the same frame, even if it reads midway through the next frame's updates.
/// </remarks>
/// <param name="Distance">The distance from the listener to the sound, in world units.</param>
/// <param name="Pan">The lateral position relative to the listener, from -1 (fully left) over 0 (centered) to 1 (fully right).</param>
internal sealed record PlacementSnapshot(float Distance, float Pan)
{
    /// <summary>A sound at the listener's own position.</summary>
    public static readonly PlacementSnapshot Default = new(Distance: 0, Pan: 0);
}
