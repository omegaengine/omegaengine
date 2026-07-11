/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using NanoByte.Common;
using NanoByte.Common.Collections;
using OmegaEngine.Foundation;
using OmegaEngine.Foundation.Storage;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Audio;

/// <summary>
/// Manages the playback of <see cref="Song"/> in the background controlled by themes.
/// </summary>
/// <param name="engine">The <see cref="Engine"/> to be used for playing the music</param>
public sealed class MusicManager(Engine engine) : IDisposable
{
    private readonly MultiDictionary<string, Song> _themes = new();

    private string? _currentTheme;
    private Song? _currentSong;

    /// <summary>
    /// Is music currently being played?
    /// </summary>
    [MemberNotNullWhen(true, nameof(_currentSong))]
    public bool Playing => _currentSong is { Playing: true };

    /// <summary>
    /// Populates the music manager with songs listed in a library file
    /// </summary>
    /// <param name="id">The ID of the library file to load</param>
    public void LoadLibrary(string id)
    {
        using var stream = ContentManager.GetFileStream("Music", id);
        using var streamReader = new StreamReader(stream);
        while (!streamReader.EndOfStream)
        {
            string line = streamReader.ReadLine();
            if (line == null) break;
            if (line.StartsWith("#")) continue;

            // Lines formatted as Song|Theme1,Theme2,Theme3,...
            string[] values = line.Split('|');
            if (values.Length < 2)
            {
                Log.Warn($"Malformed music library line in {id}: {line}");
                continue;
            }
            string[] songThemes = values[1].Split(',');
            AddSong(values[0], songThemes);
        }
    }

    /// <summary>
    /// Adds a new song to the list
    /// </summary>
    /// <param name="id">The file name of the song</param>
    /// <param name="themes">The names of all themes the song is associated to</param>
    /// <exception cref="InvalidOperationException">This song was already loaded.</exception>
    /// <exception cref="IOException">There was a problem loading the song.</exception>
    public void AddSong(string id, params string[] themes)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
        if (themes == null) throw new ArgumentNullException(nameof(themes));
        #endregion

        // Cancel if the music manager already knows the song
        if (_themes.Values.Any(song => song.ID == id))
            throw new InvalidOperationException(Resources.SongAlreadyLoaded + id);

        // Load the song and associate it with its themes
        var newSong = new Song(id) {Engine = engine};
        foreach (string theme in themes)
            _themes.Add(theme, newSong);
    }

    /// <summary>
    /// Plays a specific song
    /// </summary>
    /// <param name="id">The name of the song to play</param>
    public void PlaySong(string id)
    {
        foreach (var song in _themes.Values.Where(x => x.ID == id))
        {
            PlaySong(song);
            break;
        }
    }

    /// <summary>
    /// Plays a specific song
    /// </summary>
    /// <param name="song">The song to play</param>
    private void PlaySong(Song song)
    {
        if (Playing)
        {
            if (_currentSong == song) return;
            Fadeout();
        }
        _currentSong = song;

        _currentSong.Volume = 1f;
        _currentSong.StopPlayback();
        song.StartPlayback(looping: false);
    }

    /// <summary>
    /// Starts playing random songs from a certain theme
    /// </summary>
    /// <param name="theme">The name of the new theme - must not be <c>null</c></param>
    public void PlayTheme(string theme)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(theme)) throw new ArgumentNullException(nameof(theme));
        #endregion

        _currentTheme = theme;

        // Don't switch the song if the current song already fits the new theme
        if (Playing)
            if (_themes[theme].Contains(_currentSong)) return;

        // Find all songs that match the new theme
        var possibleSongs = _themes[_currentTheme].ToArray();

        if (possibleSongs.Length > 0)
        {
            // Plays a randomly selected song from theme
            PlaySong(possibleSongs[RandomUtils.GetRandomInt(0, possibleSongs.Length)]);
        }
    }

    /// <summary>
    /// Switches to a new theme, but doesn't interrupt the current song
    /// </summary>
    /// <param name="theme">The name of the new theme; may be <c>null</c></param>
    public void SwitchTheme(string? theme)
    {
        _currentTheme = theme;
    }

    /// <summary>
    /// Plays the next song from the current theme (if any) if the last one stopped
    /// </summary>
    public void Update()
    {
        if (!string.IsNullOrEmpty(_currentTheme) && !Playing)
            PlayTheme(_currentTheme);
    }

    /// <summary>
    /// Stops the currently playing song
    /// </summary>
    /// <param name="fade">True to fade out the music instead of immediately cutting it off</param>
    public void Stop(bool fade)
    {
        if (!Playing) return;

        if (fade) Fadeout();
        else _currentSong.StopPlayback();
    }

    /// <summary>
    /// Fades out the current song
    /// </summary>
    private void Fadeout()
    {
        if (!Playing) return;

        // No "current song" reference while fading
        Song fadeSong = _currentSong;
        _currentSong = null;

        // Prepare background thread for gradually fading out the song over ~2 seconds
        float startVolume = fadeSong.Volume;
        var fadeThread = new Thread(() =>
        {
            const int steps = 40;
            for (int i = 1; i <= steps; i++)
            {
                Thread.Sleep(50);
                if (fadeSong.IsDisposed || !fadeSong.Playing) return;
                fadeSong.Volume = startVolume * (1f - (float)i / steps);
            }
            if (fadeSong.IsDisposed) return;
            fadeSong.StopPlayback();
        });

        // Start the thread
        fadeThread.Start();
    }

    /// <summary>
    /// Disposes all <see cref="Song"/>s maintained by this <see cref="MusicManager"/>
    /// </summary>
    public void Dispose()
    {
        if (engine is { IsDisposed: false })
        {
            foreach (Song song in _themes.Values.Distinct())
                song.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    ~MusicManager()
    {
        // This block will only be executed on Garbage Collection, not by manual disposal
        Log.Error($"Forgot to call Dispose on {this}");
#if DEBUG
        throw new InvalidOperationException($"Forgot to call Dispose on {this}");
#endif
    }
}
