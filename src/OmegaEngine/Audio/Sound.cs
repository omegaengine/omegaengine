/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using OmegaEngine.Assets;
using SlimDX.DirectSound;

namespace OmegaEngine.Audio
{
    /// <summary>
    /// A memory-cached sound that is played on-demand.
    /// </summary>
    public class Sound : EngineElement, IAudio
    {
        #region Variables
        /// <summary>A reference to the asset providing the data for this sound.</summary>
        protected readonly XSound Asset;

        /// <summary>The sound buffer containing the decoded data ready for playback.</summary>
        protected SecondarySoundBuffer SoundBuffer;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public bool Playing => !SoundBuffer.Disposed && SoundBuffer.Status == BufferStatus.Playing;

        /// <inheritdoc/>
        public bool Looping => !SoundBuffer.Disposed && SoundBuffer.Status == BufferStatus.Looping;

        /// <inheritdoc/>>
        public int Volume { get { return SoundBuffer.Volume; } set { SoundBuffer.Volume = value; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Sets up a new Sound based on an <see cref="XSound"/> asset.
        /// </summary>
        /// <param name="sound">The <see cref="XSound"/> asset to get the audio data from.</param>
        public Sound(XSound sound)
        {
            #region Sanity checks
            if (sound == null) throw new ArgumentNullException(nameof(sound));
            #endregion

            Asset = sound;
            Asset.HoldReference();
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
            if (IsDisposed) throw new ObjectDisposedException(ToString());
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

        //--------------------//

        #region Engine
        /// <inheritdoc/>
        protected override void OnEngineSet()
        {
            base.OnEngineSet();

            var description = new SoundBufferDescription
            {
                Format = Asset.SoundFormat,
                SizeInBytes = (int)Asset.SoundData.Length,
                Flags = BufferFlags.ControlVolume | BufferFlags.Control3D
            };

            SoundBuffer = new SecondarySoundBuffer(Engine.AudioDevice, description);
            var data = new byte[description.SizeInBytes];
            Asset.SoundData.Read(data, 0, (int)Asset.SoundData.Length);
            SoundBuffer.Write(data, 0, LockFlags.None);
        }
        #endregion

        #region Dispose
        /// <inheritdoc/>
        protected override void OnDispose()
        {
            try
            {
                if (SoundBuffer != null && !SoundBuffer.Disposed)
                {
                    if (Playing) SoundBuffer.Stop();
                    SoundBuffer.Dispose();
                }
            }
            finally
            {
                base.OnDispose();
            }
        }
        #endregion
    }
}
