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
using SlimDX;

namespace OmegaEngine.Audio;

/// <summary>
/// Wraps a sound source and pans/attenuates it in stereo based on the position of the source relative to a <see cref="ListenerSnapshot"/>.
/// </summary>
/// <remarks>The geometry is re-evaluated on every read, so moving the source or the listener is reflected live.</remarks>
/// <param name="source">The sound to spatialize. Mono or multi-channel; down-mixed to mono before panning.</param>
/// <param name="attenuation">Factors describing how the volume attenuates with distance from the listener.</param>
/// <param name="getListener">Supplies the current listener position and orientation. Called on every <see cref="Read"/>.</param>
/// <param name="getPosition">Supplies the sound's current position in world space. Called on every <see cref="Read"/>.</param>
internal sealed class Positional3DSampleProvider(ISampleProvider source, Attenuation attenuation, Func<ListenerSnapshot> getListener, Func<DoubleVector3> getPosition) : ISampleProvider
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
        var listener = getListener();
        var delta = getPosition() - listener.Position;
        double distance = delta.Length();

        float gain = Volume * attenuation.Apply((float)distance);

        // Determine left/right balance from the lateral angle to the source
        float pan = 0f;
        if (distance > 1e-6)
        {
            var direction = (Vector3)(delta / distance);
            var right = Vector3.Normalize(Vector3.Cross(listener.Up, listener.Forward));
            pan = Math.Max(-1f, Math.Min(1f, Vector3.Dot(direction, right)));
        }

        // Constant-power panning
        double angle = (pan + 1d) * Math.PI / 4d; // 0..pi/2
        return (
            leftGain: gain * (float)Math.Cos(angle),
            rightGain: gain * (float)Math.Sin(angle)
        );
    }
}
