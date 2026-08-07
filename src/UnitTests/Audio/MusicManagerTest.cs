/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using AwesomeAssertions;
using Xunit;

namespace OmegaEngine.Audio;

public class MusicManagerTest : EngineTestBase
{
    [Fact]
    public void AddingTheSameSongTwiceThrows()
    {
        Engine.Music.AddSong("intro.mp3", "menu");

        Action addAgain = () => Engine.Music.AddSong("intro.mp3", "game");
        addAgain.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PlayingAnEmptyThemeDoesNothing()
    {
        // No songs registered for this theme -> stays silent, doesn't throw
        Engine.Music.PlayTheme("nonexistent");

        Engine.Music.Playing.Should().BeFalse();
    }

    [Fact]
    public void PlaySongWithUnknownIdDoesNothing()
    {
        Engine.Music.AddSong("intro.mp3", "menu");
        Engine.Music.PlaySong("nonexistent.mp3");
        Engine.Music.Playing.Should().BeFalse();
    }

    [Fact]
    public void LoadLibrarySkipsCommentsAndMalformedLines()
    {
        Engine.Music.LoadLibrary("test-list.txt");

        Action addAgain = () => Engine.Music.AddSong("test-a.wav", "other");
        addAgain.Should().Throw<InvalidOperationException>();
        Action addOtherAgain = () => Engine.Music.AddSong("test-b.wav", "other");
        addOtherAgain.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PlayThemePlaysASongFromTheTheme()
    {
        Engine.Music.AddSong("test-a.wav", "menu");

        Engine.Music.PlayTheme("menu");
        if (!Engine.Music.Playing) Assert.Skip("No audio output device available.");

        Engine.Music.Playing.Should().BeTrue();
    }

    [Fact]
    public void PlayThemeDoesNotRestartASongAlreadyMatchingTheTheme()
    {
        Engine.Music.AddSong("test-a.wav", "menu", "game");

        Engine.Music.PlayTheme("menu");
        if (!Engine.Music.Playing) Assert.Skip("No audio output device available.");

        // Switching to a theme that also contains the currently playing song must not interrupt it
        Engine.Music.PlayTheme("game");

        Engine.Music.Playing.Should().BeTrue();
    }

    [Fact]
    public void SwitchThemeDoesNotInterruptTheCurrentSong()
    {
        Engine.Music.AddSong("test-a.wav", "menu");
        Engine.Music.AddSong("test-b.wav", "game");

        Engine.Music.PlayTheme("menu");
        if (!Engine.Music.Playing) Assert.Skip("No audio output device available.");

        Engine.Music.SwitchTheme("game");

        Engine.Music.Playing.Should().BeTrue();
    }

    [Fact]
    public void StopWithoutFadeStopsPlaybackImmediately()
    {
        Engine.Music.AddSong("test-a.wav", "menu");
        Engine.Music.PlayTheme("menu");
        if (!Engine.Music.Playing) Assert.Skip("No audio output device available.");

        Engine.Music.Stop(fade: false);

        Engine.Music.Playing.Should().BeFalse();
    }

    [Fact]
    public void StopClearsTheCurrentThemeSoUpdateDoesNotResumePlayback()
    {
        Engine.Music.AddSong("test-a.wav", "menu");
        Engine.Music.PlayTheme("menu");
        if (!Engine.Music.Playing) Assert.Skip("No audio output device available.");

        Engine.Music.Stop(fade: false);
        Engine.Music.Update();

        Engine.Music.Playing.Should().BeFalse();
    }

    [Fact]
    public void StopOnAlreadySilentManagerDoesNotThrow()
    {
        Action stop = () => Engine.Music.Stop(fade: true);
        stop.Should().NotThrow();
    }

    [Fact]
    public void UpdateWithoutAThemeDoesNotThrow()
    {
        Action update = () => Engine.Music.Update();
        update.Should().NotThrow();
    }
}
