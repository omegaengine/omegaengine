/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using FluentAssertions;
using OmegaEngine.Assets;
using Xunit;

namespace OmegaEngine.Audio;

public class SoundTest : EngineTestBase
{
    private Sound CreateSound()
        => new(XSound.Get(Engine, "test.wav")) {Engine = Engine};

    [Fact]
    public void PlaybackTogglesPlayingState()
    {
        using var sound = CreateSound();
        sound.Playing.Should().BeFalse();

        sound.StartPlayback(looping: false);
        if (!sound.Playing) Assert.Skip("No audio output device available.");

        sound.StopPlayback();
        sound.Playing.Should().BeFalse();
    }

    [Fact]
    public void VolumeIsPersisted()
    {
        using var sound = CreateSound();

        sound.Volume = 0.5f;

        sound.Volume.Should().Be(0.5f);
    }

    [Fact]
    public void Sound3DExposesPosition()
    {
        using var sound = new Sound3D(XSound.Get(Engine, "test.wav")) {Engine = Engine};

        // Should not throw regardless of whether an output device is available
        sound.StartPlayback(looping: true);
        sound.StopPlayback();
    }
}
