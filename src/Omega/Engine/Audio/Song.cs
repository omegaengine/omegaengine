/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Common;
using OmegaEngine.Assets;
using SlimDX.DirectSound;

namespace OmegaEngine.Audio
{
    /// <summary>
    /// A streamed sound that is played in the background as music.
    /// </summary>
    /// <seealso cref="MusicManager"/>
    public sealed class Song : IAudio, IDisposable
    {
        #region Properties
        /// <summary>
        /// Was this object already disposed?
        /// </summary>
        [Browsable(false)]
        public bool Disposed { get; private set; }

        /// <summary>
        /// The ID of this song
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// The sound in DirectX format
        /// </summary>
        [CLSCompliant(false)]
        public SecondarySoundBuffer SoundBuffer { get; private set; }

        /// <inheritdoc/>
        public bool Playing { get { return false /*Disposed && SoundBuffer.Status == BufferStatus.Playing*/; } }

        /// <inheritdoc/>
        public bool Looping { get { return false /*SoundBuffer.Disposed ? false : SoundBuffer.Status == BufferStatus.Looping*/; } }

        /// <inheritdoc/>
        public int Volume
        {
            get { return 0 /*SoundBuffer.Volume*/; }
            set
            { /*SoundBuffer.Volume = value;*/
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Loads a song from an OGG file
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the song into</param>
        /// <param name="id">The OGG file to load the song from</param>
        /// <exception cref="IOException">Thrown if there was a problem loading the song</exception>
        public Song(Engine engine, string id)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");
            #endregion

            Log.Info("Loading song: " + id);

            ID = id;

            //var description = new SoundBufferDescription
            //{
            //    //Format = sound.SoundFormat,
            //    //SizeInBytes = (int)sound.SoundData.Length,
            //    Flags = BufferFlags.ControlVolume
            //};
            //SoundBuffer = new SecondarySoundBuffer(engine.SoundDevice, description);
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
            if (Disposed) throw new ObjectDisposedException(ToString());
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

        #region Dispose
        /// <summary>
        /// Disposes any local unmanaged resources and releases any <see cref="Asset"/>s used.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        ~Song()
        {
            Dispose(false);
        }

        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Only for debugging, not present in Release code")]
        private void Dispose(bool disposing)
        {
            if (Disposed || SoundBuffer == null) return; // Don't try to dispose more than once

            if (disposing)
            { // This block will only be executed on manual disposal, not by Garbage Collection
                // Make sure stop-events are raised before fully disposing the object
                if (SoundBuffer.Status == BufferStatus.Playing) SoundBuffer.Stop();

                SoundBuffer.Dispose();
            }
            else
            { // This block will only be executed on Garbage Collection, not by manual disposal
                Log.Error("Forgot to call Dispose on " + this);
#if DEBUG
                throw new InvalidOperationException("Forgot to call Dispose on " + this);
#endif
            }

            Disposed = true;
        }
        #endregion
    }
}
