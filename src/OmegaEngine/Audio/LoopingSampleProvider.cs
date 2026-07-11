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
/// Loops a streamed sample provider by invoking a reset action (e.g. seeking the underlying stream back to the start) once it runs dry.
/// </summary>
internal sealed class LoopingSampleProvider(ISampleProvider source, Action reset) : ISampleProvider
{
    /// <inheritdoc/>
    public WaveFormat WaveFormat => source.WaveFormat;

    /// <inheritdoc/>
    public int Read(float[] buffer, int offset, int count)
    {
        int total = 0;
        while (total < count)
        {
            int read = source.Read(buffer, offset + total, count - total);
            if (read == 0)
            {
                reset();
                read = source.Read(buffer, offset + total, count - total);
                if (read == 0) break; // Guard against an empty source causing an infinite loop
            }
            total += read;
        }
        return total;
    }
}
