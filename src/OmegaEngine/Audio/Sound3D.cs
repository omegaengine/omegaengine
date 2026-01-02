/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using NanoByte.Common;
using SlimDX;
using SlimDX.DirectSound;
using OmegaEngine.Assets;
using OmegaEngine.Foundation.Geometry;

namespace OmegaEngine.Audio;

/// <summary>
/// A memory-cached sound that is played on-demand simulating a position in 3D-space.
/// </summary>
public class Sound3D : Sound, IFloatingOriginAware
{
    #region Variables
    private readonly SoundBuffer3D _buffer3D;
    #endregion

    #region Properties
    private DoubleVector3 _position;

    /// <summary>
    /// The sound's position in world space
    /// </summary>
    [Description("The body's position in world space"), Category("Layout")]
    public DoubleVector3 Position { get => _position; set => value.To(ref _position, () => _buffer3D.Position = this.GetFloatingPosition()); }

    private DoubleVector3 _floatingOrigin;

    /// <inheritdoc/>
    DoubleVector3 IFloatingOriginAware.FloatingOrigin { get => _floatingOrigin; set => value.To(ref _floatingOrigin, () => _buffer3D.Position = this.GetFloatingPosition()); }

    /// <summary>
    /// The sound's position in render space, based on <see cref="Position"/>
    /// </summary>
    /// <remarks>Constantly changes based on the values set for <see cref="IFloatingOriginAware.FloatingPosition"/></remarks>
    Vector3 IFloatingOriginAware.FloatingPosition => Position.ApplyFloatingOrigin(this);
    #endregion

    #region Constructor
    /// <summary>
    /// Sets up a new Sound based on an <see cref="XSound"/> asset.
    /// </summary>
    /// <param name="sound">The <see cref="XSound"/> asset to get the audio data from.</param>
    public Sound3D(XSound sound) : base(sound)
    {
        #region Sanity checks
        if (sound == null) throw new ArgumentNullException(nameof(sound));
        #endregion

        _buffer3D = new(SoundBuffer);
    }
    #endregion

    //--------------------//

    #region Playback
    /// <summary>
    /// Starts the sound playback
    /// </summary>
    public override void StartPlayback(bool looping)
    {
        #region Sanity checks
        if (IsDisposed) throw new ObjectDisposedException(ToString());
        #endregion

        // ToDo: Implement
    }

    /// <summary>
    /// Stops the sound playback
    /// </summary>
    public override void StopPlayback()
    {
        // ToDo: Implement
    }
    #endregion

    //--------------------//

    #region Dispose
    /// <inheritdoc/>
    protected override void OnDispose()
    {
        try
        {
            if (_buffer3D is { Disposed: false }) _buffer3D.Dispose();
        }
        finally
        {
            base.OnDispose();
        }
    }
    #endregion
}
