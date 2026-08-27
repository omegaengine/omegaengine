/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using NAudio.Wave;
using OmegaEngine.Assets;

namespace OmegaEngine.Audio;

/// <summary>
/// A memory-cached sound that is played on-demand.
/// </summary>
public class Sound : AudioElement
{
    /// <summary>A reference to the asset providing the data for this sound.</summary>
    protected readonly XSound Asset;

    private ISampleProvider? _activeInput;
    private SmoothVolumeSampleProvider? _volumeProvider;

    private bool _ended;

    /// <inheritdoc/>
    public override bool Playing => _activeInput != null && !_ended;

    private bool _looping;

    /// <inheritdoc/>
    public override bool Looping => Playing && _looping;

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
    public override void StartPlayback(bool looping)
    {
        #region Sanity checks
        if (IsDisposed) throw new ObjectDisposedException(ToString());
        #endregion

        StopPlayback();

        _looping = looping;
        _ended = false;
        var input = CreatePlaybackChain(looping);
        if (Engine.Audio.AddInput(input, AudioCategory.Sound, onEnded: OnEnded))
            _activeInput = input;
    }

    /// <summary>
    /// Called when the playback finishes on its own, i.e. not via <see cref="StopPlayback"/>. Never called for looping playback.
    /// </summary>
    /// <remarks>
    /// Runs on the audio thread while the mixer is producing samples, so overrides must not block.
    /// </remarks>
    protected virtual void OnEnded() => _ended = true;

    /// <summary>
    /// Stops the sound playback
    /// </summary>
    public override void StopPlayback()
    {
        CancelFade();

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
        _volumeProvider = new(stereo) {Volume = EffectiveVolume};
        return _volumeProvider;
    }

    /// <inheritdoc/>
    protected override void ApplyVolume()
    {
        if (_volumeProvider != null)
            _volumeProvider.Volume = EffectiveVolume;
    }

    /// <inheritdoc/>
    protected override void OnDispose()
    {
        try
        {
            CancelFade();

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
