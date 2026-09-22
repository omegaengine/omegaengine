/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NAudio.Wave;
using OmegaEngine.Assets;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Graphics.Renderables;
using SlimDX;

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
    /// <remarks>While <see cref="AttachedTo"/> is set, this is recalculated from <see cref="Offset"/> once per frame and setting it has no lasting effect.</remarks>
    [Description("The sound's position in world space"), Category("Layout")]
    public DoubleVector3 Position
    {
        get => _position.Value;
        set => _position = new(value);
    }

    /// <summary>
    /// A <see cref="PositionableRenderable"/> this sound is attached to; <c>null</c> to use <see cref="Position"/> as an absolute world position.
    /// </summary>
    /// <seealso cref="Offset"/>
    [Browsable(false)]
    public PositionableRenderable? AttachedTo { get; set; }

    /// <summary>
    /// The offset from <see cref="AttachedTo"/> in its local coordinate system. Ignored while <see cref="AttachedTo"/> is <c>null</c>.
    /// </summary>
    [Description("The offset from the renderable the sound is attached to, in its local coordinate system"), Category("Layout")]
    public Vector3 Offset { get; set; }

    /// <summary>
    /// Factors describing how the sound's volume attenuates with distance from the listener.
    /// </summary>
    [Description("Factors describing how the sound's volume attenuates with distance from the listener."), Category("Behavior")]
    public Attenuation Attenuation { get; set; } = Attenuation.None;

    // Boxed in a volatile field, so the audio thread always reads a listener and a position originating from the same frame.
    private volatile PlacementSnapshot _placement = PlacementSnapshot.Default;

    /// <summary>
    /// Recalculates where this sound sits relative to <paramref name="listener"/> and publishes it for the audio thread.
    /// </summary>
    /// <remarks>Called once per frame on the main thread by <see cref="AudioManager.Update"/>.</remarks>
    /// <param name="listener">The listener to measure against. Must stem from the same frame as the current <see cref="Position"/>.</param>
    internal void UpdatePlacement(ListenerSnapshot listener)
    {
        // Resolve the position of sounds attached to renderables before measuring against the listener
        if (AttachedTo is {} parent) Position = parent.ToWorld((DoubleVector3)Offset);

        var delta = Position - listener.Position;
        double distance = delta.Length();

        // Determine left/right balance from the lateral angle to the source
        float pan = 0f;
        if (distance > 1e-6)
        {
            var direction = (Vector3)(delta / distance);
            var right = Vector3.Normalize(Vector3.Cross(listener.Up, listener.Forward));
            pan = Math.Max(-1f, Math.Min(1f, Vector3.Dot(direction, right)));
        }

        _placement = new((float)distance, pan);
    }

    /// <inheritdoc/>
    protected override ISampleProvider CreatePlaybackChain(bool looping)
    {
        // Seed the placement before the mixer can pull the first buffer
        UpdatePlacement(Engine.Audio.ListenerSnapshot);
        return _panner = new(Asset.CreateProvider(looping), Attenuation, () => _placement) { Volume = EffectiveVolume };
    }

    /// <inheritdoc/>
    protected override void ApplyVolume()
    {
        if (_panner != null)
            _panner.Volume = EffectiveVolume;
    }

    /// <inheritdoc/>
    public override void StartPlayback(bool looping)
    {
        base.StartPlayback(looping);
        if (Playing) Engine.Audio.Register(this);
    }

    /// <inheritdoc/>
    public override void StopPlayback()
    {
        Engine.Audio.Unregister(this);
        base.StopPlayback();
        _panner = null;
    }

    /// <inheritdoc/>
    protected override void OnEnded()
    {
        base.OnEnded();

        // Playback that ran to its end still holds a registration, which would otherwise keep this instance alive and updated forever
        if (!IsDisposed) Engine.Audio.Unregister(this);
    }

    /// <inheritdoc/>
    protected override void OnDispose()
    {
        try
        {
            Engine.Audio.Unregister(this);
        }
        finally
        {
            base.OnDispose();
        }
    }
}
