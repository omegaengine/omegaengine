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
using SlimDX.Multimedia;

namespace OmegaEngine.Assets
{
    /// <summary>
    /// Abstract base class for sound assets.
    /// </summary>
    public abstract class XSound : Asset
    {
        #region Properties
        public Stream SoundData { get; protected set; }

        public WaveFormat SoundFormat { get; protected set; }
        #endregion

        #region Static access
        /// <summary>
        /// Returns a cached <see cref="XWaveSound"/> or <see cref="XOggSound"/> (based on the file ending) or creates a new one if the requested ID is not cached.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> providing the cache.</param>
        /// <param name="id">The ID of the asset to be returned.</param>
        /// <returns>The requested asset; <c>null</c> if <paramref name="id"/> was empty.</returns>
        /// <exception cref="FileNotFoundException">The specified file could not be found.</exception>
        /// <exception cref="IOException">There was an error reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">The file does not contain valid sound data.</exception>
        /// <remarks>Remember to call <see cref="CacheManager.Clean"/> when done, otherwise this object will never be released.</remarks>
        public static XSound Get(Engine engine, string id)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (string.IsNullOrEmpty(id)) return null;
            #endregion

            if (id.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase)) return XOggSound.Get(engine, id);
            return XWaveSound.Get(engine, id);
        }
        #endregion

        //--------------------//

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    // This block will only be executed on manual disposal, not by Garbage Collection
                    Log.Info("Disposing " + this);
                    SoundData?.Dispose();
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
