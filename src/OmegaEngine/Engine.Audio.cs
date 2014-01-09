/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using LuaInterface;
using SlimDX.DirectSound;
using OmegaEngine.Audio;

namespace OmegaEngine
{
    // This file contains code for the Audio-subsystem
    partial class Engine
    {
        #region Variables
        //private SoundListener3D _listener;
        #endregion

        #region Properties
        /// <summary>
        /// The DirectSound device
        /// </summary>
        [LuaHide]
        public DirectSound AudioDevice { get; private set; }

        /// <summary>
        /// Controls the playback of music (theme-selection, cross-fading, etc.)
        /// </summary>
        public MusicManager Music { get; private set; }
        #endregion

        //--------------------//

        #region Setup
        /// <summary>
        /// Helper called by the constructor to setup the audio-subsystem.
        /// </summary>
        /// <exception cref="SlimDX.DirectSound.DirectSoundException">Thrown if internal errors occurred while intiliazing the sound card.</exception>
        private void SetupAudio()
        {
            AudioDevice = new DirectSound();
            AudioDevice.SetCooperativeLevel(Target.Handle, CooperativeLevel.Priority);

            // ToDo
            //var buffer = new PrimarySoundBuffer(SoundDevice, new SoundBufferDescription {Flags = BufferFlags.Control3D | BufferFlags.PrimaryBuffer});
            //_listener = new SoundListener3D(buffer) { RolloffFactor = 0.005f };

            Music = new MusicManager(this);
        }
        #endregion
    }
}
