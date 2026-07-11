/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Threading;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using OmegaEngine.Foundation.Storage;

namespace OmegaEngine.Audio;

/// <summary>
/// A streamed sound that is played in the background as music.
/// </summary>
/// <param name="id">The file name of the song relative to the <c>Music</c> content directory.</param>
public class Song(string id) : EngineElement, IAudio
{
    /// <summary>
    /// The ID of this song (the file name relative to the <c>Music</c> content directory).
    /// </summary>
    public string ID { get; } = id;

    private WaveStream? _reader;
    private ISampleProvider? _activeInput;
    private VolumeSampleProvider? _volumeProvider;

    // Set by the audio thread when the song reaches its end; volatile so the main thread sees it promptly.
    private volatile bool _ended;

    /// <inheritdoc/>
    public bool Playing => _activeInput != null && !_ended;

    private bool _looping;

    /// <inheritdoc/>
    public bool Looping => Playing && _looping;

    private float _volume = 1f;

    /// <inheritdoc/>
    public float Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            if (_volumeProvider != null) _volumeProvider.Volume = value;
        }
    }

    /// <summary>
    /// Starts the song playback
    /// </summary>
    /// <exception cref="FileNotFoundException">The song file could not be found.</exception>
    /// <exception cref="IOException">There was a problem loading the song.</exception>
    /// <exception cref="InvalidDataException">The song file does not contain valid sound data.</exception>
    public void StartPlayback(bool looping)
    {
        #region Sanity checks
        if (IsDisposed) throw new ObjectDisposedException(ToString());
        #endregion

        StopPlayback();

        string path = ContentManager.GetFilePath("Music", ID);
        var reader = AudioHelpers.OpenStream(path);
        _reader = reader;

        var chain = AudioHelpers.EnsureStereo(AudioHelpers.ResampleToMixerRate(reader.ToSampleProvider()));
        if (looping) chain = new LoopingSampleProvider(chain, () => reader.Position = 0);
        _volumeProvider = new(chain) {Volume = _volume};

        _looping = looping;
        _ended = false;
        if (Engine.Audio.AddInput(_volumeProvider, AudioCategory.Music, onEnded: OnEndedNaturally))
            _activeInput = _volumeProvider;
        else
        {
            // Audio disabled: don't hold the file open
            _reader = null;
            _volumeProvider = null;
            reader.Dispose();
        }
    }

    /// <summary>
    /// Stops the song playback
    /// </summary>
    public void StopPlayback()
    {
        // Remove from the mixer first (outside any lock) so the audio thread stops reading before we release the reader
        var input = Interlocked.Exchange(ref _activeInput, null);
        if (input != null) Engine.Audio.RemoveInput(input, AudioCategory.Music);

        _volumeProvider = null;
        _ended = false;

        // Exchange ensures the reader is disposed exactly once, even if OnEndedNaturally races us
        Interlocked.Exchange(ref _reader, null)?.Dispose();
    }

    /// <summary>
    /// Invoked on the audio thread when the song reaches its end on its own. Releases the file handle without touching the mixer (the input has already been removed from it).
    /// </summary>
    private void OnEndedNaturally()
    {
        _ended = true;
        _activeInput = null;
        _volumeProvider = null;
        Interlocked.Exchange(ref _reader, null)?.Dispose();
    }

    /// <inheritdoc/>
    protected override void OnDispose()
    {
        try
        {
            StopPlayback();
        }
        finally
        {
            base.OnDispose();
        }
    }
}
