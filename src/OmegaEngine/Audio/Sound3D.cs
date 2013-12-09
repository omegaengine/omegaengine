/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Common.Utils;
using Common.Values;
using SlimDX;
using SlimDX.DirectSound;
using OmegaEngine.Assets;

namespace OmegaEngine.Audio
{
    /// <summary>
    /// A memory-cached sound that is played on-demand simulating a position in 3D-space.
    /// </summary>
    public class Sound3D : Sound, IPositionableOffset
    {
        #region Variables
        private readonly SoundBuffer3D _buffer3D;
        #endregion

        #region Properties
        private DoubleVector3 _position;

        /// <summary>
        /// The sound's position in world space
        /// </summary>
        [Description("The body's position in world space"), Category("Layout")]
        public DoubleVector3 Position { get { return _position; } set { value.To(ref _position, () => _buffer3D.Position = ((IPositionableOffset)this).EffectivePosition); } }

        private DoubleVector3 _positionOffset;

        /// <inheritdoc/>
        DoubleVector3 IPositionableOffset.Offset { get { return _positionOffset; } set { value.To(ref _positionOffset, () => _buffer3D.Position = ((IPositionableOffset)this).EffectivePosition); } }

        /// <summary>
        /// The sounds's position in render space, based on <see cref="Position"/>
        /// </summary>
        /// <remarks>Constantly changes based on the values set for <see cref="IPositionableOffset.EffectivePosition"/></remarks>
        Vector3 IPositionableOffset.EffectivePosition { get { return Position.ApplyOffset(((IPositionableOffset)this).Offset); } }
        #endregion

        #region Constructor
        /// <summary>
        /// Sets up a new Sound based on an <see cref="XSound"/> asset.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to play the sound with.</param>
        /// <param name="sound">The <see cref="XSound"/> asset to get the audio data from.</param>
        public Sound3D(Engine engine, XSound sound) : base(engine, sound)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (sound == null) throw new ArgumentNullException("sound");
            #endregion

            _buffer3D = new SoundBuffer3D(SoundBuffer);
        }
        #endregion

        #region Static access
        /// <summary>
        /// Creates a new 3D sound using a cached <see cref="XSound"/> (loading a new one if none is cached).
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> providing the cache and audio playback capabilities.</param>
        /// <param name="id">The ID of the asset to use.</param>
        /// <exception cref="FileNotFoundException">Thrown if the specified file could not be found.</exception>
        /// <exception cref="IOException">Thrown if there was an error reading the file.</exception>
        /// <exception cref="InvalidDataException">Thrown if the file does not contain valid sound data.</exception>
        /// <returns>The 3D sound that was created.</returns>
        public new static Sound3D FromAsset(Engine engine, string id)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (string.IsNullOrEmpty(id)) return null;
            #endregion

            return new Sound3D(engine, XSound.Get(engine, id));
        }
        #endregion

        //--------------------//

        #region Playback
        /// <summary>
        /// Starts the sound playback
        /// </summary>
        public override void StartPlayback(bool looping)
        {
            #region Sanity checks
            if (Disposed) throw new ObjectDisposedException(ToString());
            #endregion

            // ToDo: Implement
        }

        /// <summary>
        /// Stops the sound playback
        /// </summary>
        public override void StopPlayback()
        {
            // ToDo: Implement
        }
        #endregion

        #region Dispose
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Only for debugging, not present in Release code")]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                { // This block will only be executed on manual disposal, not by Garbage Collection
                    if (_buffer3D != null && !_buffer3D.Disposed) _buffer3D.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion
    }
}
