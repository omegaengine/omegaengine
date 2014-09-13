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
using NanoByte.Common.Storage.SlimDX;
using NanoByte.Common.Utils;

namespace OmegaEngine.Assets
{
    /// <summary>
    /// A sound loaded from an OGG Vorbis file.
    /// </summary>
    public class XOggSound : XSound
    {
        #region Constructor
        /// <summary>
        /// Loads a sound from an OGG file.
        /// </summary>
        /// <param name="stream">The OGG file to load the sound from.</param>
        /// <exception cref="InvalidDataException"><paramref name="stream"/> does not contain a valid Ogg Vorbis sound file.</exception>
        /// <remarks>This should only be called by <see cref="Get"/> to prevent unnecessary duplicates.</remarks>
        protected XOggSound(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            throw new NotImplementedException();
        }
        #endregion

        #region Static access
        /// <summary>
        /// Returns a cached <see cref="XOggSound"/> or creates a new one if the requested ID is not cached.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> providing the cache.</param>
        /// <param name="id">The ID of the asset to be returned.</param>
        /// <returns>The requested asset; <see langword="null"/> if <paramref name="id"/> was empty.</returns>
        /// <exception cref="FileNotFoundException">The specified file could not be found.</exception>
        /// <exception cref="IOException">There was an error reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">The file does not contain valid Ogg Vorbis sound data.</exception>
        /// <remarks>Remember to call <see cref="CacheManager.Clean"/> when done, otherwise this object will never be released.</remarks>
        public new static XOggSound Get(Engine engine, string id)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (string.IsNullOrEmpty(id)) return null;
            #endregion

            // Try to find existing asset in cache
            const string type = "Sounds";
            id = FileUtils.UnifySlashes(id);
            string fullID = Path.Combine(type, id);
            var data = engine.Cache.GetAsset<XOggSound>(fullID);

            // Load from file if not in cache
            if (data == null)
            {
                using (new TimedLogEvent("Loading OGG Vorbis sound: " + id))
                    data = new XOggSound(ContentManager.GetFileStream("Sounds", id)) {Name = fullID};
                engine.Cache.AddAsset(data);
            }

            return data;
        }
        #endregion
    }
}
