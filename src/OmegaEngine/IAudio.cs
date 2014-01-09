/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace OmegaEngine
{
    /// <summary>
    /// Represents an asset that can playback audio.
    /// </summary>
    public interface IAudio
    {
        /// <summary>
        /// Is this sound currently being played?
        /// </summary>
        bool Playing { get; }

        /// <summary>
        /// Is this sound set to loop?
        /// </summary>
        bool Looping { get; }

        /// <summary>
        /// The playback volume for the asset.
        /// </summary>
        int Volume { get; set; }

        /// <summary>
        /// Starts the asset playback.
        /// </summary>
        void StartPlayback(bool looping);

        /// <summary>
        /// Stops the asset playback.
        /// </summary>
        void StopPlayback();
    }
}
