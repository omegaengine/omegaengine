/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Numerics;

namespace OmegaEngine.Graphics.Shaders;

/// <summary>
/// A post-screen shader that blurs the view radially.
/// </summary>
public class PostRadialBlurShader : PostShader
{
    #region Properties
    /// <summary>
    /// The minimum range at which to start blur sampling - values between 0 and 10
    /// </summary>
    [DefaultValue(5.0f), Description("The minimum range at which to start blur sampling - values between 0 and 10")]
    public float BlurStart
    {
        get;
        set
        {
            value = value.Clamp(0, 10);
            value.To(ref field, () => SetShaderParameter("BlurStart", value));
        }
    } = 5.0f;

    /// <summary>
    /// How wide a blur sampling is - higher value = more blurry image - values between -1 and 1
    /// </summary>
    [DefaultValue(-0.05f), Description("The minimum range at which to start blur sampling - values between -1 and 1")]
    public float BlurWidth
    {
        get;
        set
        {
            value = value.Clamp(-1, 1);
            value.To(ref field, () => SetShaderParameter("BlurWidth", value));
        }
    } = -0.05f;

    /// <summary>
    /// The center/origin of the radial blur effect
    /// </summary>
    [Description("The center/origin of the radial blur effect")]
    public Vector2 BlurCenter { get; set => value.To(ref field, () => SetShaderParameter("BlurCenter", value)); } = new(0.5f, 0.5f);
    #endregion

    #region Engine
    /// <inheritdoc/>
    protected override void OnEngineSet()
    {
        LoadShaderFile("Post_RadialBlur.fxo");

        base.OnEngineSet();
    }
    #endregion
}
