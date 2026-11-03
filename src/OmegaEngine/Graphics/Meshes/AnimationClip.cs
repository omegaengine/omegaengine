/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Numerics;

namespace OmegaEngine.Graphics.Meshes;

/// <summary>
/// A named set of keyframe animations, each driving the transform of one frame of an animated mesh.
/// </summary>
/// <param name="Name">The name of the animation.</param>
/// <param name="Period">The duration of one pass through the animation, in seconds.</param>
/// <param name="Channels">The animations of the individual frames.</param>
/// <remarks>Only samples the animations. Playback state (tracks, time, blending) is up to the caller.</remarks>
internal sealed record AnimationClip(string Name, double Period, IReadOnlyList<AnimationChannel> Channels)
{
    /// <summary>
    /// Maps a position in time into the <see cref="Period"/>, looping the animation.
    /// </summary>
    public double GetPeriodicPosition(double position)
    {
        if (Period <= 0) return 0;
        double periodic = position % Period;
        return periodic < 0 ? periodic + Period : periodic;
    }
}

/// <summary>
/// The keyframes animating a single frame.
/// </summary>
/// <param name="FrameIndex">The index of the frame being animated.</param>
/// <param name="Scaling">The scaling keys, sorted by time in seconds; may be empty.</param>
/// <param name="Rotation">The rotation keys, sorted by time in seconds; may be empty.</param>
/// <param name="Translation">The translation keys, sorted by time in seconds; may be empty.</param>
internal sealed record AnimationChannel(int FrameIndex, AnimationKeys<Vector3> Scaling, AnimationKeys<Quaternion> Rotation, AnimationKeys<Vector3> Translation)
{
    /// <summary>
    /// Samples the transformation at a point in time.
    /// </summary>
    /// <param name="position">The position in time in seconds, usually mapped with <see cref="AnimationClip.GetPeriodicPosition"/>.</param>
    /// <param name="fallback">The values used for components without any keys.</param>
    public AnimationOutput Sample(double position, AnimationOutput fallback)
        => new(
            Scaling.Sample(position, Vector3.Lerp) ?? fallback.Scaling,
            Rotation.Sample(position, Quaternion.Slerp) ?? fallback.Rotation,
            Translation.Sample(position, Vector3.Lerp) ?? fallback.Translation);
}

/// <summary>
/// Keyframes for a single value.
/// </summary>
/// <param name="Times">The time of each key in seconds, in ascending order.</param>
/// <param name="Values">The value of each key.</param>
internal sealed record AnimationKeys<T>(double[] Times, T[] Values) where T : struct
{
    /// <summary>
    /// Interpolates the value at a point in time, holding the first and last value outside the keys' range.
    /// </summary>
    /// <returns>The value; <c>null</c> if there are no keys.</returns>
    public T? Sample(double position, Func<T, T, float, T> interpolate)
    {
        if (Times.Length == 0) return null;
        if (position <= Times[0]) return Values[0];
        if (position >= Times[^1]) return Values[^1];

        int index = Array.BinarySearch(Times, position);
        if (index >= 0) return Values[index];

        int next = ~index;
        double start = Times[next - 1], end = Times[next];
        return interpolate(Values[next - 1], Values[next], (float)((position - start) / (end - start)));
    }
}

/// <summary>
/// The transformation of a single animation channel sampled at one point in time.
/// </summary>
/// <param name="Scaling">The scale.</param>
/// <param name="Rotation">The rotation, to be applied after <paramref name="Scaling"/>.</param>
/// <param name="Translation">The translation, to be applied after <paramref name="Rotation"/>.</param>
internal readonly record struct AnimationOutput(Vector3 Scaling, Quaternion Rotation, Vector3 Translation)
{
    /// <summary>
    /// Combines <see cref="Scaling"/>, <see cref="Rotation"/> and <see cref="Translation"/> into a single matrix.
    /// </summary>
    public Matrix4x4 Transformation => Matrix4x4.CreateScale(Scaling) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Translation);
}
