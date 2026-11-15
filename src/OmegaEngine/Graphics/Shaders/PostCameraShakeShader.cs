/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using SlimDX;

namespace OmegaEngine.Graphics.Shaders;

/// <summary>
/// A post-screen shader that simulates a "shaking camera" effect.
/// </summary>
/// <remarks>
/// The effect is actually fake, since the perspective cannot be extended beyound the original borders of the scene.
/// Close-up slow-motition observation would reveal the borders to be stretched or squashed.
/// </remarks>
public class PostCameraShakeShader : PostShader
{
    #region Properties
    /// <summary>
    /// How fast to shake the camera - values between -1 and 100
    /// </summary>
    [DefaultValue(20.0f), Description("How fast to shake the camera - values between -1 and 100")]
    public float Speed
    {
        get;
        set
        {
            value = value.Clamp(-1, 100);
            value.To(ref field, () => SetShaderParameter("Speed", value));
        }
    } = 20.0f;

    /// <summary>
    /// How erratically to shake the camera - values between 0 and 1
    /// </summary>
    [DefaultValue(0.25f), Description("How erratically to shake the camera - values between 0 and 1")]
    public float Shakiness
    {
        get;
        set
        {
            value = value.Clamp();
            value.To(ref field, () => SetShaderParameter("Shakiness", value));
        }
    } = 0.25f;

    /// <summary>
    /// How close the origin to keep the shaking view - values between 0 and 10
    /// </summary>
    [DefaultValue(2.2f), Description("How close the origin to keep the shaking view - values between 0 and 10")]
    public float Sharpness
    {
        get;
        set
        {
            value = value.Clamp(0, 10);
            value.To(ref field, () => SetShaderParameter("Sharpness", value));
        }
    } = 2.2f;

    public Vector2 TimeDelta { get; set => value.To(ref field, () => SetShaderParameter("TimeDelta", value)); } = new(1, 0.2f);
    #endregion

    #region Engine
    /// <inheritdoc/>
    protected override void OnEngineSet()
    {
        LoadShaderFile("Post_CameraShake.fxo");

        base.OnEngineSet();
    }
    #endregion
}
