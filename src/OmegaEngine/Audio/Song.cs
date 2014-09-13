/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using SlimDX.DirectSound;

namespace OmegaEngine.Audio
{
    /// <summary>
    /// A streamed sound that is played in the background as music.
    /// </summary>
    public class Song : EngineElement, IAudio
    {
        #region Variables
        /// <summary>The sound buffer containing the decoded data ready for playback.</summary>
        private SecondarySoundBuffer _soundBuffer;
        #endregion

        #region Properties
        /// <summary>
        /// The ID of this song
        /// </summary>
        public string ID { get; private set; }

        /// <inheritdoc/>
        public bool Playing { get { return !_soundBuffer.Disposed && _soundBuffer.Status == BufferStatus.Playing; } }

        /// <inheritdoc/>
        public bool Looping { get { return !_soundBuffer.Disposed && _soundBuffer.Status == BufferStatus.Looping; } }

        /// <inheritdoc/>>
        public int Volume { get { return _soundBuffer.Volume; } set { _soundBuffer.Volume = value; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Loads a song from an OGG file
        /// </summary>
        /// <param name="id">The OGG file to load the song from</param>
        /// <exception cref="IOException">There was a problem loading the song.</exception>
        public Song(string id)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");
            #endregion

            ID = id;
        }
        #endregion

        //--------------------//

        #region Playback
        /// <summary>
        /// Starts the song playback
        /// </summary>
        public void StartPlayback(bool looping)
        {
            #region Sanity checks
            if (IsDisposed) throw new ObjectDisposedException(ToString());
            #endregion

            //SoundBuffer.Play(0, looping ? PlayFlags.Looping : PlayFlags.None);
        }

        /// <summary>
        /// Stops the song playback
        /// </summary>
        public void StopPlayback()
        {
            //SoundBuffer.Stop();
            //SoundBuffer.CurrentPlayPosition = 0;
        }
        #endregion

        //--------------------//

        #region Engine
        /// <inheritdoc/>
        protected override void OnEngineSet()
        {
            base.OnEngineSet();

            var description = new SoundBufferDescription
            {
                Flags = BufferFlags.ControlVolume
            };

            _soundBuffer = new SecondarySoundBuffer(Engine.AudioDevice, description);
        }
        #endregion

        #region Dispose
        /// <inheritdoc/>
        protected override void OnDispose()
        {
            try
            {
                // Make sure stop-events are raised before fully disposing the object
                //if (_soundBuffer.Status == BufferStatus.Playing) _soundBuffer.Stop();
                //_soundBuffer.Dispose();
            }
            finally
            {
                base.OnDispose();
            }
        }
        #endregion
    }
}
