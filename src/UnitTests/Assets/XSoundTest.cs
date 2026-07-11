/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using FluentAssertions;
using NAudio.Wave;
using OmegaEngine.Audio;
using Xunit;

namespace OmegaEngine.Assets;

public class XSoundTest : EngineTestBase
{
    private const string Sound = "test.wav";

    [Fact]
    public void LoadsSound()
    {
        var sound = XSound.Get(Engine, Sound);

        sound.Samples.Should().NotBeEmpty();
        sound.Format.Encoding.Should().Be(WaveFormatEncoding.IeeeFloat);
        sound.Format.SampleRate.Should().Be(AudioManager.SampleRate);
    }

    [Fact]
    public void GetReturnsCachedInstance()
    {
        var first = XSound.Get(Engine, Sound);
        var second = XSound.Get(Engine, Sound);

        second.Should().BeSameAs(first);
    }

    [Fact]
    public void GetWithNullIdReturnsNull()
        => XSound.Get(Engine, null).Should().BeNull();
}
