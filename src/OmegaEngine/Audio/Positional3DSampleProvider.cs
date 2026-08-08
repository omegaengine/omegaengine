/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using NAudio.Wave;
using OmegaEngine.Foundation.Geometry;

namespace OmegaEngine.Audio;

/// <summary>
/// Wraps a sound source and pans/attenuates it in stereo based on the position of the source relative to the listener.
/// </summary>
/// <remarks>
/// The geometry is re-read on every read, so moving the source or the listener is reflected live.
/// It arrives as a <see cref="PlacementSnapshot"/>, so a read can never pair up data from two different frames.
/// </remarks>
/// <param name="source">The sound to spatialize. Mono or multi-channel; down-mixed to mono before panning.</param>
/// <param name="attenuation">Factors describing how the volume attenuates with distance from the listener.</param>
/// <param name="getPlacement">Supplies where the sound currently sits relative to the listener. Called on every <see cref="Read"/>.</param>
internal sealed class Positional3DSampleProvider(ISampleProvider source, Attenuation attenuation, Func<PlacementSnapshot> getPlacement) : ISampleProvider
{
    private readonly int _sourceChannels = source.WaveFormat.Channels;
    private float[] _sourceBuffer = [];

    /// <summary>
    /// The overall playback volume as a factor (0 = silent, 1 = normal).
    /// </summary>
    public required float Volume { get; set; }

    /// <inheritdoc/>
    public WaveFormat WaveFormat => AudioManager.MixerFormat;

    /// <inheritdoc/>
    public int Read(float[] buffer, int offset, int count)
    {
        int frames = count / 2; // Stereo output
        int sourceSamplesNeeded = frames * _sourceChannels;
        if (_sourceBuffer.Length < sourceSamplesNeeded) _sourceBuffer = new float[sourceSamplesNeeded];

        int sourceRead = source.Read(_sourceBuffer, 0, sourceSamplesNeeded);
        int framesRead = sourceRead / _sourceChannels;

        var (leftGain, rightGain) = ComputeGains();

        for (int i = 0; i < framesRead; i++)
        {
            float mono;
            if (_sourceChannels == 1) mono = _sourceBuffer[i];
            else
            {
                float sum = 0;
                for (int c = 0; c < _sourceChannels; c++) sum += _sourceBuffer[i * _sourceChannels + c];
                mono = sum / _sourceChannels;
            }

            buffer[offset + i * 2] = mono * leftGain;
            buffer[offset + i * 2 + 1] = mono * rightGain;
        }
        return framesRead * 2;
    }

    private (float leftGain, float rightGain) ComputeGains()
    {
        var placement = getPlacement();

        float gain = Volume * attenuation.Apply(placement.Distance);

        // Constant-power panning
        double angle = (placement.Pan + 1d) * Math.PI / 4d; // 0..pi/2
        return (
            leftGain: gain * (float)Math.Cos(angle),
            rightGain: gain * (float)Math.Sin(angle)
        );
    }
}
