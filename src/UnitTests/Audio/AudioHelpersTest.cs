/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using System.Runtime.CompilerServices;
using FluentAssertions;
using NAudio.Wave;
using NanoByte.Common.Storage;
using Xunit;

namespace OmegaEngine.Audio;

/// <summary>
/// Tests the NAudio decode pipeline directly, without needing a Direct3D or audio output device.
/// </summary>
public class AudioHelpersTest
{
    private static string ContentDir([CallerFilePath] string thisFile = "")
        => Path.Combine(Paths.Parent(thisFile), "..", "..", "..", "content");

    [Fact]
    public void DecodesWaveFileToMixerFormat()
    {
        using var reader = new WaveFileReader(Path.Combine(ContentDir(), "Sounds", "test.wav"));

        var (samples, format) = AudioHelpers.DecodeToMemory(reader.ToSampleProvider());

        samples.Should().NotBeEmpty();
        format.Encoding.Should().Be(WaveFormatEncoding.IeeeFloat);
        format.SampleRate.Should().Be(AudioManager.SampleRate);
    }

    [Fact]
    public void ProducesStereoOutputMatchingTheMixer()
    {
        using var reader = new WaveFileReader(Path.Combine(ContentDir(), "Sounds", "test.wav"));

        var provider = AudioHelpers.EnsureStereo(AudioHelpers.ResampleToMixerRate(reader.ToSampleProvider()));

        provider.WaveFormat.Channels.Should().Be(2);
        provider.WaveFormat.SampleRate.Should().Be(AudioManager.SampleRate);
    }
}
