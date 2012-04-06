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
using SlimDX.DirectSound;
using OmegaEngine.Assets;

namespace OmegaEngine.Audio
{
    /// <summary>
    /// A memory-cached sound that is played on-demand.
    /// </summary>
    public class Sound : IAudio, IDisposable
    {
        #region Variables
        /// <summary>A reference to the asset providing the data for this sound.</summary>
        protected readonly XSound Asset;

        /// <summary>The sound buffer containing the decoded data ready for playback.</summary>
        protected readonly SecondarySoundBuffer SoundBuffer;
        #endregion

        #region Properties
        /// <summary>
        /// Was this object already disposed?
        /// </summary>
        [Browsable(false)]
        public bool Disposed { get; private set; }

        /// <inheritdoc/>
        public bool Playing { get { return !SoundBuffer.Disposed && SoundBuffer.Status == BufferStatus.Playing; } }

        /// <inheritdoc/>
        public bool Looping { get { return !SoundBuffer.Disposed && SoundBuffer.Status == BufferStatus.Looping; } }

        /// <inheritdoc/>>
        public int Volume { get { return SoundBuffer.Volume; } set { SoundBuffer.Volume = value; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Sets up a new Sound based on an <see cref="XSound"/> asset.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to play the sound with.</param>
        /// <param name="sound">The <see cref="XSound"/> asset to get the audio data from.</param>
        public Sound(Engine engine, XSound sound)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (sound == null) throw new ArgumentNullException("sound");
            #endregion

            Asset = sound;
            Asset.HoldReference();

            var description = new SoundBufferDescription
            {
                Format = sound.SoundFormat,
                SizeInBytes = (int)sound.SoundData.Length,
                Flags = BufferFlags.ControlVolume | BufferFlags.Control3D
            };

            SoundBuffer = new SecondarySoundBuffer(engine.AudioDevice, description);
            var data = new byte[description.SizeInBytes];
            sound.SoundData.Read(data, 0, (int)sound.SoundData.Length);
            SoundBuffer.Write(data, 0, LockFlags.None);
        }
        #endregion

        #region Static access
        /// <summary>
        /// Creates a new sound using a cached <see cref="XSound"/> (loading a new one if none is cached).
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> providing the cache and audio playback capabilities.</param>
        /// <param name="id">The ID of the asset to use.</param>
        /// <exception cref="FileNotFoundException">Thrown if the specified file could not be found.</exception>
        /// <exception cref="IOException">Thrown if there was an error reading the file.</exception>
        /// <exception cref="InvalidDataException">Thrown if the file does not contain valid sound data.</exception>
        /// <returns>The sound that was created.</returns>
        public static Sound FromAsset(Engine engine, string id)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (string.IsNullOrEmpty(id)) return null;
            #endregion

            return new Sound(engine, XSound.Get(engine, id));
        }
        #endregion

        //--------------------//

        #region Playback
        /// <summary>
        /// Starts the sound playback
        /// </summary>
        public virtual void StartPlayback(bool looping)
        {
            #region Sanity checks
            if (Disposed) throw new ObjectDisposedException(ToString());
            #endregion

            SoundBuffer.Play(0, looping ? PlayFlags.Looping : PlayFlags.None);
        }

        /// <summary>
        /// Stops the sound playback
        /// </summary>
        public virtual void StopPlayback()
        {
            SoundBuffer.Stop();
            SoundBuffer.CurrentPlayPosition = 0;
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Disposes the cloned sound buffer and calls <see cref="IReferenceCount.ReleaseReference"/> on the <see cref="XWaveSound"/> resource.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        ~Sound()
        {
            Dispose(false);
        }

        /// <summary>
        /// To be called by <see cref="IDisposable.Dispose"/> and the object destructor.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if called manually and not by the garbage collector.</param>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Only for debugging, not present in Release code")]
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed) return; // Don't try to dispose more than once

            if (disposing)
            { // This block will only be executed on manual disposal, not by Garbage Collection
                if (SoundBuffer != null && !SoundBuffer.Disposed)
                {
                    if (Playing) SoundBuffer.Stop();
                    SoundBuffer.Dispose();
                }
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
