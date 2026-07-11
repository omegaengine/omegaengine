/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace OmegaEngine.Audio;

/// <summary>
/// Helpers for decoding and adapting audio to the format required by <see cref="AudioManager"/>.
/// </summary>
internal static class AudioHelpers
{
    /// <summary>
    /// Opens an audio file, picking the decoder based on the file ending.
    /// </summary>
    /// <returns>A <see cref="WaveStream"/> positioned at the start of the audio data.</returns>
    public static WaveStream OpenStream(string path)
    {
        if (path.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) return new WaveFileReader(path);
        if (path.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)) return new Mp3FileReader(path);
        return new MediaFoundationReader(path);
    }

    /// <summary>
    /// Resamples a source to <see cref="AudioManager.SampleRate"/> if necessary, keeping its channel count.
    /// </summary>
    public static ISampleProvider ResampleToMixerRate(ISampleProvider source)
        => source.WaveFormat.SampleRate == AudioManager.SampleRate
            ? source
            : new WdlResamplingSampleProvider(source, AudioManager.SampleRate);

    /// <summary>
    /// Converts a mono source to stereo; passes stereo (or higher) sources through unchanged.
    /// </summary>
    public static ISampleProvider EnsureStereo(ISampleProvider source)
        => source.WaveFormat.Channels == 1
            ? new MonoToStereoSampleProvider(source)
            : source;

    /// <summary>
    /// Reads a source completely into an in-memory buffer of interleaved IEEE-float samples.
    /// </summary>
    /// <returns>The samples and their (IEEE-float, <see cref="AudioManager.SampleRate"/>) format.</returns>
    public static (float[] samples, WaveFormat format) DecodeToMemory(ISampleProvider source)
    {
        source = ResampleToMixerRate(source);

        int blockSize = Math.Max(source.WaveFormat.SampleRate * source.WaveFormat.Channels, 1024);
        float[] chunk = new float[blockSize];
        var all = new List<float>();

        int read;
        while ((read = source.Read(chunk, 0, chunk.Length)) > 0)
            all.AddRange(read == chunk.Length ? chunk : new ArraySegment<float>(chunk, 0, read));

        return (all.ToArray(), source.WaveFormat);
    }
}
