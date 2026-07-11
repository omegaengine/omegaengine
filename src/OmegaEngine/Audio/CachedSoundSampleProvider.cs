/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using NAudio.Wave;

namespace OmegaEngine.Audio;

/// <summary>
/// Plays back an in-memory buffer of interleaved IEEE-float samples, optionally looping.
/// </summary>
/// <remarks>Each playback uses its own instance so the read position is independent, while the sample buffer is shared.</remarks>
internal sealed class CachedSoundSampleProvider(float[] samples, WaveFormat format, bool looping) : ISampleProvider
{
    private int _position;

    /// <inheritdoc/>
    public WaveFormat WaveFormat => format;

    /// <inheritdoc/>
    public int Read(float[] buffer, int offset, int count)
    {
        int copied = 0;
        while (copied < count)
        {
            if (_position >= samples.Length)
            {
                if (looping && samples.Length > 0) _position = 0;
                else break;
            }

            int toCopy = Math.Min(samples.Length - _position, count - copied);
            Array.Copy(samples, _position, buffer, offset + copied, toCopy);
            _position += toCopy;
            copied += toCopy;
        }
        return copied;
    }
}
