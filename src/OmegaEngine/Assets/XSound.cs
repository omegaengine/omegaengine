/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using NAudio.Wave;
using NanoByte.Common;
using NanoByte.Common.Storage;
using OmegaEngine.Audio;
using OmegaEngine.Foundation.Storage;

namespace OmegaEngine.Assets;

/// <summary>
/// A sound loaded from an audio file, decoded into in-memory IEEE-float samples ready for playback.
/// </summary>
public class XSound : Asset
{
    /// <summary>The decoded, interleaved IEEE-float samples at <see cref="AudioManager.SampleRate"/>.</summary>
    public float[] Samples { get; }

    /// <summary>The format of <see cref="Samples"/> (IEEE-float, <see cref="AudioManager.SampleRate"/>, native channel count).</summary>
    public WaveFormat Format { get; }

    /// <summary>
    /// Loads a sound from an audio file stream.
    /// </summary>
    /// <param name="stream">The audio file to load the sound from.</param>
    /// <remarks>This should only be called by <see cref="Get"/> to prevent unnecessary duplicates.</remarks>
    protected XSound(WaveStream stream)
    {
        #region Sanity checks
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        #endregion

        (Samples, Format) = AudioHelpers.DecodeToMemory(stream.ToSampleProvider());
    }

    /// <summary>
    /// Creates a fresh sample provider for playing back this sound.
    /// </summary>
    /// <param name="looping">Whether the playback should loop back to the start when it reaches the end.</param>
    internal ISampleProvider CreateProvider(bool looping)
        => new CachedSoundSampleProvider(Samples, Format, looping);

    /// <summary>
    /// Returns a cached <see cref="XSound"/> or creates a new one if the requested ID is not cached.
    /// </summary>
    /// <param name="engine">The <see cref="Engine"/> providing the cache.</param>
    /// <param name="id">The ID of the asset to be returned.</param>
    /// <returns>The requested asset; <c>null</c> if <paramref name="id"/> was empty.</returns>
    /// <exception cref="FileNotFoundException">The specified file could not be found.</exception>
    /// <exception cref="IOException">There was an error reading the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
    /// <exception cref="InvalidDataException">The file does not contain valid sound data.</exception>
    /// <remarks>Remember to call <see cref="CacheManager.Clean"/> when done, otherwise this object will never be released.</remarks>
    [return: NotNullIfNotNull(nameof(id))]
    public static XSound? Get(Engine engine, string? id)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        if (string.IsNullOrEmpty(id)) return null;
        #endregion

        // Try to find existing asset in cache
        const string type = "Sounds";
        id = id.ToNativePath();
        string fullID = Path.Combine(type, id);
        var data = engine.Cache.GetAsset<XSound>(fullID);

        // Load from file if not in cache
        if (data == null)
        {
            string path = ContentManager.GetFilePath(type, id);
            using (new TimedLogEvent($"Loading sound: {id}"))
            using (var stream = AudioHelpers.OpenStream(path))
                data = new(stream) {Name = fullID};
            engine.Cache.AddAsset(data);
        }

        return data;
    }
}
