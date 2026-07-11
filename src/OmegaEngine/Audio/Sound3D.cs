/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Runtime.CompilerServices;
using NAudio.Wave;
using OmegaEngine.Assets;
using OmegaEngine.Foundation.Geometry;

namespace OmegaEngine.Audio;

/// <summary>
/// A memory-cached sound that is played on-demand simulating a position in 3D-space.
/// </summary>
public class Sound3D(XSound sound) : Sound(sound)
{
    private Positional3DSampleProvider? _panner;

    // Boxed in a volatile field, so the audio thread always reads a complete, non-torn DoubleVector3 snapshot.
    private volatile StrongBox<DoubleVector3> _position = new(default);

    /// <summary>
    /// The sound's position in world space
    /// </summary>
    [Description("The sound's position in world space"), Category("Layout")]
    public DoubleVector3 Position
    {
        get => _position.Value;
        set => _position = new(value);
    }

    /// <summary>
    /// Factors describing how the sound's volume attenuates with distance from the listener.
    /// </summary>
    [Description("Factors describing how the sound's volume attenuates with distance from the listener."), Category("Behavior")]
    public Attenuation Attenuation { get; set; } = Attenuation.None;

    /// <inheritdoc/>
    protected override ISampleProvider CreatePlaybackChain(bool looping)
        => _panner = new(Asset.CreateProvider(looping), Attenuation, () => Engine.Audio.ListenerSnapshot, () => Position) { Volume = Volume };

    /// <inheritdoc/>
    protected override void ApplyVolume()
    {
        if (_panner != null)
            _panner.Volume = Volume;
    }

    /// <inheritdoc/>
    public override void StopPlayback()
    {
        base.StopPlayback();
        _panner = null;
    }
}
