/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using FluentAssertions;
using Xunit;

namespace OmegaEngine.Audio;

public class AudioManagerTest : EngineTestBase
{
    [Fact]
    public void GlobalVolumesDefaultToNormal()
    {
        Engine.Audio.SoundVolume.Should().Be(1f);
        Engine.Audio.MusicVolume.Should().Be(1f);
    }

    [Fact]
    public void GlobalVolumesArePersisted()
    {
        // Works regardless of whether an output device is available
        Engine.Audio.SoundVolume = 0.25f;
        Engine.Audio.MusicVolume = 0.5f;

        Engine.Audio.SoundVolume.Should().Be(0.25f);
        Engine.Audio.MusicVolume.Should().Be(0.5f);
    }
}
