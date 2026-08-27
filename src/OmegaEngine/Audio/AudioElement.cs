/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace OmegaEngine.Audio;

/// <summary>
/// A common base class for audio playback that can be faded in and out.
/// </summary>
public abstract class AudioElement : EngineElement, IAudio
{
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
    /// How far the current fade has progressed: 0 = silent, 1 = the full <see cref="Volume"/>.
    /// </summary>
    protected float FadeLevel { get; private set; } = 1f;

    /// <summary>
    /// The volume actually handed to the sample providers, i.e. <see cref="Volume"/> scaled by <see cref="FadeLevel"/>.
    /// </summary>
    protected float EffectiveVolume => _volume * FadeLevel;

    /// <summary>
    /// Applies the current <see cref="EffectiveVolume"/> to the active playback (if any).
    /// </summary>
    protected abstract void ApplyVolume();

    /// <inheritdoc/>
    public abstract bool Playing { get; }

    /// <inheritdoc/>
    public abstract bool Looping { get; }

    /// <inheritdoc/>
    public abstract void StartPlayback(bool looping);

    /// <inheritdoc/>
    public abstract void StopPlayback();

    /// <summary>
    /// The fade duration used when no <see cref="AnimationOptions"/> are specified.
    /// </summary>
    private static readonly AnimationOptions DefaultFadeOptions = new(Duration: TimeSpan.FromSeconds(1));

    private IDisposable? _fade;
    private bool _fadingOut;

    /// <summary>
    /// Starts the playback silently and fades it in to the full <see cref="Volume"/>.
    /// </summary>
    /// <param name="looping">Whether the playback should loop.</param>
    /// <param name="options">Options controlling the fade; leave <c>null</c> for the default duration.</param>
    /// <remarks>
    /// Safe to call repeatedly, e.g. once per frame; playback is never restarted while it is already running.
    /// Fading back in while a <see cref="FadeOut"/> is still in progress simply reverses the fade, so the playback continues uninterrupted.
    /// </remarks>
    public void FadeIn(bool looping, AnimationOptions? options = null)
    {
        if (!_fadingOut && (_fade != null || Playing)) return;

        if (!Playing)
        {
            // Start out silent, so that the very first buffer the mixer pulls is already faded down
            FadeLevel = 0;
            StartPlayback(looping);
            if (!Playing) return; // Audio disabled
        }

        StartFade(target: 1, options ?? DefaultFadeOptions);
    }

    /// <summary>
    /// Fades the playback out to silence and then stops it.
    /// </summary>
    /// <param name="options">Options controlling the fade; leave <c>null</c> for the default duration.</param>
    /// <remarks>Safe to call repeatedly, e.g. once per frame.</remarks>
    public void FadeOut(AnimationOptions? options = null)
    {
        if (!Playing || _fadingOut) return;

        StartFade(target: 0, options ?? DefaultFadeOptions);
    }

    /// <summary>
    /// Starts fading <see cref="FadeLevel"/> from its current value towards <paramref name="target"/>, replacing any fade already in progress.
    /// </summary>
    private void StartFade(float target, AnimationOptions options)
    {
        CancelFade(resetLevel: false);

        _fadingOut = target == 0;
        _fade = Engine.Animate(
            start: FadeLevel, end: target,
            callback: value =>
            {
                // The playback may have been disposed or have ended on its own in the meantime
                if (IsDisposed || !Playing)
                {
                    CancelFade();
                    return;
                }

                FadeLevel = (float)value;
                ApplyVolume();

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (value == target)
                {
                    // StopPlayback() cancels the fade and resets the level for the next playback
                    if (_fadingOut) StopPlayback();
                    else CancelFade(resetLevel: false);
                }
            },
            options);
    }

    /// <summary>
    /// Aborts any fade in progress, leaving the playback itself untouched.
    /// </summary>
    /// <param name="resetLevel">Whether to jump back to the full <see cref="Volume"/>.</param>
    protected void CancelFade(bool resetLevel = true)
    {
        _fadingOut = false;
        if (_fade == null) return;

        _fade.Dispose();
        _fade = null;

        if (resetLevel) FadeLevel = 1f;
    }
}
