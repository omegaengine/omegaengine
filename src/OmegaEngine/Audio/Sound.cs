/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using OmegaEngine.Assets;

namespace OmegaEngine.Audio;

/// <summary>
/// A memory-cached sound that is played on-demand.
/// </summary>
public class Sound : EngineElement, IAudio
{
    /// <summary>A reference to the asset providing the data for this sound.</summary>
    protected readonly XSound Asset;

    private ISampleProvider? _activeInput;
    private VolumeSampleProvider? _volumeProvider;

    private bool _ended;

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
            ApplyVolume();
        }
    }

    /// <summary>
    /// Sets up a new Sound based on an <see cref="XSound"/> asset.
    /// </summary>
    /// <param name="sound">The <see cref="XSound"/> asset to get the audio data from.</param>
    public Sound(XSound sound)
    {
        Asset = sound ?? throw new ArgumentNullException(nameof(sound));
        Asset.HoldReference();
    }

    /// <summary>
    /// Starts the sound playback
    /// </summary>
    public virtual void StartPlayback(bool looping)
    {
        #region Sanity checks
        if (IsDisposed) throw new ObjectDisposedException(ToString());
        #endregion

        StopPlayback();

        _looping = looping;
        _ended = false;
        var input = CreatePlaybackChain(looping);
        if (Engine.Audio.AddInput(input, AudioCategory.Sound, onEnded: () => _ended = true))
            _activeInput = input;
    }

    /// <summary>
    /// Stops the sound playback
    /// </summary>
    public virtual void StopPlayback()
    {
        if (_activeInput != null)
        {
            Engine.Audio.RemoveInput(_activeInput, AudioCategory.Sound);
            _activeInput = null;
        }

        _volumeProvider = null;
        _ended = false;
    }

    /// <summary>
    /// Builds the chain of sample providers feeding the mixer for a single playback.
    /// </summary>
    /// <param name="looping">Whether the playback should loop.</param>
    /// <returns>The top-level sample provider (stereo, <see cref="AudioManager.SampleRate"/>, IEEE-float).</returns>
    protected virtual ISampleProvider CreatePlaybackChain(bool looping)
    {
        var stereo = AudioHelpers.EnsureStereo(Asset.CreateProvider(looping));
        _volumeProvider = new(stereo) {Volume = _volume};
        return _volumeProvider;
    }

    /// <summary>
    /// Applies the current <see cref="Volume"/> to the active playback (if any).
    /// </summary>
    protected virtual void ApplyVolume()
    {
        if (_volumeProvider != null)
            _volumeProvider.Volume = _volume;
    }

    /// <inheritdoc/>
    protected override void OnDispose()
    {
        try
        {
            if (_activeInput != null)
                Engine.Audio.RemoveInput(_activeInput, AudioCategory.Sound);
            Asset.ReleaseReference();
        }
        finally
        {
            base.OnDispose();
        }
    }
}
