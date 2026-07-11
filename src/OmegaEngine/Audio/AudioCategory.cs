/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace OmegaEngine.Audio;

/// <summary>
/// Categorizes audio playback so it can be routed through the matching global volume bus in <see cref="AudioManager"/>.
/// </summary>
public enum AudioCategory
{
    /// <summary>A sound effect, controlled by <see cref="AudioManager.SoundVolume"/>.</summary>
    Sound,

    /// <summary>Background music, controlled by <see cref="AudioManager.MusicVolume"/>.</summary>
    Music
}
