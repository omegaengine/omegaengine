/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using FluentAssertions;
using Xunit;

namespace OmegaEngine.Audio;

public class MusicManagerTest : EngineTestBase
{
    [Fact]
    public void AddingTheSameSongTwiceThrows()
    {
        var music = Engine.Music!;
        music.AddSong("intro.mp3", "menu");

        Action addAgain = () => music.AddSong("intro.mp3", "game");

        addAgain.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PlayingAnEmptyThemeDoesNothing()
    {
        var music = Engine.Music!;

        // No songs registered for this theme -> stays silent, doesn't throw
        music.PlayTheme("nonexistent");

        music.Playing.Should().BeFalse();
    }
}
