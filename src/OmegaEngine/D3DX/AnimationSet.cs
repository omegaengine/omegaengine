/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Numerics;
using System.Runtime.InteropServices;
using SharpGen.Runtime;

namespace OmegaEngine.D3DX;

/// <summary>
/// Wraps an <c>ID3DXAnimationSet</c>: a named set of animations, each driving the transform of one <see cref="Frame"/>.
/// </summary>
/// <remarks>Only samples the animations. Playback state (tracks, time, blending) is up to the caller.</remarks>
internal sealed unsafe class AnimationSet : ComObject
{
    #region vtable
    // ID3DXAnimationSet (d3dx9anim.h)
    private const int
        VtGetName = 3,
        VtGetPeriod = 4,
        VtGetPeriodicPosition = 5,
        VtGetNumAnimations = 6,
        VtGetAnimationNameByIndex = 7,
        VtGetSRT = 9;
    #endregion

    internal AnimationSet(nint nativePointer) : base(nativePointer)
    {}

    /// <summary>
    /// The name of the animation set.
    /// </summary>
    public string Name => Marshal.PtrToStringAnsi(((delegate* unmanaged[Stdcall]<nint, nint>)this[VtGetName])(NativePointer)) ?? "";

    /// <summary>
    /// The duration of one pass through the animation set, in seconds.
    /// </summary>
    public double Period => ((delegate* unmanaged[Stdcall]<nint, double>)this[VtGetPeriod])(NativePointer);

    /// <summary>
    /// Maps a position in time into the <see cref="Period"/>, according to the set's playback type (e.g. looping).
    /// </summary>
    public double GetPeriodicPosition(double position) => ((delegate* unmanaged[Stdcall]<nint, double, double>)this[VtGetPeriodicPosition])(NativePointer, position);

    /// <summary>
    /// The number of animations in the set.
    /// </summary>
    public int AnimationCount => (int)((delegate* unmanaged[Stdcall]<nint, uint>)this[VtGetNumAnimations])(NativePointer);

    /// <summary>
    /// Gets the name of an animation, which matches the name of the <see cref="Frame"/> it drives.
    /// </summary>
    public string GetAnimationName(int animation)
    {
        nint name;
        new Result(((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)this[VtGetAnimationNameByIndex])(NativePointer, (uint)animation, &name)).CheckError();
        return Marshal.PtrToStringAnsi(name) ?? "";
    }

    /// <summary>
    /// Samples the transformation of an animation.
    /// </summary>
    /// <param name="periodicPosition">The position in time, mapped into the period with <see cref="GetPeriodicPosition"/>.</param>
    /// <param name="animation">The index of the animation.</param>
    public AnimationOutput GetTransformation(double periodicPosition, int animation)
    {
        Vector3 scale, translation;
        Quaternion rotation;
        new Result(((delegate* unmanaged[Stdcall]<nint, double, uint, Vector3*, Quaternion*, Vector3*, int>)this[VtGetSRT])(NativePointer, periodicPosition, (uint)animation, &scale, &rotation, &translation)).CheckError();
        return new(scale, rotation, translation);
    }
}

/// <summary>
/// The transformation of a single animation sampled at one point in time.
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
