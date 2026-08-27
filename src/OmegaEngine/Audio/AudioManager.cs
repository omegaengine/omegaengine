/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
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
    /// A global volume multiplier applied to all <see cref="Sound"/> effects, on top of each sound's own <see cref="AudioElement.Volume"/>.
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
    /// A global volume multiplier applied to all <see cref="Song"/> music, on top of each song's own <see cref="AudioElement.Volume"/>.
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

    /// <summary>Positional sounds currently playing, refreshed by <see cref="Update"/>.</summary>
    private readonly HashSet<Sound3D> _positionalSounds = [];

    /// <summary>Scratch copy of <see cref="_positionalSounds"/>, so <see cref="Update"/> can work outside the lock. Main thread only.</summary>
    private readonly List<Sound3D> _positionalSoundsBuffer = [];

    /// <summary>
    /// Starts refreshing a <see cref="Sound3D"/>'s placement once per frame. No-op if already registered.
    /// </summary>
    internal void Register(Sound3D sound)
    {
        lock (_positionalSounds) _positionalSounds.Add(sound);
    }

    /// <summary>
    /// Stops refreshing a <see cref="Sound3D"/>'s placement. No-op if not registered.
    /// </summary>
    internal void Unregister(Sound3D sound)
    {
        lock (_positionalSounds) _positionalSounds.Remove(sound);
    }

    /// <summary>
    /// Refreshes the snapshot taken from <see cref="Listener"/> and the placement of all playing <see cref="Sound3D"/>s. Must be called once per frame on the main thread.
    /// </summary>
    /// <remarks>
    /// Deriving each sound's placement here, from one listener snapshot, is what keeps the audio thread from combining a sound position and a listener position captured in different frames.
    /// That mismatch would otherwise scale with how far the sound and the listener travel between frames.
    /// </remarks>
    public void Update()
    {
        var listener = Listener?.To(ListenerSnapshot.FromViewpoint) ?? ListenerSnapshot.Default;
        _listenerSnapshot = listener;

        _positionalSoundsBuffer.Clear();
        lock (_positionalSounds) _positionalSoundsBuffer.AddRange(_positionalSounds);

        foreach (var sound in _positionalSoundsBuffer)
            sound.UpdatePlacement(listener);
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

            _output = new WaveOutEvent {DesiredLatency = 100, NumberOfBuffers = 4};
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
