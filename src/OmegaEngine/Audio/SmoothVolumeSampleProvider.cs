/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NAudio.Wave;

namespace OmegaEngine.Audio;

/// <summary>
/// Scales a sound's samples by a <see cref="Volume"/> factor.
/// </summary>
/// <remarks>
/// Unlike NAudio's <c>VolumeSampleProvider</c> a changed <see cref="Volume"/> is ramped in across the buffer instead of being applied to it as a whole.
/// A buffer spans a large multiple of a frame, so jumping straight to a new volume would put an audible step into the waveform.
/// </remarks>
/// <param name="source">The sound to scale.</param>
internal sealed class SmoothVolumeSampleProvider(ISampleProvider source) : ISampleProvider
{
    /// <summary>
    /// The playback volume as a factor (0 = silent, 1 = normal).
    /// </summary>
    public float Volume { get; set; } = 1f;

    /// <inheritdoc/>
    public WaveFormat WaveFormat => source.WaveFormat;

    private bool _gainKnown;
    private float _gain;

    /// <inheritdoc/>
    public int Read(float[] buffer, int offset, int count)
    {
        int read = source.Read(buffer, offset, count);
        if (read == 0) return 0;

        float target = Volume;
        if (!_gainKnown)
        {
            // Nothing to ramp from on the first buffer
            _gain = target;
            _gainKnown = true;
        }

        int channels = WaveFormat.Channels;
        int frames = read / channels;
        float step = frames == 0 ? 0 : (target - _gain) / frames;
        float gain = _gain;

        for (int i = 0; i < frames; i++)
        {
            gain += step;
            for (int c = 0; c < channels; c++)
                buffer[offset + i * channels + c] *= gain;
        }

        // Scale any samples beyond the last complete frame at the target
        for (int i = frames * channels; i < read; i++)
            buffer[offset + i] *= target;

        // Pick the exact target up again next time, rather than the accumulated approximation
        _gain = target;

        return read;
    }
}
