/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using NanoByte.Common;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace OmegaEngine.Audio;

/// <summary>
/// Owns the shared NAudio output device and mixer that all <see cref="Sound"/>s and <see cref="Song"/>s feed into.
/// </summary>
public sealed class AudioManager : IDisposable
{
    /// <summary>The sample rate all audio is mixed and played back at.</summary>
    public const int SampleRate = 44100;

    /// <summary>
    /// The wave format all mixer inputs must match: 32-bit float, stereo, <see cref="SampleRate"/>.
    /// </summary>
    public static readonly WaveFormat MixerFormat = WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, channels: 2);

    private readonly IWavePlayer? _output;

    // Per-category sub-mixers, each fed through a volume bus into the master _mixer.
    private readonly MixingSampleProvider? _soundMixer, _musicMixer;
    private readonly VolumeSampleProvider? _soundBus, _musicBus;

    private float _soundVolume = 1f;

    /// <summary>
    /// A global volume multiplier applied to all <see cref="Sound"/> effects, on top of each sound's own <see cref="Sound.Volume"/>.
    /// </summary>
    public float SoundVolume
    {
        get => _soundVolume;
        set
        {
            _soundVolume = value;
            if (_soundBus != null) _soundBus.Volume = value;
        }
    }

    private float _musicVolume = 1f;

    /// <summary>
    /// A global volume multiplier applied to all <see cref="Song"/> music, on top of each song's own <see cref="Song.Volume"/>.
    /// </summary>
    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            _musicVolume = value;
            if (_musicBus != null) _musicBus.Volume = value;
        }
    }

    /// <summary>Callbacks invoked when a mixer input finishes playing naturally</summary>
    private readonly Dictionary<ISampleProvider, Action> _endedCallbacks = new();

    /// <summary>
    /// The source of the position and orientation used as the listener for positional <see cref="Sound3D"/> playback, e.g. the active <see cref="OmegaEngine.Graphics.View"/> or <see cref="OmegaEngine.Graphics.Cameras.Camera"/>. <c>null</c> places the listener at the world origin.
    /// </summary>
    public IViewpoint? Listener { get; set; }

    private volatile ListenerSnapshot _listenerSnapshot = ListenerSnapshot.Default;

    /// <summary>
    /// The most recent snapshot of <see cref="Listener"/>, taken by <see cref="Update"/>. Safe to read from the audio thread.
    /// </summary>
    internal ListenerSnapshot ListenerSnapshot => _listenerSnapshot;

    /// <summary>
    /// Refreshes the snapshot taken from <see cref="Listener"/>. Must be called once per frame on the main thread.
    /// </summary>
    public void Update()
    {
        _listenerSnapshot = Listener?.To(ListenerSnapshot.FromViewpoint) ?? ListenerSnapshot.Default;
    }

    /// <summary>
    /// Creates the output device and mixer.
    /// </summary>
    public AudioManager()
    {
        MixingSampleProvider? mixer;
        try
        {
            // Each category gets its own sub-mixer, wrapped in a volume bus for the global multiplier
            _soundMixer = new(MixerFormat) {ReadFully = true};
            _musicMixer = new(MixerFormat) {ReadFully = true};
            _soundMixer.MixerInputEnded += OnMixerInputEnded;
            _musicMixer.MixerInputEnded += OnMixerInputEnded;
            _soundBus = new(_soundMixer) {Volume = _soundVolume};
            _musicBus = new(_musicMixer) {Volume = _musicVolume};

            mixer = new(MixerFormat) {ReadFully = true};
            mixer.AddMixerInput(_soundBus);
            mixer.AddMixerInput(_musicBus);

            _output = new WaveOutEvent();
            _output.Init(mixer);
            _output.Play();
        }
        catch (Exception ex)
        {
            // No sound card / no driver: continue silently rather than taking down the engine
            Log.Warn($"Audio playback unavailable: {ex.Message}");
            _output?.Dispose();
            _output = null;
            _soundMixer = _musicMixer = null;
            _soundBus = _musicBus = null;
        }
    }

    /// <summary>
    /// Adds a sample provider to the mixer.
    /// </summary>
    /// <param name="provider">The sample provider to play.</param>
    /// <param name="category">Which volume bus to route the provider through.</param>
    /// <param name="onEnded">Optional callback invoked when the provider finishes playing on its own (not via <see cref="RemoveInput"/>).</param>
    /// <returns><c>true</c> if the input was added; <c>false</c> if audio is disabled.</returns>
    public bool AddInput(ISampleProvider provider, AudioCategory category, Action? onEnded = null)
    {
        var mixer = category == AudioCategory.Music ? _musicMixer : _soundMixer;
        if (mixer == null) return false;

        mixer.AddMixerInput(provider);
        if (onEnded != null)
            lock (_endedCallbacks) _endedCallbacks[provider] = onEnded;
        return true;
    }

    /// <summary>
    /// Removes a sample provider previously added via <see cref="AddInput"/>. No-op when disabled or not present.
    /// </summary>
    /// <param name="provider">The sample provider to remove.</param>
    /// <param name="category">The same category the provider was added with.</param>
    public void RemoveInput(ISampleProvider provider, AudioCategory category)
    {
        var mixer = category == AudioCategory.Music ? _musicMixer : _soundMixer;
        if (mixer == null) return;

        mixer.RemoveMixerInput(provider);
        lock (_endedCallbacks) _endedCallbacks.Remove(provider);
    }

    private void OnMixerInputEnded(object? sender, SampleProviderEventArgs e)
    {
        Action? callback;
        lock (_endedCallbacks)
        {
            _endedCallbacks.TryGetValue(e.SampleProvider, out callback);
            _endedCallbacks.Remove(e.SampleProvider);
        }
        callback?.Invoke();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _output?.Stop();
        _output?.Dispose();
    }
}
