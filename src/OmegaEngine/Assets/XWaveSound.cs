/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Storage.SlimDX;
using NanoByte.Common.Streams;
using SlimDX.Multimedia;

namespace OmegaEngine.Assets
{
    /// <summary>
    /// A sound loaded from a WAVE file.
    /// </summary>
    public class XWaveSound : XSound
    {
        #region Constructor
        /// <summary>
        /// Loads a sound from an WAVE file.
        /// </summary>
        /// <param name="stream">The WAVE file to load the sound from.</param>
        /// <exception cref="InvalidDataException"><paramref name="stream"/> does not contain a valid WAVE sound file.</exception>
        /// <remarks>This should only be called by <see cref="Get"/> to prevent unnecessary duplicates.</remarks>
        protected XWaveSound(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            #endregion

            using (var waveFile = new WaveStream(stream))
            {
                SoundFormat = waveFile.Format;
                SoundData = new MemoryStream((int)waveFile.Length);
                waveFile.CopyToEx(SoundData);
            }
        }
        #endregion

        #region Static access
        /// <summary>
        /// Returns a cached <see cref="XWaveSound"/> or creates a new one if the requested ID is not cached.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> providing the cache.</param>
        /// <param name="id">The ID of the asset to be returned.</param>
        /// <returns>The requested asset; <c>null</c> if <paramref name="id"/> was empty.</returns>
        /// <exception cref="FileNotFoundException">The specified file could not be found.</exception>
        /// <exception cref="IOException">There was an error reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">The file does not contain valid WAVE sound data.</exception>
        /// <remarks>Remember to call <see cref="CacheManager.Clean"/> when done, otherwise this object will never be released.</remarks>
        public new static XWaveSound Get(Engine engine, string id)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            if (string.IsNullOrEmpty(id)) return null;
            #endregion

            // Try to find existing asset in cache
            const string type = "Sounds";
            id = FileUtils.UnifySlashes(id);
            string fullID = Path.Combine(type, id);
            var data = engine.Cache.GetAsset<XWaveSound>(fullID);

            // Load from file if not in cache
            if (data == null)
            {
                using (new TimedLogEvent("Loading WAVE sound: " + id))
                using (var stream = ContentManager.GetFileStream("Sounds", id))
                    data = new XWaveSound(stream) {Name = fullID};
                engine.Cache.AddAsset(data);
            }

            return data;
        }
        #endregion
    }
}
